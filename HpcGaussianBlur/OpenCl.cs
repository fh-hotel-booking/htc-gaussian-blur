using OpenCL.Net;
using System;
using System.IO;

namespace HpcGaussianBlur
{
    public static class OpenCl
    {
        private const string c_kernelFileName = "kernel.cl";
        private const string c_kernelProgramName = "apply_gaussian_blur";

        private static void CheckStatus(ErrorCode errorCode)
        {
            if (errorCode != ErrorCode.Success)
            {
                Console.WriteLine("OpenCL Error: " + errorCode.ToString());
                System.Environment.Exit(1);
            }
        }

        public static int[] ExecuteOpenCL(int[] inputImage, int imageWidth, int imageHeight, int gaussianFilterKernelSize)
        {
            Console.WriteLine($"Calling with Gaussian Filter Kernel Size: {gaussianFilterKernelSize}");
            // input and output arrays
            int elementSize = inputImage.Length;
            int singleElementInBytes = sizeof(int);
            int imageDataSize = elementSize * singleElementInBytes;
            double[] gaussianKernel = GaussianFilterKernel.GetKernelBySize(gaussianFilterKernelSize);
            int gaussianKernelDataSize = gaussianKernel.Length * sizeof(double);

            int[] outputImageArray = new int[elementSize];
            for (int i = 0; i < elementSize; i++)
            {
                outputImageArray[i] = 0;
            }


            // used for checking error status of api calls
            ErrorCode status;

            // retrieve the number of platforms
            uint numPlatforms = 0;
            CheckStatus(Cl.GetPlatformIDs(0, null, out numPlatforms));

            if (numPlatforms == 0)
            {
                Console.WriteLine("Error: No OpenCL platform available!");
                System.Environment.Exit(1);
            }

            // select the platform
            Platform[] platforms = new Platform[numPlatforms];
            CheckStatus(Cl.GetPlatformIDs(1, platforms, out numPlatforms));
            Platform platform = platforms[0];

            // retrieve the number of devices
            uint numDevices = 0;
            CheckStatus(Cl.GetDeviceIDs(platform, DeviceType.All, 0, null, out numDevices));

            if (numDevices == 0)
            {
                Console.WriteLine("Error: No OpenCL device available for platform!");
                System.Environment.Exit(1);
            }

            // select the device
            Device[] devices = new Device[numDevices];
            CheckStatus(Cl.GetDeviceIDs(platform, DeviceType.All, numDevices, devices, out numDevices));
            Device device = devices[0];

            // create context
            Context context = Cl.CreateContext(null, 1, new Device[] { device }, null, IntPtr.Zero, out status);
            CheckStatus(status);

            // create command queue
            CommandQueue commandQueue = Cl.CreateCommandQueue(context, device, CommandQueueProperties.None, out status);
            CheckStatus(status);

            // allocate two input and one output buffer for the three vectors
            IMem bufferInputImage = Cl.CreateBuffer(context, MemFlags.ReadOnly, new IntPtr(imageDataSize), out status);
            CheckStatus(status);
            IMem bufferOutputImage = Cl.CreateBuffer(context, MemFlags.ReadWrite, new IntPtr(imageDataSize), out status);
            CheckStatus(status);
            IMem<double> bufferGaussianKernel = Cl.CreateBuffer<double>(context, MemFlags.ReadOnly, gaussianKernelDataSize, out status);
            CheckStatus(status);

            // write data from the input vectors to the buffers
            CheckStatus(Cl.EnqueueWriteBuffer(commandQueue, bufferInputImage, Bool.True, IntPtr.Zero, new IntPtr(imageDataSize), inputImage, 0, null, out Event _));
            CheckStatus(Cl.EnqueueWriteBuffer(commandQueue, bufferGaussianKernel, Bool.True, IntPtr.Zero, new IntPtr(gaussianKernelDataSize), gaussianKernel, 0, null, out Event _));

            // create the program
            string programSource = File.ReadAllText(c_kernelFileName);
            OpenCL.Net.Program program = Cl.CreateProgramWithSource(context, 1, new string[] { programSource }, null, out status);
            CheckStatus(status);

            // build the program
            status = Cl.BuildProgram(program, 1, new Device[] { device }, "", null, IntPtr.Zero);
            if (status != ErrorCode.Success)
            {
                InfoBuffer infoBuffer = Cl.GetProgramBuildInfo(program, device, ProgramBuildInfo.Log, out status);
                CheckStatus(status);
                Console.WriteLine("Build Error: " + infoBuffer.ToString());
                System.Environment.Exit(1);
            }

            // create the vector addition kernel
            Kernel kernel = Cl.CreateKernel(program, c_kernelProgramName, out status);
            CheckStatus(status);

            // set the kernel arguments
            CheckStatus(Cl.SetKernelArg(kernel, 0, bufferInputImage));
            CheckStatus(Cl.SetKernelArg(kernel, 1, bufferOutputImage));
            CheckStatus(Cl.SetKernelArg(kernel, 2, bufferGaussianKernel));
            CheckStatus(Cl.SetKernelArg(kernel, 3, new IntPtr(imageWidth * sizeof(int) * 3), null));
            CheckStatus(Cl.SetKernelArg(kernel, 4, gaussianFilterKernelSize));
            CheckStatus(Cl.SetKernelArg(kernel, 5, 0));

            IntPtr paramSize;
            // get maxWorkItemDimensions
            CheckStatus(Cl.GetDeviceInfo(device, DeviceInfo.MaxWorkItemDimensions, IntPtr.Zero, InfoBuffer.Empty, out paramSize));
            InfoBuffer dimensionInfoBuffer = new InfoBuffer(paramSize);
            CheckStatus(Cl.GetDeviceInfo(device, DeviceInfo.MaxWorkItemDimensions, paramSize, dimensionInfoBuffer, out paramSize));
            int maxWorkItemDimensions = dimensionInfoBuffer.CastTo<int>();
            Console.WriteLine("Device Capabilities: Max work item dimensions: " + maxWorkItemDimensions);
            // check if ndRange can be at least 2-dimensional
            if (maxWorkItemDimensions < 2)
            {
                Console.WriteLine("Error: Device needs to support work items with 2 dimensions");
                System.Environment.Exit(1);
            }

            CheckStatus(Cl.GetDeviceInfo(device, DeviceInfo.MaxWorkItemSizes, IntPtr.Zero, InfoBuffer.Empty, out paramSize));
            InfoBuffer maxWorkItemSizesInfoBuffer = new InfoBuffer(paramSize);
            CheckStatus(Cl.GetDeviceInfo(device, DeviceInfo.MaxWorkItemSizes, paramSize, maxWorkItemSizesInfoBuffer, out paramSize));
            IntPtr[] maxWorkItemSizes = maxWorkItemSizesInfoBuffer.CastToArray<IntPtr>(maxWorkItemDimensions);
            Console.Write("Device Capabilities: Max work items in group per dimension:");
            for (int i = 0; i < maxWorkItemDimensions; ++i)
                Console.Write(" " + i + ":" + maxWorkItemSizes[i]);
            Console.WriteLine();

            InfoBuffer maxWorkItemsSizeInfoBuffer = new InfoBuffer(maxWorkItemSizes[0]);
            int maxWorkItemsSizeInfo =maxWorkItemSizesInfoBuffer.CastTo<int>();
            if (maxWorkItemsSizeInfo < imageWidth)
            {
                Console.WriteLine("Error: Device needs to support work group size of image width");
                System.Environment.Exit(1);
            }
            if (maxWorkItemsSizeInfo < imageHeight)
            {
                Console.WriteLine("Error: Device needs to support work group size of image height");
                System.Environment.Exit(1);
            }

            // execute the kernel
            Console.WriteLine($"NDRange: {imageWidth}, {imageHeight}");
            CheckStatus(Cl.EnqueueNDRangeKernel(commandQueue, kernel, 2, null, new IntPtr[] { new IntPtr(imageWidth), new IntPtr(imageHeight) }, new IntPtr[] { new IntPtr(imageWidth), new IntPtr(1) }, 0, null, out Event _));

            // maybe sychronize
            CheckStatus(Cl.SetKernelArg(kernel, 3, new IntPtr(imageHeight * sizeof(int) * 3), null));
            CheckStatus(Cl.SetKernelArg(kernel, 5, 1));
            CheckStatus(Cl.EnqueueNDRangeKernel(commandQueue, kernel, 2, null, new IntPtr[] { new IntPtr(imageWidth), new IntPtr(imageHeight) }, new IntPtr[] { new IntPtr(imageHeight), new IntPtr(1) }, 0, null, out Event _));

            // read the device output buffer to the host output array
            CheckStatus(Cl.EnqueueReadBuffer(commandQueue, bufferOutputImage, Bool.True, IntPtr.Zero, new IntPtr(imageDataSize), outputImageArray, 0, null, out Event _));

            // release opencl objects
            CheckStatus(Cl.ReleaseKernel(kernel));
            CheckStatus(Cl.ReleaseProgram(program));
            CheckStatus(Cl.ReleaseMemObject(bufferInputImage));
            CheckStatus(Cl.ReleaseMemObject(bufferOutputImage));
            CheckStatus(Cl.ReleaseMemObject(bufferGaussianKernel));
            CheckStatus(Cl.ReleaseCommandQueue(commandQueue));
            CheckStatus(Cl.ReleaseContext(context));
            return outputImageArray;
        }
    }
}
