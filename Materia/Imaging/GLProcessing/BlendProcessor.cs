using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Materia.Textures;
using OpenTK.Graphics.OpenGL;
using Materia.Shaders;
using OpenTK;

namespace Materia.Imaging.GLProcessing
{
    public class BlendProcessor : ImageProcessor
    {
        public int BlendMode
        {
            get; set;
        }

        public float Alpha { get; set; }

        GLShaderProgram shader;

        public BlendProcessor() : base()
        {
            shader = GetShader("image.glsl", "blend.glsl");
        }

        public void Process(int width, int height, GLTextuer2D tex, GLTextuer2D tex2, GLTextuer2D mask, GLTextuer2D output)
        {
            base.Process(width, height, tex, output);

            if (shader != null)
            {
                GLTextuer2D tempColor = new GLTextuer2D(PixelInternalFormat.Rgba);
                GLTextuer2D tempColor2 = new GLTextuer2D(PixelInternalFormat.Rgba);

                tempColor.Bind();
                tempColor.SetFilter((int)TextureMinFilter.Linear, (int)TextureMagFilter.Linear);
                GLTextuer2D.Unbind();
                tempColor2.Bind();
                tempColor2.SetFilter((int)TextureMinFilter.Linear, (int)TextureMagFilter.Linear);
                GLTextuer2D.Unbind();

                ResizeViewTo(tex, tempColor, tex.Width, tex.Height, width, height);
                tex = tempColor;
                GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
                
                ResizeViewTo(tex2, tempColor2, tex2.Width, tex2.Height, width, height);
                tex2 = tempColor2;
                GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
                
                if (mask != null)
                {
                    ResizeViewTo(mask, output, mask.Width, mask.Height, width, height);
                    mask = output;
                    GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
                }

                Vector2 tiling = new Vector2(TileX, TileY);

                shader.Use();
                shader.SetUniform2("tiling", ref tiling);
                shader.SetUniform("Foreground", 0);
                GL.ActiveTexture(TextureUnit.Texture0);
                tex.Bind();
                shader.SetUniform("Background", 1);
                GL.ActiveTexture(TextureUnit.Texture1);
                tex2.Bind();
                shader.SetUniform("blendMode", BlendMode);
                shader.SetUniform("alpha", Alpha);

                if (mask != null)
                {
                    shader.SetUniform("Mask", 2);
                    GL.ActiveTexture(TextureUnit.Texture2);
                    shader.SetUniform("hasMask", 1);
                    mask.Bind();
                }
                else
                {
                    shader.SetUniform("hasMask", 0);
                }

                if (renderQuad != null)
                {
                    renderQuad.Draw();
                }

                GLTextuer2D.Unbind();
                output.Bind();
                output.CopyFromFrameBuffer(width, height);
                GLTextuer2D.Unbind();

                tempColor.Release();
                tempColor2.Release();
            }
        }

        public override void Process(int width, int height, GLTextuer2D tex, GLTextuer2D output)
        {
            base.Process(width, height, tex, output);

            if (shader != null)
            {
                Vector2 tiling = new Vector2(TileX, TileY);

                shader.Use();
                shader.SetUniform2("tiling", ref tiling);
                shader.SetUniform("Foreground", 0);
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
