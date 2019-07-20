using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Materia.Shaders;
using Materia.Textures;
using Materia.Math3D;
using Materia.GLInterfaces;

namespace Materia.Imaging.GLProcessing
{
    public class UniformColorProcessor : ImageProcessor
    {
        public Vector4 Color { get; set; }

        IGLProgram shader;

        public UniformColorProcessor() : base()
        {
            shader = GetShader("image.glsl", "uniformcolor.glsl");
        }

        public override void Process(int width, int height, GLTextuer2D tex, GLTextuer2D output)
        {
            base.Process(width, height, tex, output);

            if (shader != null)
            {
                Vector2 tiling = new Vector2(TileX, TileY);
                Vector4 color = Color;
                shader.Use();
                shader.SetUniform2("tiling", ref tiling);
                shader.SetUniform4F("color", ref color);

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
