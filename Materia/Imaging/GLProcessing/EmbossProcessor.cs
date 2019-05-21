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
    public class EmbossProcessor : ImageProcessor
    {
        GLShaderProgram shader;

        public float Azimuth { get; set; }
        public float Elevation { get; set; }

        public EmbossProcessor() : base()
        {
            shader = GetShader("image.glsl", "emboss.glsl");
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
                shader.SetUniform2("tiling", ref tiling);
                shader.SetUniform("MainTex", 0);
                shader.SetUniform("width", (float)width);
                shader.SetUniform("height", (float)height);
                shader.SetUniform("azimuth", Azimuth);
                shader.SetUniform("elevation", Elevation);
                GL.ActiveTexture(0);
                tex.Bind();

                if(renderQuad != null)
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
