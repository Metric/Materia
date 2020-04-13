using System.Drawing;

namespace Materia.Rendering.Imaging
{
    public class FloatBitmap : GLBitmap
    {
        public float[] Image { get; set; }

        public FloatBitmap(int w, int h)
        {
            BPP = 32;
            Width = w;
            Height = h;
            Image = new float[w * h * 4];
        }

        public FloatBitmap(int w, int h, float[] data, int bpp = 32)
        {
            BPP = bpp;
            Width = w;
            Height = h;
            Image = data;
        }

        public override GLPixel GetPixel(int x, int y)
        {
            float r = 0, g = 0, b = 0, a = 0;
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

            byte br = (byte)(r * 255);
            byte bg = (byte)(g * 255);
            byte bb = (byte)(b * 255);
            byte ba = (byte)(a * 255);

            return new GLPixel() { r = br, g = bg, b = bb, a = ba, fr = r, fg = g, fb = b, fa = a };
        }

        public override void SetPixel(int x, int y, ref GLPixel pixel)
        {
            int idx = (x + y * Width) * (BPP / 8);
            if (x >= 0 && x < Width && y >= 0 && y < Height)
            {
                if (BPP == 8)
                {
                    Image[idx] = pixel.fr;
                }
                else if (BPP == 16)
                {
                    Image[idx] = pixel.fr;
                    Image[idx + 1] = pixel.fg;
                }
                else if (BPP >= 24)
                {
                    Image[idx + 2] = pixel.fr;
                    Image[idx + 1] = pixel.fg;
                    Image[idx] = pixel.fb;
                }

                if (BPP == 32)
                    Image[idx + 3] = pixel.fa;
            }
        }
    }
}
