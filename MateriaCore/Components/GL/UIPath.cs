using InfinityUI.Components;
using InfinityUI.Core;
using InfinityUI.Interfaces;
using Materia.Rendering.Mathematics;
using Materia.Rendering.Shaders;
using Materia.Rendering.Geometry;
using System;
using System.Collections.Generic;
using System.Text;

namespace MateriaCore.Components.GL
{
    public class UIPath : UIDrawable
    {
        LineRenderer renderer;

        public override void Awake()
        {
            base.Awake();
            
            if (Parent != null)
            {
                Parent.RaycastTarget = false;
            }

            renderer = new LineRenderer();
            Shader = GLShaderCache.GetShader("line.glsl", "line.glsl");
        }

        public void Set(List<Line> lines)
        {
            if (renderer == null) return;
            renderer.Lines.Clear();
            renderer.Lines.AddRange(lines);
            renderer.Update();
        }

        public override void Draw(Matrix4 projection)
        {
            if (Shader == null) return;
            if (Parent == null) return;
            if (renderer == null) return;
            if (!Parent.Visible) return;

            OnBeforeDraw(this);

            Matrix4 m = Parent.WorldMatrix;
            Vector2 pos = Parent.WorldPosition;

            Shader.Use();
            Shader.SetUniform2("offset", ref pos);
            Shader.SetUniformMatrix4("projectionMatrix", ref projection);
            Shader.SetUniformMatrix4("modelMatrix", ref m);

            LineRenderer.SharedVao.Bind();

            renderer?.Draw();

            UIRenderer.Bind();
        }

        public override void Invalidate()
        {
            if (!NeedsUpdate) return;
            renderer?.Update();   
        }

        public override void Dispose()
        {
            base.Dispose();
            renderer?.Dispose();
        }
    }
}
