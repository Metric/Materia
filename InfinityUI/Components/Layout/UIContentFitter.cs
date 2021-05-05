using InfinityUI.Core;
using InfinityUI.Interfaces;
using Materia.Rendering.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;

namespace InfinityUI.Components.Layout
{
    public class UIContentFitter : IComponent, ILayout
    {
        public UIObject Parent { get; set; }

        public Axis Axis { get; set; } = Axis.Both;

        public virtual void Awake()
        {
            Invalidate();
            AddEvents();
        }

        protected void AddEvents()
        {
            if (Parent == null) return;
            var children = Parent.Children;

            for (int i = 0; i < children.Count; ++i)
            {
                var child = children[i];
                if (child == null) continue;
                child.Resize += Child_Resize;
            }

            Parent.ChildAdded += Parent_ChildAdded;
            Parent.ChildRemoved += Parent_ChildRemoved;
        }

        private void Parent_ChildRemoved(UIObject child)
        {
            child.Resize -= Child_Resize;

            //have to invalidate to get true area
            Invalidate();
        }

        private void Parent_ChildAdded(UIObject child)
        {
            child.Resize += Child_Resize;
            Child_Resize(child);
        }

        private void Child_Resize(UIObject obj)
        {
            if (Parent == null) return;

            //do not have to do a full invalidate
            //we can just adjust
            Box2 area = Parent.LocalRect;
            area.Encapsulate(obj.LocalRect);
            ResizeTo(ref area);
        }

        protected void ResizeTo(ref Box2 area)
        {
            if (Parent == null) return;

            switch (Axis)
            {
                case Axis.Both:
                    Parent.Position = new Vector2(area.Left, area.Top);
                    Parent.Size = new Vector2(MathF.Abs(area.Right = area.Left), MathF.Abs(area.Bottom - area.Top));
                    break;
                case Axis.Horizontal:
                    Parent.Position = new Vector2(area.Left, Parent.Position.Y);
                    Parent.Size = new Vector2(MathF.Abs(area.Right - area.Left), Parent.Size.Y);
                    break;
                case Axis.Vertical:
                    Parent.Position = new Vector2(Parent.Position.X, area.Top);
                    Parent.Size = new Vector2(Parent.Size.X, MathF.Abs(area.Bottom - area.Top));
                    break;
            }
        }

        protected void RemoveEvents()
        {
            if (Parent == null) return;
            Parent.ChildAdded -= Parent_ChildAdded;
            Parent.ChildRemoved -= Parent_ChildRemoved;
        }

        public virtual void Invalidate()
        {
            if (Parent == null) return;
            var children = Parent.Children;

            Box2 area = new Box2(0,0,0,0);

            for (int i = 0; i < children.Count; ++i)
            {
                var child = children[i];
                area.Encapsulate(child.LocalRect);
            }

            ResizeTo(ref area);
        }

        public virtual void Dispose()
        {
            RemoveEvents();
        }
    }
}
