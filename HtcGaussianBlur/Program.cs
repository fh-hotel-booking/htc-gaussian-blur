using System;
using System.Drawing;
using System.IO;
using CommandLine;
using HtcGaussianBlur;
using OpenCL.Net;

namespace OpenCLdotNet
{
    class Program
    {
        private const string c_outpuFileName = "Output";

        static void Main(string[] args)
        {
            Image inputImage = null;
            Bitmap inputBitmap = null;
            var gaussianFilterKernelSize = 0;
            var outputFilePath = "";
            var verboseOutput = false;
            Parser.Default.ParseArguments<CliOptions>(args)
                .WithParsed((parsedArgs) => {
                    if (parsedArgs.Verbose)
                    {
                        verboseOutput = true;
                    }
                    gaussianFilterKernelSize = parsedArgs.GaussianFilterKernelSize;
                    if(parsedArgs.GaussianFilterKernelSize <= 0 || parsedArgs.GaussianFilterKernelSize > 9)
                    {
                        Console.WriteLine("Gaussian gilter kernel is outside range! - it is being set to 9");
                        gaussianFilterKernelSize = 9;
                    }

                    outputFilePath = parsedArgs.OutputFilePath;
                    if (parsedArgs.OutputFilePath == null)
                    {
                        outputFilePath = Path.Combine(Path.GetDirectoryName(parsedArgs.InputFilePath), c_outpuFileName + Path.GetExtension(parsedArgs.InputFilePath));
                    }
                    
                    if(!File.Exists(parsedArgs.InputFilePath))
                    {
                        Console.WriteLine("Input filed could not be found!");
                        return;
                    }
                    try
                    {
                        inputImage = Bitmap.FromFile(parsedArgs.InputFilePath);
                        inputBitmap = new Bitmap(inputImage);
                    } catch (Exception ex)
                    {
                        Console.WriteLine("Input filed could not be loaded!");
                        if (verboseOutput) {  Console.WriteLine(ex); }
                        return;
                    }
                });
            int3[] outputImageArray;
            try
            {
                var inputImageArray = BitmapToInt3Array(inputBitmap);
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
                var outputImage = Int3ArrayToBitmap(outputImageArray, inputImage.Width, inputImage.Height);
                outputImage.Save(outputFilePath);
            } catch(Exception ex)
            {
                Console.WriteLine("Output File could not be saved!");
                if (verboseOutput) { Console.WriteLine(ex); }
                return;
            }
            Console.WriteLine($"Success: Output Image has been saved to {outputFilePath}");
        }

        public static int3[] BitmapToInt3Array(Bitmap bitmap)
        {
            int3[] result = new int3[bitmap.Width * bitmap.Height];
            for (int i = 0; i < bitmap.Width; i++)
            {
                for(int j = 0; j < bitmap.Height; j++)
                {
                    var color = bitmap.GetPixel(i, j);
                    result[i * bitmap.Width + j] = new int3(color.R, color.G, color.B);
                }
            }
            return result;
        }

        public static Bitmap Int3ArrayToBitmap(int3[] array, int width, int height)
        {
            Bitmap bitmap = new Bitmap(width, height);
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    var index = i * width + j;
                    var color = Color.FromArgb(255, array[index].s0, array[index].s1, array[index].s2);
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
            var memoryStream = new MemoryStream(byteArrayIn);
            var returnImage = Image.FromStream(memoryStream, useEmbeddedColorManagement: true, validateImageData: true);
            return returnImage;
        }
    }
}
