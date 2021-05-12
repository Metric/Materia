using Materia.Rendering.Textures;
using Materia.Rendering.Interfaces;
using Materia.Rendering.Mathematics;

namespace Materia.Rendering.Imaging.Processing
{
    public class NormalsProcessor : ImageProcessor
    {
        public float Intensity { get; set; }
        public bool DirectX { get; set; }
        public float NoiseReduction { get; set; }

        public NormalsProcessor() : base()
        {
            shader = GetShader("image.glsl", "normals.glsl");
        }

        protected override void SetUniqueUniforms()
        {
            base.SetUniqueUniforms();

            float width = outputBuff.Width;
            float height = outputBuff.Height;

            shader?.SetUniform("directx", DirectX);
            shader?.SetUniform("intensity", Intensity);
            shader?.SetUniform("width", (float)width);
            shader?.SetUniform("height", (float)height);
            shader?.SetUniform("reduce", NoiseReduction);
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
