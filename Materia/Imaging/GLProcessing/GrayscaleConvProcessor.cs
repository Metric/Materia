using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Materia.Textures;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using Materia.Shaders;

namespace Materia.Imaging.GLProcessing
{
    public class GrayscaleConvProcessor : ImageProcessor
    {
        public Vector4 Weight { get; set; }

        GLShaderProgram shader;

        public GrayscaleConvProcessor() : base()
        {
            shader = GetShader("image.glsl", "grayscaleconv.glsl");
        }

        public override void Process(int width, int height, GLTextuer2D tex, GLTextuer2D output)
        {
            base.Process(width, height, tex, output);

            if (shader != null)
            {
                Vector2 tiling = new Vector2(TileX, TileY);

                Vector4 w = Weight;
                shader.Use();
                shader.SetUniform2("tiling", ref tiling);
                shader.SetUniform("MainTex", 0);
                shader.SetUniform4F("weight", ref w);
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
