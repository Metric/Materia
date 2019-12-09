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
    public class OcclusionProcessor : ImageProcessor
    {
        IGLProgram shader;

        public OcclusionProcessor() : base()
        {
            shader = GetShader("image.glsl", "occlusion.glsl");
        }

        public void Process(int width, int height, GLTextuer2D blur, GLTextuer2D orig, GLTextuer2D output)
        {
            base.Process(width, height, blur, output);

            if (shader != null)
            {
                GLTextuer2D tempColor = new GLTextuer2D(blur.InternalFormat);
                tempColor.Bind();
                tempColor.SetData(IntPtr.Zero, PixelFormat.Rgba, blur.Width, blur.Height);
                if (blur.InternalFormat == PixelInternalFormat.R16f || blur.InternalFormat == PixelInternalFormat.R32f)
                {
                    tempColor.SetSwizzleLuminance();
                }
                else if(blur.IsRGBBased)
                {
                    tempColor.SetSwizzleRGB();
                }
                tempColor.SetFilter((int)TextureMinFilter.Linear, (int)TextureMagFilter.Linear);
                GLTextuer2D.Unbind();

                ResizeViewTo(blur, tempColor, blur.Width, blur.Height, width, height);
                blur = tempColor;
                IGL.Primary.Clear((int)(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit));

                ResizeViewTo(orig, output, orig.Width, orig.Height, width, height);
                orig = output;
                IGL.Primary.Clear((int)(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit));

                Vector2 tiling = new Vector2(TileX, TileY);

                shader.Use();
                shader.SetUniform2("tiling", ref tiling);
                shader.SetUniform("MainTex", 0);
                IGL.Primary.ActiveTexture((int)TextureUnit.Texture0);
                blur.Bind();
                shader.SetUniform("Original", 1);
                IGL.Primary.ActiveTexture((int)TextureUnit.Texture1);
                orig.Bind();

                if (renderQuad != null)
                {
                    renderQuad.Draw();
                }

                GLTextuer2D.Unbind();
                //output.Bind();
                //output.CopyFromFrameBuffer(width, height);
                //GLTextuer2D.Unbind();

                Blit(output, width, height);
                tempColor.Release();
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
                shader.SetUniform("MainTex", 0);
                IGL.Primary.ActiveTexture((int)TextureUnit.Texture0);
                tex.Bind();

                if (renderQuad != null)
                {
                    renderQuad.Draw();
                }

                GLTextuer2D.Unbind();
                //output.Bind();
                //output.CopyFromFrameBuffer(width, height);
                //GLTextuer2D.Unbind();
                Blit(output, width, height);
            }
        }
    }
}
