using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Materia.Math3D;
using Materia.Shaders;
using Materia.Textures;
using Materia.GLInterfaces;

namespace Materia.Imaging.GLProcessing
{
    public class PreviewProcessor
    {
        public float Luminosity { get; set; }
        public Matrix4 Model { get; set; }
        public Matrix4 Projection { get; set; }
        public Matrix4 View { get; set; }
        public bool FlipY { get; set; }

        IGLProgram shader;
        public PreviewProcessor()
        {
            FlipY = false;
            Luminosity = 1.0f;
            shader = Material.Material.GetShader("preview.glsl", "preview.glsl");
        }

        public void Bind(GLTextuer2D tex)
        {
            if(shader != null)
            {
                Matrix4 model = Model;
                Matrix4 view = View;
                Matrix4 proj = Projection;

                shader.Use();
                shader.SetUniformMatrix4("modelMatrix", ref model);
                shader.SetUniformMatrix4("viewMatrix", ref view);
                shader.SetUniformMatrix4("projectionMatrix", ref proj);
                shader.SetUniform("MainTex", 0);
                shader.SetUniform("luminosity", Luminosity);
                shader.SetUniform("flipY", FlipY);
                IGL.Primary.ActiveTexture((int)TextureUnit.Texture0);

                if (tex != null)
                {
                    tex.Bind();
                }
            }
        }

        public void Unbind()
        {
            GLTextuer2D.Unbind();
        }
    }
}
