using System;
using Materia.Rendering.Textures;
using Materia.Rendering.Interfaces;
using Materia.Rendering.Mathematics;

namespace Materia.Rendering.Imaging.Processing
{
    public class BlendProcessor : ImageProcessor
    {
        public int BlendMode
        {
            get; set;
        }

        public int AlphaMode
        {
            get; set;
        }

        public float Alpha { get; set; }

        protected bool hasMask = false;

        public BlendProcessor() : base()
        {
            shader = GetShader("image.glsl", "blend.glsl");
            AlphaMode = 0;
        }

        protected override void SetUniqueUniforms()
        {
            base.SetUniqueUniforms();

            shader.SetUniform("blendMode", BlendMode);
            shader.SetUniform("alpha", Alpha);
            shader.SetUniform("alphaMode", AlphaMode);
            shader.SetUniform("hasMask", hasMask ? 1 : 0);
        }

        protected override void SetTexturePositions()
        {
            shader.SetUniform("Foreground", 0);
            shader.SetUniform("Background", 1);
        }

        public void Process(GLTexture2D foreground, GLTexture2D background, GLTexture2D mask)
        {
            Identity();
            hasMask = mask != null;
            Bind();
            SetTextures(foreground, background, mask);
            renderQuad?.Draw();
            Unbind();
        }
    }
}
