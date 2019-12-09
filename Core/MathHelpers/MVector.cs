using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Materia.Nodes.Helpers;
using Newtonsoft.Json;

namespace Materia.MathHelpers
{
    public struct MVector
    {
        public float X;
        public float Y;
        public float Z;
        public float W;

        [JsonIgnore]
        public float Length
        {
            get
            {
                return (float)Math.Sqrt(LengthSqr);
            }
        }

        [JsonIgnore]
        public float LengthSqr
        {
            get
            {
                return X * X + Y * Y + Z * Z + W * W;
            }
        }

        [JsonIgnore]
        public MVector Normalized
        {
            get
            {
                if(LengthSqr == 0)
                {
                    return new MVector();
                }

                float l = 1.0f / Length;
                return this * l;
            }
        }

        public MVector(float x, float y)
        {
            this.X = x;
            this.Y = y;
            Z = 0;
            W = 0;
        }

        public MVector(float x, float y, float z)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
            W = 0;
        }

        public MVector(float x, float y, float z, float w)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
            this.W = w;
        }

        public MVector Abs()
        {
            return new MVector(Math.Abs(X), Math.Abs(Y), Math.Abs(Z), Math.Abs(W));
        }

        public MVector Ceil()
        {
            return new MVector((float)Math.Ceiling(X), (float)Math.Ceiling(Y), (float)Math.Ceiling(Z), (float)Math.Ceiling(W));
        }

        public MVector Floor()
        {
            return new MVector((float)Math.Floor(X), (float)Math.Floor(Y), (float)Math.Floor(Z), (float)Math.Floor(W));
        }

        public MVector Clamp(MVector min, MVector max)
        {
            return new MVector(Math.Min(max.X, Math.Max(min.X, X)), Math.Min(max.Y, Math.Max(min.Y, Y)), Math.Min(max.Z, Math.Max(min.Z, Z)), Math.Min(max.W, Math.Max(min.W, W)));
        }

        public MVector Cos()
        {
            return new MVector((float)Math.Cos(X), (float)Math.Cos(Y), (float)Math.Cos(Z), (float)Math.Cos(W));
        }

        public MVector Sin()
        {
            return new MVector((float)Math.Sin(X), (float)Math.Sin(Y), (float)Math.Sin(Z), (float)Math.Sin(W));
        }

        public MVector Exp()
        {
            return new MVector((float)Math.Exp(X), (float)Math.Exp(Y), (float)Math.Exp(Z), (float)Math.Exp(W));
        }

        public MVector Fract()
        {
            return new MVector(X - (float)Math.Floor(X), Y - (float)Math.Floor(Y), Z - (float)Math.Floor(Z), W - (float)Math.Floor(W));
        }

        public MVector Mod(float m)
        {
            return new MVector(X % m, Y % m, Z % m, W % m);
        }

        public MVector Negate()
        {
            return new MVector(-X, -Y, -Z, -W);
        }

        public MVector Round()
        {
            return new MVector((float)Math.Round(X), (float)Math.Round(Y), (float)Math.Round(Z), (float)Math.Round(W));
        }

        public MVector Sqrt()
        {
            return new MVector((float)Math.Sqrt(X), (float)Math.Sqrt(Y), (float)Math.Sqrt(Z), (float)Math.Sqrt(W));
        }

        public float Distance(MVector v)
        {
            float dx = X - v.X;
            float dy = Y - v.Y;
            float dz = Z - v.Z;
            float dw = W - v.W;

            return (float)Math.Sqrt(dx * dx + dy * dy + dz * dz + dw * dw);
        }

        public void Normalize()
        {
            if (LengthSqr == 0) return;

            float l = 1.0f / Length;
            X *= l;
            Y *= l;
            Z *= l;
            W *= l;
        }

        public override bool Equals(object obj)
        {
            if(obj is MVector)
            {
                MVector c = (MVector)obj;
                return c.X == X && c.Y == Y && c.Z == Z && c.W == W;
            }

            return false;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int k = 0;

                k = (int)(X * 23);
                k ^= (int)(Y * 17);
                k ^= (int)(Z * 17);
                k ^= (int)(W * 17);

                k = k % (int.MaxValue - 1);

                return k;
            }
        }

        public float[] ToArray()
        {
            return new float[] { X, Y, Z, W };
        }

        public static MVector FromArray(float[] arr)
        {
            MVector m = new MVector();

            if(arr.Length == 1)
            {
                m.X = arr[0];
            }
            else if(arr.Length == 2)
            {
                m.X = arr[0];
                m.Y = arr[1];
            }
            else if(arr.Length == 3)
            {
                m.X = arr[0];
                m.Y = arr[1];
                m.Z = arr[2];
            }
            else if(arr.Length >= 4)
            {
                m.X = arr[0];
                m.Y = arr[1];
                m.Z = arr[2];
                m.W = arr[3];
            }

            return m;
        }

        public static MVector operator *(float t, MVector v1)
        {
            return new MVector(v1.X * t, v1.Y * t, v1.Z * t, v1.W * t);
        }

        public static MVector operator *(MVector v1, float t)
        {
            return new MVector(v1.X * t, v1.Y * t, v1.Z * t, v1.W * t);
        }

        public static MVector operator *(MVector v1, MVector v2)
        {
            return new MVector(
                                v1.X * v2.X,
                                v1.Y * v2.Y,
                                v1.Z * v2.Z,
                                v1.W * v2.W
                               );
        }

        public static MVector operator /(float t, MVector v1)
        {
            return new MVector(
                    t / (v1.X + float.Epsilon),
                    t / (v1.Y + float.Epsilon),
                    t / (v1.Z + float.Epsilon),
                    t / (v1.W + float.Epsilon)
                );
        }

        public static MVector operator /(MVector v1, float t)
        {
            if (Math.Abs(t) <= float.Epsilon) return new MVector();
            return new MVector(v1.X / t, v1.Y / t, v1.Z / t, v1.W / t);
        }

        public static MVector operator /(MVector v1, MVector v2)
        {
            return new MVector(
                    v1.X / (v2.X + float.Epsilon),
                    v1.Y / (v2.Y + float.Epsilon),
                    v1.Z / (v2.Z + float.Epsilon),
                    v1.W / (v2.W + float.Epsilon)
                );
        }

        public static MVector operator -(float t, MVector v1)
        {
            return new MVector(t - v1.X, t - v1.Y, t - v1.Z, t - v1.W);
        }

        public static MVector operator -(MVector v1, float t)
        {
            return new MVector(v1.X - t, v1.Y - t, v1.Z - t, v1.W - t);
        }

        public static MVector operator -(MVector v1, MVector v2)
        {
            return new MVector(v1.X - v2.X, v1.Y - v2.Y, v1.Z - v2.Z, v1.W - v2.W);
        }

        public static MVector operator +(float t, MVector v1)
        {
            return new MVector(v1.X + t, v1.Y + t, v1.Z + t, v1.W + t);
        }

        public static MVector operator +(MVector v1, float t)
        {
            return new MVector(v1.X + t, v1.Y + t, v1.Z + t, v1.W + t);
        }

        public static MVector operator +(MVector v1, MVector v2)
        {
            return new MVector(v1.X + v2.X, v1.Y + v2.Y, v1.Z + v2.Z, v1.W + v2.W);
        }

        public static float Dot(MVector v1, MVector v2)
        {
            return v1.X * v2.X + v1.Y * v2.Y + v1.Z * v2.Z + v1.W * v2.W;
        }

        public static MVector Cross(MVector v1, MVector v2)
        {
            return new MVector(
                            v1.Y * v2.Z - v1.Z * v1.Y,
                            v1.Z * v2.X - v1.X * v2.Z,
                            v1.X * v2.Y - v1.Y * v2.X,
                            v1.Y * v2.W - v1.W * v2.Y
                            );
        }

        public static MVector Lerp(MVector v1, MVector v2, float t)
        {
            return new MVector(Utils.Lerp(v1.X, v2.X, t), Utils.Lerp(v1.Y, v2.Y, t), Utils.Lerp(v1.Z, v2.Z, t), Utils.Lerp(v1.W, v2.W, t));
        }

        public override string ToString()
        {
            return string.Format("{0:0.00},{1:0.00},{2:0.00},{3:0.00}", X, Y, Z, W);
        }
    }
}
