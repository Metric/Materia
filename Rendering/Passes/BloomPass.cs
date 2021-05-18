using Materia.Rendering.Textures;
using Materia.Rendering.Imaging.Processing;
using Materia.Rendering.Geometry;
using Materia.Rendering.Interfaces;
using Materia.Rendering.Mathematics;
using Materia.Rendering.Shaders;
using System;
using Materia.Rendering.Buffers;
using MLog;

namespace Materia.Rendering.Passes
{
    public class BloomPass : RenderStackItem
    {
        BlurProcessor blur;
        FullScreenQuad quad;
        IGLProgram shader;
        GLFrameBuffer frame;

        public float Intensity
        {
            get; set;
        }

        public BloomPass(GLFrameBuffer frameBuffer)
        {
            frame = frameBuffer;
            Intensity = 8;
            quad = new FullScreenQuad();
            blur = new BlurProcessor();
            shader = GLShaderCache.GetShader("raw.glsl", "bloom.glsl");
        }

        public override void Dispose()
        {
            blur?.Dispose();
            blur = null;

            quad?.Dispose();
            quad = null;
        }

        public override void Render(GLTexture2D[] inputs, out GLTexture2D[] outputs, Action<RenderStackState> renderScene = null)
        {
            outputs = inputs;

            if (frame == null) return;
            if (shader == null) return;
            if (inputs == null) return;
            if (inputs.Length < 4) return;

            FullScreenQuad.SharedVao?.Bind();

            blur.PrepareView(inputs[2]);
            blur.Intensity = Intensity;
            blur.Process(inputs[1]);
            blur.Complete();

            frame.Bind();
            frame.AttachColor(inputs[3], 0);

            if (!frame.IsValid)
            {
                Log.Error("Invalid frame buffer");
            }

            int width = inputs[3].Width;
            int height = inputs[3].Height;

            IGL.Primary.DrawBuffers(new int[] { (int)DrawBuffersEnum.ColorAttachment0 });
            IGL.Primary.Viewport(0, 0, width, height);
            IGL.Primary.ClearColor(0, 0, 0, 0);
            IGL.Primary.Clear((int)ClearBufferMask.ColorBufferBit | (int)ClearBufferMask.DepthBufferBit | (int)ClearBufferMask.StencilBufferBit);

            Vector2 tiling = new Vector2(1);

            shader.Use();
            shader.SetUniform("MainTex", 0);
            shader.SetUniform("Bloom", 1);
            shader.SetUniform2("tiling", ref tiling);

            IGL.Primary.ActiveTexture((int)TextureUnit.Texture0);
            inputs[0].Bind();

            IGL.Primary.ActiveTexture((int)TextureUnit.Texture1);
            inputs[2].Bind();

            //ensure polygon is actually rendered instead of wireframe during this step
            IGL.Primary.PolygonMode((int)MaterialFace.FrontAndBack, (int)PolygonMode.Fill);
            IGL.Primary.Disable((int)EnableCap.DepthTest);
            IGL.Primary.Disable((int)EnableCap.CullFace);

            quad.Draw();

            IGL.Primary.Enable((int)EnableCap.CullFace);
            IGL.Primary.Enable((int)EnableCap.DepthTest);

            GLTexture2D.Unbind();

            frame.Unbind();

            FullScreenQuad.SharedVao?.Unbind();
        }
    }
}
