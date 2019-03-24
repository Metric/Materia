using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using Materia.Shaders;
using Materia.Textures;

namespace Materia.Imaging.GLProcessing
{
    public class PreviewProcessor
    {
        public Matrix4 Model { get; set; }
        public Matrix4 Projection { get; set; }
        public Matrix4 View { get; set; }

        GLShaderProgram shader;
        public PreviewProcessor()
        {
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
                GL.ActiveTexture(TextureUnit.Texture0);

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
