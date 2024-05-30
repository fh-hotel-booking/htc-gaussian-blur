using CommandLine;
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
                    if (parsedArgs.GaussianFilterKernelSize <= 1 || parsedArgs.GaussianFilterKernelSize > 9 || parsedArgs.GaussianFilterKernelSize % 2 == 0)
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
                        Console.WriteLine("Input file could not be found!");
                        System.Environment.Exit(1);
                    }
                    try
                    {
                        inputImage = Image.FromFile(parsedArgs.InputFilePath);
                        inputBitmap = new Bitmap(inputImage);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Input file could not be loaded!");
                        if (verboseOutput) { Console.WriteLine(ex); }
                        System.Environment.Exit(1);
                    }
                });
            int[] outputImageArray;
            try
            {
                int[] inputImageArray = BitmapToIntArray(inputBitmap);
                outputImageArray = OpenCl.ExecuteOpenCL(inputImageArray, inputBitmap.Width, inputBitmap.Height, gaussianFilterKernelSize);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Input image could not be processed!");
                if (verboseOutput) { Console.WriteLine(ex); }
                return;
            }
            try
            {
                Bitmap outputImage = IntArrayToBitmap(outputImageArray, inputImage.Width, inputImage.Height);
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

        public static int[] BitmapToIntArray(Bitmap bitmap)
        {
            int[] result = new int[bitmap.Width * bitmap.Height * 3];
            for (int y = 0; y < bitmap.Height; y++)
            {
                for (int x = 0; x < bitmap.Width; x++)
                {
                    Color color = bitmap.GetPixel(x, y);
                    int index = (x + y * bitmap.Width) * 3;
                    result[index] = color.R;
                    result[index + 1] = color.G;
                    result[index + 2] = color.B;
                }
            }
            return result;
        }

        public static Bitmap IntArrayToBitmap(int[] array, int width, int height)
        {
            Bitmap bitmap = new Bitmap(width, height);
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int index = (x + y * width) * 3;
                    Color color = Color.FromArgb(255, array[index], array[index + 1], array[index + 2]);
                    bitmap.SetPixel(x, y, color);
                }
            }
            return bitmap;
        }
    }
}
