using Materia.Rendering.Textures;
using Materia.Rendering.Interfaces;
using Materia.Rendering.Mathematics;

namespace Materia.Rendering.Imaging.Processing
{
    public class LevelsProcessor : ImageProcessor
    {
        public Vector3 Min { get; set; }
        public Vector3 Mid { get; set; }
        public Vector3 Max { get; set; }
        public Vector2 Value { get; set; }

        public LevelsProcessor() : base()
        {
            shader = GetShader("image.glsl", "levels.glsl");
        }

        protected override void SetUniqueUniforms()
        {
            base.SetUniqueUniforms();

            Vector3 min = Min;
            Vector3 max = Max;
            Vector3 mid = Mid;
            Vector2 value = Value;

            shader?.SetUniform3("maxValues", ref max);
            shader?.SetUniform3("minValues", ref min);
            shader?.SetUniform3("midValues", ref mid);
            shader?.SetUniform2("value", ref value);
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
