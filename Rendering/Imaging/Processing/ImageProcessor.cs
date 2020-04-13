using System;
using System.Collections.Generic;
using Materia.Rendering.Textures;
using Materia.Rendering.Interfaces;
using Materia.Rendering.Geometry;
using Materia.Rendering.Shaders;
using Materia.Rendering.Buffers;
using Materia.Rendering.Mathematics;
using MLog;

namespace Materia.Rendering.Imaging.Processing
{
    public class ImageProcessor : IDisposable
    {
        

        protected static FullScreenQuad renderQuad;
        protected static GLRenderBuffer renderBuff;
        protected static GLFrameBuffer frameBuff;
        protected static GLFrameBuffer temp;
        protected static GLTexture2D colorBuff;

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

        protected void ApplyTransformNoAuto(GLTexture2D inc, GLTexture2D o, int owidth, int oheight, MVector translation, MVector scale, float angle, MVector pivot)
        {
            Matrix4 proj = Matrix4.CreateOrthographic(owidth, oheight, 0.03f, 1000f);
            Matrix4 pTrans = Matrix4.CreateTranslation(-pivot.X, -pivot.Y, 0);
            Matrix4 iPTrans = Matrix4.CreateTranslation(pivot.X, pivot.Y, 0);
            Matrix4 trans = Matrix4.CreateTranslation(translation.X, translation.Y, 0);
            Matrix4 sm = Matrix4.CreateScale(scale.X, scale.Y, 1);
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

            if (renderQuad != null)
            {
                renderQuad.Draw();
            }

            inc.Repeat();
            resizeProcessor.Unbind();

            o.Bind();
            o.Repeat();
            GLTexture2D.Unbind();

            Blit(o, owidth, oheight);

            /*o.Bind();
            o.Repeat();
            o.CopyFromFrameBuffer(owidth, oheight);
            GLTexture2D.Unbind();*/
        }

        protected void ApplyTransform(GLTexture2D inc, GLTexture2D o, int owidth, int oheight, MVector translation, MVector scale, float angle, MVector pivot)
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
            GLTexture2D.Unbind();

            Blit(o, owidth, oheight);

            /*o.Bind();
            o.Repeat();
            o.CopyFromFrameBuffer(owidth, oheight);
            GLTexture2D.Unbind();*/

        } 

        protected void ResizeViewTo(GLTexture2D inc, GLTexture2D o, int owidth, int oheight, int nwidth, int nheight)
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

            Blit(o, nwidth, nheight);
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
                colorBuff = new GLTexture2D(PixelInternalFormat.Rgba32f);
                colorBuff.Bind();
                colorBuff.SetData(IntPtr.Zero, PixelFormat.Rgba, 4096, 4096);
                colorBuff.Nearest();
                colorBuff.Repeat();
                GLTexture2D.Unbind();
            }
            if (frameBuff == null)
            {
                frameBuff = new GLFrameBuffer();
                frameBuff.Bind();
                frameBuff.AttachColor(colorBuff);
                frameBuff.AttachDepth(renderBuff);
                IGL.Primary.DrawBuffers(new int[] { (int)DrawBufferMode.ColorAttachment0 });
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

        public virtual void Process(int width, int height, GLTexture2D tex, GLTexture2D output)
        {
            IGL.Primary.Enable((int)EnableCap.Dither);

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

        protected void Blit(GLTexture2D output, int width, int height)
        {
            if (temp == null)
            {
                temp = new GLFrameBuffer();
            }
            temp.Bind();
            temp.BindRead();
            temp.AttachColor(colorBuff, 0);
            IGL.Primary.ReadBuffer((int)ReadBufferMode.ColorAttachment0);
            temp.AttachColor(output, 1);
            IGL.Primary.DrawBuffers(new int[] { (int)DrawBufferMode.ColorAttachment1 });

            if (!temp.IsValid)
            {
                Log.Error("Frame buff is invalid on blit:\r\n" + Environment.StackTrace);
            }

            IGL.Primary.BlitFramebuffer(0, 0, width, height, 0, 0, width, height, (int)ClearBufferMask.ColorBufferBit, (int)BlitFramebufferFilter.Linear);
            GLFrameBuffer.UnbindRead();
            GLFrameBuffer.Unbind();
            frameBuff.Bind();
        }

        protected IGLProgram GetShader(string vertFile, string fragFile)
        {
            return GLShaderCache.GetShader(vertFile, fragFile);
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

        public virtual void Dispose()
        {

        }

        public static void DisposeCache()
        {
            if(temp != null)
            {
                temp.Dispose();
                temp = null;
            }

            if (colorBuff != null)
            {
                colorBuff.Dispose();
                colorBuff = null;
            }

            if (renderQuad != null)
            {
                renderQuad.Dispose();
                renderQuad = null;
            }

            if (renderBuff != null)
            {
                renderBuff.Dispose();
                renderBuff = null;
            }

            if (frameBuff != null)
            {
                frameBuff.Dispose();
                frameBuff = null;
            }
        }
    }
}
