using Materia.Rendering.Mathematics;
using System;
using System.Threading.Tasks;

namespace Materia.Rendering.Imaging
{
    public abstract class GLBitmap
    {
        public int Width { get; protected set; }
        public int Height { get; protected set; }

        public int BPP { get; protected set; }

        protected static GLPixel BLANK = new GLPixel() { r = 0, g = 0, b = 0, a = 0, fr = 0, fg = 0, fb = 0, fa = 0 };

        public abstract void GetPixel(int x, int y, ref GLPixel pixel);

        public abstract GLPixel GetPixel(int x, int y);
        /*{
            r = g = b = 0;
            a = 0;
            int idx = (x + y * Width) * (BPP / 8);

            if (x >= 0 && x < Width && y >= 0 && y < Height)
            {
                if (BPP == 8)
                {
                    r = g = b = Image[idx];
                }
                else if (BPP >= 16)
                {
                    r = Image[idx];
                    g = Image[idx + 1];
                }
                else if (BPP >= 24)
                {
                    r = Image[idx + 2];
                    g = Image[idx + 1];
                    b = Image[idx];
                }

                if (BPP == 32)
                    a = Image[idx + 3];
                else
                    a = 1;
            }
        }*/

        public virtual void Premult()
        {
            Parallel.For(0, Height, y =>
            {
                GLPixel rgba = new GLPixel();
                for (int x = 0; x < Width; ++x)
                {
                    GetPixel(x, y, ref rgba);
                    GLPixel.Premult(ref rgba);
                    SetPixel(x, y, ref rgba);
                }
            });
        }

        public void CopyRedToGreen(GLBitmap src)
        {
            Parallel.For(0, Height, y =>
            {
                for (int x = 0; x < Width; ++x)
                {
                    GLPixel spx = src.GetPixel(x, y);
                    GLPixel dpx = GetPixel(x, y);

                    dpx.fg = spx.fr;
                    dpx.g = spx.r;

                    SetPixel(x, y, ref dpx);

                    /*int idxsrc = ((x % src.Width) + (y % src.Height) * src.Width) * 4;
                    int idx = (x + y * Width) * 4;

                    if (BPP >= 16 && src.BPP >= 24)
                    {
                        Image[idx + 1] = src.Image[idxsrc + 2];
                    }
                    else if (BPP >= 16 && src.BPP >= 8)
                    {
                        Image[idx + 1] = src.Image[idxsrc];
                    }
                    else if (BPP >= 8 && src.BPP >= 24)
                    {
                        Image[idx] = src.Image[idxsrc + 2];
                    }
                    else if (BPP >= 8 && src.BPP >= 8)
                    {
                        Image[idx] = src.Image[idxsrc];
                    }*/
                }
            });
        }

        public void CopyRedToBlue(GLBitmap src)
        {
            Parallel.For(0, Height, y =>
            {
                for (int x = 0; x < Width; ++x)
                {
                    GLPixel spx = src.GetPixel(x, y);
                    GLPixel dpx = GetPixel(x, y);

                    dpx.fb = spx.fr;
                    dpx.b = spx.r;

                    SetPixel(x, y, ref dpx);

                    /*int idxsrc = ((x % src.Width) + (y % src.Height) * src.Width) * (src.BPP / 8);
                    int idx = (x + y * Width) * (BPP / 8);


                    if (BPP >= 24 && src.BPP >= 24)
                    {
                        Image[idx] = src.Image[idxsrc + 2];
                    }
                    else if (BPP >= 24 && src.BPP >= 8)
                    {
                        Image[idx] = src.Image[idxsrc];
                    }
                    else if (BPP >= 8 && src.BPP >= 24)
                    {
                        Image[idx] = src.Image[idxsrc + 2];
                    }
                    else if (BPP >= 8 && src.BPP >= 8)
                    {
                        Image[idx] = src.Image[idxsrc];
                    }*/
                }
            });
        }

        public void CopyRedToRed(GLBitmap src)
        {
            Parallel.For(0, Height, y => {
                for (int x = 0; x < Width; ++x)
                {
                    GLPixel spx = src.GetPixel(x, y);
                    GLPixel dpx = GetPixel(x, y);

                    dpx.fr = spx.fr;
                    dpx.r = spx.r;

                    SetPixel(x, y, ref dpx);

                    /*int idxsrc = ((x % src.Width) + (y % src.Height) * src.Width) * (src.BPP / 8);
                    int idx = (x + y * Width) * (BPP / 8);

                    if (BPP >= 24 && src.BPP >= 24)
                    {
                        Image[idx + 2] = src.Image[idxsrc + 2];
                    }
                    else if (BPP >= 8 && src.BPP >= 24)
                    {
                        Image[idx] = src.Image[idxsrc + 2];
                    }
                    else if (BPP >= 24 && src.BPP >= 8)
                    {
                        Image[idx + 2] = src.Image[idxsrc];
                    }
                    else if (BPP >= 8 && src.BPP >= 8)
                    {
                        Image[idx] = src.Image[idxsrc];
                    }*/
                }
            });
        }

        public void CopyRedToAlpha(GLBitmap src)
        {
            if (BPP < 32) return;

            Parallel.For(0, Height, y =>
            {
                for (int x = 0; x < Width; ++x)
                {
                    GLPixel spx = src.GetPixel(x, y);
                    GLPixel dpx = GetPixel(x, y);

                    dpx.fa = spx.fr;
                    dpx.a = spx.r;

                    SetPixel(x, y, ref dpx);

                    /*int idxsrc = ((x % src.Width) + (y % src.Height) * src.Width) * (src.BPP / 8);
                    int idx = (x + y * Width) * (BPP / 8);

                    if (src.BPP >= 24)
                    {
                        Image[idx + 3] = src.Image[idxsrc + 2];
                    }
                    else if (src.BPP >= 8)
                    {
                        Image[idx + 3] = src.Image[idxsrc];
                    }*/
                }
            });
        }

        public GLPixel GetPixelBilinear(float x, float y)
        {
            int minX = (int)Math.Floor(x);
            int minY = (int)Math.Floor(y);
            int maxX = (int)Math.Ceiling(x);
            int maxY = (int)Math.Ceiling(y);

            float deltaX = Math.Abs(x - minX);
            float deltaY = Math.Abs(y - minY);

            //apply bilinear interp
            minX = Math.Abs(minX);
            maxX = Math.Abs(maxX);
            minY = Math.Abs(minY);
            maxY = Math.Abs(maxY);

            GLPixel tl = GetPixel(minX % Width, minY % Height);
            GLPixel tr = GetPixel(maxX % Width, minY % Height);
            GLPixel bl = GetPixel(minX % Width, maxY % Height);
            GLPixel br = GetPixel(maxX % Width, maxY % Height);

            //lerp horizontal first
            GLPixel ftop = GLPixel.Lerp(ref tl, ref tr, deltaX);
            GLPixel fbot = GLPixel.Lerp(ref bl, ref br, deltaX);

            return GLPixel.Lerp(ref ftop, ref fbot, deltaY);
        }

        public abstract void SetPixel(int x, int y, ref GLPixel pixel);

        public abstract void SetPixel(int x, int y, ref Vector4 c);

        /*public void SetPixel(int x, int y, byte r, byte g, byte b, byte a)
        {
            int idx = (x + y * Width) * (BPP / 8);
            if (x >= 0 && x < Width && y >= 0 && y < Height)
            {
                if (BPP == 8)
                {
                    Image[idx] = r;
                }
                else if (BPP == 16)
                {
                    Image[idx] = r;
                    Image[idx + 1] = g;
                }
                else if (BPP >= 24)
                {
                    Image[idx + 2] = r;
                    Image[idx + 1] = g;
                    Image[idx] = b;
                }

                if (BPP == 32)
                    Image[idx + 3] = a;
            }
        }*/

        public void DrawYLine(int x, GLPixel pixel)
        {
            Parallel.For(0, Height, y =>
            {
                SetPixel(x, y, ref pixel);
            });
        }

        public void DrawXLine(int y, GLPixel pixel)
        {
            Parallel.For(0, Width, x =>
            {
                SetPixel(x, y, ref pixel);
            });
        }

        public void DrawLine(int x1, int y1, int x2, int y2, int thickness, ref GLPixel pixel)
        {
            DrawLine(x1, y1, x2, y2, ref pixel);

            if ((float)(y2 - y1) / (float)(x2 - x1) < 1)
            {
                for (int i = 1; i < thickness; ++i)
                {
                    DrawLine(x1, y1 - i, x2, y2 - i, ref pixel);
                    DrawLine(x1, y1 + i, x2, y2 + i, ref pixel);
                }
            }
            else
            {
                for (int i = 1; i < thickness; ++i)
                {
                    DrawLine(x1 - i, y1, x2 - i, y2, ref pixel);
                    DrawLine(x1 + i, y1, x2 + i, y2, ref pixel);
                }
            }
        }

        //Bresenham
        public void DrawLine(int x1, int y1, int x2, int y2, ref GLPixel pixel)
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
                    SetPixel(x, y, ref pixel);
                    if (balance >= 0)
                    {
                        y += incy;
                        balance -= dx;
                    }
                    balance += dy;
                    x += incx;
                }
                SetPixel(x, y, ref pixel);
            }
            else
            {
                dx <<= 1;
                balance = dx - dy;
                dy <<= 1;

                while (y != y2)
                {
                    SetPixel(x, y, ref pixel);
                    if (balance >= 0)
                    {
                        x += incx;
                        balance -= dy;
                    }
                    balance += dx;
                    y += incy;
                }
                SetPixel(x, y, ref pixel);
            }
        }

        public void FillCircle(int radius, int xOffset, int yOffset, GLPixel pixel)
        {
            int wh = Width / 2;
            int hh = Height / 2;

            Parallel.For(-radius, radius, y =>
            {
                for (int x = -radius; x < radius; ++x)
                {
                    if (x * x + y * y <= radius * radius)
                    {
                        SetPixel(Math.Abs(wh + x + xOffset) % Width, Math.Abs(hh + y + yOffset) % Height, ref pixel);
                    }
                }
            });
        }

        public void Clear()
        {
            Clear(BLANK);
        }

        public void Clear(GLPixel pixel)
        {
            Parallel.For(0, Height, y =>
            {
                for (int x = 0; x < Width; ++x)
                {
                    SetPixel(x, y, ref pixel);
                }
            });
        }

        public GLPixel AverageColor()
        {
            float fr = 0;
            float fg = 0;
            float fb = 0;
            float fa = 0;

            GLPixel pixel = new GLPixel();

            int total = 0;

            for (int y = 0; y < Height; ++y)
            {
                for (int x = 0; x < Width; ++x)
                {
                    GetPixel(x, y, ref pixel);

                    fr += pixel.fr;
                    fg += pixel.fg;
                    fb += pixel.fb;
                    fa += pixel.fa;
                    ++total;
                }
            }

            fr /= total;
            fg /= total;
            fb /= total;
            fa /= total;

            byte r = (byte)(fr * 255);
            byte g = (byte)(fg * 255);
            byte b = (byte)(fb * 255);
            byte a = (byte)(fa * 255);

            return new GLPixel()
            {
                r = r,
                g = g,
                b = b,
                a = a,
                fr = fr,
                fg = fg,
                fb = fb,
                fa = fa
            };
        }
    }
}
