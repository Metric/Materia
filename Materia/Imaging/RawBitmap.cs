using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using Materia.Nodes.Helpers;

namespace Materia.Imaging
{
    public struct ByteColor
    {
        public byte r;
        public byte g;
        public byte b;
        public byte a;

        public ByteColor(byte red, byte green, byte blue, byte alpha)
        {
            r = red;
            g = green;
            b = blue;
            a = alpha;
        }
    }

    public class RawBitmap
    {
        public int Width { get; protected set; }
        public int Height { get; protected set; }

        public byte[] Image { get; set; }

        public RawBitmap(int w, int h, byte[] data)
        {
            Width = w;
            Height = h;
            Image = data;
        }

        public RawBitmap(int w, int h)
        {
            Width = w;
            Height = h;
            Image = new byte[w * h * 4];
        }

        public void GetPixel(int x, int y, out byte r, out byte g, out byte b, out byte a)
        {
            r = g = b = 0;
            a = 0;
            int idx = (x + y * Width) * 4;

            if (x >= 0 && x < Width && y >= 0 && y < Height)
            {
                r = Image[idx + 2];
                g = Image[idx + 1];
                b = Image[idx];
                a = Image[idx + 3];
            }
        }

        public void GetPixelBilinear(float x, float y, out byte r, out byte g, out byte b, out byte a)
        {
            r = g = b = 0;
            a = 0;

            int minX = (int)Math.Floor(x);
            int minY = (int)Math.Floor(y);
            int maxX = (int)Math.Ceiling(x);
            int maxY = (int)Math.Ceiling(y);

            byte tlr = 0, tlg = 0, tlb = 0, tla = 0;
            byte trr = 0, trg = 0, trb = 0, tra = 0;
            byte blr = 0, blg = 0, blb = 0, bla = 0;
            byte brr = 0, brg = 0, brb = 0, bra = 0;

            byte deltaX = (byte)Math.Abs(x - minX);
            byte deltaY = (byte)Math.Abs(y - minY);

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

            byte ftopR = Utils.Lerp(tlr, trr, deltaX);
            byte ftopG = Utils.Lerp(tlg, trg, deltaX);
            byte ftopB = Utils.Lerp(tlb, trb, deltaX);
            byte ftopA = Utils.Lerp(tla, tra, deltaX);

            byte fbotR = Utils.Lerp(blr, brr, deltaX);
            byte fbotG = Utils.Lerp(blg, brg, deltaX);
            byte fbotB = Utils.Lerp(blb, brb, deltaX);
            byte fbotA = Utils.Lerp(bla, bra, deltaX);

            //lerp vertical

            byte fR = Utils.Lerp(ftopR, fbotR, deltaY);
            byte fG = Utils.Lerp(ftopG, fbotG, deltaY);
            byte fB = Utils.Lerp(ftopB, fbotB, deltaY);
            byte fA = Utils.Lerp(ftopA, fbotA, deltaY);

            if (fR < 0) fR = 0;
            if (fG < 0) fG = 0;
            if (fB < 0) fB = 0;
            if (fA < 0) fA = 0;

            r = fR;
            g = fG;
            b = fB;
            a = fA;
        }

        public void SetPixel(int x, int y, byte r, byte g, byte b, byte a)
        {
            int idx = (x + y * Width) * 4;
            if (x >= 0 && x < Width && y >= 0 && y < Height)
            {
                Image[idx + 2] = r;
                Image[idx + 1] = g;
                Image[idx] = b;
                Image[idx + 3] = a;
            }
        }

        public void DrawYLine(int x, byte r, byte g, byte b, byte a)
        {
            Parallel.For(0, Height, y =>
            {
                SetPixel(x, y, r, g, b, a);
            });
        }

        public void DrawXLine(int y, byte r, byte g, byte b, byte a)
        {
            Parallel.For(0, Width, x =>
            {
                SetPixel(x, y, r, g, b, a);
            });
        }

        public void DrawLine(int x1, int y1, int x2,  int y2, int thickness, byte r, byte g, byte b, byte a)
        {
            DrawLine(x1, y1, x2, y2, r, g, b, a);

            if((float)(y2 - y1) / (float)(x2 - x1) < 1)
            {
                for(int i = 1; i < thickness; i++)
                {
                    DrawLine(x1, y1 - i, x2, y2 - i, r, g, b, a);
                    DrawLine(x1, y1 + i, x2, y2 + 1, r, g, b, a);
                }
            }
            else
            {
                for(int i = 1; i < thickness; i++)
                {
                    DrawLine(x1 - i, y1, x2 - i, y2, r, g, b, a);
                    DrawLine(x1 + i, y1, x2 + i, y2, r, g, b, a);
                }
            }
        }

        //Bresenham
        public void DrawLine(int x1, int y1, int x2, int y2, byte r, byte g, byte b, byte a)
        {
            int x, y;
            int dx, dy;
            int incx, incy;
            int balance;

            if(x2 >= x1)
            {
                dx = x2 - x1;
                incx = 1;
            }
            else
            {
                dx = x1 - x2;
                incx = -1;
            }

            if(y2 >= y1)
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

            if(dx >= dy)
            {
                dy <<= 1;
                balance = dy - dx;
                dx <<= 1;

                while(x != x2)
                {
                    SetPixel(x, y, r, g, b, a);
                    if(balance >= 0)
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

                while(y != y2)
                {
                    SetPixel(x, y, r, g, b, a);
                    if(balance >= 0)
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

        public void FillCircle(int r, byte red, byte green, byte blue, byte alpha)
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
            BitmapSource source = BitmapSource.Create(Width, Height, 72, 72, PixelFormats.Bgra32, null, Image, Width * 4);
            return source;
        }
    }
}
