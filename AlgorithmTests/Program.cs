using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Materia.Imaging;

namespace AlgorithmTests
{
    class Program
    {
        const float INF = 1e16f;

        static float square(int q)
        {
            return q * q;
        }

        static float square(float q)
        {
            return q * q;
        }

        static float[] dt(float[] f, int n)
        {
            float[] d = new float[n];
            int[] v = new int[n];
            float[] z = new float[n + 1];
            int k = 0;
            v[0] = 0;
            z[0] = -INF;
            z[1] = INF;
            for(int q = 1; q < n; ++q)
            {
                float s = ((f[q] + square(q)) - (f[v[k]] + square(v[k]))) / (2 * (q / n) - 2 * v[k]);
                while (s <= z[k] && k >= 0)
                {
                    --k;
                    s = ((f[q] + square(q)) - (f[v[k]] + square(v[k]))) / (2 * q - 2 * v[k]);
                }

                ++k;
                v[k] = q;
                z[k] = s;
                z[k + 1] = INF;
            }

            k = 0;
            for(int q = 0; q < n; ++q)
            {
                while (z[k + 1] < q && k < n - 1) ++k;
                d[q] = square(q / (float)n - v[k] / (float)n) + f[v[k]];
            }
            return d;
        }

        static void Main(string[] args)
        {
            string fpath = "E:\\Test\\input.png";
            string spath = "E:\\Test\\output.png";
            using (System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(fpath))
            {
                FloatBitmap bitmap = FloatBitmap.FromBitmap(bmp);

                float[] f = new float[Math.Max(bitmap.Width, bitmap.Height)];

                for(int x = 0; x < bitmap.Width; ++x)
                {
                    for(int y = 0; y < bitmap.Height; ++y)
                    {
                        float r, g, b, a;
                        bitmap.GetPixel(x, y, out r, out g, out b, out a);
                        f[y] = r;
                    }
                    float[] d = dt(f, bitmap.Height);
                    for(int y = 0; y < bitmap.Height; ++y)
                    {
                        float fx = d[y];
                        bitmap.SetPixel(x, y, fx, fx, fx, 1);
                    }
                }

                for(int y = 0; y < bitmap.Height; ++y)
                {
                    for(int x = 0; x < bitmap.Width; ++x)
                    {
                        float r, g, b, a;
                        bitmap.GetPixel(x, y, out r, out g, out b, out a);
                        f[x] = r;
                    }
                    float[] d = dt(f, bitmap.Width);
                    for(int x = 0; x < bitmap.Width; ++x)
                    {
                        float fx = d[x];
                        bitmap.SetPixel(x, y, fx, fx, fx, 1);
                    }
                }
            }
        }
    }
}
