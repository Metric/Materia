using Materia.Rendering.Textures;
using Materia.Rendering.Imaging.Processing;
using Materia.Rendering.Geometry;
using Materia.Rendering.Interfaces;
using Materia.Rendering.Mathematics;
using Materia.Rendering.Shaders;
using System;

namespace Materia.Rendering.Passes
{
    public class BloomPass : RenderStackItem
    {
        BlurProcessor blur;
        FullScreenQuad quad;
        GLTexture2D buffer;

        IGLProgram shader;

        public float Intensity
        {
            get; set;
        }

        public BloomPass()
        {
            Intensity = 8;
            quad = new FullScreenQuad();
            blur = new BlurProcessor();
            shader = GLShaderCache.GetShader("image.glsl", "bloom.glsl");
        }


        public override void Dispose()
        {
            blur?.Dispose();
            blur = null;

            quad?.Dispose();
            quad = null;
        }

        public override void Render(GLTexture2D[] inputs, out GLTexture2D[] outputs, Action renderScene = null)
        {
            outputs = null;

            if (shader == null) return;

            if (inputs == null) return;

            if (inputs.Length < 2) return;

            FullScreenQuad.SharedVao?.Bind();

            buffer?.Dispose();
            buffer = inputs[1].Copy();

            blur.PrepareView(buffer);
            blur.Intensity = Intensity;
            blur.Process(inputs[1]);
            blur.Complete();

            Vector2 tiling = new Vector2(1);

            shader.Use();
            shader.SetUniform("MainTex", 0);
            shader.SetUniform("Bloom", 1);
            shader.SetUniform2("tiling", ref tiling);

            IGL.Primary.ActiveTexture((int)TextureUnit.Texture0);
            inputs[0].Bind();

            IGL.Primary.ActiveTexture((int)TextureUnit.Texture1);
            buffer.Bind();

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
