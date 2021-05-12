using Materia.Rendering.Textures;
using Materia.Rendering.Interfaces;
using Materia.Rendering.Mathematics;

namespace Materia.Rendering.Imaging.Processing
{
    public class BlurProcessor : ImageProcessor
    {
        public float Intensity { get; set; }

        public BlurProcessor() : base()
        {
            shader = GetShader("image.glsl", "blur.glsl");
        }

        protected void SetPass(bool horizontal, float intensity)
        {
            shader?.SetUniform("horizontal", horizontal);
            shader?.SetUniform("intensity", intensity);
        }

        /// <summary>
        /// This would be too slow for real time performance at 60fps
        /// but since we are not doing 60fps it is fine
        /// it is a fast box gaussian blur
        /// produces results better than adobes gaussian blur
        /// and faster than theirs!
        /// can easily blur 4kx4k textures in low ms on a 1080 gtx gpu with 64+ intensity
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="tex"></param>
        /// <param name="output"></param>
        public void Process(GLTexture2D input)
        {
            float[] boxes = Blur.BoxesForGaussian(Intensity, 3);

            float pass1 = (boxes[0] - 1.0f) / 2.0f;
            float pass2 = (boxes[1] - 1.0f) / 2.0f;
            float pass3 = (boxes[2] - 1.0f) / 2.0f;

            //clamp output buff to edge
            outputBuff.Bind();
            outputBuff.ClampToEdge();

            Identity();
            Bind();

            //pass 1
            SetPass(true, pass1);
            SetTextures(input);

            renderQuad?.Draw();

            SetPass(false, pass1);
            SetTextures(outputBuff);

            renderQuad?.Draw();

            //pass 2
            SetPass(true, pass2);

            renderQuad?.Draw();

            SetPass(false, pass2);

            renderQuad?.Draw();

            //pass 3
            SetPass(true, pass3);

            renderQuad?.Draw();

            SetPass(false, pass3);

            renderQuad?.Draw();

            //restore output to repeat
            outputBuff.Bind();
            outputBuff.Repeat();

            Unbind();
        }
    }
}
