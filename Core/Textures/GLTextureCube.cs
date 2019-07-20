using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Materia.GLInterfaces;

namespace Materia.Textures
{
    public class GLTextureCube : GLTexture
    {
        public int Id { get; protected set; }

        PixelInternalFormat iformat;

        public GLTextureCube(PixelInternalFormat format)
        {
            iformat = format;
            Id = IGL.Primary.GenTexture();
        }

        public void Bind()
        {
            IGL.Primary.BindTexture((int)TextureTarget.TextureCubeMap, Id);
        }

        public static void Unbind()
        {
            IGL.Primary.BindTexture((int)TextureTarget.TextureCubeMap, 0);
        }

        public void GenerateMipMaps()
        {
            IGL.Primary.GenerateMipmap((int)GenerateMipmapTarget.TextureCubeMap);
        }

        public void Release()
        {
            if(Id != 0)
            {
                IGL.Primary.DeleteTexture(Id);
                Id = 0;
            }
        }

        public void SetData(int cubeIndex, IntPtr data, PixelFormat format, int width, int height)
        {
            IGL.Primary.TexImage2D((int)TextureTarget.TextureCubeMapPositiveX + cubeIndex, 0, (int)iformat, width, height, 0, (int)format, (int)PixelType.UnsignedByte, data);
        }

        public void SetData(int cubeIndex, byte[] data, PixelFormat format, int width, int height)
        {
            IGL.Primary.TexImage2D((int)TextureTarget.TextureCubeMapPositiveX + cubeIndex, 0, (int)iformat, width, height, 0, (int)format, (int)PixelType.UnsignedByte, data);
        }

        public void SetData(int cubeIndex, float[] data, PixelFormat format, int width, int height)
        {
            IGL.Primary.TexImage2D((int)TextureTarget.TextureCubeMapPositiveX + cubeIndex, 0, (int)iformat, width, height, 0, (int)format, (int)PixelType.Float, data);
        }

        public void SetFilter(int min, int mag)
        {
            IGL.Primary.TexParameter((int)TextureTarget.TextureCubeMap, (int)TextureParameterName.TextureMinFilter, min);
            IGL.Primary.TexParameter((int)TextureTarget.TextureCubeMap, (int)TextureParameterName.TextureMagFilter, mag);
        }

        public void SetWrap(int wrap)
        {
            IGL.Primary.TexParameter((int)TextureTarget.TextureCubeMap, (int)TextureParameterName.TextureWrapS, wrap);
            IGL.Primary.TexParameter((int)TextureTarget.TextureCubeMap, (int)TextureParameterName.TextureWrapT, wrap);
            IGL.Primary.TexParameter((int)TextureTarget.TextureCubeMap, (int)TextureParameterName.TextureWrapR, wrap);
        }
    }
}
