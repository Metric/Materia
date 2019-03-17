using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL;

namespace Materia.Textures
{
    public class GLTextuer2D : GLTexture
    {
        public int Id { get; protected set; }

        public int Width { get; protected set; }
        public int Height { get; protected set; }

        PixelInternalFormat iformat;

        public GLTextuer2D(PixelInternalFormat format)
        {
            iformat = format;
            Id = GL.GenTexture();
        }

        /// <summary>
        /// Use this before using anything else in this class
        /// </summary>
        public void Bind()
        {
            GL.BindTexture(TextureTarget.Texture2D, Id);
        }

        public static void Unbind()
        {
            GL.BindTexture(TextureTarget.Texture2D, 0);
        }

        /// <summary>
        /// Only use after SetData
        /// </summary>
        public void GenerateMipMaps()
        {
            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
        }

        public void Release()
        {
            if (Id != 0)
            {
                GL.DeleteTexture(Id);
                Id = 0;
            }
        }

        public void SetAsDepth(int width, int height)
        {
            Width = width;
            Height = height;
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.DepthComponent24, width, height, 0, PixelFormat.DepthComponent, PixelType.Float, IntPtr.Zero);
        }

        public void CopyFromFrameBuffer(int width, int height)
        {
            Width = width;
            Height = height;
            GL.CopyTexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Rgba8, 0, 0, width, height, 0);
        }

        public void SetData(IntPtr data, PixelFormat format, int width, int height, int mipLevel = 0)
        {
            Width = width;
            Height = height;
            GL.TexImage2D(TextureTarget.Texture2D, mipLevel, iformat, width, height, 0, format, PixelType.UnsignedByte, data);
        }

        public void SetData(byte[] data, PixelFormat format, int width, int height, int mipLevel = 0)
        {
            Width = width;
            Height = height;
            GL.TexImage2D(TextureTarget.Texture2D, mipLevel, iformat, width, height, 0, format, PixelType.UnsignedByte, data);
        }

        public void SetData(float[] data, PixelFormat format, int width, int height, int mipLevel = 0)
        {
            Width = width;
            Height = height;
            GL.TexImage2D(TextureTarget.Texture2D, mipLevel, iformat, width, height, 0, format, PixelType.Float, data);
        }

        public void SetDataAsFloat(byte[] data, PixelFormat format, int width, int height, int mipLevel = 0)
        {
            Width = width;
            Height = height;
            GL.TexImage2D(TextureTarget.Texture2D, mipLevel, iformat, width, height, 0, format, PixelType.Float, data);
        }

        public void SetSwizzleLuminance()
        {
            GL.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureSwizzleRgba, new int[] { (int)All.Red, (int)All.Red, (int)All.Red, (int)All.One });
        }

        public void SetMaxMipLevel(int max)
        {
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureBaseLevel, 0);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMaxLevel, max);
        }

        public void SetFilter(int min, int mag)
        {
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, min);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, mag);
        }

        public void SetWrap(int wrap)
        {
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, wrap);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, wrap);
        }
    }
}
