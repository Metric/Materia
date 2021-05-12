using Materia.Rendering.Textures;
using Materia.Rendering.Interfaces;
using Materia.Rendering.Mathematics;

namespace Materia.Rendering.Imaging.Processing
{
    public class CurvesProcessor : ImageProcessor
    {
        GLTexture2D CurveLUT;
        public CurvesProcessor(GLTexture2D lut) : base()
        {
            shader = GetShader("image.glsl", "curve.glsl");
            CurveLUT = lut;
        }

        protected override void SetTexturePositions()
        {
            base.SetTexturePositions();
            shader?.SetUniform("CurveLUT", 1);
        }

        public void Process(GLTexture2D input)
        {
            Identity();
            Bind();
            SetTextures(input, CurveLUT);
            renderQuad?.Draw();
            Unbind();
        }
    }
}
