using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL;

namespace Materia.Textures
{
    public class GLTextureCube : GLTexture
    {
        public int Id { get; protected set; }

        PixelInternalFormat iformat;

        public GLTextureCube(PixelInternalFormat format)
        {
            iformat = format;
            Id = GL.GenTexture();
        }

        public void Bind()
        {
            GL.BindTexture(TextureTarget.TextureCubeMap, Id);
        }

        public static void Unbind()
        {
            GL.BindTexture(TextureTarget.TextureCubeMap, 0);
        }

        public void GenerateMipMaps()
        {
            GL.GenerateMipmap(GenerateMipmapTarget.TextureCubeMap);
        }

        public void Release()
        {
            if(Id != 0)
            {
                GL.DeleteTexture(Id);
                Id = 0;
            }
        }

        public void SetData(int cubeIndex, IntPtr data, PixelFormat format, int width, int height)
        {
            GL.TexImage2D(TextureTarget.TextureCubeMapPositiveX + cubeIndex, 0, iformat, width, height, 0, format, PixelType.UnsignedByte, data);
        }

        public void SetData(int cubeIndex, byte[] data, PixelFormat format, int width, int height)
        {
            GL.TexImage2D(TextureTarget.TextureCubeMapPositiveX + cubeIndex, 0, iformat, width, height, 0, format, PixelType.UnsignedByte, data);
        }

        public void SetData(int cubeIndex, float[] data, PixelFormat format, int width, int height)
        {
            GL.TexImage2D(TextureTarget.TextureCubeMapPositiveX + cubeIndex, 0, iformat, width, height, 0, format, PixelType.Float, data);
        }

        public void SetFilter(int min, int mag)
        {
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMinFilter, min);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMagFilter, mag);
        }

        public void SetWrap(int wrap)
        {
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapS, wrap);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapT, wrap);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapR, wrap);
        }
    }
}
