using Materia.Rendering.Textures;
using Materia.Rendering.Interfaces;
using Materia.Rendering.Geometry;
using Materia.Rendering.Buffers;
using Materia.Rendering.Mathematics;

namespace Materia.Rendering.Imaging.Processing
{
    public class MeshProcessor : ImageProcessor
    {
        public MeshRenderer Mesh { get; set; }

        IGLProgram shader;

        public MeshProcessor() : base()
        {
            shader = GetShader("image.glsl", "image-basic.glsl");
        }

        public void Process(int width, int height, GLTexture2D output)
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

                //output.Bind();
                //output.CopyFromFrameBuffer(width, height);
                //GLTexture2D.Unbind();

                Blit(output, width, height);

                frameBuff.Unbind();
            }

            base.Process(width, height, output, output);

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

                GLTexture2D.Unbind();

                //output.Bind();
                //output.CopyFromFrameBuffer(width, height);
                //GLTexture2D.Unbind();
                Blit(output, width, height);
            }
        }
    }
}
