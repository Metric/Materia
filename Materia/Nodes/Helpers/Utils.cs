using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Materia.Imaging;

namespace Materia.Nodes.Helpers
{
    public static class Utils
    {
        public static void Clear(FloatBitmap dst)
        {
            Parallel.For(0, dst.Image.Length, i =>
            {
                dst.Image[i] = 0;
            });
        }

        public static void Clear(RawBitmap dst)
        {
            Parallel.For(0, dst.Image.Length, i =>
            {
                dst.Image[i] = 0;
            });
        }

        public static void Fill(FloatBitmap dst, int xOffset, int yOffset, float r = 0, float g = 0, float b = 0, float a = 0)
        {
            Parallel.For(yOffset, dst.Height, y =>
            {
                for (int x = xOffset; x < dst.Width; x++)
                {
                    dst.SetPixel(x, y, r, g, b, a);
                }
            });
        }

        public static void Copy(FloatBitmap src, FloatBitmap dst)
        {
            Parallel.For(0, dst.Image.Length, i =>
            {
                dst.Image[i] = src.Image[i % src.Image.Length];
            });
        }

        public static byte Lerp(byte v0, byte v1, float t)
        {
            return (byte)Math.Min(255, Math.Max(0, (1 - t) * v0 + t * v1));
        }

        public static float Lerp(float v0, float v1, float t)
        {
            return (1 - t) * v0 + t * v1;
        }

        public static float CubicInterp(float v0, float v1, float v2, float v3, float t)
        {
            float a0, a1, a2, a3, mu2;
            mu2 = t * t;
            a0 = v3 - v2 - v0 + v1;
            a1 = v0 - v1 - a0;
            a2 = v2 - v0;
            a3 = v1;

            return (a0 * t * mu2 + a1 * mu2 + a2 * t + a3);
        }

        public static float CosineInterp(float v0, float v1, float t)
        {
            float mu2 = (1.0f - (float)Math.Cos(t * Math.PI)) * 0.5f;
            return (v0 * (1.0f - mu2) + v1 * mu2);
        }
    }
}
