using Materia.Rendering.Extensions;
using System;

namespace Materia.Rendering.Mathematics
{
    public struct MVector
    {
        public float X;
        public float Y;
        public float Z;
        public float W;

        public static readonly MVector One = new MVector(1, 1, 1, 1);
        public static readonly MVector Zero = new MVector(0, 0, 0, 0);
        public static readonly MVector Up = new MVector(0, 1, 0, 1);
        public static readonly MVector Left = new MVector(-1, 0, 0, 1);
        public static readonly MVector Forward = new MVector(0, 0, 1, 1);
        public static readonly MVector Right = new MVector(1, 0, 0, 1);
        public static readonly MVector Down = new MVector(0, -1, 0, 1);
        public static readonly MVector Backward = new MVector(0, 0, -1, 1);

        public float Length()
        {
            return MathF.Sqrt(LengthSqr());
        }

        public float LengthSqr()
        {
            return X * X + Y * Y + Z * Z + W * W;
        }

        public MVector Normalized()
        {
            if(LengthSqr() == 0)
            {
                return new MVector();
            }

            float l = 1.0f / Length();
            return this * l;
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

        public Vector4 ToVector4()
        {
            return new Vector4(X, Y, Z, W);
        }

        public Vector3 ToVector3()
        {
            return new Vector3(X, Y, Z);
        }

        public Vector2 ToVector2()
        {
            return new Vector2(X, Y);
        }

        public MVector Abs()
        {
            return new MVector(Math.Abs(X), Math.Abs(Y), Math.Abs(Z), Math.Abs(W));
        }

        public MVector Ceil()
        {
            return new MVector(MathF.Ceiling(X), MathF.Ceiling(Y), MathF.Ceiling(Z), MathF.Ceiling(W));
        }

        public MVector Floor()
        {
            return new MVector(MathF.Floor(X), MathF.Floor(Y), MathF.Floor(Z), MathF.Floor(W));
        }

        public MVector Clamp(MVector min, MVector max)
        {
            return new MVector(Math.Min(max.X, Math.Max(min.X, X)), Math.Min(max.Y, Math.Max(min.Y, Y)), Math.Min(max.Z, Math.Max(min.Z, Z)), Math.Min(max.W, Math.Max(min.W, W)));
        }

        public MVector Cos()
        {
            return new MVector(MathF.Cos(X), MathF.Cos(Y), MathF.Cos(Z), MathF.Cos(W));
        }

        public MVector Sin()
        {
            return new MVector(MathF.Sin(X), MathF.Sin(Y), MathF.Sin(Z), MathF.Sin(W));
        }

        public MVector Exp()
        {
            return new MVector(MathF.Exp(X), MathF.Exp(Y), MathF.Exp(Z), MathF.Exp(W));
        }

        public MVector Fract()
        {
            return new MVector(X.Fract(), Y.Fract(), Z.Fract(), W.Fract());
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
            return new MVector(MathF.Round(X), MathF.Round(Y), MathF.Round(Z), MathF.Round(W));
        }

        public MVector Sqrt()
        {
            return new MVector(MathF.Sqrt(X), MathF.Sqrt(Y), MathF.Sqrt(Z), MathF.Sqrt(W));
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
            if (LengthSqr() == 0) return;

            float l = 1.0f / Length();
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
            return new MVector(v1.X.Lerp(v2.X, t), v1.Y.Lerp(v2.Y, t), v1.Z.Lerp(v2.Z, t), v1.W.Lerp(v2.W, t));
        }

        public override string ToString()
        {
            return string.Format("{0:0.00},{1:0.00},{2:0.00},{3:0.00}", X, Y, Z, W);
        }
    }
}
