using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Materia.Imaging;

namespace Materia.UI.Helpers
{
    public static class RawBitmapExtensions
    {
        public static BitmapSource ToImageSource(this RawBitmap b)
        {
            BitmapSource source = BitmapSource.Create(b.Width, b.Height, 72, 72, PixelFormats.Bgra32, null, b.Image, b.Width * 4);
            return source;
        }

        public static BitmapSource ToImageSource(this FloatBitmap b)
        {
            try
            {
                BitmapSource source = BitmapSource.Create(b.Width, b.Height, 72, 72, PixelFormats.Rgba128Float, null, b.Image, b.Width * 4 * 4);
                return source;
            }
            catch
            {
                return null;
            }
        }
    }
}
