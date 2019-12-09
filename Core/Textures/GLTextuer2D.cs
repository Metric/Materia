using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Materia.GLInterfaces;

namespace Materia.Textures
{
    public class GLTextuer2D : GLTexture
    {
        public int Id { get; protected set; }

        public int Width { get; protected set; }
        public int Height { get; protected set; }

        public bool IsRGBBased { get; protected set; }

        public PixelInternalFormat InternalFormat
        {
            get
            {
                return iformat;
            }
        }

        PixelInternalFormat iformat;

        public GLTextuer2D(PixelInternalFormat format)
        {
            iformat = format;
            
            if (iformat == PixelInternalFormat.Rgb8)
            {
                IsRGBBased = true;
                iformat = PixelInternalFormat.Rgba8;
            }
            else if(iformat == PixelInternalFormat.Rgb16f)
            {
                IsRGBBased = true;
                iformat = PixelInternalFormat.Rgba16f;
            }
            else if(iformat == PixelInternalFormat.Rgb32f)
            {
                IsRGBBased = true;
                iformat = PixelInternalFormat.Rgba32f;
            }
            else
            {
                IsRGBBased = false;
            }

            Id = IGL.Primary.GenTexture();
        }

        /// <summary>
        /// Use this before using anything else in this class
        /// </summary>
        public void Bind()
        {
            IGL.Primary.BindTexture((int)TextureTarget.Texture2D, Id);
        }

        public void BindAsImage(int unit, bool read, bool write)
        {
            SizedInternalFormat format = SizedInternalFormat.Rgba32f;

            if (InternalFormat == PixelInternalFormat.Rgba16f || InternalFormat == PixelInternalFormat.Rgb16f)
            {
                format = SizedInternalFormat.Rgba16f;
            }
            else if(InternalFormat == PixelInternalFormat.Rgb || InternalFormat == PixelInternalFormat.Rgba 
                || InternalFormat == PixelInternalFormat.Rgb8 || InternalFormat == PixelInternalFormat.Rgba8)
            { 
                format = SizedInternalFormat.Rgba8;
            }
            else if(InternalFormat == PixelInternalFormat.R32f)
            {
                format = SizedInternalFormat.R32f;
            }
            else if(InternalFormat == PixelInternalFormat.R16f)
            {
                format = SizedInternalFormat.R16f;
            }

            TextureAccess access = TextureAccess.ReadWrite;

            if (read && !write)
            {
                access = TextureAccess.ReadOnly;
            }
            else if(write && !read)
            {
                access = TextureAccess.WriteOnly;
            }

            IGL.Primary.BindImageTexture(unit, Id, 0, false, 0, (int)access, (int)format);
        }

        public static void UnbindAsImage(int unit)
        {
            SizedInternalFormat format = SizedInternalFormat.Rgba32f;
            TextureAccess access = TextureAccess.ReadWrite;

            IGL.Primary.BindImageTexture(unit, 0, 0, false, 0, (int)access, (int)format);
        }

        public static void Unbind()
        {
            IGL.Primary.BindTexture((int)TextureTarget.Texture2D, 0);
        }

        /// <summary>
        /// Only use after SetData
        /// </summary>
        public void GenerateMipMaps()
        {
            IGL.Primary.GenerateMipmap((int)GenerateMipmapTarget.Texture2D);
        }

        public void Release()
        {
            if (Id != 0)
            {
                IGL.Primary.DeleteTexture(Id);
                Id = 0;
            }
        }

        public void Store(int width, int height)
        {
            SizedInternalFormat format = SizedInternalFormat.Rgba32f;

            if (InternalFormat == PixelInternalFormat.Rgba16f || InternalFormat == PixelInternalFormat.Rgb16f)
            {
                format = SizedInternalFormat.Rgba16f;
            }
            else if (InternalFormat == PixelInternalFormat.Rgb || InternalFormat == PixelInternalFormat.Rgba
                || InternalFormat == PixelInternalFormat.Rgb8 || InternalFormat == PixelInternalFormat.Rgba8)
            {
                format = SizedInternalFormat.Rgba8;
            }
            else if (InternalFormat == PixelInternalFormat.R32f)
            {
                format = SizedInternalFormat.R32f;
            }
            else if (InternalFormat == PixelInternalFormat.R16f)
            {
                format = SizedInternalFormat.R16f;
            }

            IGL.Primary.TexStorage2D((int)TextureTarget2d.Texture2D, 0, (int)format, width, height);
        }

        public void SetAsDepth(int width, int height)
        {
            Width = width;
            Height = height;
            IGL.Primary.TexImage2D((int)TextureTarget.Texture2D, 0, (int)PixelInternalFormat.DepthComponent24, width, height, 0, (int)PixelFormat.DepthComponent, (int)PixelType.Float, IntPtr.Zero);
        }

        public void CopyFromFrameBuffer(int width, int height)
        {
            Width = width;
            Height = height;
            int format = (int)iformat;
            IGL.Primary.CopyTexImage2D((int)TextureTarget.Texture2D, 0, format, 0, 0, width, height, 0);
        }

        public void SetData(IntPtr data, PixelFormat format, int width, int height, int mipLevel = 0)
        {
            Width = width;
            Height = height;
            IGL.Primary.TexImage2D((int)TextureTarget.Texture2D, mipLevel, (int)iformat, width, height, 0, (int)format, (int)PixelType.UnsignedByte, data);
        }

        public void SetData(byte[] data, PixelFormat format, int width, int height, int mipLevel = 0)
        {
            Width = width;
            Height = height;
            IGL.Primary.TexImage2D((int)TextureTarget.Texture2D, mipLevel, (int)iformat, width, height, 0, (int)format, (int)PixelType.UnsignedByte, data);
        }

        public void SetData(float[] data, PixelFormat format, int width, int height, int mipLevel = 0)
        {
            Width = width;
            Height = height;
            IGL.Primary.TexImage2D((int)TextureTarget.Texture2D, mipLevel, (int)iformat, width, height, 0, (int)format, (int)PixelType.Float, data);
        }

        public void SetDataAsFloat(byte[] data, PixelFormat format, int width, int height, int mipLevel = 0)
        {
            Width = width;
            Height = height;
            IGL.Primary.TexImage2D((int)TextureTarget.Texture2D, mipLevel, (int)iformat, width, height, 0, (int)format, (int)PixelType.Float, data);
        }

        public void SetSwizzleRGB()
        {
            IGL.Primary.TexParameterI((int)TextureTarget.Texture2D, (int)TextureParameterName.TextureSwizzleRgba, new int[] { (int)All.Red, (int)All.Green, (int)All.Blue, (int)All.One });
        }

        public void SetSwizzleLuminance()
        {
            IGL.Primary.TexParameterI((int)TextureTarget.Texture2D, (int)TextureParameterName.TextureSwizzleRgba, new int[] { (int)All.Red, (int)All.Red, (int)All.Red, (int)All.One });
        }

        public void SetMaxMipLevel(int max)
        {
            IGL.Primary.TexParameter((int)TextureTarget.Texture2D, (int)TextureParameterName.TextureBaseLevel, 0);
            IGL.Primary.TexParameter((int)TextureTarget.Texture2D, (int)TextureParameterName.TextureMaxLevel, max);
        }

        public void SetFilter(int min, int mag)
        {
            IGL.Primary.TexParameter((int)TextureTarget.Texture2D, (int)TextureParameterName.TextureMinFilter, min);
            IGL.Primary.TexParameter((int)TextureTarget.Texture2D, (int)TextureParameterName.TextureMagFilter, mag);
        }

        public void Nearest()
        {
            SetFilter((int)TextureMinFilter.Nearest, (int)TextureMagFilter.Nearest);
        }

        public void Linear()
        {
            SetFilter((int)TextureMinFilter.Linear, (int)TextureMagFilter.Linear);
        }

        public void ClampToEdge()
        {
            SetWrap((int)TextureWrapMode.ClampToEdge);
        }
        
        public void Repeat()
        {
            SetWrap((int)TextureWrapMode.Repeat);
        }

        public void SetWrap(int wrap)
        {
            IGL.Primary.TexParameter((int)TextureTarget.Texture2D, (int)TextureParameterName.TextureWrapS, wrap);
            IGL.Primary.TexParameter((int)TextureTarget.Texture2D, (int)TextureParameterName.TextureWrapT, wrap);
        }
    }
}
