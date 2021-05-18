using Materia.Rendering;
using System;
using System.Collections.Generic;
using System.Text;

namespace MateriaCore.Components.GL.Renderer
{
    public class SceneObject : Transform, IDisposable
    {
        public string Id { get; protected set; } = Guid.NewGuid().ToString();

        private bool visible = true;
        public bool Visible
        {
            get => visible;
            set
            {
                if (visible != value)
                {
                    visible = value;
                    UpdateChildrenVisibility();
                }
            }
        }

        private void UpdateChildrenVisibility()
        {
            for (int i = 0; i < Children.Count; ++i)
            {
                var m = Children[i] as SceneObject;
                if (m == null) continue;
                m.Visible = visible;
            }
        }

        public virtual void Dispose() { }

        public virtual void Draw() { }
    }
}
