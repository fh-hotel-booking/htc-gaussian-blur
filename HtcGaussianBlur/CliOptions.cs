using CommandLine;

namespace HtcGaussianBlur
{
    public class CliOptions
    {
        [Option('v', "verbose", Required = false, HelpText = "Enable verbose output.")]
        public bool Verbose { get; set; }

        [Option('f', "file", Required = true, HelpText = "Image input file path.")]
        public string InputFilePath { get; set; }

        [Option('o', "output", Required = false, HelpText = "Image output file path.")]
        public string OutputFilePath { get; set; }

        [Option('s', "gaussian-filter-kernel-size", Required = false, Default = 9, HelpText = "Kernel size of Gaussian Filter")]
        public int GaussianFilterKernelSize { get; set; }
    }
}
