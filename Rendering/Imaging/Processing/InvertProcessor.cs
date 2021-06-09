using Materia.Rendering.Textures;
using Materia.Rendering.Interfaces;
using Materia.Rendering.Mathematics;

namespace Materia.Rendering.Imaging.Processing
{
    public class InvertProcessor : ImageProcessor
    {
        public bool Red { get; set; }
        public bool Green { get; set; }
        public bool Blue { get; set; }
        public bool Alpha { get; set; }

        public InvertProcessor() : base()
        {
            shader = GetShader("image.glsl", "invert.glsl");
        }

        protected override void SetUniqueUniforms()
        {
            base.SetUniqueUniforms();
            shader?.SetUniform("invertRed", Red);
            shader?.SetUniform("invertGreen", Green);
            shader?.SetUniform("invertBlue", Blue);
            shader?.SetUniform("invertAlpha", Alpha);
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
