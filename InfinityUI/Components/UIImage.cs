﻿using InfinityUI.Core;
using InfinityUI.Interfaces;
using Materia.Rendering.Textures;
using Materia.Rendering.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;
using Materia.Rendering.Interfaces;
using Materia.Rendering.Shaders;

namespace InfinityUI.Components
{
    public class UIImage : UIDrawable
    {
        public GLTexture2D Texture { get; set; }

        public override void Awake()
        {
            //set default texture to UI.DefaultWhite
            Texture = UI.DefaultWhite;
        }

        public override void Draw(Matrix4 projection)
        {
            if (Parent == null || Texture == null) return;
            if (!Parent.Visible) return;
            if (Shader == null) return;

            Vector2 size = Parent.AnchoredSize;

            if (size.X <= float.Epsilon || size.Y <= float.Epsilon) return;

            Matrix4 m = Parent.ModelMatrix;
            Vector2 pos = Parent.AnchoredPosition;
            Vector4 color = Color;

            if (Texture != null)
            {
                IGL.Primary.ActiveTexture((int)TextureUnit.Texture0);
                Texture.Bind();
            }

            Shader.Use();
            Shader.SetUniformMatrix4("projectionMatrix", ref projection);
            Shader.SetUniformMatrix4("modelMatrix", ref m);
            Shader.SetUniform2("position", ref pos);
            Shader.SetUniform2("size", ref size);
            Shader.SetUniform4("color", ref color);
            Shader.SetUniform("MainTex", 0);
            Shader.SetUniform("flipY", FlipY ? 1 : 0);

            OnBeforeDraw(this);

            if (!Clip)
            {
                UIRenderer.Draw();
                return;
            }

            UIRenderer.StencilStage++;
            //wrap stencil stage as max is 255 for stencil
            UIRenderer.StencilStage %= 255;

            IGL.Primary.StencilOp((int)StencilOp.Keep, (int)StencilOp.Keep, (int)StencilOp.Replace);

            IGL.Primary.StencilFunc((int)StencilFunction.Always, UIRenderer.StencilStage, UIRenderer.StencilStage);
            IGL.Primary.StencilMask(0xFF);

            UIRenderer.Draw();

            IGL.Primary.StencilFunc((int)StencilFunction.Equal, UIRenderer.StencilStage, UIRenderer.StencilStage);
            IGL.Primary.StencilMask(0x00);
        }
    }
}