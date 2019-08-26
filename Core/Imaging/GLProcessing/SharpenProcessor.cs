using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Materia.GLInterfaces;
using Materia.Textures;
using Materia.Math3D;

namespace Materia.Imaging.GLProcessing
{
    public class SharpenProcessor : ImageProcessor
    {
        public float Intensity { get; set; }

        IGLProgram shader;

        public SharpenProcessor()
        {
            shader = GetShader("image.glsl", "sharpen.glsl");
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

                shader.SetUniform("intensity", Intensity);
                shader.SetUniform2("tiling", ref tiling);


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
