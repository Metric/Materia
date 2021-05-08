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
        public event Action<UIDrawable> BeforeDraw;

        public UIObject Parent { get; set; }

        public Vector2 Tiling { get; set; } = new Vector2(1, 1);
        public bool FlipY { get; set; } = false;
        public bool Clip { get; set; } = false;

        public IGLProgram Shader { get; set; } = GLShaderCache.GetShader("pointui.glsl", "pointui.glsl", "pointui.glsl");
        public Vector4 Color { get; set; } = new Vector4(1, 1, 1, 1);

        public virtual void Awake()
        {
            if (Parent == null) return;
            Parent.RaycastTarget = true;
        }

        public virtual void Dispose()
        {

        }

        public virtual void Draw(Matrix4 projection)
        {

        }

        public virtual void Invalidate()
        {

        }

        protected void OnBeforeDraw(UIDrawable d)
        {
            BeforeDraw?.Invoke(d);
        }
    }
}
