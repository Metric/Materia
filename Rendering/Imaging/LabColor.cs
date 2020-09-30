using Materia.Rendering.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;

namespace Materia.Rendering.Imaging
{
    public class LabColor
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

        public static void ConvertToLRGB(ref Vector3 v)
        {
            v.X = MathF.Pow(v.X, 2.2f);
            v.Y = MathF.Pow(v.Y, 2.2f);
            v.Z = MathF.Pow(v.Z, 2.2f);
        }

        public static void ConvertToSRGB(ref Vector3 v)
        {
            v.X = MathF.Pow(v.X, 1.0f / 2.2f);
            v.Y = MathF.Pow(v.Y, 1.0f / 2.2f);
            v.Z = MathF.Pow(v.Z, 1.0f / 2.2f);
        }

        public static void ConvertToLRGB(ref Vector4 v)
        {
            v.X = MathF.Pow(v.X, 2.2f);
            v.Y = MathF.Pow(v.Y, 2.2f);
            v.Z = MathF.Pow(v.Z, 2.2f);
        }

        public static void ConvertToSRGB(ref Vector4 v)
        {
            v.X = MathF.Pow(v.X, 1.0f / 2.2f);
            v.Y = MathF.Pow(v.Y, 1.0f / 2.2f);
            v.Z = MathF.Pow(v.Z, 1.0f / 2.2f);
        }

        protected static float LABFunc(float f)
        {
            if (f > LabSigma3)
            {
                return (float)Math.Pow(f, 1.0f / 3.0f);
            }
            else
            {
                return (f / (3 * LabSigma2)) + LabAdd;
            }
        }

        protected static float InvLABFunc(float f)
        {
            if (f > LabSigma)
            {
                return f * f * f;
            }
            else
            {
                return (3 * LabSigma2) * (f - LabAdd);
            }
        }

        public static void ConvertToLAB(ref Vector3 v)
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

        public static void ConvertToLAB(ref Vector4 v)
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

        public static void ConvertLABToRGB(ref Vector3 v)
        {
            float x = LabXn * InvLABFunc((v.X + 16) / 116 + v.Y / 500);
            float y = LabYn * InvLABFunc((v.X + 16) / 116);
            float z = LabZn * InvLABFunc((v.X + 16) / 116 - v.Z / 200);

            Vector3 xyz = new Vector3(x, y, z);
            Vector3 rgb = ConvertXYZtoRGB(ref xyz);

            v.X = rgb.X;
            v.Y = rgb.Y;
            v.Z = rgb.Z;
        }

        public static void ConvertLABToRGB(ref Vector4 v)
        {
            float x = LabXn * InvLABFunc((v.X + 16) / 116 + v.Y / 500);
            float y = LabYn * InvLABFunc((v.X + 16) / 116);
            float z = LabZn * InvLABFunc((v.X + 16) / 116 - v.Z / 200);

            Vector3 xyz = new Vector3(x, y, z);
            Vector3 rgb = ConvertXYZtoRGB(ref xyz);

            v.X = rgb.X;
            v.Y = rgb.Y;
            v.Z = rgb.Z;
        }

        protected static Vector3 ConvertRGBToXYZ(ref Vector3 v)
        {
            Vector3 v2 = new Vector3(v.X, v.Y, v.Z);
            return RGBToXYZMatrix * v2;
        }

        protected static Vector3 ConvertRGBToXYZ(ref Vector4 v)
        {

            Vector3 v2 = new Vector3(v.X, v.Y, v.Z);
            return RGBToXYZMatrix * v2;
        }

        protected static Vector3 ConvertXYZtoRGB(ref Vector3 v)
        {
            Vector3 v2 = XYZToRGBMatrix * v;
            v2.X = Math.Min(1f, Math.Max(0f, v2.X));
            v2.Y = Math.Min(1f, Math.Max(0f, v2.Y));
            v2.Z = Math.Min(1f, Math.Max(0f, v2.Z));

            return new Vector3(v2.X, v2.Y, v2.Z);
        }
    }
}
