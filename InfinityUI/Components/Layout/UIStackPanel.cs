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

        protected Anchor childAlignment = Anchor.TopLeft;
        public Anchor ChildAlignment
        {
            get => childAlignment;
            set
            {
                childAlignment = value;
                NeedsUpdate = true;
            }
        }

        protected bool autoSize;
        public bool AutoSize
        {
            get => autoSize;
            set
            {
                autoSize = value;
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
        }

        private Vector3 Arrange(UIObject child, UIObject prev, float offset, float width, float height)
        {
            float marginOffset;
            float verticalOffset;

            var csize = child.AnchorSize;

            child.RelativeTo = childAlignment;

            if (direction == Orientation.Horizontal)
            {
                verticalOffset = Parent.Padding.Top + child.Margin.Top;

                marginOffset = (prev != null ? prev.Margin.Right : Parent.Padding.Left) + child.Margin.Left;

                child.Position = new Vector2(offset, verticalOffset);
                float s = csize.X + marginOffset;
                offset += s;
                width += s;
                height = MathF.Max(height, csize.Y);
            }
            else
            {
                verticalOffset = Parent.Padding.Left + child.Margin.Left;

                marginOffset = (prev != null ? prev.Margin.Bottom : Parent.Padding.Top) + child.Margin.Top;

                child.Position = new Vector2(verticalOffset, offset);
                float s = csize.Y + marginOffset;
                offset += s;
                height += s;
                width = MathF.Max(width, csize.X);
            }

            return new Vector3(offset, width, height);
        }

        public virtual void Invalidate()
        {
            if (Parent == null || !NeedsUpdate) return;

            var Children = Parent.Children;
            UIObject prev = null;

            float width = 0;
            float height = 0;
            float offset = 0;

            if (autoSize)
            {
                float maxSize = 0;
                for (int i = 0; i < Children.Count; ++i)
                {
                    var child = Children[i];
                    var fitter = child.GetComponent<UIContentFitter>();
                    if (fitter != null)
                    {
                        switch (direction)
                        {
                            case Orientation.Horizontal:
                                fitter.Axis = Axis.Horizontal;
                                break;
                            case Orientation.Vertical:
                                fitter.Axis = Axis.Vertical;
                                break;
                        }
                    }

                    Vector2 size = child.AnchorSize;
                    switch (direction)
                    {
                        case Orientation.Horizontal:
                            maxSize = MathF.Max(size.Y, maxSize);
                            break;
                        case Orientation.Vertical:
                            maxSize = MathF.Max(size.X, maxSize);
                            break;
                    }
                }

                for (int i = 0; i < Children.Count; ++i)
                {
                    var child = Children[i];
                    var size = child.AnchorSize;
                    switch (direction)
                    {
                        case Orientation.Horizontal:
                            child.Size = new Vector2(size.X, maxSize);
                            break;
                        case Orientation.Vertical:
                            child.Size = new Vector2(maxSize, size.Y);
                            break;
                    }
                }
            }

            for (int i = 0; i < Children.Count; ++i)
            {
                int k = i;
                if (reverse) k = (Children.Count - 1) - i;
                var child = Children[k];
                if (!child.Visible) continue;

                Vector3 sizes = Arrange(child, prev, offset, width, height);

                offset = sizes.X;
                width = sizes.Y;
                height = sizes.Z;

                prev = child;
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

            NeedsUpdate = false;
        }

        public virtual void Dispose()
        {
            RemoveEvents();
        }
    }
}
