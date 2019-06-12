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
using Materia.MathHelpers;
using System.IO;
using NLog;

namespace Materia.Imaging.GLProcessing
{
    public class ImageProcessor
    {
        protected static ILogger Log = LogManager.GetCurrentClassLogger();

        protected static FullScreenQuad renderQuad;
        protected static GLRenderBuffer renderBuff;
        protected static GLFrameBuffer frameBuff;
        protected static GLTextuer2D colorBuff;

        protected static PreviewProcessor resizeProcessor;

        protected static Dictionary<string, GLShaderProgram> Shaders = new Dictionary<string, GLShaderProgram>();

        protected int Width { get; set; }
        protected int Height { get; set; }

        public bool Stretch { get; set; }

        public float TileX { get; set; }
        public float TileY { get; set; }

        public float Luminosity { get; set; }

        public ImageProcessor()
        {
            Luminosity = 1.0f;
            Stretch = true;
            TileX = 1;
            TileY = 1;
        }

        protected void ApplyTransform(GLTextuer2D inc, GLTextuer2D o, int owidth, int oheight, MVector translation, MVector scale, float angle, MVector pivot)
        {
            Matrix4 proj = Matrix4.CreateOrthographic(owidth, oheight, 0.03f, 1000f);
            Matrix4 pTrans = Matrix4.CreateTranslation(-pivot.X, -pivot.Y, 0);
            Matrix4 iPTrans = Matrix4.CreateTranslation(pivot.X, pivot.Y, 0);
            Matrix4 trans = Matrix4.CreateTranslation(translation.X, translation.Y, 0);
            Matrix4 sm = Matrix4.CreateScale(((float)inc.Width * 0.5f) * scale.X, ((float)inc.Height * 0.5f) * scale.Y, 1);
            Matrix4 rot = Matrix4.CreateRotationZ(angle);
            Matrix4 model = pTrans * sm * rot * iPTrans * trans;
            Matrix4 view = Matrix4.LookAt(new Vector3(0, 0, 1), Vector3.Zero, Vector3.UnitY);

            resizeProcessor.Model = model;
            resizeProcessor.View = view;
            resizeProcessor.Projection = proj;
            resizeProcessor.Luminosity = Luminosity;

            resizeProcessor.Bind(inc);

            if(renderQuad != null)
            {
                renderQuad.Draw();
            }

            resizeProcessor.Unbind();

            o.Bind();
            o.CopyFromFrameBuffer(owidth, oheight);
            GLTextuer2D.Unbind();
        } 

        protected void ResizeViewTo(GLTextuer2D inc, GLTextuer2D o, int owidth, int oheight, int nwidth, int nheight)
        {
            float wp = (float)nwidth / (float)owidth;
            float hp = (float)nheight / (float)oheight;

            float fp = wp < hp ? wp : hp;

            Matrix4 proj = Matrix4.CreateOrthographic(nwidth, nheight, 0.03f, 1000f);
            Matrix4 translation = Matrix4.CreateTranslation(0, 0, 0);
            //half width/height for scale as it is centered based
            Matrix4 sm = Matrix4.CreateScale(fp * (float)(owidth * 0.5f), -fp * (float)(oheight * 0.5f), 1);
            Matrix4 model = sm * translation;
            Matrix4 view = Matrix4.LookAt(new Vector3(0, 0, 1), Vector3.Zero, Vector3.UnitY);

            resizeProcessor.Model = model;
            resizeProcessor.View = view;
            resizeProcessor.Projection = proj;
            resizeProcessor.Luminosity = Luminosity;

            resizeProcessor.Bind(inc);

            if(renderQuad != null)
            {
                renderQuad.Draw();
            }

            resizeProcessor.Unbind();

            o.Bind();
            o.CopyFromFrameBuffer(nwidth, nheight);
            GLTextuer2D.Unbind();
        }

        protected void CreateBuffersIfNeeded()
        {
            if(resizeProcessor == null)
            {
                resizeProcessor = new PreviewProcessor();
            }
            if (renderBuff == null)
            {
                renderBuff = new GLRenderBuffer();
                renderBuff.Bind();
                renderBuff.SetBufferStorageAsDepth(4096, 4096);
                Log.Debug("render buff id: " + renderBuff.Id);
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
                Log.Debug("color buff id: " + colorBuff.Id);
                GLTextuer2D.Unbind();
            }
            if (frameBuff == null)
            {
                frameBuff = new GLFrameBuffer();
                Console.WriteLine("frame buff id: " + frameBuff.Id);
                frameBuff.Bind();
                frameBuff.AttachColor(colorBuff);
                frameBuff.AttachDepth(renderBuff);
                GL.DrawBuffer(DrawBufferMode.ColorAttachment0);
                GL.ReadBuffer(ReadBufferMode.ColorAttachment0);

                if (!frameBuff.IsValid)
                {
                    var status = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);  
                    Log.Error("Framebuffer not complete!!! with status: " + status);
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
                colorBuff = null;
            }
            if (renderQuad != null)
            {
                renderQuad.Release();
                renderQuad = null;
            }
            if (renderBuff != null)
            {
                renderBuff.Release();
                renderBuff = null;
            }
            if (frameBuff != null)
            {
                frameBuff.Release();
                frameBuff = null;
            }
        }
    }
}
