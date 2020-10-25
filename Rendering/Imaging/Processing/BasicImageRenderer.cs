﻿using Materia.Rendering.Textures;
using Materia.Rendering.Interfaces;
using Materia.Rendering.Mathematics;

namespace Materia.Rendering.Imaging.Processing
{
    public class BasicImageRenderer : ImageProcessor
    {
        IGLProgram shader;

        public BasicImageRenderer() : base()
        {
            shader = GetShader("image.glsl", "image-basic.glsl");
        }

        /// <summary>
        /// This is used to simply render to frame buffer
        /// at the specified width and height
        /// it is used primarily to pull preview images
        /// from the stored textures
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="tex"></param>
        public void Process(int width, int height, GLTexture2D tex)
        {
            base.Process(width, height, tex, null);

            if (shader != null)
            {
                Vector2 tiling = new Vector2(TileX, TileY);

                shader.Use();
                shader.SetUniform2("tiling", ref tiling);
                shader.SetUniform("MainTex", 0);
                IGL.Primary.ActiveTexture((int)TextureUnit.Texture0);
                tex?.Bind();

                renderQuad?.Draw();

                GLTexture2D.Unbind();
            }
        }

        public override void Process(int width, int height, GLTexture2D tex, GLTexture2D output)
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
                IGL.Primary.ActiveTexture((int)TextureUnit.Texture0);
                tex?.Bind();

                renderQuad?.Draw();

                GLTexture2D.Unbind();

                Blit(output, width, height);
            }
        }
    }
}
