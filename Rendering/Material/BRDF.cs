using Materia.Rendering.Imaging;
using Materia.Rendering.Interfaces;
using Materia.Rendering.Textures;
using Microsoft.Extensions.FileProviders;
using System.Drawing;
using System.IO;
using System;
using System.Diagnostics;
using Materia.Rendering.Shaders;
using Materia.Rendering.Geometry;
using Materia.Rendering.Buffers;
using Materia.Rendering.Mathematics;

namespace Materia.Rendering.Material
{
    public class BRDF
    {
        public static GLTexture2D Lut { get; protected set; }

        public static void Create()
        {
            if (Lut == null || Lut.Id == 0)
            {
                Lut = new GLTexture2D(PixelInternalFormat.Rg32f);
            }
            else
            {
                return;
            }

            try
            {
                GLRenderBuffer renderBuffer = new GLRenderBuffer();
                GLFrameBuffer frame = new GLFrameBuffer();
                FullScreenQuad quad = new FullScreenQuad();
                IGLProgram shader = GLShaderCache.GetShader("raw.glsl", "brdf.glsl");

                FullScreenQuad.SharedVao?.Bind();

                Lut.Bind();
                Lut.SetData(IntPtr.Zero, PixelFormat.Rg, 512, 512);
                Lut.ClampToEdge();
                Lut.Linear();
                GLTexture2D.Unbind();

                renderBuffer.Bind();
                renderBuffer.SetBufferStorageAsDepth(512, 512);
                renderBuffer.Unbind();

                frame.Bind();
                frame.AttachDepth(renderBuffer);
                frame.AttachColor(Lut);

                Vector2 tiling = Vector2.One;

                IGL.Primary.Disable((int)EnableCap.DepthTest);
                IGL.Primary.Disable((int)EnableCap.CullFace);

                IGL.Primary.Viewport(0, 0, 512, 512);
                IGL.Primary.ClearColor(0, 0, 0, 1);
                IGL.Primary.PolygonMode((int)MaterialFace.FrontAndBack, (int)PolygonMode.Fill);
                IGL.Primary.Clear((int)ClearBufferMask.ColorBufferBit | (int)ClearBufferMask.DepthBufferBit);

                shader.Use();
                shader.SetUniform2("tiling", ref tiling);

                quad.Draw();

                frame.Unbind();

                quad.Dispose();
                renderBuffer.Dispose();
                frame.Dispose();

                FullScreenQuad.SharedVao?.Unbind();

                IGL.Primary.Enable((int)EnableCap.DepthTest);
                IGL.Primary.Enable((int)EnableCap.CullFace);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.StackTrace);
            }
        }

        public static void Dispose()
        {
            Lut?.Dispose();
            Lut = null;
        }
    }
}
