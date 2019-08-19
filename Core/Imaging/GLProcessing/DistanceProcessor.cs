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
    public class DistanceProcessor : ImageProcessor
    {
        IGLProgram shader;

        public float Distance { get; set; }

        public DistanceProcessor() : base()
        {
            shader = GetShader("image.glsl", "distance.glsl");
        }

        public void Process(int width, int height, GLTextuer2D tex, GLTextuer2D other, GLTextuer2D output)
        {
            base.Process(width, height, tex, output);

            if (shader != null)
            {
                ResizeViewTo(tex, output, tex.Width, tex.Height, width, height);
                tex = output;
                IGL.Primary.Clear((int)(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit));

                Vector2 tiling = new Vector2(TileX, TileY);
                shader.Use();
                shader.SetUniform2("tiling", ref tiling);
                shader.SetUniform("MainTex", 0);
                shader.SetUniform("maxDistance", Distance);
                IGL.Primary.ActiveTexture((int)TextureUnit.Texture0);
                tex.Bind();
                //shader.SetUniform("Other", 1);
                //IGL.Primary.ActiveTexture((int)TextureUnit.Texture1);
                //other.Bind();

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
