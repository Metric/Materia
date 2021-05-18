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
            shader = GetShader("raw.glsl", "blur.glsl");
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

            GLTexture2D output = outputBuff;

            output.Bind();
            output.ClampToEdge();
            GLTexture2D.Unbind();

            GLTexture2D temp = output.Copy();
            temp.Bind();
            temp.ClampToEdge();
            GLTexture2D.Unbind();

            PrepareView(temp);

            Identity();
            Bind();

            //pass 1
            SetPass(true, pass1);
            SetTextures(input);

            renderQuad?.Draw();

            PrepareView(output);
            SetPass(false, pass1);
            SetTextures(temp);

            renderQuad?.Draw();

            PrepareView(temp);
            //pass 2
            SetPass(true, pass2);
            SetTextures(output);

            renderQuad?.Draw();

            PrepareView(output);
            SetPass(false, pass2);
            SetTextures(temp);

            renderQuad?.Draw();

            PrepareView(temp);
            //pass 3
            SetPass(true, pass3);
            SetTextures(output);

            renderQuad?.Draw();

            PrepareView(output);
            SetPass(false, pass3);
            SetTextures(temp);

            renderQuad?.Draw();

            output.Bind();
            output.Repeat();

            temp.Dispose();

            Unbind();
        }
    }
}
