using Materia.Rendering.Textures;
using Materia.Rendering.Interfaces;
using Materia.Rendering.Mathematics;

namespace Materia.Rendering.Imaging.Processing
{
    public class HSLProcessor : ImageProcessor
    {
        public float Hue { get; set; }
        public float Saturation { get; set; }
        public float Lightness { get; set; }

        public HSLProcessor()
        {
            shader = GetShader("image.glsl", "hsl.glsl");
        }

        protected override void SetUniqueUniforms()
        {
            base.SetUniqueUniforms();
            shader?.SetUniform("hue", Hue);
            shader?.SetUniform("saturation", Saturation);
            shader?.SetUniform("lightness", Lightness);
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
