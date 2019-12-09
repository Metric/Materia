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
    public class DirectionalWarpProcessor : ImageProcessor
    {
        public float Angle { get; set; }
        public float Intensity { get; set; }

        IGLProgram shader;

        public DirectionalWarpProcessor() : base()
        {
            shader = GetShader("image.glsl", "warpdirectional.glsl");
            Intensity = 1;
        }

        public void Process(int width, int height, GLTextuer2D tex, GLTextuer2D warp, GLTextuer2D output)
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
                shader.SetUniform("angle", Angle);
                shader.SetUniform("intensity", Intensity);
                IGL.Primary.ActiveTexture((int)TextureUnit.Texture0);
                tex.Bind();
                tex.Repeat();
                shader.SetUniform("Warp", 1);
                IGL.Primary.ActiveTexture((int)TextureUnit.Texture1);
                warp.Bind();

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
