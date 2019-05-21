using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Materia.Imaging;
using Materia.MathHelpers;
using OpenTK;

namespace Materia.Nodes.Helpers
{
    public static class Utils
    {
        static Matrix3 XYZToRGBMatrix = new Matrix3(new Vector3(3.240479f, -1.537150f, -0.498535f),
                                                    new Vector3(-0.969256f, 1.875992f, 0.041556f),
                                                    new Vector3(0.055648f, -0.204043f, 1.057311f)
                                                    );
        static Matrix3 RGBToXYZMatrix = new Matrix3(new Vector3(0.412453f, 0.357580f, 0.180423f),
                                                    new Vector3(0.212671f, 0.715160f, 0.072169f),
                                                    new Vector3(0.019334f, 0.119193f, 0.950227f)
                                                    );

        static float LabXn = 95.0489f;
        static float LabYn = 100;
        static float LabZn = 108.8840f;
        static float LabAdd = 4.0f / 29.0f;
        static float LabSigma = 6.0f / 29.0f;
        static float LabSigma2 = LabSigma * LabSigma;
        static float LabSigma3 = LabSigma * LabSigma * LabSigma;

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

        public static void ConvertToLRGB(ref MVector v)
        {
            v.X = (float)Math.Pow(v.X, 2.2f);
            v.Y = (float)Math.Pow(v.Y, 2.2f);
            v.Z = (float)Math.Pow(v.Z, 2.2f);
        }

        public static void ConvertToSRGB(ref MVector v)
        {
            v.X = (float)Math.Pow(v.X, 1.0f / 2.2f);
            v.Y = (float)Math.Pow(v.Y, 1.0f / 2.2f);
            v.Z = (float)Math.Pow(v.Z, 1.0f / 2.2f);
        }

        public static float LABFunc(float f)
        {
            if(f > LabSigma3)
            {
                return (float)Math.Pow(f, 1.0f / 3.0f);
            }
            else
            {
                return (f / (3 * LabSigma2)) + LabAdd;
            }
        }

        public static float InvLABFunc(float f)
        {
            if(f > LabSigma)
            {
                return f * f * f;
            }
            else
            {
                return (3 * LabSigma2) * (f - LabAdd);
            }
        }

        public static void ConvertToLAB(ref MVector v)
        {
            Vector3 xyz = ConvertRGBToXYZ(ref v);
            float yfunc = LABFunc(xyz.Y / LabYn);
            float L = 116 * yfunc - 16;
            float a = 500 * (LABFunc(xyz.X / LabXn) - yfunc);
            float b = 200 * (yfunc - LABFunc(xyz.Z / LabZn));

            v.X = L;
            v.Y = a;
            v.Z = b;
        }

        public static void ConvertLABToRGB(ref MVector v)
        {
            float x = LabXn * InvLABFunc((v.X + 16) / 116 + v.Y / 500);
            float y = LabYn * InvLABFunc((v.X + 16) / 116);
            float z = LabZn * InvLABFunc((v.X + 16) / 116 - v.Z / 200);

            Vector3 xyz = new Vector3(x, y, z);
            MVector rgb = ConvertXYZtoRGB(ref xyz);

            v.X = rgb.X;
            v.Y = rgb.Y;
            v.Z = rgb.Z;
        }

        public static Vector3 ConvertRGBToXYZ(ref MVector v)
        {
            Vector3 v2 = new Vector3(v.X, v.Y, v.Z);
            return RGBToXYZMatrix * v2;
        }

        public static MVector ConvertXYZtoRGB(ref Vector3 v)
        {
            Vector3 v2 = XYZToRGBMatrix * v;
            v2.X = Math.Min(1f, Math.Max(0f, v2.X));
            v2.Y = Math.Min(1f, Math.Max(0f, v2.Y));
            v2.Z = Math.Min(1f, Math.Max(0f, v2.Z));

            return new MVector(v2.X, v2.Y, v2.Z);
        }

        /// <summary>
        /// helper function for filling a float bitmap
        /// with a gradient from positions and colors
        /// colors and positions must be the same length
        /// as they should be 1 to 1 reference
        /// </summary>
        /// <param name="dst"></param>
        /// <param name="positions"></param>
        /// <param name="colors"></param>
        public static void CreateGradient(FloatBitmap dst, float[] positions, MVector[] colors)
        {
            if(positions.Length != colors.Length)
            {
                return;
            }


            //sort from least to greater
            Array.Sort(positions);

            List<float> pos = new List<float>(positions);
            List<MVector> cols = new List<MVector>(colors);

            if (positions[0] > 0)
            {
                pos.Insert(0, 0);
                MVector c = colors[0];
                cols.Insert(0, c);
            }

            if(positions[positions.Length - 1] < 1)
            {
                pos.Add(1);
                MVector c = colors[colors.Length - 1];
                cols.Add(c);
            }

            for (int i = 0; i < pos.Count - 1; i++)
            {
                float p1 = pos[i];
                float p2 = pos[i + 1];

                MVector c1 = cols[i];
                MVector c2 = cols[i + 1];

                ConvertToLAB(ref c1);
                ConvertToLAB(ref c2);

                int imin = (int)(p1 * dst.Width);
                int imax = (int)(p2 * dst.Width);

                for (int x = imin; x < imax; x++)
                {
                    //minus 1 on imax otherwise we won't reach 1 for t
                    float t = (float)(x - imin) / (float)(imax - 1 - imin);

                    MVector n = MVector.Lerp(c1, c2, t);
                    
                    ConvertLABToRGB(ref n);

                    for (int y = 0; y < dst.Height; y++)
                    {
                        dst.SetPixel(x, y, n.X, n.Y, n.Z, n.W);
                    }
                }
            }
        }
    }
}
