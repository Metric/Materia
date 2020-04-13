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

        IGLProgram shader;
        public MotionBlurProcessor() : base()
        {
            shader = GetShader("image.glsl", "motionblur.glsl");
        }

        public override void Process(int width, int height, GLTexture2D tex, GLTexture2D output)
        {
            base.Process(width, height, tex, output);

            if (shader != null)
            {
                ResizeViewTo(tex, output, tex.Width, tex.Height, width, height);
                tex = output;
                IGL.Primary.Clear((int)(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit));

                Vector2 tiling = new Vector2(TileX, TileY);

                //by using the boxes for gaussian we can produce a more realisitc motion blur
                //then simply using a standard box blur alone (aka a non linear motion blur)
                float[] boxes = Blur.BoxesForGaussian(Magnitude, 3);

                shader.Use();
                shader.SetUniform2("tiling", ref tiling);
                shader.SetUniform("width", (float)width);
                shader.SetUniform("height", (float)height);

                //the direction to blur in
                shader.SetUniform("tx", (float)Math.Cos(Direction));
                shader.SetUniform("ty", -(float)Math.Sin(Direction));

                //intensity / magnitude of motion blur
                shader.SetUniform("magnitude", (boxes[0] - 1.0f) / 2.0f);
                shader.SetUniform("MainTex", 0);
                IGL.Primary.ActiveTexture((int)TextureUnit.Texture0);
                tex.Bind();

                if (renderQuad != null)
                {
                    renderQuad.Draw();
                }

                GLTexture2D.Unbind();
                //output.Bind();
                //output.CopyFromFrameBuffer(width, height);
                //GLTexture2D.Unbind();

                Blit(output, width, height);

                for(int i = 1; i < 3; ++i)
                {
                    shader.SetUniform("magnitude", (boxes[i] - 1.0f) / 2.0f);
                    IGL.Primary.ActiveTexture((int)TextureUnit.Texture0);
                    output.Bind();

                    IGL.Primary.Clear((int)ClearBufferMask.DepthBufferBit);
                    IGL.Primary.Clear((int)ClearBufferMask.ColorBufferBit);

                    if (renderQuad != null)
                    {
                        renderQuad.Draw();
                    }

                    GLTexture2D.Unbind();

                    //output.Bind();
                    //output.CopyFromFrameBuffer(width, height);
                    //GLTexture2D.Unbind();
                    Blit(output, width, height);
                }
            }
        }
    }
}
