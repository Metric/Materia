using Materia.Rendering.Textures;
using Materia.Rendering.Interfaces;
using Materia.Rendering.Mathematics;

namespace Materia.Rendering.Imaging.Processing
{
    public class SharpenProcessor : ImageProcessor
    {
        public float Intensity { get; set; }

        public SharpenProcessor()
        {
            shader = GetShader("image.glsl", "sharpen.glsl");
        }

        protected override void SetUniqueUniforms()
        {
            base.SetUniqueUniforms();
            shader?.SetUniform("intensity", Intensity);
        }

        public override void Process(GLTexture2D input)
        {
            Identity();
            Resize(input);
            Bind();
            SetTextures(input);
            renderQuad?.Draw();
            Unbind();
        }
    }
}
