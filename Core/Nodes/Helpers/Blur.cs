using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Materia.Imaging;

namespace Materia.Nodes.Helpers
{
    public static class Blur
    {
        public static float[] BoxesForGaussian(double sigma, int n)
        {

            double wIdeal = Math.Sqrt((12 * sigma * sigma / n) + 1);
            double wl = Math.Floor(wIdeal);

            if (wl % 2 == 0) wl--;
            double wu = wl + 2;

            double mIdeal = (12 * sigma * sigma - n * wl * wl - 4 * n * wl - 3 * n) / (-4 * wl - 4);
            double m = Math.Round(mIdeal);

            float[] sizes = new float[n];
            for (int i = 0; i < n; ++i)
            {
                if (i < m)
                {
                    sizes[i] = (float)wl;
                }
                else
                {
                    sizes[i] = (float)wu;
                }
            }
            return sizes;
        }
    }
}
