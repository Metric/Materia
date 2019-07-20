using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using Materia.Imaging;

namespace Materia.Exporters
{
    public static class RawBitmapExtensions
    {
        public static Bitmap ToBitmap(this RawBitmap b)
        {
            Bitmap map = null;
            unsafe
            {
                fixed (byte* r_ptr = &b.Image[0])
                {
                    IntPtr ptr = new IntPtr(r_ptr);
                    map = new Bitmap(b.Width, b.Height, 4 * b.Width, System.Drawing.Imaging.PixelFormat.Format32bppArgb, ptr);
                }
            }
            return map;
        }
    }
}
