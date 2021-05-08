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
            if (!Parent.Visible) return;

            OnBeforeDraw(this);

            Matrix4 model = Parent.ModelMatrix;
            Vector2 offset = Parent.AnchoredPosition;
          
            Shader.Use();
            Shader.SetUniform2("offset", ref offset);
            Shader.SetUniformMatrix4("projectionMatrix", ref projection);
            Shader.SetUniformMatrix4("modelMatrix", ref model);

            renderer?.Draw();
        }

        public override void Invalidate()
        { 
            renderer?.Update();   
        }

        public override void Dispose()
        {
            base.Dispose();
            renderer?.Dispose();
        }
    }
}
