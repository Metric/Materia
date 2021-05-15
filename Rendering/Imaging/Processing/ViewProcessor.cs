using Materia.Rendering.Textures;
using Materia.Rendering.Interfaces;
using Materia.Rendering.Shaders;
using Materia.Rendering.Mathematics;
using System;

namespace Materia.Rendering.Imaging.Processing
{
    public class ViewProcessor : IDisposable
    {
        public Vector2 Tiling { get; set; } = new Vector2(1, 1);
        public float Luminosity { get; set; } = 1;
        public Matrix4 Model { get; set; } = Matrix4.Identity;
        public Matrix4 Projection { get; set; } = Matrix4.Identity;
        public Matrix4 View { get; set; } = Matrix4.Identity;
        public bool FlipY { get; set; }

        protected IGLProgram shader;
        public IGLProgram Shader { get => shader; set => shader = value; }

        protected bool isMatrixBased = false;

        public ViewProcessor()
        {
            FlipY = false;
            Luminosity = 1.0f;
            shader = GLShaderCache.GetShader("raw.glsl", "image.glsl");
        }

        public void Bind()
        {
            if(shader != null)
            {
                shader.Use();

                SetBasicUniforms();
                SetUniqueUniforms();
                SetTexturePositions();
            }
        }

        protected virtual void SetBasicUniforms()
        {
            if (!isMatrixBased) return;

            Matrix4 model = Model;
            Matrix4 view = View;
            Matrix4 proj = Projection;

            shader.SetUniformMatrix4("modelMatrix", ref model);
            shader.SetUniformMatrix4("viewMatrix", ref view);
            shader.SetUniformMatrix4("projectionMatrix", ref proj);
            shader.SetUniform("flipY", FlipY);
        }

        protected virtual void SetTexturePositions()
        {
            shader.SetUniform("MainTex", 0);
        }

        protected virtual void SetUniqueUniforms()
        {
            Vector2 tiling = Tiling;
            shader.SetUniform2("tiling", ref tiling);
            shader.SetUniform("luminosity", Luminosity);
        }

        protected virtual void SetTextures(params GLTexture2D[] textures)
        {
            if (textures == null) return;

            for (int i = 0; i < textures.Length; ++i)
            {
                IGL.Primary.ActiveTexture((int)TextureUnit.Texture0 + i);
                textures[i]?.Bind();
            }
        }

        public virtual void Unbind()
        {
            GLTexture2D.Unbind();
        }

        public virtual void Dispose() { }
    }
}
