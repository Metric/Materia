using Materia.Rendering.Textures;
using Materia.Rendering.Interfaces;
using Materia.Rendering.Mathematics;

namespace Materia.Rendering.Imaging.Processing
{
    public class GrayscaleConvProcessor : ImageProcessor
    {
        public Vector4 Weight { get; set; }

        public GrayscaleConvProcessor() : base()
        {
            shader = GetShader("image.glsl", "grayscaleconv.glsl");
        }

        protected override void SetUniqueUniforms()
        {
            base.SetUniqueUniforms();

            Vector4 w = Weight;
            shader?.SetUniform4("weight", ref w);
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
