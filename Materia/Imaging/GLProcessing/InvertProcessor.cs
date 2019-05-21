using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Materia.Shaders;
using Materia.Textures;
using OpenTK.Graphics.OpenGL;
using OpenTK;

namespace Materia.Imaging.GLProcessing
{
    public class InvertProcessor : ImageProcessor
    {
        public bool Red { get; set; }
        public bool Green { get; set; }
        public bool Blue { get; set; }
        public bool Alpha { get; set; }

        GLShaderProgram shader;

        public InvertProcessor() : base()
        {
            shader = GetShader("image.glsl", "invert.glsl");
        }

        public override void Process(int width, int height, GLTextuer2D tex, GLTextuer2D output)
        {
            base.Process(width, height, tex, output);

            if (shader != null)
            {
                ResizeViewTo(tex, output, tex.Width, tex.Height, width, height);
                tex = output;
                GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

                Vector2 tiling = new Vector2(TileX, TileY);

                shader.Use();
                shader.SetUniform("MainTex", 0);
                shader.SetUniform2("tiling", ref tiling);
                shader.SetUniform("invertRed", Red);
                shader.SetUniform("invertGreen", Green);
                shader.SetUniform("invertBlue", Blue);
                shader.SetUniform("invertAlpha", Alpha);

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
            }
        }
    }
}
