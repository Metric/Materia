using Materia.Rendering.Textures;
using Materia.Rendering.Interfaces;
using Materia.Rendering.Mathematics;

namespace Materia.Rendering.Imaging.Processing
{
    public class EmbossProcessor : ImageProcessor
    {
        public float Azimuth { get; set; }
        public float Elevation { get; set; }

        public EmbossProcessor() : base()
        {
            shader = GetShader("raw.glsl", "emboss.glsl");
        }

        protected override void SetUniqueUniforms()
        {
            base.SetUniqueUniforms();

            float width = outputBuff.Width;
            float height = outputBuff.Height;

            shader?.SetUniform("width", width);
            shader?.SetUniform("height", height);
            shader?.SetUniform("azimuth", Azimuth);
            shader?.SetUniform("elevation", Elevation);
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
