using OpenCL.Net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HtcGaussianBlur
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

        private static void PrintVector(byte[] vector, int elementSize, string label)
        {
            Console.WriteLine(label + ":");

            for (int i = 0; i < elementSize; ++i)
            {
                Console.Write(vector[i] + " ");
            }

            Console.WriteLine();
        }

        public static byte[] ExecuteOpenCL(byte[] inputImage, int imageWidth, int imageHeight, int gaussianFilterKernelSize)
        {

            // input and output arrays
            int elementSize = imageWidth * imageHeight;
            int dataSize = elementSize * sizeof(byte);
            byte[] inputImageArray = new byte[elementSize];
            byte[] outputImageArray = new byte[elementSize];
            double[] gaussianKernel = GaussianFilterKernel.getKernelBySize(gaussianFilterKernelSize);
            int gaussianKernelDataSize = gaussianKernel.Length * sizeof(byte);

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
            IMem<byte> bufferInputImage = Cl.CreateBuffer<byte>(context, MemFlags.ReadWrite, dataSize, out status);
            CheckStatus(status);
            IMem<byte> bufferOutputImage = Cl.CreateBuffer<byte>(context, MemFlags.ReadOnly, dataSize, out status); ;
            CheckStatus(status);
            IMem<double> bufferGaussianKernel = Cl.CreateBuffer<double>(context, MemFlags.ReadOnly, gaussianKernelDataSize, out status); ;
            CheckStatus(status);

            // write data from the input vectors to the buffers
            CheckStatus(Cl.EnqueueWriteBuffer(commandQueue, bufferInputImage, Bool.True, IntPtr.Zero, new IntPtr(dataSize), inputImageArray, 0, null, out var _));
            CheckStatus(Cl.EnqueueWriteBuffer(commandQueue, bufferGaussianKernel, Bool.True, IntPtr.Zero, new IntPtr(gaussianKernelDataSize), gaussianKernel, 0, null, out var _));
            // CheckStatus(Cl.EnqueueWriteBuffer(commandQueue, bufferOutputImage, Bool.True, IntPtr.Zero, new IntPtr(dataSize), outputImageArray, 0, null, out var _));

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
            OpenCL.Net.Kernel kernel = Cl.CreateKernel(program, c_kernelProgramName, out status);
            CheckStatus(status);

            // set the kernel arguments
            CheckStatus(Cl.SetKernelArg(kernel, 0, bufferInputImage));
            CheckStatus(Cl.SetKernelArg(kernel, 1, bufferOutputImage));
            CheckStatus(Cl.SetKernelArg(kernel, 2, bufferGaussianKernel));
            // CheckStatus(Cl.SetKernelArg(kernel, 2, bufferC));

            // output device capabilities
            IntPtr paramSize;
            CheckStatus(Cl.GetDeviceInfo(device, DeviceInfo.MaxWorkGroupSize, IntPtr.Zero, InfoBuffer.Empty, out paramSize));
            InfoBuffer maxWorkGroupSizeBuffer = new InfoBuffer(paramSize);
            CheckStatus(Cl.GetDeviceInfo(device, DeviceInfo.MaxWorkGroupSize, paramSize, maxWorkGroupSizeBuffer, out paramSize));
            int maxWorkGroupSize = maxWorkGroupSizeBuffer.CastTo<int>();
            Console.WriteLine("Device Capabilities: Max work items in single group: " + maxWorkGroupSize);

            CheckStatus(Cl.GetDeviceInfo(device, DeviceInfo.MaxWorkItemDimensions, IntPtr.Zero, InfoBuffer.Empty, out paramSize));
            InfoBuffer dimensionInfoBuffer = new InfoBuffer(paramSize);
            CheckStatus(Cl.GetDeviceInfo(device, DeviceInfo.MaxWorkItemDimensions, paramSize, dimensionInfoBuffer, out paramSize));
            int maxWorkItemDimensions = dimensionInfoBuffer.CastTo<int>();
            Console.WriteLine("Device Capabilities: Max work item dimensions: " + maxWorkItemDimensions);
            // check if ndRange can be at least 2-dimensional
            if(maxWorkItemDimensions < 2)
            {
                Console.WriteLine("Error: Device needs to support work items with 2 dimensions");
                System.Environment.Exit(1);
            }

            // TODO add check if 2-dimensional NDRange (using width and height of source image) is supported
            CheckStatus(Cl.GetDeviceInfo(device, DeviceInfo.MaxWorkItemSizes, IntPtr.Zero, InfoBuffer.Empty, out paramSize));
            InfoBuffer maxWorkItemSizesInfoBuffer = new InfoBuffer(paramSize);
            CheckStatus(Cl.GetDeviceInfo(device, DeviceInfo.MaxWorkItemSizes, paramSize, maxWorkItemSizesInfoBuffer, out paramSize));
            IntPtr[] maxWorkItemSizes = maxWorkItemSizesInfoBuffer.CastToArray<IntPtr>(maxWorkItemDimensions);
            Console.Write("Device Capabilities: Max work items in group per dimension:");
            for (int i = 0; i < maxWorkItemDimensions; ++i)
                Console.Write(" " + i + ":" + maxWorkItemSizes[i]);
            Console.WriteLine();

            // execute the kernel
            // ndrange capabilities only need to be checked when we specify a local work group size manually
            // in our case we provide NULL as local work group size, which means groups get formed automatically
            CheckStatus(Cl.EnqueueNDRangeKernel(commandQueue, kernel, 2, null, new IntPtr[] { new IntPtr(imageWidth), new IntPtr(imageHeight) }, null, 0, null, out var _));

            // read the device output buffer to the host output array
            CheckStatus(Cl.EnqueueReadBuffer(commandQueue, bufferOutputImage, Bool.True, IntPtr.Zero, new IntPtr(dataSize), outputImageArray, 0, null, out var _));

            // output result
            //PrintVector(inputImageArray, elementSize, "Input Image");
            //PrintVector(outputImageArray, elementSize, "Output Image");

            // release opencl objects
            CheckStatus(Cl.ReleaseKernel(kernel));
            CheckStatus(Cl.ReleaseProgram(program));
            CheckStatus(Cl.ReleaseMemObject(bufferInputImage));
            CheckStatus(Cl.ReleaseMemObject(bufferOutputImage));
            CheckStatus(Cl.ReleaseMemObject(bufferGaussianKernel));
            CheckStatus(Cl.ReleaseCommandQueue(commandQueue));
            CheckStatus(Cl.ReleaseContext(context));
            return inputImage;
        }
    }
}
