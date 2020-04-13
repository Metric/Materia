using Materia.Rendering.Textures;
using Materia.Rendering.Interfaces;
using Materia.Rendering.Mathematics;

namespace Materia.Rendering.Imaging.Processing
{
    public class WarpProcessor : ImageProcessor
    {
        public float Intensity { get; set; }

        IGLProgram shader;

        public WarpProcessor() : base()
        {
            shader = GetShader("image.glsl", "warp.glsl");
            Intensity = 1;
        }

        public void Process(int width, int height, GLTexture2D tex, GLTexture2D warp, GLTexture2D output)
        {
            base.Process(width, height, tex, output);

            if (shader != null)
            {
                ResizeViewTo(tex, output, tex.Width, tex.Height, width, height);
                tex = output;
                IGL.Primary.Clear((int)(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit));

                Vector2 tiling = new Vector2(TileX, TileY);

                shader.Use();
                shader.SetUniform2("tiling", ref tiling);
                shader.SetUniform("MainTex", 0);
                shader.SetUniform("intensity", Intensity);
                IGL.Primary.ActiveTexture((int)TextureUnit.Texture0);
                tex.Bind();
                tex.Repeat();
                shader.SetUniform("Warp", 1);
                IGL.Primary.ActiveTexture((int)TextureUnit.Texture1);
                warp.Bind();

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
