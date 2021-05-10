using Materia.Rendering;
using System;
using System.Collections.Generic;
using System.Text;

namespace MateriaCore.Components.GL.Renderer
{
    public class SceneObject : Transform, IDisposable
    {
        public string Id { get; protected set; } = Guid.NewGuid().ToString();
        public bool Visible { get; set; } = true;

        public virtual void Dispose() { }

        public virtual void Draw() { }
    }
}
