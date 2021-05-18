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
        public bool NeedsUpdate { get; set; }
        public UIObject Parent { get; set; }

        public Axis Axis { get; set; } = Axis.Both;

        protected HashSet<Type> ignoreTypes = new HashSet<Type>();

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
                child.Resize += Child_Resize;
            }

            Parent.ChildAdded += Parent_ChildAdded;
            Parent.ChildRemoved += Parent_ChildRemoved;
            NeedsUpdate = true;
        }

        public void Ignore(Type t)
        {
            ignoreTypes.Add(t);
        }

        private void Parent_ChildRemoved(UIObject child)
        {
            child.Resize -= Child_Resize;
            NeedsUpdate = true;
        }

        private void Parent_ChildAdded(UIObject child)
        {
            child.Resize += Child_Resize;
            Child_Resize(child);
        }

        private void Child_Resize(UIObject obj)
        {
            if (Parent == null) return;

            if (ignoreTypes.Contains(obj.GetType())) return;

            //do not have to do a full invalidate
            //we can just adjust
            Box2 area = Parent.AnchoredRect;
            area.Encapsulate(obj.AnchoredRect);
            ResizeTo(ref area);
        }

        protected void ResizeTo(ref Box2 area)
        {
            if (Parent == null) return;

            switch (Axis)
            {
                case Axis.Both:
                    Parent.Size = new Vector2(area.Width, area.Height);
                    break;
                case Axis.Horizontal:
                    Parent.Size = new Vector2(area.Width, Parent.Size.Y);
                    break;
                case Axis.Vertical:
                    Parent.Size = new Vector2(Parent.Size.X, area.Height);
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
            if (Parent == null || !NeedsUpdate) return;
            NeedsUpdate = false;
            var children = Parent.Children;

            Box2 area = new Box2(0,0,0,0);

            for (int i = 0; i < children.Count; ++i)
            {
                var child = children[i];
                if (ignoreTypes.Contains(child.GetType())) continue;
                area.Encapsulate(child.AnchoredRect);
            }

            ResizeTo(ref area);
        }

        public virtual void Dispose()
        {
            RemoveEvents();
        }
    }
}
