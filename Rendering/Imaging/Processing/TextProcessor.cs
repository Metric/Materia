using Materia.Rendering.Textures;
using Materia.Rendering.Interfaces;
using Materia.Rendering.Mathematics;

namespace Materia.Rendering.Imaging.Processing
{
    public class TextProcessor : ImageProcessor
    {
        public MVector Translation { get; set; }
        public MVector Scale { get; set; }
        public float Angle { get; set; }
        public MVector Pivot { get; set; }

        public void Process(GLTexture2D input, Vector4 uv)
        {
            Transform(Translation, Scale, Angle, Pivot);
            Bind();
            SetTextures(input);
            renderQuad?.SetUV(ref uv);
            renderQuad?.Draw();
            Unbind();
        }

        public override void Complete()
        {
            renderQuad?.DefaultUV();
            base.Complete();
        }
    }
}
