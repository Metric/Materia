using Materia.Textures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Materia.Shaders;
using OpenTK.Graphics.OpenGL;
using OpenTK;

namespace Materia.Imaging.GLProcessing
{
    public class MotionBlurProcessor : ImageProcessor
    {
        public float Direction { get; set; }
        public float Magnitude { get; set; }

        GLShaderProgram shader;
        public MotionBlurProcessor() : base()
        {
            shader = GetShader("image.glsl", "motionblur.glsl");
        }

        public override void Process(int width, int height, GLTextuer2D tex, GLTextuer2D output)
        {
            base.Process(width, height, tex, output);

            if (shader != null)
            {
                Vector2 tiling = new Vector2(TileX, TileY);

                //by using the boxes for gaussian we can produce a more realisitc motion blur
                //then simply using a standard box blur alone (aka a non linear motion blur)
                float[] boxes = Materia.Nodes.Helpers.Blur.BoxesForGaussian(Magnitude, 3);

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
                GL.ActiveTexture(TextureUnit.Texture0);
                tex.Bind();

                if (renderQuad != null)
                {
                    renderQuad.Draw();
                }

                GLTextuer2D.Unbind();
                output.Bind();
                output.CopyFromFrameBuffer(width, height);
                GLTextuer2D.Unbind();

                for(int i = 1; i < 3; i++)
                {
                    shader.SetUniform("magnitude", (boxes[i] - 1.0f) / 2.0f);
                    GL.ActiveTexture(TextureUnit.Texture0);
                    output.Bind();

                    GL.Clear(ClearBufferMask.DepthBufferBit);
                    GL.Clear(ClearBufferMask.ColorBufferBit);

                    if (renderQuad != null)
                    {
                        renderQuad.Draw();
                    }

                    GLTextuer2D.Unbind();

                    output.Bind();
                    output.CopyFromFrameBuffer(width, height);
                    GLTextuer2D.Unbind();
                }
            }
        }
    }
}
