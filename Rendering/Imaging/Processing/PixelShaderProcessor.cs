using Materia.Rendering.Textures;
using Materia.Rendering.Interfaces;
using MLog;

namespace Materia.Rendering.Imaging.Processing
{
    public class PixelShaderProcessor : ImageProcessor
    {
        public PixelShaderProcessor() : base()
        {

        }

        public void Prepare(GLTexture2D tex, GLTexture2D tex2, GLTexture2D tex3, GLTexture2D tex4)
        {
            if (Shader != null)
            {
                Shader.Use();
                
                outputBuff?.Bind();
                outputBuff?.BindAsImage(0, false, true);

                Shader.SetUniform("Input0", 1);
                IGL.Primary.ActiveTexture((int)TextureUnit.Texture1);
                tex?.Bind();

                Shader.SetUniform("Input1", 2);
                IGL.Primary.ActiveTexture((int)TextureUnit.Texture2);
                tex2?.Bind();

                Shader.SetUniform("Input2", 3);
                IGL.Primary.ActiveTexture((int)TextureUnit.Texture3);
                tex2?.Bind();

                Shader.SetUniform("Input3", 4);
                IGL.Primary.ActiveTexture((int)TextureUnit.Texture4);
                tex4?.Bind();
            }
        }

        public void Process()
        {
            IGL.Primary.DispatchCompute(Width / 8, Height / 8, 1);
            IGL.Primary.MemoryBarrier((int)MemoryBarrierFlags.AllBarrierBits);

            GLTexture2D.UnbindAsImage(0);
            GLTexture2D.Unbind();
        }
    }
}
