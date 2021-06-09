using Materia.Rendering.Textures;
using Materia.Rendering.Interfaces;
using Materia.Rendering.Mathematics;

namespace Materia.Rendering.Imaging.Processing
{
    public class TransformProcessor : ImageProcessor
    {
        public Matrix3 Rotation { get; set; }
        public Matrix3 Scale { get; set; }
        public Vector3 Translation { get; set; }

        public TransformProcessor() : base()
        {
            shader = GetShader("image.glsl", "transform.glsl");
        }

        protected override void SetUniqueUniforms()
        {
            base.SetUniqueUniforms();

            Matrix3 rot = Rotation;
            Matrix3 sc = Scale;
            Vector3 tr = Translation;

            shader?.SetUniformMatrix3("rotation", ref rot);
            shader?.SetUniformMatrix3("scale", ref sc);
            shader?.SetUniform3("translation", ref tr);
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
