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
    public class ChannelSwitchProcessor : ImageProcessor
    {
        public int RedChannel { get; set; }
        public int GreenChannel { get; set; }
        public int BlueChannel { get; set; }
        public int AlphaChannel { get; set; }

        GLShaderProgram shader;

        public ChannelSwitchProcessor() : base()
        {
            shader = GetShader("image.glsl", "channelswitch.glsl");
            
        }

        public void Process(int width, int height, GLTextuer2D tex, GLTextuer2D other, GLTextuer2D output)
        {
            base.Process(width, height, tex, output);

            if (shader != null)
            {
                Vector2 tiling = new Vector2(TileX, TileY);

                shader.Use();
                shader.SetUniform2("tiling", ref tiling);

                shader.SetUniform("redChannel", RedChannel);
                shader.SetUniform("greenChannel", GreenChannel);
                shader.SetUniform("blueChannel", BlueChannel);
                shader.SetUniform("alphaChannel", AlphaChannel);

                shader.SetUniform("MainTex", 0);
                GL.ActiveTexture(TextureUnit.Texture0);
                tex.Bind();
                shader.SetUniform("Other", 1);
                GL.ActiveTexture(TextureUnit.Texture1);
                other.Bind();

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
