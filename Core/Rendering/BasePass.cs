using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Materia.Geometry;
using Materia.Textures;
using Materia.Buffers;
using Materia.GLInterfaces;
using NLog;

namespace Materia.Rendering
{
    public class BasePass : RenderStackItem
    {
        private static ILogger Log = LogManager.GetCurrentClassLogger();

        GLTextuer2D[] color;
        GLRenderBuffer depth;
        GLFrameBuffer frame;

        MeshRenderer[] Meshes { get; set; }

        int width;
        int height;

        public BasePass(MeshRenderer[] m, int w, int h)
        {
            Meshes = m;

            color = new GLTextuer2D[2];

            width = w;
            height = h;

            for(int i = 0; i < color.Length; i++)
            {
                color[i] = new GLTextuer2D(PixelInternalFormat.Rgba16f);
                color[i].Bind();
                color[i].SetData(IntPtr.Zero, PixelFormat.Rgba, w, h);
                color[i].Linear();
                color[i].ClampToEdge();
                GLTextuer2D.Unbind();
            }

            depth = new GLRenderBuffer();
            depth.Bind();
            depth.SetBufferStorageAsDepth(w, h);
            GLRenderBuffer.Unbind();

            frame = new GLFrameBuffer();
            frame.Bind();
            frame.AttachColor(color[0], 0);
            frame.AttachColor(color[1], 1);
            frame.AttachDepth(depth);

            if(!frame.IsValid)
            {
                Log.Error("Invalid frame buffer");
            }

            GLFrameBuffer.Unbind();
        }

        public void Update(MeshRenderer[] m, int w, int h)
        {
            Meshes = m;

            width = w;
            height = h;

            if(color != null)
            {
                for(int i = 0; i < color.Length; i++)
                {
                    color[i].Bind();
                    color[i].SetData(IntPtr.Zero, PixelFormat.Rgba, w, h);
                    GLTextuer2D.Unbind();
                }
            }

            if(depth != null)
            {
                depth.Bind();
                depth.SetBufferStorageAsDepth(w, h);
                GLRenderBuffer.Unbind();
            }
        }

        public override void Render(GLTextuer2D[] inputs, out GLTextuer2D[] outputs)
        {
            outputs = color;

            if (Meshes == null) return;

            frame.Bind();
            IGL.Primary.DrawBuffers(new int[] { (int)DrawBuffersEnum.ColorAttachment0, (int)DrawBuffersEnum.ColorAttachment1 });
            IGL.Primary.Viewport(0, 0, width, height);
            IGL.Primary.ClearColor(0, 0, 0, 0);
            IGL.Primary.Clear((int)ClearBufferMask.ColorBufferBit);
            IGL.Primary.Clear((int)ClearBufferMask.DepthBufferBit);
            for (int i = 0; i < Meshes.Length; i++)
            {
                Meshes[i].Draw();
            }
            GLFrameBuffer.Unbind();
        }

        public override void Release()
        {
            if(frame != null)
            {
                frame.Release();
                frame = null;
            }

            if(depth != null)
            {
                depth.Release();
                depth = null;
            }

            if(color != null)
            {
                for(int i = 0; i < color.Length; i++)
                {
                    color[i].Release();
                }

                color = null;
            }
        }
    }
}
