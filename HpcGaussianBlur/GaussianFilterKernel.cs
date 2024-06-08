using System;

namespace HpcGaussianBlur
{
    internal static class GaussianFilterKernel
    {
        /// <summary>
        /// Returns a gaussian blure kernel of <see langword="param">size</see>
        /// </summary>
        /// <param name="size"></param>
        /// <returns>gaussian blure kernel of specified size</returns>
        public static double[] GetKernelBySize(int size)
        {
            switch (size)
            {
                case 3: return kernelOptimized3;
                case 5: return kernelOptimized5;
                case 7: return kernelOptimized7;
                case 9: return kernelOptimized9;
                default: return kernelOptimized9;
            }
        }

        private static readonly double[] kernelOptimized9 =
        {
            0.000134, 0.004432, 0.053991, 0.241971, 0.398943, 0.241971, 0.053991, 0.004432, 0.000134,
        };
        private static readonly double[] kernelOptimized7 =
        {
            0.004433, 0.054006, 0.242036, 0.399050, 0.242036, 0.054006, 0.004433,
        };
        private static readonly double[] kernelOptimized5 =
        {
            0.054489, 0.244201, 0.402620, 0.244201, 0.054489,
        };
        private static readonly double[] kernelOptimized3 =
        {
            0.274069, 0.451863, 0.274069,
        };

    }
}
