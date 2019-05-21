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
    public class BasicImageRenderer : ImageProcessor
    {
        GLShaderProgram shader;

        public BasicImageRenderer() : base()
        {
            shader = GetShader("image.glsl", "image-basic.glsl");
        }

        /// <summary>
        /// This is used to simply render to frame buffer
        /// at the specified width and height
        /// it is used primarily to pull preview images
        /// from the stored textures
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="tex"></param>
        public void Process(int width, int height, GLTextuer2D tex)
        {
            base.Process(width, height, tex, null);

            if (shader != null)
            {
                Vector2 tiling = new Vector2(TileX, TileY);

                shader.Use();
                shader.SetUniform2("tiling", ref tiling);
                shader.SetUniform("MainTex", 0);
                GL.ActiveTexture(TextureUnit.Texture0);
                tex.Bind();

                if (renderQuad != null)
                {
                    renderQuad.Draw();
                }

                GLTextuer2D.Unbind();
            }
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

        public override void Release()
        {

        }
    }
}
