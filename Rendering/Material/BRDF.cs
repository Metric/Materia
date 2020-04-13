using Materia.Rendering.Imaging;
using Materia.Rendering.Interfaces;
using Materia.Rendering.Textures;
using Microsoft.Extensions.FileProviders;
using System.Drawing;
using System.IO;
using System;

namespace Materia.Rendering.Material
{
    public class BRDF
    {
        public static GLTexture2D Lut { get; protected set; }

        public static void Create()
        {
            if(Lut != null || Lut.Id != 0)
            {
                return;
            }

            Lut = new GLTexture2D(PixelInternalFormat.Rgba8);

            try
            {
                EmbeddedFileProvider provider = new EmbeddedFileProvider(typeof(BRDF).Assembly);

                using (Bitmap bmp = (Bitmap)Bitmap.FromStream(provider.GetFileInfo(Path.Combine("Embedded", "brdf.png")).CreateReadStream()))
                {
                    RawBitmap fbmp = RawBitmap.FromBitmap(bmp);

                    Lut.Bind();
                    Lut.SetData(fbmp.Image, PixelFormat.Rgba, fbmp.Width, fbmp.Height);
                    Lut.SetFilter((int)TextureMinFilter.Linear, (int)TextureMagFilter.Linear);
                    Lut.SetWrap((int)TextureWrapMode.Repeat);
                    GLTexture2D.Unbind();
                }
            }
            catch (Exception e)
            {

            }
        }

        public static void Dispose()
        {
            if (Lut != null)
            {
                Lut.Dispose();
                Lut = null;
            }
        }
    }
}
