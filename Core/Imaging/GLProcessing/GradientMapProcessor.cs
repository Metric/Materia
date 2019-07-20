using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Materia.Shaders;
using Materia.Textures;
using Materia.Math3D;
using Materia.GLInterfaces;

namespace Materia.Imaging.GLProcessing
{
    public class GradientMapProcessor : ImageProcessor
    {
        public GLTextuer2D ColorLUT { get; set; }
        public GLTextuer2D Mask { get; set; }
        public bool UseMask { get; set; }

        IGLProgram shader;

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
                IGL.Primary.Clear((int)(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit));

                Vector2 tiling = new Vector2(TileX, TileY);

                shader.Use();

                shader.SetUniform("MainTex", 0);
                IGL.Primary.ActiveTexture((int)TextureUnit.Texture0);
                tex.Bind();

                shader.SetUniform("ColorLUT", 1);
                IGL.Primary.ActiveTexture((int)TextureUnit.Texture1);
                ColorLUT.Bind();

                shader.SetUniform2("tiling", ref tiling);
                shader.SetUniform("Mask", 2);
                IGL.Primary.ActiveTexture((int)TextureUnit.Texture2);

                if (Mask != null)
                {
                    UseMask = true;
                    Mask.Bind();
                }
                else
                {
                    UseMask = false;
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
