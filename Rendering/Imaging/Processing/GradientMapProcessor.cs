using Materia.Rendering.Textures;
using Materia.Rendering.Interfaces;
using Materia.Rendering.Mathematics;

namespace Materia.Rendering.Imaging.Processing
{
    public class GradientMapProcessor : ImageProcessor
    {
        public GLTexture2D ColorLUT { get; set; }
        public GLTexture2D Mask { get; set; }
        public bool UseMask { get; set; }
        public bool Horizontal { get; set; }

        public GradientMapProcessor()
        {
            Horizontal = true;
            shader = GetShader("raw.glsl", "gradientmap.glsl");
            UseMask = false;
        }

        protected override void SetUniqueUniforms()
        {
            base.SetUniqueUniforms();
            shader?.SetUniform("horizontal", Horizontal);
            shader?.SetUniform("useMask", UseMask);
        }

        protected override void SetTexturePositions()
        {
            base.SetTexturePositions();
            shader?.SetUniform("ColorLUT", 1);
            shader?.SetUniform("Mask", 2);
        }

        public void Process(GLTexture2D input)
        {
            UseMask = Mask != null;
            Identity();
            Bind();
            SetTextures(input, ColorLUT, Mask);
            renderQuad?.Draw();
            Unbind();
        }
    }
}
