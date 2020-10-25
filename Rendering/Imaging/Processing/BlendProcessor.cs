using System;
using Materia.Rendering.Textures;
using Materia.Rendering.Interfaces;
using Materia.Rendering.Mathematics;

namespace Materia.Rendering.Imaging.Processing
{
    public class BlendProcessor : ImageProcessor
    {
        public int BlendMode
        {
            get; set;
        }

        public int AlphaMode
        {
            get; set;
        }

        public float Alpha { get; set; }

        IGLProgram shader;

        public BlendProcessor() : base()
        {
            shader = GetShader("image.glsl", "blend.glsl");
            AlphaMode = 0;
        }

        public void Process(int width, int height, GLTexture2D tex, GLTexture2D tex2, GLTexture2D mask, GLTexture2D output)
        {
            base.Process(width, height, tex, output);

            if (shader != null)
            {
                GLTexture2D tempColor = new GLTexture2D(tex.InternalFormat);
                GLTexture2D tempColor2 = new GLTexture2D(tex2.InternalFormat);

                tempColor.Bind();
                tempColor.SetData(IntPtr.Zero, PixelFormat.Rgba, tex.Width, tex.Height);
                if (tex.InternalFormat == PixelInternalFormat.R16f || tex.InternalFormat == PixelInternalFormat.R32f)
                {
                    tempColor.SetSwizzleLuminance();
                }
                else if(tex.IsRGBBased)
                {
                    tempColor.SetSwizzleRGB();
                }
                tempColor.SetFilter((int)TextureMinFilter.Linear, (int)TextureMagFilter.Linear);
                GLTexture2D.Unbind();
                tempColor2.Bind();
                tempColor2.SetData(IntPtr.Zero, PixelFormat.Rgba, tex2.Width, tex2.Height);
                if (tex2.InternalFormat == PixelInternalFormat.R16f || tex2.InternalFormat == PixelInternalFormat.R32f)
                {
                    tempColor2.SetSwizzleLuminance();
                }
                else if (tex.IsRGBBased)
                {
                    tempColor.SetSwizzleRGB();
                }
                tempColor2.SetFilter((int)TextureMinFilter.Linear, (int)TextureMagFilter.Linear);
                GLTexture2D.Unbind();

                ResizeViewTo(tex, tempColor, tex.Width, tex.Height, width, height);
                tex = tempColor;
                IGL.Primary.Clear((int)(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit));
                
                ResizeViewTo(tex2, tempColor2, tex2.Width, tex2.Height, width, height);
                tex2 = tempColor2;
                IGL.Primary.Clear((int)(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit));
                
                if (mask != null)
                {
                    ResizeViewTo(mask, output, mask.Width, mask.Height, width, height);
                    mask = output;
                    IGL.Primary.Clear((int)(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit));
                }

                Vector2 tiling = new Vector2(TileX, TileY);

                shader.Use();
                shader.SetUniform2("tiling", ref tiling);
                shader.SetUniform("Foreground", 0);
                IGL.Primary.ActiveTexture((int)TextureUnit.Texture0);
                tex.Bind();
                shader.SetUniform("Background", 1);
                IGL.Primary.ActiveTexture((int)TextureUnit.Texture1);
                tex2.Bind();
                shader.SetUniform("blendMode", BlendMode);
                shader.SetUniform("alpha", Alpha);
                shader.SetUniform("alphaMode", AlphaMode);

                if (mask != null)
                {
                    shader.SetUniform("Mask", 2);
                    IGL.Primary.ActiveTexture((int)TextureUnit.Texture2);
                    shader.SetUniform("hasMask", 1);
                    mask.Bind();
                }
                else
                {
                    shader.SetUniform("hasMask", 0);
                }

                renderQuad?.Draw();

                GLTexture2D.Unbind();
                //output.Bind();
                //output.CopyFromFrameBuffer(width, height);
                //GLTexture2D.Unbind();
                Blit(output, width, height);

                tempColor.Dispose();
                tempColor2.Dispose();
            }
        }

        public override void Process(int width, int height, GLTexture2D tex, GLTexture2D output)
        {
            base.Process(width, height, tex, output);

            if (shader != null)
            {
                Vector2 tiling = new Vector2(TileX, TileY);

                shader.Use();
                shader.SetUniform2("tiling", ref tiling);
                shader.SetUniform("Foreground", 0);
                IGL.Primary.ActiveTexture((int)TextureUnit.Texture0);
                tex.Bind();

                if (renderQuad != null)
                {
                    renderQuad.Draw();
                }

                GLTexture2D.Unbind();
                //output.Bind();
                //output.CopyFromFrameBuffer(width, height);
                //GLTexture2D.Unbind();
                Blit(output, width, height);
            }
        }
    }
}
