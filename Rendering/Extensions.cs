using System;
using Materia.Rendering.Mathematics;

namespace Materia.Rendering.Extensions
{
    public static class Extensions
    {
        public static float ToRadians(this float v)
        {
            return v * MathF.PI / 180.0f;
        }

        public static float ToDegrees(this float v)
        {
            return v * 180.0f / MathF.PI;
        }

        public static float Lerp(this float v0, float v1, float t)
        {
            return (1 - t) * v0 + t * v1;
        }

        public static byte Lerp(this byte v0, byte v1, float t)
        {
            return (byte)((1 - t) * v0 + t * v1);
        }

        public static float Clamp(this float f, float min, float max)
        {
            if (f < min)
            {
                return min;
            }
            else if (f > max)
            {
                return max;
            }

            return f;
        }

        public static float Fract(this float f)
        {
            return f - MathF.Floor(f);
        }

        public static bool IsBool(this Type t)
        {
            return t.Equals(typeof(bool));
        }

        public static bool IsNumber(this object o)
        {
            return o is float || o is long || o is double || o is int;
        }

        public static bool IsNumber(this Type t)
        {
            return t.Equals(typeof(float)) || t.Equals(typeof(int))
               || t.Equals(typeof(long)) || t.Equals(typeof(double));
        }

        public static bool IsVector(this Type t)
        {
            return t.Equals(typeof(MVector));
        }

        public static bool ToBool(this object o)
        {
            if (o is MVector)
            {
                MVector v = (MVector)o;
                float f = Math.Max(Math.Max(v.X, v.Y), Math.Max(v.Z, v.W));
                return f > 0;
            }
            else if (o is bool)
            {
                return (bool)o;
            }
            else if (o is float)
            {
                float f = (float)o;
                return f > 0;
            }
            else if (o is double)
            {
                double d = (double)o;
                return d > 0;
            }
            else if (o is long)
            {
                long l = (long)o;
                return l > 0;
            }
            else if (o is int)
            {
                int i = (int)o;
                return i > 0;
            }

            return false;
        }

        public static int ToInt(this object o)
        {
            if (o is int)
            {
                return (int)o;
            }
            else if (o is float)
            {
                float f = (float)o;
                return (int)f;
            }
            else if (o is long)
            {
                long l = (long)o;
                return (int)l;
            }
            else if (o is double)
            {
                double d = (double)o;
                return (int)d;
            }

            return 0;
        }

        public static float ToFloat(this object o)
        {
            if (o is float)
            {
                return (float)o;
            }
            else if (o is bool)
            {
                bool b = (bool)o;
                return b ? 1 : 0;
            }
            else if (o is double)
            {
                double d = (double)o;
                return (float)d;
            }
            else if (o is long)
            {
                long l = (long)o;
                return l;
            }
            else if (o is int)
            {
                int i = (int)o;
                return i;
            }

            return 0;
        }
    }
}
