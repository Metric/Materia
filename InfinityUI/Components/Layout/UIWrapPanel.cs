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
        public bool NeedsUpdate { get; set; }

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
                NeedsUpdate = true;
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
                NeedsUpdate = true;
            }
        }

        public UIObject Parent { get; set; }

        public virtual void Awake()
        {
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

            NeedsUpdate = true;
        }

        private void Parent_Resize(UIObject obj)
        {
            NeedsUpdate = true;
        }

        private void Parent_ChildRemoved(UIObject obj)
        {
            if (obj == null) return;
            obj.Resize -= Obj_Resize;
            NeedsUpdate = true;
        }

        private void Parent_ChildAdded(UIObject obj)
        {
            if (obj == null) return;
            obj.Resize += Obj_Resize;
            NeedsUpdate = true;
        }

        private void Obj_Resize(UIObject obj)
        {
            NeedsUpdate = true;
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
            if (Parent == null || !NeedsUpdate) return;

            var Children = Parent.Children;
            var Size = Parent.AnchorSize;

            UIObject prev = null;

            float yOffset = 0;
            float xOffset = 0;
            float maxOffset = 0;

            int rowCount = 0;
            int prevRowCount = 0;

            for (int i = 0; i < Children.Count; ++i)
            {
                int k = i;
                if (reverse) k = (Children.Count - 1) - i;

                var child = Children[k];

                if (!child.Visible) continue;

                var csize = child.AnchorSize;

                if (direction == Orientation.Horizontal)
                {
                    float marginOffset = 0;

                    marginOffset = (prev != null ? prev.Margin.Right : Parent.Padding.Left) + child.Margin.Left;

                    if (xOffset + csize.X + marginOffset > Size.X)
                    {
                        prevRowCount = rowCount;
                        rowCount = 0;
                        prev = null;
                        xOffset = 0;
                        yOffset += maxOffset;
                        maxOffset = csize.Y;
                    }
                    else
                    {
                        prev = Children[i];
                    }

                    int t = i - (prevRowCount - rowCount);
                    if (reverse) t = (Children.Count - 1) - t;
                    float verticalOffset = (prevRowCount == 0 ? Parent.Padding.Top : Children[t].Margin.Bottom) + child.Margin.Top;

                    child.Position = new Vector2(xOffset + marginOffset, yOffset + verticalOffset);
                    maxOffset = MathF.Max(maxOffset, csize.Y + verticalOffset);
                    xOffset += csize.X + marginOffset;
                }
                else
                {
                    float marginOffset = 0;

                    marginOffset = (prev != null ? prev.Margin.Bottom : Parent.Padding.Top) + child.Margin.Top;

                    if (yOffset + csize.Y + marginOffset > Size.Y)
                    {
                        prev = null;
                        prevRowCount = rowCount;
                        rowCount = 0;
                        yOffset = 0;
                        xOffset += maxOffset;
                        maxOffset = csize.X;
                    }
                    else
                    {
                        prev = Children[i];
                    }

                    int t = i - (prevRowCount - rowCount);
                    if (reverse) t = (Children.Count - 1) - t;
                    float verticalOffset = (prevRowCount == 0 ? Parent.Padding.Left : Children[t].Margin.Bottom) + child.Margin.Top;

                    child.Position = new Vector2(xOffset + verticalOffset, yOffset + marginOffset);
                    maxOffset = MathF.Max(maxOffset, csize.X + verticalOffset);
                    yOffset += csize.Y + marginOffset;
                }

                ++rowCount;
            }

            NeedsUpdate = false;
        }

        public virtual void Dispose()
        {
            RemoveEvents();
        }
    }
}
