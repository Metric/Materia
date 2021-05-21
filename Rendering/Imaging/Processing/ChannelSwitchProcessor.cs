using System;
using Materia.Rendering.Textures;
using Materia.Rendering.Interfaces;
using Materia.Rendering.Mathematics;

namespace Materia.Rendering.Imaging.Processing
{
    public class ChannelSwitchProcessor : ImageProcessor
    {
        public int RedChannel { get; set; }
        public int GreenChannel { get; set; }
        public int BlueChannel { get; set; }
        public int AlphaChannel { get; set; }

        public ChannelSwitchProcessor() : base()
        {
            shader = GetShader("raw.glsl", "channelswitch.glsl");    
        }

        protected override void SetUniqueUniforms()
        {
            base.SetUniqueUniforms();
            shader?.SetUniform("redChannel", RedChannel);
            shader?.SetUniform("greenChannel", GreenChannel);
            shader?.SetUniform("blueChannel", BlueChannel);
            shader?.SetUniform("alphaChannel", AlphaChannel);
        }

        protected override void SetTexturePositions()
        {
            base.SetTexturePositions();
            shader?.SetUniform("Other", 1);
        }

        public void Process(GLTexture2D first, GLTexture2D second)
        {
            Identity();

            Bind();
            SetTextures(first, second);

            renderQuad?.Draw();

            Unbind();
        }
    }
}
