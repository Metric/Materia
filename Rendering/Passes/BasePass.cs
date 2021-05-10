﻿using System;
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
        GLRenderBuffer depth;
        GLFrameBuffer frame;

        int width;
        int height;

        public BasePass(int w, int h)
        {
            color = new GLTexture2D[2];

            width = w;
            height = h;

            for(int i = 0; i < color.Length; ++i)
            {
                color[i] = new GLTexture2D(PixelInternalFormat.Rgba32f);
                color[i].Bind();
                color[i].SetData(IntPtr.Zero, PixelFormat.Rgba, w, h);
                color[i].Linear();
                GLTexture2D.Unbind();
            }

            depth = new GLRenderBuffer();
            depth.Bind();
            depth.SetBufferStorageAsDepth(w, h);
            depth.Unbind();

            frame = new GLFrameBuffer();
            frame.Bind();
            frame.AttachColor(color[0], 0);
            frame.AttachColor(color[1], 1);
            frame.AttachDepth(depth);

            if(!frame.IsValid)
            {
                Log.Error("Invalid frame buffer");
            }

            frame.Unbind();
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
                    color[i].SetData(IntPtr.Zero, PixelFormat.Rgba, w, h);
                    GLTexture2D.Unbind();
                }
            }

            depth?.Dispose();

            depth = new GLRenderBuffer();
            depth.Bind();
            depth.SetBufferStorageAsDepth(w, h);
            depth.Unbind();

            frame?.Bind();
            frame?.AttachDepth(depth);
            frame?.Unbind();
        }

        public override void Render(GLTexture2D[] inputs, out GLTexture2D[] outputs, Action renderScene = null)
        {
            outputs = color;

            frame.Bind();
            IGL.Primary.DrawBuffers(new int[] { (int)DrawBuffersEnum.ColorAttachment0, (int)DrawBuffersEnum.ColorAttachment1 });
            IGL.Primary.Viewport(0, 0, width, height);
            IGL.Primary.ClearColor(0, 0, 0, 0);
            IGL.Primary.Clear((int)ClearBufferMask.ColorBufferBit | (int)ClearBufferMask.DepthBufferBit);

            renderScene?.Invoke();

            frame.Unbind();
        }

        public override void Dispose()
        {
            frame?.Dispose();
            frame = null;

            depth?.Dispose();
            depth = null;

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
