using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Materia.Textures;
using Materia.Shaders;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace Materia.Imaging.GLProcessing
{
    public class PixelShaderProcessor : ImageProcessor
    {
        public GLShaderProgram Shader { get; set; }

        public PixelShaderProcessor()
        {
        }

        public void Process(int width, int height, GLTextuer2D tex, GLTextuer2D tex2, GLTextuer2D tex3, GLTextuer2D tex4, GLTextuer2D output)
        {
            base.Process(width, height, tex, output);

            if (Shader != null)
            {
                Vector2 tiling = new Vector2(TileX, TileY);
                Shader.Use();
                Shader.SetUniform2("tiling", ref tiling);
                Shader.SetUniform("Input0", 0);
                GL.ActiveTexture(TextureUnit.Texture0);

                if (tex != null)
                {
                    tex.Bind();
                }

                Shader.SetUniform("Input1", 1);
                GL.ActiveTexture(TextureUnit.Texture1);

                if(tex2 != null)
                {
                    tex2.Bind();
                }


                Shader.SetUniform("Input2", 2);
                GL.ActiveTexture(TextureUnit.Texture2);

                if(tex3 != null)
                {
                    tex3.Bind();
                }

                Shader.SetUniform("Input3", 3);
                GL.ActiveTexture(TextureUnit.Texture3);

                if(tex4 != null)
                {
                    tex4.Bind();
                }

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
