using System;
using System.Drawing;
using Materia.Rendering.Mathematics;

namespace Materia.Rendering.Imaging
{
    public struct HsvColor
    {
        public float H { get; set; }
        public float S { get; set; }
        public float V { get; set; }

        public HsvColor(float h, float s, float v)
        {
            H = h;
            S = s;
            V = v;
        }

        public GLPixel ToGLPixel(float a = 1)
        {
            MVector v = ToMVector(a);
            byte rr = (byte)(v.X * 255);
            byte rg = (byte)(v.Y * 255);
            byte rb = (byte)(v.Z * 255);
            byte ra = (byte)(v.W * 255);

            return new GLPixel()
            {
                r = rr,
                g = rg,
                b = rb,
                a = ra,
                fr = v.X,
                fg = v.Y,
                fb = v.Z,
                fa = v.W
            };
        }

        public Vector4 ToVector(float a)
        {
            MVector v = ToMVector(a);
            return new Vector4(v.X, v.Y, v.Z, a);
        }

        public Vector3 ToVector()
        {
            MVector v = ToMVector(1);
            return new Vector3(v.X, v.Y, v.Z);
        }

        public MVector ToMVector(float a = 1)
        {
            Vector3 c = new Vector3(H, S, V);
            Vector4 k = new Vector4(1.0f, 2.0f / 3.0f, 1.0f / 3.0f, 3.0f);
            Vector3 p = ((c.Xxx + k.Xyz).Fract() * 6.0f - k.Www).Abs();
            Vector3 f = c.Z * Vector3.Lerp(k.Xxx, Vector3.Clamp(p - k.Xxx, Vector3.Zero, Vector3.One), c.Y);
            return new MVector(f.X, f.Y, f.Z, a);
        }

        public Color ToColor()
        {
            Vector3 c = new Vector3(H, S, V);
            Vector4 k = new Vector4(1.0f, 2.0f / 3.0f, 1.0f / 3.0f, 3.0f);
            Vector3 p = ((c.Xxx + k.Xyz).Fract() * 6.0f - k.Www).Abs();
            Vector3 f = c.Z * Vector3.Lerp(k.Xxx, Vector3.Clamp(p - k.Xxx, Vector3.Zero, Vector3.One), c.Y);

            return Color.FromArgb(255, (int)(f.X * 255), (int)(f.Y * 255), (int)(f.Z * 255));
        }

        public static float step(float edge0, float x)
        {
            if (edge0 > x)
            {
                return 0;
            }

            return 1;
        }

        public static float smoothstep(float edge0, float edge1, float x)
        {
            // Scale, bias and saturate x to 0..1 range
            x = clamp((x - edge0) / (edge1 - edge0), 0.0f, 1.0f);
            // Evaluate polynomial
            return x * x * (3 - 2 * x);
        }

        public static float clamp(float x, float lowerlimit, float upperlimit)
        {
            if (x < lowerlimit)
                x = lowerlimit;
            if (x > upperlimit)
                x = upperlimit;
            return x;
        }

        public static HsvColor FromColor(Color c)
        {
            HsvColor hsv = new HsvColor();
            Vector3 v = new Vector3(c.R / 255.0f, c.G / 255.0f, c.B / 255.0f);
            Vector4 k = new Vector4(0, -1.0f / 3.0f, 2.0f / 3.0f, -1.0f);
            Vector4 p = Vector4.Lerp(new Vector4(v.Zy, k.Wz), new Vector4(v.Yz, k.Xy), HsvColor.step(v.Z, v.Y));
            Vector4 q = Vector4.Lerp(new Vector4(p.Xyw, v.X), new Vector4(v.X, p.Yzx), HsvColor.step(p.X, v.X));

            float d = q.X - MathF.Min(q.W, q.Y);
            float e = 1.0e-10f;
            hsv.H = MathF.Abs(q.Z + (q.W - q.Y) / (6.0f * d + e));
            hsv.S = d / (q.X + e);
            hsv.V = q.X;

            return hsv;
        }

        public static HsvColor FromVector(ref Vector3 c)
        {
            Vector4 v = new Vector4(c, 1);
            return FromVector(ref v);
        }

        public static HsvColor FromVector(ref Vector4 v)
        {
            HsvColor hsv = new HsvColor();

            Vector4 k = new Vector4(0, -1.0f / 3.0f, 2.0f / 3.0f, -1.0f);
            Vector4 p = Vector4.Lerp(new Vector4(v.Zy, k.Wz), new Vector4(v.Yz, k.Xy), HsvColor.step(v.Z, v.Y));
            Vector4 q = Vector4.Lerp(new Vector4(p.Xyw, v.X), new Vector4(v.X, p.Yzx), HsvColor.step(p.X, v.X));

            float d = q.X - MathF.Min(q.W, q.Y);
            float e = 1.0e-10f;
            hsv.H = MathF.Abs(q.Z + (q.W - q.Y) / (6.0f * d + e));
            hsv.S = d / (q.X + e);
            hsv.V = q.X;

            return hsv;
        }


        public static HsvColor FromMVector(ref MVector c)
        {
            HsvColor hsv = new HsvColor();

            Vector3 v = new Vector3(c.X, c.Y, c.Z);
            Vector4 k = new Vector4(0, -1.0f / 3.0f, 2.0f / 3.0f, -1.0f);
            Vector4 p = Vector4.Lerp(new Vector4(v.Zy, k.Wz), new Vector4(v.Yz, k.Xy), HsvColor.step(v.Z, v.Y));
            Vector4 q = Vector4.Lerp(new Vector4(p.Xyw, v.X), new Vector4(v.X, p.Yzx), HsvColor.step(p.X, v.X));

            float d = q.X - MathF.Min(q.W, q.Y);
            float e = 1.0e-10f;
            hsv.H = MathF.Abs(q.Z + (q.W - q.Y) / (6.0f * d + e));
            hsv.S = d / (q.X + e);
            hsv.V = q.X;

            return hsv;
        }
    }

    public struct HslColor
    {
        public float H { get; set; }
        public float S { get; set; }
        public float L { get; set; }

        public HslColor(float h, float s, float l)
        {
            H = h;
            S = s;
            L = l;
        }

        public static HslColor FromVector(ref Vector3 c)
        {
            Vector4 v = new Vector4(c, 1);
            return FromVector(ref v);
        }

        public static HslColor FromVector(ref Vector4 c)
        {
            float r = c.X;
            float g = c.Y;
            float b = c.Z;

            float min = MathF.Min(MathF.Min(r, g), b);
            float max = MathF.Max(MathF.Max(r, g), b);
            float delta = max - min;

            float h = 0;
            float s = 0;
            float l = (max + min) * 0.5f;

            if (delta > float.Epsilon)
            {
                if (l < 0.5f)
                {
                    s = (delta / (max + min));
                }
                else
                {
                    s = (delta / (2.0f - max - min));
                }

                if (r == max)
                {
                    h = (g - b) / delta;
                }
                else if (g == max)
                {
                    h = 2f + (b - r) / delta;
                }
                else if (b == max)
                {
                    h = 4f + (r - g) / delta;
                }
            }

            return new HslColor(h, s, l);
        }

        public static HslColor FromMVector(ref MVector c)
        {
            float r = c.X;
            float g = c.Y;
            float b = c.Z;

            float min = MathF.Min(MathF.Min(r, g), b);
            float max = MathF.Max(MathF.Max(r, g), b);
            float delta = max - min;

            float h = 0;
            float s = 0;
            float l = (max + min) * 0.5f;

            if (delta > float.Epsilon)
            {
                if (l < 0.5f)
                {
                    s = (delta / (max + min));
                }
                else
                {
                    s = (delta / (2.0f - max - min));
                }

                if (r == max)
                {
                    h = (g - b) / delta;
                }
                else if (g == max)
                {
                    h = 2f + (b - r) / delta;
                }
                else if (b == max)
                {
                    h = 4f + (r - g) / delta;
                }
            }

            return new HslColor(h, s, l);
        }

        public Color ToColor()
        {
            float r;
            float g;
            float b;

            if (S <= float.Epsilon)
            {
                r = g = b = MathF.Round(MathF.Min(1, MathF.Max(0, L)) * 255);
            }
            else
            {
                float t1, t2;
                float th = H / 6.0f;

                if (L < 0.5f)
                {
                    t2 = L * (1f + S);
                }
                else
                {
                    t2 = (L + S) - (L * S);
                }

                t1 = 2f * L - t2;

                float tr, tg, tb;
                tr = th + (1f / 3f);
                tg = th;
                tb = th - (1f / 3f);

                tr = ColorCalc(tr, t1, t2);
                tg = ColorCalc(tg, t1, t2);
                tb = ColorCalc(tb, t1, t2);

                r = MathF.Round(MathF.Min(1, MathF.Max(0, tr)) * 255);
                g = MathF.Round(MathF.Min(1, MathF.Max(0, tg)) * 255);
                b = MathF.Round(MathF.Min(1, MathF.Max(0, tb)) * 255);
            }

            return Color.FromArgb(255, (int)r, (int)g, (int)b);
        }

        public Vector4 ToVector(float a)
        {
            MVector v = ToMVector(a);
            return new Vector4(v.X, v.Y, v.Z, a);
        }

        public Vector3 ToVector()
        {
            MVector v = ToMVector(1);
            return new Vector3(v.X, v.Y, v.Z);
        }

        public MVector ToMVector(float a = 1)
        {
            float r;
            float g;
            float b;

            if (S <= float.Epsilon)
            {
                r = g = b = MathF.Min(1, MathF.Max(0, L));
            }
            else
            {
                float t1, t2;
                float th = H / 6.0f;

                if (L < 0.5f)
                {
                    t2 = L * (1f + S);
                }
                else
                {
                    t2 = (L + S) - (L * S);
                }

                t1 = 2f * L - t2;

                float tr, tg, tb;
                tr = th + (1f / 3f);
                tg = th;
                tb = th - (1f / 3f);

                tr = ColorCalc(tr, t1, t2);
                tg = ColorCalc(tg, t1, t2);
                tb = ColorCalc(tb, t1, t2);

                if (float.IsNaN(tr))
                {
                    tr = 0;
                }
                if (float.IsNaN(tg))
                {
                    tg = 0;
                }
                if (float.IsNaN(tb))
                {
                    tb = 0;
                }

                r = MathF.Min(1, MathF.Max(0, tr));
                g = MathF.Min(1, MathF.Max(0, tg));
                b = MathF.Min(1, MathF.Max(0, tb));
            }

            return new MVector(r, g, b, a);
        }

        private static float ColorCalc(float c, float t1, float t2)
        {
            if (c < 0) c += 1;
            if (c > 1) c -= 1;
            if (6f * c < 1f) return t1 + (t2 - t1) * 6f * c;
            if (2f * c < 1f) return t2;
            if (3f * c < 2f) return t1 + (t2 - t1) * (2f / 3f - c) * 6f;
            return t1;
        }
    }
}
