using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using Materia.Textures;

namespace Materia.Buffers
{
    public class GLFrameBuffer
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
            fbo = GL.GenFramebuffer();
        }

        public void Bind()
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, fbo);
        }

        public static void Unbind()
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        }

        public void AttachDepth(GLTextuer2D tex)
        {
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, TextureTarget.Texture2D, tex.Id, 0);
        }

        public void AttachColor(GLTextuer2D tex)
        {
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, tex.Id, 0);
        }

        public void AttachColor(GLRenderBuffer buf)
        {
            GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, RenderbufferTarget.Renderbuffer, buf.Id);
        }

        public void AttachDepth(GLRenderBuffer buf)
        {
            GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthStencilAttachment, RenderbufferTarget.Renderbuffer, buf.Id);
        }

        public bool IsValid
        {
            get
            {
                return GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer) == FramebufferErrorCode.FramebufferComplete;
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
                GL.ReadPixels(0, 0, width, height, PixelFormat.Rgba, PixelType.Float, buf);
            }
            else if (bpp == 3)
            {
                GL.ReadPixels(0, 0, width, height, PixelFormat.Rgb, PixelType.Float, buf);
            }
            else if (bpp == 2)
            {
                GL.ReadPixels(0, 0, width, height, PixelFormat.Rg, PixelType.Float, buf);
            }
            else if (bpp == 1)
            {
                GL.ReadPixels(0, 0, width, height, PixelFormat.Red, PixelType.Float, buf);
            }
            return buf;
        }

        public float[] ReadFloatPixels(int x, int y, int width, int height, int bpp = 4)
        {
            float[] buf = new float[width * height * bpp];
            if (bpp == 4)
            {
                GL.ReadPixels(x, y, width, height, PixelFormat.Rgba, PixelType.Float, buf);
            }
            else if (bpp == 3)
            {
                GL.ReadPixels(x, y, width, height, PixelFormat.Rgb, PixelType.Float, buf);
            }
            else if (bpp == 2)
            {
                GL.ReadPixels(x, y, width, height, PixelFormat.Rg, PixelType.Float, buf);
            }
            else if (bpp == 1)
            {
                GL.ReadPixels(x, y, width, height, PixelFormat.Red, PixelType.Float, buf);
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
                GL.ReadPixels(0, 0, width, height, PixelFormat.Bgra, PixelType.UnsignedByte, buf);
            }
            else if(bpp == 3)
            {
                GL.ReadPixels(0, 0, width, height, PixelFormat.Bgr, PixelType.UnsignedByte, buf);
            }

            return buf;
        }

        public byte[] ReadBytePixels(int x, int y, int width, int height, int bpp = 4)
        {
            byte[] buf = new byte[width * height * bpp];

            if (bpp == 4)
            {
                GL.ReadPixels(x, y, width, height, PixelFormat.Bgra, PixelType.UnsignedByte, buf);
            }
            else if (bpp == 3)
            {
                GL.ReadPixels(x, y, width, height, PixelFormat.Bgr, PixelType.UnsignedByte, buf);
            }

            return buf;
        }

        public void Release()
        {
            if(fbo != 0)
            {
                GL.DeleteFramebuffer(fbo);
                fbo = 0;
            }
        }
    }
}
