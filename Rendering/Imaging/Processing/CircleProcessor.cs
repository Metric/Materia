using Materia.Rendering.Textures;
using Materia.Rendering.Interfaces;
using Materia.Rendering.Mathematics;

namespace Materia.Rendering.Imaging.Processing
{
    public class CircleProcessor : ImageProcessor
    {
        public float Radius { get; set; }
        public float Outline { get; set; }

        public CircleProcessor() : base()
        {
            shader = GetShader("raw.glsl", "circle.glsl");
        }

        protected override void SetUniqueUniforms()
        {
            base.SetUniqueUniforms();

            shader?.SetUniform("width", (float)outputBuff.Width);
            shader?.SetUniform("height", (float)outputBuff.Height);
            shader?.SetUniform("radius", Radius);
            shader?.SetUniform("outline", Outline);
        }

        public void Process()
        {
            Identity();

            Bind();
            renderQuad?.Draw();
            Unbind();
        }
    }
}
