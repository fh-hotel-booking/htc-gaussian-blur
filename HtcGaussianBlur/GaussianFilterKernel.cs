using System;

namespace HtcGaussianBlur
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
            switch(size)
            {
                case 3: return kernel3;
                case 5: return kernel5;
                case 7: return kernel7;
                case 9: return kernel9;
                default: return kernel9;
            }
        }

        public static double[] GetKernelBySize(int size, double sigma = 1.0)
        {
            int size2 = size * size;
            double[] kernel = new double[size2];
            int radius = size / 2;
            double sigma2 = sigma * sigma;
            double normalization = 1.0f / (2.0f * Math.PI * sigma2);

            for (int dy = -radius; dy <= radius; dy++)
            {
                for (int dx = -radius; dx <= radius; dx++)
                {
                    double distance2 = (dx * dx + dy * dy);
                    int index = (dx + radius) * size + (dy + radius);
                    kernel[index] = normalization * Math.Exp(-distance2 / (2.0 * sigma2));
                }
            }
            return kernel;
        }

        private static readonly double[] kernel9 = {
            0.000000, 0.000001, 0.000007, 0.000032, 0.000053, 0.000032, 0.000007, 0.000001, 0.000000,
            0.000001, 0.000020, 0.000239, 0.001072, 0.001768, 0.001072, 0.000239, 0.000020, 0.000001,
            0.000007, 0.000239, 0.002915, 0.013064, 0.021539, 0.013064, 0.002915, 0.000239, 0.000007,
            0.000032, 0.001072, 0.013064, 0.058550, 0.096533, 0.058550, 0.013064, 0.001072, 0.000032,
            0.000053, 0.001768, 0.021539, 0.096533, 0.159156, 0.096533, 0.021539, 0.001768, 0.000053,
            0.000032, 0.001072, 0.013064, 0.058550, 0.096533, 0.058550, 0.013064, 0.001072, 0.000032,
            0.000007, 0.000239, 0.002915, 0.013064, 0.021539, 0.013064, 0.002915, 0.000239, 0.000007,
            0.000001, 0.000020, 0.000239, 0.001072, 0.001768, 0.001072, 0.000239, 0.000020, 0.000001,
            0.000000, 0.000001, 0.000007, 0.000032, 0.000053, 0.000032, 0.000007, 0.000001, 0.000000,
        };

        private static readonly double[] kernel7 = {
            0.000020, 0.000239, 0.001073, 0.001769, 0.001073, 0.000239, 0.000020,
            0.000239, 0.002917, 0.013071, 0.021551, 0.013071, 0.002917, 0.000239,
            0.001073, 0.013071, 0.058582, 0.096585, 0.058582, 0.013071, 0.001073,
            0.001769, 0.021551, 0.096585, 0.159241, 0.096585, 0.021551, 0.001769,
            0.001073, 0.013071, 0.058582, 0.096585, 0.058582, 0.013071, 0.001073,
            0.000239, 0.002917, 0.013071, 0.021551, 0.013071, 0.002917, 0.000239,
            0.000020, 0.000239, 0.001073, 0.001769, 0.001073, 0.000239, 0.000020,
        };

        private static readonly double[] kernel5 = {
            0.002969, 0.013306, 0.021938, 0.013306, 0.002969,
            0.013306, 0.059634, 0.098320, 0.059634, 0.013306,
            0.021938, 0.098320, 0.162103, 0.098320, 0.021938,
            0.013306, 0.059634, 0.098320, 0.059634, 0.013306,
            0.002969, 0.013306, 0.021938, 0.013306, 0.002969,
        };

        private static readonly double[] kernel3 = {
            0.075114, 0.123841, 0.075114,
            0.123841, 0.204180, 0.123841,
            0.075114, 0.123841, 0.075114,};

    }
}
