using System;
using Materia.Rendering.Mathematics;

namespace Materia.Rendering.Extensions
{
    public static class Extensions
    {
        public static float ToRadians(this float v)
        {
            return v * MathHelper.Deg2Rad;
        }

        public static float ToDegrees(this float v)
        {
            return v * MathHelper.Rad2Deg;
        }

        public static float Lerp(this float v0, float v1, float t)
        {
            return (1 - t) * v0 + t * v1;
        }

        public static byte Lerp(this byte v0, byte v1, float t)
        {
            return (byte)((1 - t) * v0 + t * v1);
        }

        public static int Min(this int i, int min)
        {
            return Math.Min(i, min);
        }

        public static int Max(this int i, int max)
        {
            return Math.Max(i, max);
        }

        public static float Min(this float f, float min)
        {
            return MathF.Min(f, min);
        }

        public static float Max(this float f, float max)
        {
            return MathF.Max(f, max);
        }

        public static long Clamp(this long i, long min, long max)
        {
            if (i < min)
            {
                return min;
            }
            else if (i > max)
            {
                return max;
            }
            return i;
        }

        public static int Clamp(this int i, int min, int max)
        {
            if (i < min)
            {
                return min;
            }
            else if(i > max)
            {
                return max;
            }
            return i;
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

        public static bool IsBool(this object o)
        {
            return o is bool;
        }

        public static bool IsBool(this Type t)
        {
            return t.Equals(typeof(bool));
        }

        public static bool IsNumber(this object o)
        {
            return o is float || o is long || o is double || o is int || o is ulong || o is uint || o is ushort || o is short;
        }

        public static bool IsNumber(this Type t)
        {
            return t.Equals(typeof(float)) || t.Equals(typeof(int))
               || t.Equals(typeof(long)) || t.Equals(typeof(double))
               || t.Equals(typeof(ulong)) || t.Equals(typeof(uint))
               || t.Equals(typeof(short)) || t.Equals(typeof(ushort));
        }

        public static bool IsMatrix(this object o)
        {
            return o is Matrix4;
        }

        public static bool IsVector(this object o)
        {
            return o is MVector;
        }

        public static bool IsVector(this Type t)
        {
            return t.Equals(typeof(MVector));
        }

        public static bool IsMatrix(this Type t)
        {
            return t.Equals(typeof(Matrix4));
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
                float f = Convert.ToSingle(o);
                return f > 0;
            }
            else if (o is double)
            {
                double d = Convert.ToDouble(o);
                return d > 0;
            }
            else if (o is long)
            {
                long l = Convert.ToInt64(o);
                return l > 0;
            }
            else if (o is int)
            {
                int i = Convert.ToInt32(o);
                return i > 0;
            }
            else if(o is uint)
            {
                uint i = Convert.ToUInt32(o);
                return i > 0;
            }
            else if(o is ulong)
            {
                ulong i = Convert.ToUInt64(o);
                return i > 0;
            }
            else if(o is ushort)
            {
                ushort i = Convert.ToUInt16(o);
                return i > 0;
            }
            else if(o is short)
            {
                short i = Convert.ToInt16(o);
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
            else if(o is uint)
            {
                return Convert.ToInt32(o);
            }
            else if (o is float)
            {
                return Convert.ToInt32(o);
            }
            else if (o is long)
            {
                return Convert.ToInt32(o);
            }
            else if (o is double)
            {
                return Convert.ToInt32(o);
            }
            else if(o is ulong)
            {
                return Convert.ToInt32(o);
            }
            else if(o is short)
            {
                return Convert.ToInt32(o);
            }
            else if(o is ushort)
            {
                return Convert.ToInt32(o);
            }

            return 0;
        }

        public static float ToFloat(this object o)
        {
            if (o.GetType().IsEnum)
            {
                return Convert.ToSingle(o);
            }
            else if (o is float)
            {
                return (float)o;
            }
            else if (o is bool)
            {
                bool b = Convert.ToBoolean(o);
                return b ? 1 : 0;
            }
            else if (o is double)
            {
                return Convert.ToSingle(o);
            }
            else if (o is long)
            {
                return Convert.ToSingle(o);
            }
            else if (o is int)
            {
                return Convert.ToSingle(o);
            }
            else if (o is uint)
            {
                return Convert.ToSingle(o);
            }
            else if(o is ulong)
            {
                return Convert.ToSingle(o);
            }
            else if(o is short)
            {
                return Convert.ToSingle(o);
            }
            else if(o is ushort)
            {
                return Convert.ToSingle(o);
            }

            return 0;
        }
    }
}
