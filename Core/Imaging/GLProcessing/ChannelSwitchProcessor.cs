using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Materia.Textures;
using Materia.Shaders;
using Materia.Math3D;
using Materia.GLInterfaces;

namespace Materia.Imaging.GLProcessing
{
    public class ChannelSwitchProcessor : ImageProcessor
    {
        public int RedChannel { get; set; }
        public int GreenChannel { get; set; }
        public int BlueChannel { get; set; }
        public int AlphaChannel { get; set; }

        IGLProgram shader;

        public ChannelSwitchProcessor() : base()
        {
            shader = GetShader("image.glsl", "channelswitch.glsl");
            
        }

        public void Process(int width, int height, GLTextuer2D tex, GLTextuer2D other, GLTextuer2D output)
        {
            base.Process(width, height, tex, output);

            if (shader != null)
            {
                GLTextuer2D tempColor = new GLTextuer2D(tex.InternalFormat);
                tempColor.Bind();
                tempColor.SetData(IntPtr.Zero, PixelFormat.Rgba, tex.Width, tex.Height);
                if (tex.InternalFormat == PixelInternalFormat.R16f || tex.InternalFormat == PixelInternalFormat.R32f)
                {
                    tempColor.SetSwizzleLuminance();
                }
                tempColor.SetFilter((int)TextureMinFilter.Linear, (int)TextureMagFilter.Linear);
                GLTextuer2D.Unbind();

                ResizeViewTo(tex, tempColor, tex.Width, tex.Height, width, height);
                tex = tempColor;
                IGL.Primary.Clear((int)(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit));

                ResizeViewTo(other, output, other.Width, other.Height, width, height);
                other = output;
                IGL.Primary.Clear((int)(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit));


                Vector2 tiling = new Vector2(TileX, TileY);

                shader.Use();
                shader.SetUniform2("tiling", ref tiling);

                shader.SetUniform("redChannel", RedChannel);
                shader.SetUniform("greenChannel", GreenChannel);
                shader.SetUniform("blueChannel", BlueChannel);
                shader.SetUniform("alphaChannel", AlphaChannel);

                shader.SetUniform("MainTex", 0);
                IGL.Primary.ActiveTexture((int)TextureUnit.Texture0);
                tex.Bind();
                shader.SetUniform("Other", 1);
                IGL.Primary.ActiveTexture((int)TextureUnit.Texture1);
                other.Bind();

                if (renderQuad != null)
                {
                    renderQuad.Draw();
                }

                GLTextuer2D.Unbind();
                Blit(output, width, height);
                tempColor.Release();
            }
        }
    }
}
