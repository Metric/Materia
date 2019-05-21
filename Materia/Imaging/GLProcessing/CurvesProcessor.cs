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
    public class CurvesProcessor : ImageProcessor
    {
        GLShaderProgram shader;
        GLTextuer2D CurveLUT;
        public CurvesProcessor(GLTextuer2D lut) : base()
        {
            shader = GetShader("image.glsl", "curve.glsl");
            CurveLUT = lut;
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
                GL.ActiveTexture(TextureUnit.Texture0);
                tex.Bind();
                shader.SetUniform("CurveLUT", 1);
                GL.ActiveTexture(TextureUnit.Texture1);
                CurveLUT.Bind();

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
