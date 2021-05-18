using InfinityUI.Core;
using InfinityUI.Interfaces;
using Materia.Rendering.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;

namespace InfinityUI.Components.Layout
{
    public class UIScrollPanel : IComponent, ILayout, IMouseWheel
    {
        public event Action<UIScrollPanel> Scrolled;

        public bool NeedsUpdate { get; set; }

        public UIObject Parent { get; set; }

        public float ScrollStep { get; set; } = 10;

        protected Vector2 offset = Vector2.Zero;
        protected Vector2 normalizedOffset = Vector2.Zero;
        public Vector2 NormalizedOffset
        {
            get
            {
                return normalizedOffset;
            }
            set
            {
                normalizedOffset = Vector2.Clamp(value, Vector2.Zero, Vector2.One);
                offset = normalizedOffset * MaximumOffset;
                NeedsUpdate = true;
            }
        }

        public Vector2 MaximumOffset { get; protected set; }

        protected UIObject view;
        public UIObject View
        {
            get => view;
        }

        public void Awake()
        {
            CreateView();
            AddEvents();
        }

        protected void CreateView()
        {
            view = new UIObject();

            if (Parent != null)
            {
                Parent.RaycastTarget = true;
                var children = Parent.Children;
                for (int i = 0; i < children.Count; ++i)
                {
                    view.AddChild(children[i]);
                }
            }

            //add size fitter
            view.AddComponent<UIContentFitter>();

            //add back to parent view
            Parent?.AddChild(view);
        }

        private void View_Resize(UIObject obj)
        {
            NeedsUpdate = true;
        }

        protected void AddEvents()
        {
            NeedsUpdate = true;
            view.Resize += View_Resize;

            if (Parent == null) return;
            Parent.Resize += Parent_Resize;
        }

        protected void RemoveEvents()
        {
            if (Parent == null) return;
            Parent.Resize -= Parent_Resize;
        }

        private void Parent_Resize(UIObject obj)
        {
            NeedsUpdate = true;
        }

        public virtual void Dispose()
        {
            RemoveEvents();
        }

        public virtual void ScrollTo(UIObject o)
        {
            //do nothing if already in view
            if (Parent.Rect.Intersects(o.Rect)) return;

            offset = new Vector2(o.Rect.Left, o.Rect.Top);
            NeedsUpdate = true;
            Scrolled?.Invoke(this);
        }

        public virtual void Invalidate()
        {
            if (Parent == null || !NeedsUpdate) return;
            var size = Parent.WorldSize;
            var vSize = view.WorldSize;
            if (vSize.X > size.X || vSize.Y > size.Y)
            {
                MaximumOffset = vSize - size;
                MaximumOffset = Vector2.Clamp(MaximumOffset, Vector2.Zero, MaximumOffset);
            }
            else
            {
                MaximumOffset = Vector2.Zero;
            }

            offset = Vector2.Clamp(offset, Vector2.Zero, MaximumOffset);
            normalizedOffset = new Vector2(offset.X / (MaximumOffset.X <= float.Epsilon ? 1 : MaximumOffset.X), offset.Y / (MaximumOffset.Y <= float.Epsilon ? 1 : MaximumOffset.Y));

            view.Position = -offset;

            NeedsUpdate = false;
        }

        public virtual void OnMouseWheel(MouseWheelArgs e)
        {
            if (e.IsHandled) return;
            e.IsHandled = true;
            offset -= e.Delta * ScrollStep;
            NeedsUpdate = true;
            Scrolled?.Invoke(this);
        }
    }
}
