using System;
using Materia.Rendering.Interfaces;
using Materia.Rendering.Textures;

namespace Materia.Rendering.Buffers
{
    public class GLFrameBuffer : IDisposable
    {
        int fbo;

        public int Id
        {
            get
            {
                return fbo;
            }
        }

        public GLFrameBuffer()
        {
            fbo = IGL.Primary.GenFramebuffer();
        }

        public void Bind()
        {
            IGL.Primary.BindFramebuffer((int)FramebufferTarget.Framebuffer, fbo);
        }

        public void BindRead()
        {
            IGL.Primary.BindFramebuffer((int)FramebufferTarget.ReadFramebuffer, fbo);
        }

        public void Unbind()
        {
            IGL.Primary.BindFramebuffer((int)FramebufferTarget.Framebuffer, 0);
        }

        public void UnbindRead()
        {
            IGL.Primary.BindFramebuffer((int)FramebufferTarget.ReadFramebuffer, 0);
        }

        public void AttachDepth(GLTexture2D tex)
        {
            IGL.Primary.FramebufferTexture2D((int)FramebufferTarget.Framebuffer, (int)FramebufferAttachment.DepthAttachment, (int)TextureTarget.Texture2D, tex.Id, 0);
        }

        public void AttachColor(GLTexture2D tex, int index = 0)
        {
            IGL.Primary.FramebufferTexture2D((int)FramebufferTarget.Framebuffer, (int)FramebufferAttachment.ColorAttachment0 + index, (int)TextureTarget.Texture2D, tex.Id, 0);
        }

        public void AttachColor(GLTextureCube tex, int cubeFace, int index = 0, int mip = 0)
        {
            IGL.Primary.FramebufferTexture2D((int)FramebufferTarget.Framebuffer, (int)FramebufferAttachment.ColorAttachment0 + index, (int)TextureTarget.TextureCubeMapPositiveX + cubeFace, tex.Id, mip);
        }

        public void AttachColor(GLRenderBuffer buf, int index = 0)
        {
            IGL.Primary.FramebufferRenderbuffer((int)FramebufferTarget.Framebuffer, (int)FramebufferAttachment.ColorAttachment0 + index, (int)RenderbufferTarget.Renderbuffer, buf.Id);
        }

        public void AttachDepth(GLRenderBuffer buf)
        {
            IGL.Primary.FramebufferRenderbuffer((int)FramebufferTarget.Framebuffer, (int)FramebufferAttachment.DepthStencilAttachment, (int)RenderbufferTarget.Renderbuffer, buf.Id);
        }

        public bool IsValid
        {
            get
            {
                return IGL.Primary.CheckFramebufferStatus((int)FramebufferTarget.Framebuffer) == (int)FramebufferErrorCode.FramebufferComplete;
            }
        }

        /// <summary>
        /// bpp is bytes per pixel
        /// not bits per pixel
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="bpp"></param>
        /// <returns></returns>
        public float[] ReadFloatPixels(int width, int height, int bpp = 4)
        {
            float[] buf = new float[width * height * bpp];
            if (bpp == 4)
            {
                IGL.Primary.ReadPixels(0, 0, width, height, (int)PixelFormat.Bgra, (int)PixelType.Float, buf);
            }
            else if (bpp == 3)
            {
                IGL.Primary.ReadPixels(0, 0, width, height, (int)PixelFormat.Bgr, (int)PixelType.Float, buf);
            }
            else if (bpp == 2)
            {
                IGL.Primary.ReadPixels(0, 0, width, height, (int)PixelFormat.Rg, (int)PixelType.Float, buf);
            }
            else if (bpp == 1)
            {
                IGL.Primary.ReadPixels(0, 0, width, height, (int)PixelFormat.Red, (int)PixelType.Float, buf);
            }
            return buf;
        }

        public float[] ReadFloatPixels(int x, int y, int width, int height, int bpp = 4)
        {
            float[] buf = new float[width * height * bpp];
            if (bpp == 4)
            {
                IGL.Primary.ReadPixels(x, y, width, height, (int)PixelFormat.Bgra, (int)PixelType.Float, buf);
            }
            else if (bpp == 3)
            {
                IGL.Primary.ReadPixels(x, y, width, height, (int)PixelFormat.Bgr, (int)PixelType.Float, buf);
            }
            else if (bpp == 2)
            {
                IGL.Primary.ReadPixels(x, y, width, height, (int)PixelFormat.Rg, (int)PixelType.Float, buf);
            }
            else if (bpp == 1)
            {
                IGL.Primary.ReadPixels(x, y, width, height, (int)PixelFormat.Red, (int)PixelType.Float, buf);
            }
            return buf;
        }

        /// <summary>
        /// bpp is bytes per pixel
        /// not bits per pixel
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="bpp"></param>
        /// <returns></returns>
        public byte[] ReadBytePixels(int width, int height, int bpp = 4)
        {
            byte[] buf = new byte[width * height * bpp];

            if (bpp == 4)
            {
                IGL.Primary.ReadPixels(0, 0, width, height, (int)PixelFormat.Bgra, (int)PixelType.UnsignedByte, buf);
            }
            else if(bpp == 3)
            {
                IGL.Primary.ReadPixels(0, 0, width, height, (int)PixelFormat.Bgr, (int)PixelType.UnsignedByte, buf);
            }

            return buf;
        }

        public byte[] ReadBytePixels(int x, int y, int width, int height, int bpp = 4)
        {
            byte[] buf = new byte[width * height * bpp];

            if (bpp == 4)
            {
                IGL.Primary.ReadPixels(x, y, width, height, (int)PixelFormat.Bgra, (int)PixelType.UnsignedByte, buf);
            }
            else if (bpp == 3)
            {
                IGL.Primary.ReadPixels(x, y, width, height, (int)PixelFormat.Bgr, (int)PixelType.UnsignedByte, buf);
            }

            return buf;
        }

        public void Dispose()
        {
            if(fbo != 0)
            {
                IGL.Primary.DeleteFramebuffer(fbo);
                fbo = 0;
            }
        }
    }
}
