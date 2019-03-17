using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Materia.Buffers;
using Materia.Geometry;
using Materia.Textures;
using OpenTK.Graphics.OpenGL;
using Materia.Shaders;
using OpenTK;

namespace Materia.Imaging.GLProcessing
{
    public class MeshDepthProcessor : ImageProcessor
    {
        public MeshRenderer Mesh { get; set; }

        GLShaderProgram shader;

        public MeshDepthProcessor() : base()
        {
            shader = GetShader("image.glsl", "image-basic.glsl");
        }

        public void Process(int width, int height, GLTextuer2D output)
        {
            CreateBuffersIfNeeded();

            if (Mesh != null)
            {
                //bind our depth framebuffer
                frameBuff.Bind();
                GL.Viewport(0, 0, width, height);
                GL.ClearColor(0, 0, 0, 0);
                GL.Clear(ClearBufferMask.DepthBufferBit);
                GL.Clear(ClearBufferMask.ColorBufferBit);

                //draw in depth
                Mesh.DrawForDepth();

                output.Bind();
                output.CopyFromFrameBuffer(width, height);
                GLTextuer2D.Unbind();

                GLFrameBuffer.Unbind();
            }

            Process(width, height, output, output);

            if(shader != null)
            {
                Vector2 tiling = new Vector2(TileX, TileY);

                shader.Use();
                shader.SetUniform2("tiling", ref tiling);

                shader.SetUniform("MainTex", 0);
                GL.ActiveTexture(TextureUnit.Texture0);
                output.Bind();

                if (renderQuad != null)
                {
                    renderQuad.Draw();
                }

                GLTextuer2D.Unbind();

                output.Bind();
                output.CopyFromFrameBuffer(width, height);
                GLTextuer2D.Unbind();
            }
        }
    }
}
