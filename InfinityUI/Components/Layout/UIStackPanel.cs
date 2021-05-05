using InfinityUI.Core;
using InfinityUI.Interfaces;
using Materia.Rendering.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;

namespace InfinityUI.Components.Layout
{
    public class UIStackPanel : IComponent, ILayout
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
        }

        public virtual void Invalidate()
        {
            if (Parent == null)
            {
                return;
            }

            var Children = Parent.Children;

            float width = 0;
            float height = 0;
            float offset = 0;

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
                    Children[i].Position = new Vector2(offset, 0);
                    offset += Children[i].Size.X;
                    width += Children[i].Size.X;
                    height = MathF.Max(height, Children[i].Size.Y);
                }
                else
                {
                    Children[i].Position = new Vector2(0, offset);
                    offset += Children[i].Size.Y;
                    height += Children[i].Size.Y;
                    width = MathF.Max(width, Children[i].Size.X);
                }
            }

            var RelativeTo = Parent.RelativeTo;

            switch (RelativeTo)
            {
                case Anchor.TopHorizFill:
                case Anchor.BottomHorizFill:
                case Anchor.CenterHorizFill:
                    Parent.Size = new Vector2(Parent.Size.X, height);
                    break;
                case Anchor.Fill:
                    break;
                default:
                    Parent.Size = new Vector2(width, height);
                    break;
            }
        }

        public virtual void Dispose()
        {
            RemoveEvents();
        }
    }
}
