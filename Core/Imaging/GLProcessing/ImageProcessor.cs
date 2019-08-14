using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Materia.Textures;
using Materia.Geometry;
using Materia.Buffers;
using Materia.Math3D;
using Materia.GLInterfaces;
using Materia.Shaders;
using Materia.MathHelpers;

namespace Materia.Imaging.GLProcessing
{
    public class ImageProcessor
    {
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
            Matrix4 trans = Matrix4.CreateTranslation(translation.X * inc.Width, translation.Y * inc.Height, 0);
            Matrix4 sm = Matrix4.CreateScale(((float)inc.Width * 0.5f) * scale.X, ((float)inc.Height * 0.5f) * scale.Y, 1);
            Matrix4 rot = Matrix4.CreateRotationZ(angle);
            Matrix4 model = pTrans * sm * rot * iPTrans * trans;
            Matrix4 view = Matrix4.LookAt(new Vector3(0, 0, 1), Vector3.Zero, Vector3.UnitY);

            IGL.Primary.Viewport(0, 0, owidth, oheight);

            resizeProcessor.Model = model;
            resizeProcessor.View = view;
            resizeProcessor.Projection = proj;
            resizeProcessor.Luminosity = Luminosity;

            resizeProcessor.Bind(inc);

            inc.ClampToEdge();

            if(renderQuad != null)
            {
                renderQuad.Draw();
            }

            inc.Repeat();
            resizeProcessor.Unbind();

            o.Bind();
            o.Repeat();
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
            Matrix4 sm = Matrix4.CreateScale(fp * (float)(owidth * 0.5f), fp * (float)(oheight * 0.5f), 1);
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
                colorBuff.Repeat();
                GLTextuer2D.Unbind();
            }
            if (frameBuff == null)
            {
                frameBuff = new GLFrameBuffer();
                frameBuff.Bind();
                frameBuff.AttachColor(colorBuff);
                frameBuff.AttachDepth(renderBuff);
                IGL.Primary.DrawBuffer((int)DrawBufferMode.ColorAttachment0);
                IGL.Primary.ReadBuffer((int)ReadBufferMode.ColorAttachment0);

                if (!frameBuff.IsValid)
                {
                    var status = IGL.Primary.CheckFramebufferStatus((int)FramebufferTarget.Framebuffer);  
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
            IGL.Primary.Disable((int)EnableCap.DepthTest);
            //IGL.Primary.Disable((int)EnableCap.CullFace);
            IGL.Primary.Disable((int)EnableCap.Blend);
            frameBuff.Bind();
            IGL.Primary.Viewport(0, 0, width, height);
            IGL.Primary.ClearColor(0, 0, 0, 0);
            IGL.Primary.Clear((int)ClearBufferMask.ColorBufferBit);
            IGL.Primary.Clear((int)ClearBufferMask.DepthBufferBit);
        }

        protected IGLProgram GetShader(string vertFile, string fragFile)
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

        public virtual void Complete()
        {
            if(frameBuff != null)
            {
                GLFrameBuffer.Unbind();
            }

            //need to re-enable the ones we disabled
            IGL.Primary.Enable((int)EnableCap.DepthTest);
            IGL.Primary.Enable((int)EnableCap.CullFace);
            IGL.Primary.Enable((int)EnableCap.Blend);
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
