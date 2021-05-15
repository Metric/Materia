using System;
using Materia.Rendering.Geometry;
using Materia.Rendering.Textures;
using Materia.Rendering.Buffers;
using Materia.Rendering.Interfaces;
using MLog;
using System.Linq;

namespace Materia.Rendering.Passes
{
    public class BasePass : RenderStackItem
    {
        GLTexture2D[] color;
        GLFrameBuffer frame;

        int width;
        int height;

        public BasePass(GLFrameBuffer frameBuffer, int w, int h)
        {
            frame = frameBuffer;

            //we create 4 colors buffs
            //0 = base color
            //1 = bloom color
            //2 = for intermediate output
            //3 = final output
            color = new GLTexture2D[4];

            width = w;
            height = h;

            for(int i = 0; i < color.Length; ++i)
            {
                color[i] = new GLTexture2D(PixelInternalFormat.Rgba32f);
                color[i].Bind();
                color[i].SetData(IntPtr.Zero, PixelFormat.Bgra, w, h);
                color[i].Linear();
                color[i].ClampToEdge();
                GLTexture2D.Unbind();
            }
        }

        public void Update(int w, int h)
        {
            width = w;
            height = h;

            if(color != null)
            {
                for(int i = 0; i < color.Length; ++i)
                {
                    color[i].Bind();
                    color[i].SetData(IntPtr.Zero, PixelFormat.Bgra, w, h);
                    GLTexture2D.Unbind();
                }
            }
        }

        public override void Render(GLTexture2D[] inputs, out GLTexture2D[] outputs, Action<RenderStackState> renderScene = null)
        {
            outputs = color;

            if (frame == null) return;

            frame.Bind();
            frame.AttachColor(color[0], 0);
            frame.AttachColor(color[1], 1);
            
            if (!frame.IsValid)
            {
                Log.Error("Invalid frame buffer");
            }

            IGL.Primary.DrawBuffers(new int[] { (int)DrawBuffersEnum.ColorAttachment0, (int)DrawBuffersEnum.ColorAttachment1 });
            IGL.Primary.Viewport(0, 0, width, height);
            IGL.Primary.ClearColor(0, 0, 0, 0);
            IGL.Primary.Clear((int)ClearBufferMask.ColorBufferBit | (int)ClearBufferMask.DepthBufferBit);

            renderScene?.Invoke(RenderStackState.Color);

            frame.Unbind();
        }

        public override void Dispose()
        {
            if(color != null)
            {
                for(int i = 0; i < color.Length; ++i)
                {
                    color[i]?.Dispose();
                }

                color = null;
            }
        }
    }
}
