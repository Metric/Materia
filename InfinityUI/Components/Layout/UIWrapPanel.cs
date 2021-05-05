using InfinityUI.Core;
using InfinityUI.Interfaces;
using Materia.Rendering.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;

namespace InfinityUI.Components.Layout
{
    public class UIWrapPanel : IComponent, ILayout
    {
        protected Orientation direction;
        public Orientation Direction
        {
            get
            {
                return direction;
            }
            set
            {
                direction = value;
                Invalidate();
            }
        }

        protected bool reverse;
        public bool Reverse
        {
            get
            {
                return reverse;
            }
            set
            {
                reverse = value;
                Invalidate();
            }
        }

        public UIObject Parent { get; set; }

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
                child.Resize += Obj_Resize;
            }

            Parent.ChildAdded += Parent_ChildAdded;
            Parent.ChildRemoved += Parent_ChildRemoved;
            Parent.Resize += Parent_Resize;
        }

        private void Parent_Resize(UIObject obj)
        {
            Invalidate();
        }

        private void Parent_ChildRemoved(UIObject obj)
        {
            if (obj == null) return;
            obj.Resize -= Obj_Resize;
            Invalidate();
        }

        private void Parent_ChildAdded(UIObject obj)
        {
            if (obj == null) return;
            obj.Resize += Obj_Resize;
            Invalidate();
        }

        private void Obj_Resize(UIObject obj)
        {
            Invalidate();
        }

        protected void RemoveEvents()
        {
            if (Parent == null) return;
            Parent.ChildAdded -= Parent_ChildAdded;
            Parent.ChildRemoved -= Parent_ChildRemoved;
            Parent.Resize -= Parent_Resize;
        }

        public virtual void Invalidate()
        {
            if (Parent == null)
            {
                return;
            }

            var Children = Parent.Children;
            var Size = Parent.Size;

            float yOffset = 0;
            float xOffset = 0;
            float maxOffset = 0;

            for (int i = 0; i < Children.Count; ++i)
            {
                if (!Children[i].Visible) continue;

                if (!reverse)
                {
                    Children[i].RelativeTo = Anchor.TopLeft;
                }
                else
                {
                    if (direction == Orientation.Horizontal)
                    {
                        Children[i].RelativeTo = Anchor.TopRight;
                    }
                    else
                    {
                        Children[i].RelativeTo = Anchor.BottomLeft;
                    }
                }

                if (direction == Orientation.Horizontal)
                {
                    if (xOffset + Children[i].Size.X > Size.X)
                    {
                        xOffset = 0;
                        yOffset += maxOffset;
                        maxOffset = Children[i].Size.Y;
                    }

                    Children[i].Position = new Vector2(xOffset, yOffset);
                    maxOffset = MathF.Max(maxOffset, Children[i].Size.Y);
                    xOffset += Children[i].Size.X;
                }
                else
                {
                    if (yOffset + Children[i].Size.Y > Size.Y)
                    {
                        yOffset = 0;
                        xOffset += maxOffset;
                        maxOffset = Children[i].Size.X;
                    }

                    Children[i].Position = new Vector2(xOffset, yOffset);
                    maxOffset = MathF.Max(maxOffset, Children[i].Size.X);
                    yOffset += Children[i].Size.X;
                }
            }
        }

        public virtual void Dispose()
        {
            RemoveEvents();
        }
    }
}
