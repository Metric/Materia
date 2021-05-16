﻿using Materia.Rendering.Textures;
using Materia.Rendering.Interfaces;
using Materia.Rendering.Mathematics;

namespace Materia.Rendering.Imaging.Processing
{
    public class DirectionalWarpProcessor : ImageProcessor
    {
        public float Angle { get; set; }
        public float Intensity { get; set; }

        public DirectionalWarpProcessor() : base()
        {
            shader = GetShader("image.glsl", "warpdirectional.glsl");
            Intensity = 1;
        }

        protected override void SetUniqueUniforms()
        {
            base.SetUniqueUniforms();
            shader?.SetUniform("angle", Angle);
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