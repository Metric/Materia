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
    public class TransformProcessor : ImageProcessor
    {
        public Matrix3 Rotation { get; set; }
        public Matrix3 Scale { get; set; }
        public Vector3 Translation { get; set; }

        IGLProgram shader;

        public TransformProcessor() : base()
        {
            Stretch = false;
            shader = GetShader("image.glsl", "transform.glsl");
        }

        public override void Process(int width, int height, GLTextuer2D tex, GLTextuer2D output)
        {
            base.Process(width, height, tex, output);

            if (shader != null)
            {
                ResizeViewTo(tex, output, tex.Width, tex.Height, width, height);
                tex = output;
                IGL.Primary.Clear((int)(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit));

                Matrix3 rot = Rotation;
                Matrix3 sc = Scale;
                Vector3 tr = Translation;

                Vector2 tiling = new Vector2(TileX, TileY);

                shader.Use();
                shader.SetUniform2("tiling", ref tiling);
                shader.SetUniform("MainTex", 0);
                IGL.Primary.ActiveTexture((int)TextureUnit.Texture0);
                tex.Bind();
                shader.SetUniformMatrix3("rotation", ref rot);
                shader.SetUniformMatrix3("scale", ref sc);
                shader.SetUniform3("translation", ref tr);

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
