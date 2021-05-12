using System;
using Materia.Rendering.Textures;
using Materia.Rendering.Interfaces;
using Materia.Rendering.Mathematics;

namespace Materia.Rendering.Imaging.Processing
{
    public class MotionBlurProcessor : ImageProcessor
    {
        public float Direction { get; set; }
        public float Magnitude { get; set; }

        public MotionBlurProcessor() : base()
        {
            shader = GetShader("image.glsl", "motionblur.glsl");
        }

        protected override void SetUniqueUniforms()
        {
            base.SetUniqueUniforms();

            float width = outputBuff.Width;
            float height = outputBuff.Height;

            shader?.SetUniform("width", (float)width);
            shader?.SetUniform("height", (float)height);

            //the direction to blur in
            shader?.SetUniform("tx", (float)Math.Cos(Direction));
            shader?.SetUniform("ty", -(float)Math.Sin(Direction));
        }

        protected void SetPass(float mag)
        {
            shader?.SetUniform("magnitude", mag);
        }

        public void Process(GLTexture2D input)
        {
            float[] boxes = Blur.BoxesForGaussian(Magnitude, 3);

            float pass1 = (boxes[0] - 1.0f) / 2.0f;
            float pass2 = (boxes[1] - 1.0f) / 2.0f;
            float pass3 = (boxes[2] - 1.0f) / 2.0f;

            //clamp output buff to edge
            outputBuff.Bind();
            outputBuff.ClampToEdge();

            Identity();
            Bind();

            //pass 1
            SetPass(pass1);
            SetTextures(input);

            renderQuad?.Draw();

            //pass 2
            SetPass(pass2);
            SetTextures(outputBuff);

            renderQuad?.Draw();

            //pass 3
            SetPass(pass3);

            renderQuad?.Draw();

            //restore output to repeat
            outputBuff.Bind();
            outputBuff.Repeat();

            Unbind();
        }
    }
}
