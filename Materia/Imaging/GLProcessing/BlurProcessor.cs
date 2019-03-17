using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Materia.Textures;
using Materia.Shaders;
using OpenTK.Graphics.OpenGL;
using OpenTK;

namespace Materia.Imaging.GLProcessing
{
    public class BlurProcessor : ImageProcessor
    {
        public float Intensity { get; set; }

        GLShaderProgram shader;

        public BlurProcessor() : base()
        {
            shader = GetShader("image.glsl", "blur.glsl");
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
        public override void Process(int width, int height, GLTextuer2D tex, GLTextuer2D output)
        {
            base.Process(width, height, tex, output);

            if (shader != null)
            {
                Vector2 tiling = new Vector2(TileX, TileY);
                float[] boxes = Materia.Nodes.Helpers.Blur.BoxesForGaussian(Intensity, 3);

                shader.Use();
                shader.SetUniform2("tiling", ref tiling);
                shader.SetUniform("MainTex", 0);
                shader.SetUniform("horizontal", true);
                shader.SetUniform("intensity", (boxes[0] - 1.0f) / 2.0f);
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

                shader.SetUniform("horizontal", false);
                GL.ActiveTexture(TextureUnit.Texture0);
                output.Bind();

                GL.Clear(ClearBufferMask.ColorBufferBit);
                GL.Clear(ClearBufferMask.DepthBufferBit);

                if (renderQuad != null)
                {
                    renderQuad.Draw();
                }

                GLTextuer2D.Unbind();
                output.Bind();
                output.CopyFromFrameBuffer(width, height);
                GLTextuer2D.Unbind();

                //begin second box
                shader.SetUniform("horizontal", true);
                shader.SetUniform("intensity", (boxes[1] - 1.0f) / 2.0f);
                GL.ActiveTexture(TextureUnit.Texture0);
                output.Bind();

                GL.Clear(ClearBufferMask.ColorBufferBit);
                GL.Clear(ClearBufferMask.DepthBufferBit);

                if (renderQuad != null)
                {
                    renderQuad.Draw();
                }

                GLTextuer2D.Unbind();
                output.Bind();
                output.CopyFromFrameBuffer(width, height);
                GLTextuer2D.Unbind();

                shader.SetUniform("horizontal", false);
                shader.SetUniform("intensity", (boxes[1] - 1.0f) / 2.0f);
                GL.ActiveTexture(TextureUnit.Texture0);
                output.Bind();

                GL.Clear(ClearBufferMask.ColorBufferBit);
                GL.Clear(ClearBufferMask.DepthBufferBit);

                if (renderQuad != null)
                {
                    renderQuad.Draw();
                }

                GLTextuer2D.Unbind();
                output.Bind();
                output.CopyFromFrameBuffer(width, height);
                GLTextuer2D.Unbind();

                //begin third box
                shader.SetUniform("horizontal", true);
                shader.SetUniform("intensity", (boxes[2] - 1.0f) / 2.0f);
                GL.ActiveTexture(TextureUnit.Texture0);
                output.Bind();

                GL.Clear(ClearBufferMask.ColorBufferBit);
                GL.Clear(ClearBufferMask.DepthBufferBit);

                if (renderQuad != null)
                {
                    renderQuad.Draw();
                }

                GLTextuer2D.Unbind();
                output.Bind();
                output.CopyFromFrameBuffer(width, height);
                GLTextuer2D.Unbind();

                shader.SetUniform("horizontal", false);
                shader.SetUniform("intensity", (boxes[2] - 1.0f) / 2.0f);
                GL.ActiveTexture(TextureUnit.Texture0);
                output.Bind();

                GL.Clear(ClearBufferMask.ColorBufferBit);
                GL.Clear(ClearBufferMask.DepthBufferBit);

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

        public override void Release()
        {
            base.Release();
        }
    }
}
