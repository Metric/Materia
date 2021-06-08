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

        protected Box2 lastVisibleArea;

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
            var Size = Parent.ExtendedRect;

            float yOffset = 0;
            float xOffset = 0;
            float maxOffset = 0;

            for (int i = 0; i < Children.Count; ++i)
            {
                int k = i;
                if (reverse) k = (Children.Count - 1) - i;

                var child = Children[k];
                if (!child.Visible) continue;

                var previousAlignment = child.RelativeTo;
                child.RelativeTo = Anchor.TopLeft;

                var csize = child.ExtendedRect;

                if (direction == Orientation.Horizontal)
                {
                    if (xOffset + csize.Width > Size.Width)
                    {
                        yOffset += maxOffset;
                        xOffset = 0;
                        maxOffset = 0;
                    }

                    maxOffset = MathF.Max(csize.Height, maxOffset);
                    child.Position = new Vector2(xOffset, yOffset);
                    xOffset += csize.Width;
                }
                else
                {
                    if (yOffset + csize.Height > Size.Height)
                    {
                        xOffset += maxOffset;
                        yOffset = 0;
                        maxOffset = 0;
                    }

                    maxOffset = MathF.Max(csize.Width, maxOffset);
                    child.Position = new Vector2(xOffset, yOffset);
                    yOffset += csize.Height;
                }

                child.RelativeTo = previousAlignment;
            }

            NeedsUpdate = false;
        }

        public virtual void Dispose()
        {
            RemoveEvents();
        }

        public virtual void Update()
        {
            if (Parent == null) return;
            if (lastVisibleArea != Parent.VisibleRect)
            {
                lastVisibleArea = Parent.VisibleRect;
                NeedsUpdate = true;
            }
        }
    }
}
