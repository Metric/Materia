using InfinityUI.Core;
using InfinityUI.Interfaces;
using Materia.Rendering.Mathematics;
using System;
using System.Collections.Generic;
using Materia.Rendering.Geometry;
using System.Text;

namespace MateriaCore.Components.GL
{
    public class UIPathPreview : IComponent
    {
        protected UINodePoint previousPoint;

        public UIObject Parent { get; set; }
        public bool NeedsUpdate { get; set; }

        #region sub components
        protected UINodePath path;
        #endregion

        public void Awake()
        {
            if (Parent == null) return;
            path = new UINodePath
            {
                RelativeTo = Anchor.TopLeft,
                ZOrder = -99
            };
            Parent.AddChild(path);
        }

        public void Dispose()
        {
            
        }

        public void Update()
        {
            if (Parent == null) return;

            if (Parent.Canvas != null)
            {
                path.SecondaryPoint = Parent.Canvas.ToCanvasSpace(UI.MousePosition);
            }

            if (UINodePoint.SelectedOrigin != previousPoint && previousPoint == null)
            {
                path.Visible = true;
                path.Invalidate(); //we must invalidate on the same frame
            }
            else if (UINodePoint.SelectedOrigin == null && previousPoint != null)
            {
                path.Visible = false;
            }

            path.PrimaryPoint = previousPoint = UINodePoint.SelectedOrigin;
        }
    }
}
