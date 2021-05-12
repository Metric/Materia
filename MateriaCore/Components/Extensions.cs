using Avalonia;
using Avalonia.Media.Imaging;
using Materia.Rendering.Imaging;
using System;

namespace MateriaCore.Components
{
    public static class Extensions
    {
        public static Bitmap ToAvBitmap(this RawBitmap raw)
        {
            Bitmap map = null;
            unsafe
            {
                fixed (byte* r_ptr = &raw.Image[0])
                {
                    IntPtr ptr = new IntPtr(r_ptr);
                    map = new Bitmap(Avalonia.Platform.PixelFormat.Bgra8888, Avalonia.Platform.AlphaFormat.Unpremul, ptr, PixelSize.FromSize(new Size(raw.Width, raw.Height), 1.0), new Vector(72, 72), raw.Width * 4);
                }
            }
            return map;
        }
    }
}
