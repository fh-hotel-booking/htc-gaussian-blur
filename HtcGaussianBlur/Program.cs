using CommandLine;
using OpenCL.Net;
using System;
using System.Drawing;
using System.IO;

namespace HtcGaussianBlur
{
    class Program
    {
        private const string c_outpuFileName = "Output";

        static void Main(string[] args)
        {
            Image inputImage = null;
            Bitmap inputBitmap = null;
            int gaussianFilterKernelSize = 0;
            string outputFilePath = "";
            bool verboseOutput = false;
            Parser.Default.ParseArguments<CliOptions>(args)
                .WithParsed((parsedArgs) =>
                {
                    if (parsedArgs.Verbose)
                    {
                        verboseOutput = true;
                    }
                    gaussianFilterKernelSize = parsedArgs.GaussianFilterKernelSize;
                    if (parsedArgs.GaussianFilterKernelSize <= 0 || parsedArgs.GaussianFilterKernelSize > 9)
                    {
                        Console.WriteLine("Gaussian gilter kernel is outside range! - it is being set to 9");
                        gaussianFilterKernelSize = 9;
                    }

                    outputFilePath = parsedArgs.OutputFilePath;
                    if (parsedArgs.OutputFilePath == null)
                    {
                        outputFilePath = Path.Combine(Path.GetDirectoryName(parsedArgs.InputFilePath), c_outpuFileName + Path.GetExtension(parsedArgs.InputFilePath));
                    }

                    if (!File.Exists(parsedArgs.InputFilePath))
                    {
                        Console.WriteLine("Input filed could not be found!");
                        return;
                    }
                    try
                    {
                        inputImage = Image.FromFile(parsedArgs.InputFilePath);
                        inputBitmap = new Bitmap(inputImage);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Input filed could not be loaded!");
                        if (verboseOutput) { Console.WriteLine(ex); }
                        return;
                    }
                });
            double3[] outputImageArray;
            try
            {
                double3[] inputImageArray = BitmapToInt3Array(inputBitmap);
                outputImageArray = OpenCl.ExecuteOpenCL(inputImageArray, inputImage.Width, inputImage.Height, gaussianFilterKernelSize);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Input image could not be processed!");
                if (verboseOutput) { Console.WriteLine(ex); }
                return;
            }
            try
            {
                Bitmap outputImage = Int3ArrayToBitmap(outputImageArray, inputImage.Width, inputImage.Height);
                outputImage.Save(outputFilePath, inputBitmap.RawFormat);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Output File could not be saved!");
                if (verboseOutput) { Console.WriteLine(ex); }
                return;
            }
            Console.WriteLine($"Success: Output Image has been saved to {outputFilePath}");
        }

        public static double3[] BitmapToInt3Array(Bitmap bitmap)
        {
            double3[] result = new double3[bitmap.Width * bitmap.Height];
            for (int i = 0; i < bitmap.Width; i++)
            {
                for (int j = 0; j < bitmap.Height; j++)
                {
                    Color color = bitmap.GetPixel(i, j);
                    result[j + i * bitmap.Height] = new double3(color.R / 255, color.G / 255, color.B / 255);
                }
            }
            return result;
        }

        public static Bitmap Int3ArrayToBitmap(double3[] array, int width, int height)
        {
            Bitmap bitmap = new Bitmap(width, height);
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    int index = j + i * height;
                    Color color = Color.FromArgb(255, Convert.ToInt32(array[index].s0 * 255), Convert.ToInt32(array[index].s1 * 255), Convert.ToInt32(array[index].s2 * 255));
                    bitmap.SetPixel(i, j, color);
                }
            }
            return bitmap;
        }

        public static byte[] ImageToByteArray(Image imageIn)
        {
            MemoryStream memoryStream = new MemoryStream();
            imageIn.Save(memoryStream, imageIn.RawFormat);
            return memoryStream.ToArray();
        }

        public static Image ByteArrayToImage(byte[] byteArrayIn)
        {
            MemoryStream memoryStream = new MemoryStream(byteArrayIn);
            Image returnImage = Image.FromStream(memoryStream, useEmbeddedColorManagement: true, validateImageData: true);
            return returnImage;
        }
    }
}
