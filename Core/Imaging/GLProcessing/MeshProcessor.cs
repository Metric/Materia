using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Materia.Buffers;
using Materia.Textures;
using Materia.Shaders;
using Materia.Geometry;
using Materia.Math3D;
using Materia.GLInterfaces;

namespace Materia.Imaging.GLProcessing
{
    public class MeshProcessor : ImageProcessor
    {
        public MeshRenderer Mesh { get; set; }

        IGLProgram shader;

        public MeshProcessor() : base()
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
                IGL.Primary.Viewport(0, 0, width, height);
                IGL.Primary.ClearColor(0, 0, 0, 0);
                IGL.Primary.Clear((int)ClearBufferMask.DepthBufferBit);
                IGL.Primary.Clear((int)ClearBufferMask.ColorBufferBit);

                //draw in depth
                Mesh.Draw();

                output.Bind();
                output.CopyFromFrameBuffer(width, height);
                GLTextuer2D.Unbind();

                GLFrameBuffer.Unbind();
            }

            Process(width, height, output, output);

            if (shader != null)
            {
                Vector2 tiling = new Vector2(TileX, TileY);

                shader.Use();
                shader.SetUniform2("tiling", ref tiling);

                shader.SetUniform("MainTex", 0);
                IGL.Primary.ActiveTexture((int)TextureUnit.Texture0);
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
