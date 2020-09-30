using Materia.Rendering.Textures;
using Materia.Rendering.Interfaces;
using Materia.Rendering.Mathematics;

namespace Materia.Rendering.Imaging.Processing
{
    public class UniformColorProcessor : ImageProcessor
    {
        public Vector4 Color { get; set; }

        IGLProgram shader;

        public UniformColorProcessor() : base()
        {
            shader = GetShader("image.glsl", "uniformcolor.glsl");
        }

        public override void Process(int width, int height, GLTexture2D tex, GLTexture2D output)
        {
            base.Process(width, height, tex, output);

            if (shader != null)
            {
                Vector2 tiling = new Vector2(TileX, TileY);
                Vector4 color = Color;
                shader.Use();
                shader.SetUniform2("tiling", ref tiling);
                shader.SetUniform4("color", ref color);

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
