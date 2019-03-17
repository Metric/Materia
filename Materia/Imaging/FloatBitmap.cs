using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Materia.Nodes.Helpers;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Drawing;

namespace Materia.Imaging
{
    public struct FloatColor
    {
        public float r;
        public float g;
        public float b;
        public float a;

        public FloatColor(float r, float g, float b, float a)
        {
            this.r = r;
            this.g = g;
            this.b = b;
            this.a = a;
        }
    }

    public class FloatBitmap
    {
        public int Width { get; protected set; }
        public int Height { get; protected set; }

        public float[] Image { get; set; }

        public FloatBitmap(int w, int h)
        {
            Width = w;
            Height = h;
            Image = new float[w * h * 4];
        }

        public FloatBitmap(int w, int h, float[] data)
        {
            Width = w;
            Height = h;
            Image = data;
        }

        public static FloatBitmap FromBitmap(Bitmap src)
        {
            FloatBitmap f = new FloatBitmap(src.Width, src.Height);


            int w = src.Width;
            int h = src.Height;

            for(int y = 0; y < h; y++)
            { 
                for (int x = 0; x < w; x++)
                {
                    var c = src.GetPixel(x, y);
                    f.SetPixel(x, y, c.R / 255.0f, c.G / 255.0f, c.B / 255.0f, c.A / 255.0f);
                }
            }

            return f;
        }

        public void GetPixel(int x, int y, out float r, out float g, out float b, out float a)
        {
            r = g = b = 0;
            a = 0;
            int idx = (x + y * Width) * 4;

            if (x >= 0 && x < Width && y >= 0 && y < Height)
            {
                r = Image[idx];
                g = Image[idx + 1];
                b = Image[idx + 2];
                a = Image[idx + 3];
            }
        }

        public void GetPixelBilinear(float x, float y, out float r, out float g, out float b, out float a)
        {
            r = g = b = 0;
            a = 0;

            int minX = (int)Math.Floor(x);
            int minY = (int)Math.Floor(y);
            int maxX = (int)Math.Ceiling(x);
            int maxY = (int)Math.Ceiling(y);

            float tlr = 0, tlg = 0, tlb = 0, tla = 0;
            float trr = 0, trg = 0, trb = 0, tra = 0;
            float blr = 0, blg = 0, blb = 0, bla = 0;
            float brr = 0, brg = 0, brb = 0, bra = 0;

            float deltaX = Math.Abs(x - minX);
            float deltaY = Math.Abs(y - minY);

            //apply bilinear interp

            minX = Math.Abs(minX);
            maxX = Math.Abs(maxX);
            minY = Math.Abs(minY);
            maxY = Math.Abs(maxY);

            GetPixel(minX % Width, minY % Height, out tlr, out tlg, out tlb, out tla);
            GetPixel(maxX % Width, minY % Height, out trr, out trg, out trb, out tra);
            GetPixel(minX % Width, maxY % Height, out blr, out blg, out blb, out bla);
            GetPixel(maxX % Width, maxY % Height, out brr, out brg, out brb, out bra);

            //lerp horizontal first

            float ftopR = Utils.Lerp(tlr, trr, deltaX);
            float ftopG = Utils.Lerp(tlg, trg, deltaX);
            float ftopB = Utils.Lerp(tlb, trb, deltaX);
            float ftopA = Utils.Lerp(tla, tra, deltaX);

            float fbotR = Utils.Lerp(blr, brr, deltaX);
            float fbotG = Utils.Lerp(blg, brg, deltaX);
            float fbotB = Utils.Lerp(blb, brb, deltaX);
            float fbotA = Utils.Lerp(bla, bra, deltaX);

            //lerp vertical

            float fR = Utils.Lerp(ftopR, fbotR, deltaY);
            float fG = Utils.Lerp(ftopG, fbotG, deltaY);
            float fB = Utils.Lerp(ftopB, fbotB, deltaY);
            float fA = Utils.Lerp(ftopA, fbotA, deltaY);

            if (fR < 0) fR = 0;
            if (fG < 0) fG = 0;
            if (fB < 0) fB = 0;
            if (fA < 0) fA = 0;

            r = fR;
            g = fG;
            b = fB;
            a = fA;
        }

        public void SetPixel(int x, int y, float r, float g, float b, float a)
        {
            int idx = (x + y * Width) * 4;
            if (x >= 0 && x < Width && y >= 0 && y < Height)
            {
                Image[idx] = r;
                Image[idx + 1] = g;
                Image[idx + 2] = b;
                Image[idx + 3] = a;
            }
        }

        public void DrawYLine(int x, float r, float g, float b, float a)
        {
            Parallel.For(0, Height, y =>
            {
                SetPixel(x, y, r, g, b, a);
            });
        }

        public void DrawXLine(int y, float r, float g, float b, float a)
        {
            Parallel.For(0, Width, x =>
            {
                SetPixel(x, y, r, g, b, a);
            });
        }

        //Bresenham
        public void DrawLine(int x1, int y1, int x2, int y2, float r, float g, float b, float a)
        {
            int x, y;
            int dx, dy;
            int incx, incy;
            int balance;

            if (x2 >= x1)
            {
                dx = x2 - x1;
                incx = 1;
            }
            else
            {
                dx = x1 - x2;
                incx = -1;
            }

            if (y2 >= y1)
            {
                dy = y2 - y1;
                incy = 1;
            }
            else
            {
                dy = y1 - y2;
                incy = -1;
            }

            x = x1;
            y = y1;

            if (dx >= dy)
            {
                dy <<= 1;
                balance = dy - dx;
                dx <<= 1;

                while (x != x2)
                {
                    SetPixel(x, y, r, g, b, a);
                    if (balance >= 0)
                    {
                        y += incy;
                        balance -= dx;
                    }
                    balance += dy;
                    x += incx;
                }
                SetPixel(x, y, r, g, b, a);
            }
            else
            {
                dx <<= 1;
                balance = dx - dy;
                dy <<= 1;

                while (y != y2)
                {
                    SetPixel(x, y, r, g, b, a);
                    if (balance >= 0)
                    {
                        x += incx;
                        balance -= dy;
                    }
                    balance += dx;
                    y += incy;
                }
                SetPixel(x, y, r, g, b, a);
            }
        }

        public void FillCircle(int r, float red, float green, float blue, float alpha)
        {
            int wh = Width / 2;
            int hh = Height / 2;

            for (int y = -r; y <= r; y++)
            {
                for (int x = -r; x <= r; x++)
                {
                    if (x * x + y * y <= r * r)
                    {
                        SetPixel(Math.Abs(wh + x) % Width, Math.Abs(hh + y) % Height, red, green, blue, alpha);
                    }
                }
            }
        }

        public BitmapSource ToImageSource()
        {
            try
            {
                BitmapSource source = BitmapSource.Create(Width, Height, 72, 72, PixelFormats.Rgba128Float, null, Image, Width * 4 * 4);
                return source;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
                return null;
            }
        }
    }
}
