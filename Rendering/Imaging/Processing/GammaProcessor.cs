using Materia.Rendering.Textures;
using Materia.Rendering.Interfaces;
using Materia.Rendering.Mathematics;

namespace Materia.Rendering.Imaging.Processing
{
    public class GammaProcessor : ImageProcessor
    {
        public float Gamma { get; set; }

        public GammaProcessor()
        {
            Gamma = 1;
            shader = GetShader("raw.glsl", "gamma.glsl");
        }

        protected override void SetUniqueUniforms()
        {
            base.SetUniqueUniforms();
            shader?.SetUniform("gamma", Gamma);
        }

        public void Process(GLTexture2D input)
        {
            Identity();
            Bind();
            SetTextures(input);
            renderQuad?.Draw();
            Unbind();
        }
    }
}
