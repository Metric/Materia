using Materia.Rendering.Textures;
using Materia.Rendering.Interfaces;
using Materia.Rendering.Mathematics;

namespace Materia.Rendering.Imaging.Processing
{
    public class UniformColorProcessor : ImageProcessor
    {
        public Vector4 Color { get; set; }

        public UniformColorProcessor() : base()
        {
            shader = GetShader("raw.glsl", "uniformcolor.glsl");
        }

        protected override void SetUniqueUniforms()
        {
            base.SetUniqueUniforms();

            Vector4 color = Color;
            shader?.SetUniform4("color", ref color);
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
