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

        public MVector ToMVector(float a = 1)
        {
            float h = H;
            while (h < 0) h += 360;
            while (h >= 360) h -= 360;
            float R, G, B;
            if (V <= 0)
            {
                R = G = B = 0;
            }
            else if (S <= 0)
            {
                R = G = B = V;
            }
            else
            {
                float hf = h / 60.0f;
                int i = (int)Math.Floor(hf);
                float f = hf - i;
                float pv = V * (1 - S);
                float qv = V * (1 - S * f);
                float tv = V * (1 - S * (1 - f));

                switch (i)
                {
                    case 0:
                        R = V;
                        G = tv;
                        B = pv;
                        break;

                    // Green is the dominant color

                    case 1:
                        R = qv;
                        G = V;
                        B = pv;
                        break;
                    case 2:
                        R = pv;
                        G = V;
                        B = tv;
                        break;

                    // Blue is the dominant color

                    case 3:
                        R = pv;
                        G = qv;
                        B = V;
                        break;
                    case 4:
                        R = tv;
                        G = pv;
                        B = V;
                        break;

                    // Red is the dominant color

                    case 5:
                        R = V;
                        G = pv;
                        B = qv;
                        break;
                    // Just in case we overshoot on our math by a little, we put these here. Since its a switch it won't slow us down at all to put these here.
                    case 6:
                        R = V;
                        G = tv;
                        B = pv;
                        break;
                    case -1:
                        R = V;
                        G = pv;
                        B = qv;
                        break;
                    default:
                        //LFATAL("i Value error in Pixel conversion, Value is %d", i);
                        R = G = B = V; // Just pretend its black/white
                        break;
                }
            }

            R = MathF.Min(1, MathF.Max(0, R));
            G = MathF.Min(1, MathF.Max(0, G));
            B = MathF.Min(1, MathF.Max(0, B));

            if (float.IsNaN(R))
            {
                R = 0;
            }
            if (float.IsNaN(G))
            {
                G = 0;
            }
            if (float.IsNaN(B))
            {
                B = 0;
            }

            return new MVector(R, G, B, a);
        }

        public Color ToColor()
        {
            float h = H;
            while (h < 0) h += 360;
            while (h >= 360) h -= 360;
            float R, G, B;
            if (V <= 0)
            {
                R = G = B = 0;
            }
            else if (S <= 0)
            {
                R = G = B = V;
            }
            else
            {
                float hf = h / 60.0f;
                int i = (int)Math.Floor(hf);
                float f = hf - i;
                float pv = V * (1 - S);
                float qv = V * (1 - S * f);
                float tv = V * (1 - S * (1 - f));

                switch (i)
                {
                    case 0:
                        R = V;
                        G = tv;
                        B = pv;
                        break;

                    // Green is the dominant color

                    case 1:
                        R = qv;
                        G = V;
                        B = pv;
                        break;
                    case 2:
                        R = pv;
                        G = V;
                        B = tv;
                        break;

                    // Blue is the dominant color

                    case 3:
                        R = pv;
                        G = qv;
                        B = V;
                        break;
                    case 4:
                        R = tv;
                        G = pv;
                        B = V;
                        break;

                    // Red is the dominant color

                    case 5:
                        R = V;
                        G = pv;
                        B = qv;
                        break;
                    // Just in case we overshoot on our math by a little, we put these here. Since its a switch it won't slow us down at all to put these here.
                    case 6:
                        R = V;
                        G = tv;
                        B = pv;
                        break;
                    case -1:
                        R = V;
                        G = pv;
                        B = qv;
                        break;
                    default:
                        //LFATAL("i Value error in Pixel conversion, Value is %d", i);
                        R = G = B = V; // Just pretend its black/white
                        break;
                }
            }

            R = Math.Min(255, Math.Max(0, R * 255));
            G = Math.Min(255, Math.Max(0, G * 255));
            B = Math.Min(255, Math.Max(0, B * 255));

            if (float.IsNaN(R))
            {
                R = 0;
            }
            if (float.IsNaN(G))
            {
                G = 0;
            }
            if (float.IsNaN(B))
            {
                B = 0;
            }

            return Color.FromArgb(255, (int)R, (int)G, (int)B);
        }

        public static HsvColor FromColor(Color c)
        {
            HsvColor hsv = new HsvColor();

            int max = Math.Max(c.R, Math.Max(c.G, c.B));
            int min = Math.Min(c.R, Math.Min(c.G, c.B));

            hsv.H = c.GetHue();
            hsv.S = (max == 0) ? 0 : 1.0f - (1.0f * (float)min / (float)max);
            hsv.V = max / 255.0f;

            return hsv;
        }

        public static HsvColor FromMVector(ref MVector c)
        {
            HsvColor hsv = new HsvColor();

            int r = (int)(c.X * 255);
            int g = (int)(c.Y * 255);
            int b = (int)(c.Z * 255);

            int max = Math.Max(r, Math.Max(g, b));
            int min = Math.Min(r, Math.Min(g, b));

            float delta = max - min;
            float h = 0;

            if (delta > float.Epsilon)
            {
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

            hsv.H = h * 360;
            hsv.S = (max == 0) ? 0 : 1.0f - (1.0f * (float)min / (float)max);
            hsv.V = max / 255.0f;

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
