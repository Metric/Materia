using Materia.Rendering.Textures;
using Materia.Rendering.Interfaces;
using Materia.Rendering.Mathematics;

namespace Materia.Rendering.Imaging.Processing
{
    public class GradientMapProcessor : ImageProcessor
    {
        public GLTexture2D ColorLUT { get; set; }
        public GLTexture2D Mask { get; set; }
        public bool UseMask { get; set; }
        public bool Horizontal { get; set; }

        IGLProgram shader;

        public GradientMapProcessor()
        {
            Horizontal = true;
            shader = GetShader("image.glsl", "gradientmap.glsl");
            UseMask = false;
        }

        public override void Process(int width, int height, GLTexture2D tex, GLTexture2D output)
        {
            base.Process(width, height, tex, output);

            if (shader != null)
            {
                ResizeViewTo(tex, output, tex.Width, tex.Height, width, height);
                tex = output;
                IGL.Primary.Clear((int)(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit));

                Vector2 tiling = new Vector2(TileX, TileY);

                shader.Use();

                shader.SetUniform("MainTex", 0);
                IGL.Primary.ActiveTexture((int)TextureUnit.Texture0);
                tex.Bind();

                shader.SetUniform("ColorLUT", 1);
                IGL.Primary.ActiveTexture((int)TextureUnit.Texture1);
                ColorLUT.Bind();

                shader.SetUniform("horizontal", Horizontal);
                shader.SetUniform2("tiling", ref tiling);
                shader.SetUniform("Mask", 2);
                IGL.Primary.ActiveTexture((int)TextureUnit.Texture2);

                if (Mask != null)
                {
                    UseMask = true;
                    Mask.Bind();
                }
                else
                {
                    UseMask = false;
                }

                shader.SetUniform("useMask", UseMask);

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
