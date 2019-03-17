using System;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using D = System.Drawing;

namespace Materia.UI.Components
{
    /// <summary>
    /// Interaction logic for Magnifier.xaml
    /// </summary>
    public partial class Magnifier : Window
    {
        public Magnifier()
        {
            InitializeComponent();
        }

        public void Update(D.Bitmap bmp, double x, double y, D.Rectangle area, double scale)
        {
            Preview.Source = GetSource(bmp);

            bool wasGreaterThanX = false;
            bool wasGreaterThanY = false;

            wasGreaterThanX = x >= area.X / scale;
            wasGreaterThanY = y >= area.Y / scale;

            double xoffset = 0;
            double yoffset = 0;

            if ((x - 64 - 16) * scale >= area.X)
            {
                xoffset -= 64 + 16;
            }
            else if (((x - 64 - 16) * scale < area.X && wasGreaterThanX && wasGreaterThanY) || x <= 0)
            {
                xoffset += 16;
            }
            else
            {
                xoffset -= 64 + 16;
            }

            if ((y - 64 - 16) * scale >= area.Y)
            {
                yoffset -= 64 + 15;
            }
            else if(((y - 64 - 16) * scale < area.Y && wasGreaterThanY && wasGreaterThanX) || y <= 0)
            {
                yoffset += 16;
            }
            else
            {
                yoffset -= 64 + 15;
            }

            Left = x / scale + xoffset;
            Top = y / scale + yoffset;
        }

        public static BitmapSource GetSource(System.Drawing.Bitmap source)
        {
            var rect = new System.Drawing.Rectangle(0, 0, source.Width, source.Height);

            var bitmapData = source.LockBits(
                rect,
                ImageLockMode.ReadWrite,
                System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            try
            {
                var size = (rect.Width * rect.Height) * 4;

                return BitmapSource.Create(
                    source.Width,
                    source.Height,
                    source.HorizontalResolution,
                    source.VerticalResolution,
                    PixelFormats.Bgra32,
                    null,
                    bitmapData.Scan0,
                    size,
                    bitmapData.Stride);
            }
            finally
            {
                source.UnlockBits(bitmapData);
            }
        }
    }
}
