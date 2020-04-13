using Materia.Rendering.Textures;
using Materia.Rendering.Interfaces;
using Materia.Rendering.Shaders;
using Materia.Rendering.Mathematics;

namespace Materia.Rendering.Imaging.Processing
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
            shader = GLShaderCache.GetShader("preview.glsl", "preview.glsl");
        }

        public void Bind(GLTexture2D tex)
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
            GLTexture2D.Unbind();
        }
    }
}
