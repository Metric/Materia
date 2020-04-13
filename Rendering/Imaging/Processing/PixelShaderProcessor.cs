using Materia.Rendering.Textures;
using Materia.Rendering.Interfaces;
using MLog;

namespace Materia.Rendering.Imaging.Processing
{
    public class PixelShaderProcessor : ImageProcessor
    {
        
        public IGLProgram Shader { get; set; }

        public PixelShaderProcessor()
        {
        }

        public void Prepare(int width, int height, GLTexture2D tex, GLTexture2D tex2, GLTexture2D tex3, GLTexture2D tex4, GLTexture2D output)
        {
            base.Process(width, height, tex, output);

            if (Shader != null)
            {
                Shader.Use();
                if (output != null)
                {
                    output.Bind();
                    output.BindAsImage(0, false, true);
                }

                Shader.SetUniform("Input0", 1);
                IGL.Primary.ActiveTexture((int)TextureUnit.Texture1);
                if (tex != null)
                {
                    tex.Bind();
                }

                Shader.SetUniform("Input1", 2);
                IGL.Primary.ActiveTexture((int)TextureUnit.Texture2);
                if (tex2 != null)
                {
                    tex2.Bind();
                }

                Shader.SetUniform("Input2", 3);
                IGL.Primary.ActiveTexture((int)TextureUnit.Texture3);
                if (tex3 != null)
                {
                    tex3.Bind();
                }

                Shader.SetUniform("Input3", 4);
                IGL.Primary.ActiveTexture((int)TextureUnit.Texture4);
                if (tex4 != null)
                {
                    tex4.Bind();
                }
            }
        }

        public override void Complete()
        {
            IGL.Primary.DispatchCompute(Width / 8, Height / 8, 1);

            GLTexture2D.UnbindAsImage(0);
            GLTexture2D.Unbind();

            base.Complete();
        }
    }
}
