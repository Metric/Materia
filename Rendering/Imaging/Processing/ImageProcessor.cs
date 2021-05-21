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
    public class ImageProcessor : ViewProcessor
    {
        protected static FullScreenQuad renderQuad;
        protected static GLRenderBuffer renderBuff;
        protected static GLFrameBuffer frameBuff;
        
        //do we really need this now?
        protected static GLFrameBuffer temp;

        protected static GLTexture2D outputBuff;

        public static readonly Matrix4 view = Matrix4.LookAt(new Vector3(0, 0, 1), Vector3.Zero, Vector3.UnitY);

        protected int Width { get; set; }
        protected int Height { get; set; }

        public bool Stretch { get; set; }

        public ImageProcessor() : base()
        {
            Luminosity = 1.0f;
            Stretch = true;
        }

        protected void Transform(MVector translation, MVector scale, float angle, MVector pivot)
        {
            int owidth = Width;
            int oheight = Height;

            Matrix4 proj = Matrix4.CreateOrthographic(owidth, oheight, 0f, 1000f);
            Matrix4 pTrans = Matrix4.CreateTranslation(-pivot.X, -pivot.Y, 0);
            Matrix4 iPTrans = Matrix4.CreateTranslation(pivot.X, pivot.Y, 0);
            Matrix4 trans = Matrix4.CreateTranslation(translation.X, translation.Y, 0);

            Matrix4 sm = pTrans * Matrix4.CreateScale(scale.X, scale.Y, 1) * iPTrans;
            Matrix4 rot = pTrans * Matrix4.CreateRotationZ(angle) * iPTrans;

            //note old method
            //Matrix4 model = pTrans * sm * rot * iPTrans * trans;

            Matrix4 model = sm * rot * trans;

            Model = model;
            View = view;
            Projection = proj;
        }

        protected void TransformAutoSize(GLTexture2D inc, MVector translation, MVector scale, float angle, MVector pivot)
        {
            int owidth = Width;
            int oheight = Height;

            Matrix4 proj = Matrix4.CreateOrthographic(owidth, oheight, 0f, 1000f);
            Matrix4 pTrans = Matrix4.CreateTranslation(-pivot.X, -pivot.Y, 0);
            Matrix4 iPTrans = Matrix4.CreateTranslation(pivot.X, pivot.Y, 0);
            Matrix4 trans = Matrix4.CreateTranslation(translation.X * inc.Width, translation.Y * inc.Height, 0);
            
            Matrix4 sm = pTrans * Matrix4.CreateScale(((float)inc.Width * 0.5f) * scale.X, ((float)inc.Height * 0.5f) * scale.Y, 1) * iPTrans;
            Matrix4 rot = pTrans * Matrix4.CreateRotationZ(angle) * iPTrans;

            //note old method
            //Matrix4 model = pTrans * sm * rot * iPTrans * trans;

            Matrix4 model = sm * rot * trans;

            Model = model;
            View = view;
            Projection = proj;
        } 

        protected void Resize(GLTexture2D inc)
        {
            int nwidth = Width; 
            int nheight = Height;

            int owidth = inc.Width;
            int oheight = inc.Height;

            float wp = (float)nwidth / (float)owidth;
            float hp = (float)nheight / (float)oheight;

            float fp = wp < hp ? wp : hp;

            Matrix4 proj = Matrix4.CreateOrthographic(nwidth, nheight, 0f, 1000f);
            //half width/height for scale as it is centered based
            Matrix4 sm = Matrix4.CreateScale(fp * (float)(owidth * 0.5f), fp * (float)(oheight * 0.5f), 1);

            Model = sm;
            View = view;
            Projection = proj;
        }

        protected void Identity()
        {
            int nwidth = Width;
            int nheight = Height;

            Projection = Matrix4.CreateOrthographic(nwidth, nheight, 0f, 1000f);
            View = view;
            Model = Matrix4.Identity;
        }

        protected void CreateBuffersIfNeeded()
        {
            if (renderBuff == null)
            {
                renderBuff = new GLRenderBuffer();
                renderBuff.Bind();
                renderBuff.SetBufferStorageAsDepth(4096, 4096);
                renderBuff.Unbind();
            }

            if (frameBuff == null)
            {
                frameBuff = new GLFrameBuffer();
                frameBuff.Bind();
                frameBuff.AttachDepth(renderBuff);

                IGL.Primary.DrawBuffers(new int[] { (int)DrawBufferMode.ColorAttachment0 });
                IGL.Primary.ReadBuffer((int)ReadBufferMode.ColorAttachment0);

                frameBuff.Unbind();
            }
        }

        public virtual void PrepareView(GLTexture2D output)
        {
            CreateBuffersIfNeeded();

            outputBuff = output;

            Width = output.Width;
            Height = output.Height;

            if(renderQuad == null)
            {
                renderQuad = new FullScreenQuad();
            }

            //these must be disabled
            //otherwise image processing will not work properly
            IGL.Primary.Disable((int)EnableCap.DepthTest);
            IGL.Primary.Disable((int)EnableCap.CullFace);

            frameBuff.Bind();
            frameBuff.AttachColor(outputBuff);

            IGL.Primary.Viewport(0, 0, Width, Height);
            IGL.Primary.ClearColor(0, 0, 0, 0);

            IGL.Primary.Clear((int)ClearBufferMask.ColorBufferBit | (int)ClearBufferMask.DepthBufferBit | (int)ClearBufferMask.StencilBufferBit);
        }

        public void Process(GLTexture2D input)
        {
            Bind();
            SetTextures(input);
            renderQuad?.Draw();
            Unbind();
        }

        /*
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
            temp.UnbindRead();
            temp.Unbind();
            frameBuff?.Bind();
        }
        */

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
            frameBuff?.Unbind();
            IGL.Primary.Enable((int)EnableCap.CullFace);
            IGL.Primary.Enable((int)EnableCap.DepthTest);
        }

        public static void DisposeCache()
        {
            temp?.Dispose();
            temp = null;

            renderQuad?.Dispose();
            renderQuad = null;

            renderBuff?.Dispose();
            renderBuff = null;

            frameBuff?.Dispose();
            frameBuff = null;
        }
    }
}
