using Materia.Rendering.Textures;
using Materia.Rendering.Imaging.Processing;
using Materia.Rendering.Geometry;
using Materia.Rendering.Interfaces;
using Materia.Rendering.Mathematics;
using Materia.Rendering.Shaders;

namespace Materia.Rendering.Passes
{
    public class BloomPass : RenderStackItem
    {
        BlurProcessor blur;
        FullScreenQuad quad;
        IGLProgram shader;

        int width;
        int height;

        public float Intensity
        {
            get; set;
        }

        public BloomPass(int w, int h)
        {
            Intensity = 8;
            width = w;
            height = h;
            quad = new FullScreenQuad();
            blur = new BlurProcessor();
            shader = GLShaderCache.GetShader("image.glsl", "bloom.glsl");
        }

        public void Update(int w, int h)
        {
            width = w;
            height = h;
        }

        public override void Dispose()
        {
            blur?.Dispose();
            blur = null;

            quad?.Dispose();
            quad = null;
        }

        public override void Render(GLTexture2D[] inputs, out GLTexture2D[] outputs)
        {
            outputs = null;

            if (shader == null) return;

            if (inputs == null) return;

            if (inputs.Length < 2) return;

            FullScreenQuad.SharedVao?.Bind();

            blur.Intensity = Intensity;
            blur.Process(width, height, inputs[1], inputs[1]);
            blur.Complete();

            Vector2 tiling = new Vector2(1);

            shader.Use();
            shader.SetUniform("MainTex", 0);
            shader.SetUniform("Bloom", 1);
            shader.SetUniform2("tiling", ref tiling);

            IGL.Primary.ActiveTexture((int)TextureUnit.Texture0);
            inputs[0].Bind();

            IGL.Primary.ActiveTexture((int)TextureUnit.Texture1);
            inputs[1].Bind();

            //ensure polygon is actually rendered instead of wireframe during this step
            IGL.Primary.PolygonMode((int)MaterialFace.FrontAndBack, (int)PolygonMode.Fill);

            IGL.Primary.Disable((int)EnableCap.CullFace);

            quad.Draw();

            IGL.Primary.Enable((int)EnableCap.CullFace);

            GLTexture2D.Unbind();

            FullScreenQuad.SharedVao?.Unbind();
        }
    }
}
