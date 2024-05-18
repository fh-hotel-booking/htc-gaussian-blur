using System;
using System.Drawing;
using System.IO;
using System.Reflection;
using CommandLine;
using CommandLine.Text;
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
                    } catch (Exception ex)
                    {
                        Console.WriteLine("Input filed could not be loaded!");
                        if (verboseOutput) {  Console.WriteLine(ex); }
                        return;
                    } 
                });
            byte[] outputImageBytes;
            try
            {
                var inputImageBytes = ImageToByteArray(inputImage);
                outputImageBytes = OpenCl.ExecuteOpenCL(inputImageBytes, inputImage.Width, inputImage.Height, gaussianFilterKernelSize);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Input image could not be processed!");
                if (verboseOutput) { Console.WriteLine(ex); }
                return;
            }
            try
            {
                var outputImage = ByteArrayToImage(outputImageBytes);
                outputImage.Save(outputFilePath);
            } catch(Exception ex)
            {
                Console.WriteLine("Output File could not be saved!");
                if (verboseOutput) { Console.WriteLine(ex); }
                return;
            }
            Console.WriteLine($"Success: Output Image has been saved to {outputFilePath}");
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
