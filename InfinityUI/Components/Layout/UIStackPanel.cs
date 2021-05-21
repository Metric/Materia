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

        protected Box2 lastVisibleArea;

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
            var fitter = child.GetComponent<UIContentFitter>();

            if (fitter != null) {
                switch (ChildAlignment)
                {
                    case Anchor.TopLeft:
                    case Anchor.TopRight:
                    case Anchor.BottomLeft:
                    case Anchor.BottomRight:
                    case Anchor.Right:
                    case Anchor.Left:
                    case Anchor.Center:
                    case Anchor.Bottom:
                    case Anchor.Top:
                        fitter.Axis = Axis.Both;
                        break;
                    case Anchor.BottomHorizFill:
                    case Anchor.CenterHorizFill:
                    case Anchor.TopHorizFill:
                        fitter.Axis = Axis.Vertical;
                        break;
                    case Anchor.LeftVerticalFill:
                    case Anchor.RightVerticalFill:
                        fitter.Axis = Axis.Horizontal;
                        break;
                }
            }

            //reset anchor to top left
            child.RelativeTo = Anchor.TopLeft;

            var csize = child.ExtendedRect;

            if (direction == Orientation.Horizontal)
            {
                child.Position = new Vector2(offset, 0);
                float s = csize.Width;
                offset += s;
                width += s;
                height = MathF.Max(height, csize.Height);
            }
            else
            {
                child.Position = new Vector2(0, offset);
                float s = csize.Height;
                offset += s;
                height += s;
                width = MathF.Max(width, csize.Width);
            }

            child.RelativeTo = ChildAlignment;

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
                    Parent.Size = new Vector2(1, height);
                    break;
                case Anchor.Fill:
                    break;
                default:
                    Parent.Size = new Vector2(width, height);
                    break;
            }

            NeedsUpdate = false;
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

        public virtual void Dispose()
        {
            RemoveEvents();
        }
    }
}
