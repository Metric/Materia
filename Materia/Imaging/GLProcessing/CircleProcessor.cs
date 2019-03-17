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
    public class CircleProcessor : ImageProcessor
    {
        public float Radius { get; set; }
        public float Outline { get; set; }

        GLShaderProgram shader;

        public CircleProcessor() : base()
        {
            shader = GetShader("image.glsl", "circle.glsl");
        }

        public override void Process(int width, int height, GLTextuer2D tex, GLTextuer2D output)
        {
            base.Process(width, height, tex, output);

            if (shader != null)
            {
                Vector2 tiling = new Vector2(TileX, TileY);

                shader.Use();
                shader.SetUniform2("tiling", ref tiling);
                shader.SetUniform("width", (float)width);
                shader.SetUniform("height", (float)height);
                shader.SetUniform("radius", Radius);
                shader.SetUniform("outline", Outline);

                if (renderQuad != null)
                {
                    renderQuad.Draw();
                }

                output.Bind();
                output.CopyFromFrameBuffer(width, height);
                GLTextuer2D.Unbind();
            }
        }
    }
}
