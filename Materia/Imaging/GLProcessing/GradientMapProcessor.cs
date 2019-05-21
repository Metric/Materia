using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Materia.Shaders;
using Materia.Textures;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace Materia.Imaging.GLProcessing
{
    public class GradientMapProcessor : ImageProcessor
    {
        public GLTextuer2D ColorLUT { get; set; }
        public GLTextuer2D Mask { get; set; }
        public bool UseMask { get; set; }

        GLShaderProgram shader;

        public GradientMapProcessor()
        {
            shader = GetShader("image.glsl", "gradientmap.glsl");
            UseMask = false;
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
                GL.ActiveTexture(TextureUnit.Texture0);
                tex.Bind();

                shader.SetUniform("ColorLUT", 1);
                GL.ActiveTexture(TextureUnit.Texture1);
                ColorLUT.Bind();

                shader.SetUniform2("tiling", ref tiling);
                shader.SetUniform("Mask", 2);
                GL.ActiveTexture(TextureUnit.Texture2);

                if (Mask != null)
                {
                    UseMask = true;
                    Mask.Bind();
                }

                shader.SetUniform("useMask", UseMask);

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
