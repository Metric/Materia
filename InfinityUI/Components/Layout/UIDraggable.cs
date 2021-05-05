using InfinityUI.Core;
using InfinityUI.Interfaces;
using Materia.Rendering.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;

namespace InfinityUI.Components.Layout
{
    public class UIDraggable : IComponent, IMouseInput
    {
        bool isMouseDown;

        public Axis DragAxis { get; set; } = Axis.Both;

        public UIObject Parent { get; set; }

        public virtual void Awake()
        {
            if (Parent == null) return;
            Parent.RaycastTarget = true;
        }

        public virtual void Dispose()
        {
            
        }

        public virtual void OnMouseClick(MouseEventArgs e)
        {
           
        }

        public virtual void OnMouseDown(MouseEventArgs e)
        {
            if (e.IsHandled) return;
            if (!e.Button.HasFlag(MouseButton.Left)) return;
            e.IsHandled = true;
            isMouseDown = true;
        }

        public virtual void OnMouseEnter(MouseEventArgs e)
        {
            
        }

        public virtual void OnMouseLeave(MouseEventArgs e)
        {
            if (e.IsHandled) return;
            e.IsHandled = true;
            isMouseDown = false;
        }

        public virtual void OnMouseMove(MouseEventArgs e)
        {
            if (Parent == null) return;
            if (e.IsHandled) return;
            if (!isMouseDown) return;
            e.IsHandled = true;

            switch (DragAxis) 
            {
                case Axis.Both:
                    Parent.Position += e.Delta;
                    break;
                case Axis.Horizontal:
                    Parent.Position += new Vector2(e.Delta.X, 0);
                    break;
                case Axis.Vertical:
                    Parent.Position += new Vector2(0, e.Delta.Y);
                    break;
            }
        }

        public virtual void OnMouseUp(MouseEventArgs e)
        {
            if (e.IsHandled) return;
            if (!e.Button.HasFlag(MouseButton.Left)) return;
            e.IsHandled = true;
            isMouseDown = false;
        }
    }
}
