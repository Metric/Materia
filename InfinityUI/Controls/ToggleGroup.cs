using System;
using System.Collections.Generic;
using Materia.Rendering.Mathematics;
using System.Text;
using InfinityUI.Core;
using InfinityUI.Interfaces;
using InfinityUI.Components;

namespace InfinityUI.Controls
{
    public class ToggleGroup : IComponent
    {
        public UIObject Parent { get; set; }

        public event Action<ToggleGroup, UIToggleable,int> ToggleChanged;

        public UIToggleable ActiveToggle { get; protected set; }

        public virtual void Awake()
        {
            if (Parent == null) return;
            Parent.ChildAdded += Parent_ChildAdded;
            Parent.ChildRemoved += Parent_ChildRemoved;

            for (int i = 0; i < Parent.Children.Count; ++i)
            {
                Parent_ChildAdded(Parent.Children[i]);
            }
        }

        private void Parent_ChildRemoved(UIObject obj)
        {
            if (!obj.HasComponent<UIToggleable>()) return;
            obj.GetComponent<UIToggleable>().ValueChanged -= ToggleGroup_ValueChanged;
            if (obj == ActiveToggle.Parent)
            {
                ActiveToggle = null;
            }
        }

        private void Parent_ChildAdded(UIObject obj)
        {
            if (!obj.HasComponent<UIToggleable>()) return;
            obj.GetComponent<UIToggleable>().ValueChanged += ToggleGroup_ValueChanged;
            if (Parent != null && Parent.Children.Count == 1)
            {
                SetActive(0);
            }
        }

        public virtual void Dispose()
        {
            if (Parent == null) return;
            Parent.ChildRemoved -= Parent_ChildRemoved;
            Parent.ChildAdded -= Parent_ChildAdded;
        }

        public void SetActive(int idx)
        {
            if (idx < 0 || idx >= Parent.Children.Count) return;
            for (int i = 0; i < Parent.Children.Count; ++i)
            {
                var c = Parent.Children[i];
                if (!c.HasComponent<UIToggleable>()) continue;
                c.GetComponent<UIToggleable>().Assign(false);
            }
            ToggleGroup_ValueChanged(Parent.Children[idx].GetComponent<UIToggleable>(), true);
        }

        private void ToggleGroup_ValueChanged(UIToggleable arg1, bool arg2)
        {
            if (arg1 == null) return;

            if (ActiveToggle != null && ActiveToggle != arg1)
            {
                ActiveToggle.Assign(false);
            }
            if (ActiveToggle != arg1)
            {
                ActiveToggle = arg1;
                ActiveToggle.Assign(true);
                ToggleChanged?.Invoke(this, ActiveToggle, Parent.Children.IndexOf(ActiveToggle.Parent));
            }
        }
    }
}
