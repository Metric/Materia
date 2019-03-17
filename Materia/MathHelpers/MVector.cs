using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Materia.Nodes.Helpers;

namespace Materia.MathHelpers
{
    public struct MVector
    {
        public float X;
        public float Y;
        public float Z;
        public float W;

        public float Length
        {
            get
            {
                return (float)Math.Sqrt(LengthSqr);
            }
        }

        public float LengthSqr
        {
            get
            {
                return X * X + Y * Y + Z * Z + W * W;
            }
        }

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
    }
}
