using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Drawing.Imaging;

namespace Materia.Imaging
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

        public FloatColor ToFloatColor(float a = 1)
        {
            var c = ToColor();
            return new FloatColor(c.R / 255.0f, c.G / 255.0f, c.B / 255.0f, a);
        }

        public Color ToColor()
        {
            float h = H;
            while (h < 0) h += 360;
            while (h >= 360) h -= 360;
            float R, G, B;
            if(V <= 0)
            {
                R = G = B = 0;
            }
            else if( S <= 0)
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

                switch(i)
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

            if(float.IsNaN(R))
            {
                R = 0;
            }
            if(float.IsNaN(G))
            {
                G = 0;
            }
            if(float.IsNaN(B))
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

        public static HsvColor FromFloatColor(ref FloatColor c)
        {
            HsvColor hsv = new HsvColor();

            int r = (int)(c.r * 255);
            int g = (int)(c.g * 255);
            int b = (int)(c.b * 255);

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

        public static HslColor FromFloatColor(ref FloatColor c)
        {
            float r = c.r;
            float g = c.g;
            float b = c.b;

            float min = Math.Min(Math.Min(r, g), b);
            float max = Math.Max(Math.Max(r, g), b);
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

        public static HslColor FromIntColor(ref IntColor c)
        {
            float r = c.r;
            float g = c.g;
            float b = c.b;

            float min = Math.Min(Math.Min(r, g), b);
            float max = Math.Max(Math.Max(r, g), b);
            float delta = max - min;

            float h = 0;
            float s = 0;
            float l = (max + min) * 0.5f;

            if(delta > float.Epsilon)
            {
                if(l < 0.5f)
                {
                    s = (delta / (max + min));
                }
                else
                {
                    s = (delta / (2.0f - max - min));
                }

                if(r == max)
                {
                    h = (g - b) / delta;
                }
                else if(g == max)
                {
                    h = 2f + (b - r) / delta;
                } 
                else if(b == max)
                {
                    h = 4f + (r - g) / delta;
                }
            }

            return new HslColor(h, s, l);
        }

        public IntColor ToIntColor(int withAlpha = 255)
        {
            int r;
            int g;
            int b;

            if (S <= float.Epsilon)
            {
                r = g = b = (int)Math.Round(L * 255f);
            }
            else
            {
                float t1, t2;
                float th = H / 6.0f;

                if(L < 0.5f)
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

                r = (int)Math.Round(tr * 255f);
                g = (int)Math.Round(tg * 255f);
                b = (int)Math.Round(tb * 255f);
            }

            return IntColor.FromArgb(withAlpha, r, g, b);
        }

        public FloatColor ToFloatColor(float withAlpha = 1)
        {
            float r;
            float g;
            float b;

            if (S <= float.Epsilon)
            {
                r = g = b = (byte)Math.Min(1, Math.Max(0, Math.Round(L)));
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

                r = (float)Math.Min(1, Math.Max(0, Math.Round(tr)));
                g = (float)Math.Min(1, Math.Max(0, Math.Round(tg)));
                b = (float)Math.Min(1, Math.Max(0, Math.Round(tb)));
            }

            return new FloatColor(r, g, b, withAlpha);
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
    /// <summary>
    /// This class it to get around the weird bug
    /// with the Color class duplicate in the CoreCompat.System.Drawing 
    /// and CoreCompat.System.Drawing.Primitives
    /// it is very minimal
    /// </summary>
    public struct IntColor
    {
        static float InverseInt = 1.0f / 255.0f;

        public int R { get; set; }
        public int G { get; set; }
        public int B { get; set; }
        public int A { get; set; }

        public float r { get; set; }
        public float g { get; set; }
        public float b { get; set; }
        public float a { get; set; }

        private static IntColor white = new IntColor(255, 255, 255, 255);
        private static IntColor black = new IntColor(255, 0, 0, 0);
        public static IntColor White
        {
            get
            {
                return white;
            }
        }
        public static IntColor Black
        {
            get
            {
                return black;
            }
        }

        public IntColor(int a, int r, int g, int b)
        {
            R = r; G = g; B = b; A = a;
            this.r = r * InverseInt;
            this.g = g * InverseInt;
            this.b = b * InverseInt;
            this.a = a * InverseInt;
        }

        public IntColor(int ARGB)
        {
            A = (ARGB >> 24) & 255;
            R = (ARGB >> 16) & 255;
            G = (ARGB >> 8) & 255;
            B = (ARGB & 255);
            a = A * InverseInt;
            r = R * InverseInt;
            g = G * InverseInt;
            b = B * InverseInt;
        }

        public int ToArgb()
        {
            return (A & 255) << 24 | (R & 255) << 16 | (G & 255) << 8 | (B & 255);
        }

        public static IntColor FromArgb(int a, int r, int g, int b)
        {
            return new IntColor(a, r, g, b);
        }

        public static IntColor FromArgb(int argb)
        {
            return new IntColor(argb);
        }
    }

    /// <summary>
    /// see https://stackoverflow.com/questions/24701703/c-sharp-faster-alternatives-to-setpixel-and-getpixel-for-bitmaps-for-windows-f
    /// this is also doable with the CoreCompat.System.Drawing in .net core 2.1!
    /// </summary>

    public class DirectBitmap : IDisposable
    {
        public Bitmap Bitmap { get; private set; }
        public Int32[] Bits { get; private set; }
        public bool Disposed { get; private set; }
        public int Height { get; private set; }
        public int Width { get; private set; }

        protected GCHandle BitsHandle { get; private set; }

        public DirectBitmap(Bitmap src)
        {
            Width = src.Width;
            Height = src.Height;
            Bits = new Int32[Width * Height];
            BitsHandle = GCHandle.Alloc(Bits, GCHandleType.Pinned);
            Bitmap = new Bitmap(Width, Height, Width * 4, PixelFormat.Format32bppPArgb, BitsHandle.AddrOfPinnedObject());
            CopyFromBitmap(src);
        }

        public DirectBitmap(int width, int height)
        {
            Width = width;
            Height = height;
            Bits = new Int32[width * height];
            BitsHandle = GCHandle.Alloc(Bits, GCHandleType.Pinned);
            Bitmap = new Bitmap(width, height, width * 4, PixelFormat.Format32bppPArgb, BitsHandle.AddrOfPinnedObject());
        }

        public void SetPixel(int x, int y, ref IntColor colour)
        {
            int index = x + (y * Width);
            int col = colour.ToArgb();

            Bits[index] = col;
        }

        public void CopyFromBitmap(Bitmap src)
        {
            for(int y = 0; y < Height; y++)
            {
                for(int x = 0; x < Width; x++)
                {
                    Color c = src.GetPixel(x, y);
                    IntColor ic = IntColor.FromArgb(c.A, c.R, c.G, c.B);
                    SetPixel(x, y, ref ic);
                }
            }
        }

        public IntColor GetPixel(int x, int y)
        {
            int index = x + (y * Width);
            int col = Bits[index];
            IntColor result = IntColor.FromArgb(col);

            return result;
        }

        public void Dispose()
        {
            if (Disposed) return;
            Disposed = true;
            Bitmap.Dispose();
            BitsHandle.Free();
        }
    }
}
