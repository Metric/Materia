using InfinityUI.Core;
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

        public UIImage() : base()
        {
            Texture = UI.DefaultWhite;
        }

        public override void Draw(DrawEvent e)
        {
            StencilStage = UIRenderer.StencilStage;

            if (Parent == null || Texture == null) return;
            if (!Parent.Visible) return;
            if (Shader == null) return;

            //basically if we are outside the clipping bounds of the
            //parent that is clipping, if there is one, then do not render
            if (!IsInClipBounds())
            {
                return;
            }

            OnBeforeDraw(this);

            Vector2 size = Parent.WorldSize;

            if (size.X <= float.Epsilon || size.Y <= float.Epsilon) return;

            Matrix4 m = Parent.WorldMatrix;
            Vector2 pos = Parent.WorldPosition;
            Vector4 color = Color;
            Vector2 tiling = Tiling;
            Vector2 offset = Offset;
            Matrix4 proj = e.projection;

            Shader.Use();
            Shader.SetUniformMatrix4("projectionMatrix", ref proj);
            Shader.SetUniformMatrix4("modelMatrix", ref m);
            Shader.SetUniform2("position", ref pos);
            Shader.SetUniform2("size", ref size);
            Shader.SetUniform4("color", ref color);
            Shader.SetUniform("MainTex", 0);
            Shader.SetUniform("flipY", FlipY ? 1 : 0);
            Shader.SetUniform2("tiling", ref tiling);
            Shader.SetUniform2("uvoffset", ref offset);

            IGL.Primary.ActiveTexture((int)TextureUnit.Texture0);
            Texture.Bind();

            AdjustStencil(e);

            if (!Clip)
            {
                UIRenderer.Draw();
                return;
            }

            UIRenderer.StencilStage++;

            //wrap stencil stage as max is 255 for stencil
            UIRenderer.StencilStage %= 255;

            StencilStage = UIRenderer.StencilStage;

            IGL.Primary.StencilOp((int)StencilOp.Keep, (int)StencilOp.Keep, (int)StencilOp.Replace);
            IGL.Primary.StencilFunc((int)StencilFunction.Always, UIRenderer.StencilStage, UIRenderer.StencilStage);

            UIRenderer.Draw();

            IGL.Primary.StencilFunc((int)StencilFunction.Equal, UIRenderer.StencilStage, UIRenderer.StencilStage);
        }
    }
}
