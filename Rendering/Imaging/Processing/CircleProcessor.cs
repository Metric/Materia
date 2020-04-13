using Materia.Rendering.Textures;
using Materia.Rendering.Interfaces;
using Materia.Rendering.Mathematics;

namespace Materia.Rendering.Imaging.Processing
{
    public class CircleProcessor : ImageProcessor
    {
        public float Radius { get; set; }
        public float Outline { get; set; }

        IGLProgram shader;

        public CircleProcessor() : base()
        {
            shader = GetShader("image.glsl", "circle.glsl");
        }

        public override void Process(int width, int height, GLTexture2D tex, GLTexture2D output)
        {
            base.Process(width, height, tex, output);

            if (shader != null)
            {
                Vector2 tiling = new Vector2(TileX, TileY);

                shader.Use();
                shader.SetUniform2("tiling", ref tiling);
                shader.SetUniform("width", (float)width);
                shader.SetUniform("height", (float)height);
                shader.SetUniform("radius", Radius);
                shader.SetUniform("outline", Outline);

                if (renderQuad != null)
                {
                    renderQuad.Draw();
                }

                //output.Bind();
                //output.CopyFromFrameBuffer(width, height);
                //GLTexture2D.Unbind();
                Blit(output, width, height);
            }
        }
    }
}
