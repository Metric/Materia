using InfinityUI.Core;
using InfinityUI.Interfaces;
using Materia.Rendering.Interfaces;
using Materia.Rendering.Mathematics;
using Materia.Rendering.Shaders;
using System;
using System.Collections.Generic;
using System.Text;

namespace InfinityUI.Components
{
    public class UIDrawable : IComponent, IDrawable
    {
        public bool NeedsUpdate { get; set; }

        public event Action<UIDrawable> BeforeDraw;

        public UIObject Parent { get; set; }

        public Vector2 Tiling { get; set; } = Vector2.One;
        public Vector2 Offset { get; set; } = Vector2.Zero;

        public bool FlipY { get; set; } = false;
        public bool Clip { get; set; } = false;

        public IGLProgram Shader { get; set; } = GLShaderCache.GetShader("pointui.glsl", "pointui.glsl", "pointui.glsl");
        public Vector4 Color { get; set; } = Vector4.One;

        protected virtual bool IsInClipBounds()
        {
            if (Parent == null) return false;
            var p = Parent.ClippingParent;
            if (p != null)
            {
                return p.Rect.Intersects(Parent.Rect);
            }
            return true;
        }

        protected virtual void AdjustStencil(DrawEvent e)
        {
            bool adjustStencil = e.previous != null && e.previous.Parent == Parent.Parent && e.previous.IsClipped;

            if (adjustStencil)
            {
                UIRenderer.StencilStage--;

                if (UIRenderer.StencilStage < 0)
                {
                    UIRenderer.StencilStage = 0;
                }

                IGL.Primary.StencilFunc((int)StencilFunction.Equal, UIRenderer.StencilStage, UIRenderer.StencilStage);
                IGL.Primary.StencilMask(0x00);
            }
        }

        public virtual void Awake()
        {
            if (Parent == null) return;
            Parent.RaycastTarget = true;
        }

        public virtual void Dispose()
        {

        }

        public virtual void Draw(DrawEvent e)
        {

        }

        public virtual void Invalidate()
        {

        }

        protected void OnBeforeDraw(UIDrawable d)
        {
            BeforeDraw?.Invoke(d);
        }

        public virtual void Update()
        {

        }
    }
}
