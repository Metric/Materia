using System;
using Materia.Rendering.Textures;
using Materia.Rendering.Interfaces;
using Materia.Rendering.Mathematics;

namespace Materia.Rendering.Imaging.Processing
{
    public class OcclusionProcessor : ImageProcessor
    {
        public OcclusionProcessor() : base()
        {
            shader = GetShader("image.glsl", "occlusion.glsl");
        }

        protected override void SetTexturePositions()
        {
            base.SetTexturePositions();
            shader?.SetUniform("Original", 1);
        }

        public void Process(GLTexture2D blur, GLTexture2D orig)
        {
            Identity();
            Bind();
            SetTextures(blur, orig);
            renderQuad?.Draw();
            Unbind();
        }
    }
}
