using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Runtime.InteropServices;

namespace Materia.Rendering.Imaging
{
    public class RawBitmap : GLBitmap
    {
        public byte[] Image { get; set; }

        public RawBitmap(int w, int h, byte[] data, int bpp = 32)
        {
            Width = w;
            Height = h;
            Image = data;
            BPP = bpp;
        }

        public RawBitmap(int w, int h, int bpp = 32)
        {
            Width = w;
            Height = h;
            BPP = bpp;
            Image = new byte[w * h * (BPP / 8)];
        }

        public static RawBitmap FromBitmap(Bitmap bmp)
        {
            int bpp = System.Drawing.Bitmap.GetPixelFormatSize(bmp.PixelFormat);
            var data = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, bmp.PixelFormat);
            byte[] raw = new byte[data.Height * data.Stride];
            Marshal.Copy(data.Scan0, raw, 0, raw.Length);
            bmp.UnlockBits(data);
            return new RawBitmap(bmp.Width, bmp.Height, raw, bpp);
        }

        public override GLPixel GetPixel(int x, int y)
        {
            int r = 0, g = 0, b = 0, a = 0;
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

            float fr = r / 255.0f;
            float fg = g / 255.0f;
            float fb = b / 255.0f;
            float fa = a / 255.0f;

            return new GLPixel() { r = (byte)r, g = (byte)g, b = (byte)b, a = (byte)a, fr = fr, fg = fg, fb = fb, fa = fa };
        }

        public override void SetPixel(int x, int y, ref GLPixel pixel)
        {
            int idx = (x + y * Width) * (BPP / 8);
            if (x >= 0 && x < Width && y >= 0 && y < Height)
            {
                if (BPP == 8)
                {
                    Image[idx] = pixel.r;
                }
                else if (BPP == 16)
                {
                    Image[idx] = pixel.r;
                    Image[idx + 1] = pixel.g;
                }
                else if (BPP >= 24)
                {
                    Image[idx + 2] = pixel.r;
                    Image[idx + 1] = pixel.g;
                    Image[idx] = pixel.b;
                }

                if (BPP == 32)
                    Image[idx + 3] = pixel.a;
            }
        }
    }
}
