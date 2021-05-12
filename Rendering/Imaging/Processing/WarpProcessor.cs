using Materia.Rendering.Textures;
using Materia.Rendering.Interfaces;
using Materia.Rendering.Mathematics;

namespace Materia.Rendering.Imaging.Processing
{
    public class WarpProcessor : ImageProcessor
    {
        public float Intensity { get; set; }

        public WarpProcessor() : base()
        {
            shader = GetShader("image.glsl", "warp.glsl");
            Intensity = 1;
        }

        protected override void SetUniqueUniforms()
        {
            base.SetUniqueUniforms();
            shader?.SetUniform("intensity", Intensity);
        }

        protected override void SetTexturePositions()
        {
            base.SetTexturePositions();
            shader?.SetUniform("Warp", 1);
        }

        public void Process(GLTexture2D input, GLTexture2D warp)
        {
            Identity();
            Bind();
            SetTextures(input, warp);
            renderQuad?.Draw();
            Unbind();
        }
    }
}
