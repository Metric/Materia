#region Usings

using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

#endregion

namespace DDSReader
{
    public static class DDSMipMapExtensions
    {
        public static Task<BitmapSource> ToBitmapSource(this DDSMipMap mipmap, int mipmapIndex = 0, Dispatcher targetDispatcher = null)
        {
            Debug.Assert(mipmap != null);
            Debug.Assert(mipmapIndex >= 0 && mipmapIndex < mipmap.MipmapData.Count);

            var width = (int) mipmap.Width;
            var height = (int) mipmap.Height;


            if (mipmapIndex > 0)
            {
                width = width / (width * 2);
                height = height / (height * 2);
            }

            var frameData = mipmap.MipmapData[mipmapIndex];

            if (targetDispatcher == null)
            {
                var writable = new WriteableBitmap(width, height, 96, 96, PixelFormats.Bgra32, null);
                writable.WritePixels(new Int32Rect(0, 0, width, height), frameData, 4 * width, 0);

                return Task.FromResult((BitmapSource)writable);
            }

            return targetDispatcher.InvokeAsync(() =>
            {
                var writable = new WriteableBitmap(width, height, 96, 96, PixelFormats.Bgra32, null);
                writable.WritePixels(new Int32Rect(0, 0, width, height), frameData, 4 * width, 0);

                return (BitmapSource)writable;
            }).Task;
        }
    }
}
