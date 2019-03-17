using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Materia.Textures;
using Materia.Geometry;
using Materia.Buffers;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using Materia.Shaders;
using System.IO;

namespace Materia.Imaging.GLProcessing
{
    public class ImageProcessor
    {
        protected static FullScreenQuad renderQuad;
        protected static GLRenderBuffer renderBuff;
        protected static GLFrameBuffer frameBuff;
        protected static GLTextuer2D colorBuff;

        protected static Dictionary<string, GLShaderProgram> Shaders = new Dictionary<string, GLShaderProgram>();

        protected int Width { get; set; }
        protected int Height { get; set; }

        public float TileX { get; set; }
        public float TileY { get; set; }

        public ImageProcessor()
        {
            TileX = 1;
            TileY = 1;
        }

        protected void CreateBuffersIfNeeded()
        {
            if (renderBuff == null)
            {
                renderBuff = new GLRenderBuffer();
                renderBuff.Bind();
                renderBuff.SetBufferStorageAsDepth(4096, 4096);
                GLRenderBuffer.Unbind();
            }
            if (colorBuff == null)
            {
                //colorbuff part of the framebuffer is always Rgba32f to support all texture formats
                //that could be rendered into it
                colorBuff = new GLTextuer2D(PixelInternalFormat.Rgba32f);
                colorBuff.Bind();
                colorBuff.SetData(new float[0], PixelFormat.Rgba, 4096, 4096);
                colorBuff.SetFilter((int)TextureMinFilter.Linear, (int)TextureMagFilter.Linear);
                Console.WriteLine("color buff id: " + colorBuff.Id);
                GLTextuer2D.Unbind();
            }
            if (frameBuff == null)
            {
                frameBuff = new GLFrameBuffer();
                Console.WriteLine("frame buff id: " + frameBuff.Id);
                frameBuff.Bind();
                frameBuff.AttachColor(colorBuff);
                frameBuff.AttachDepth(renderBuff);

                if (!frameBuff.IsValid)
                {

                    Console.WriteLine("Framebuffer not complete!!!");
                    GLFrameBuffer.Unbind();
                    return;
                }

                GLFrameBuffer.Unbind();
            }
        }

        public virtual void Process(int width, int height, GLTextuer2D tex, GLTextuer2D output)
        {
            Width = width;
            Height = height;

            CreateBuffersIfNeeded();

            if(renderQuad == null)
            {
                renderQuad = new FullScreenQuad();
            }

            //these must be disabled
            //otherwise image processing will not work properly
            GL.Disable(EnableCap.DepthTest);
            GL.Disable(EnableCap.CullFace);
            GL.Disable(EnableCap.Blend);
            frameBuff.Bind();
            GL.Viewport(0, 0, width, height);
            GL.ClearColor(0, 0, 0, 0);
            GL.Clear(ClearBufferMask.ColorBufferBit);
            GL.Clear(ClearBufferMask.DepthBufferBit);
        }

        protected GLShaderProgram GetShader(string vertFile, string fragFile)
        {
            return Material.Material.GetShader(vertFile, fragFile);
        }

        public virtual float[] ReadFloat(int x, int y, int width, int height)
        {
            if(frameBuff != null)
            {
                return frameBuff.ReadFloatPixels(x, y, width, height);
            }

            return null;
        }

        public virtual float[] ReadFloat(int width, int height)
        {
            if(frameBuff != null)
            {
                return frameBuff.ReadFloatPixels(width, height);
            }

            return null;
        }

        public virtual byte[] ReadByte(int width, int height)
        {
            if(frameBuff != null)
            {
                return frameBuff.ReadBytePixels(width, height);
            }

            return null;
        }

        public virtual byte[] ReadByte(int x, int y, int width, int height)
        {
            if(frameBuff != null)
            {
                return frameBuff.ReadBytePixels(x, y, width, height);
            }

            return null;
        }

        public void Complete()
        {
            if(frameBuff != null)
            {
                GLFrameBuffer.Unbind();
            }

            //need to re-enable the ones we disabled
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.CullFace);
            GL.Enable(EnableCap.Blend);
        }

        public virtual void Release()
        {

        }

        public static void ReleaseAll()
        {
            if (colorBuff != null)
            {
                colorBuff.Release();
            }
            if (renderQuad != null)
            {
                renderQuad.Release();
            }
            if (renderBuff != null)
            {
                renderBuff.Release();
            }
            if (frameBuff != null)
            {
                frameBuff.Release();
            }
        }
    }
}
