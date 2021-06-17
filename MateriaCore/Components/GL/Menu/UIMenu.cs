using InfinityUI.Components;
using InfinityUI.Components.Layout;
using InfinityUI.Core;
using InfinityUI.Interfaces;
using Materia.Rendering.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;

namespace MateriaCore.Components.GL.Menu
{
    public class UIMenu : UIObject
    {
        public event Action<UIMenu, MouseEventArgs> PointerExit;
        public event Action<UIMenu, MouseEventArgs> PointerEnter;
        public event Action<UIMenu, FocusEvent> Focused;
        public event Action<UIMenu, FocusEvent> Unfocused;

        #region components
        protected UISelectable selectable;
        protected UIImage background;
        protected UIStackPanel stack;

        public UIImage Background { get => background; }
        #endregion

        public Orientation Direction
        {
            get
            {
                return stack == null ? Orientation.Horizontal : stack.Direction;
            }
            set
            {
                if (stack == null) return;
                stack.Direction = value;
            }
        }

        public Anchor ChildAlignment
        {
            get
            {
                return stack == null ? Anchor.TopLeft : stack.ChildAlignment;
            }
            set
            {
                if (stack == null) return;
                stack.ChildAlignment = value;
            }
        }

        public UIMenu() : base() 
        {
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            RelativeTo = Anchor.TopHorizFill;
            background = AddComponent<UIImage>();
            background.Color = new Vector4(0.075f, 0.075f, 0.075f, 1);
            selectable = AddComponent<UISelectable>();
            selectable.IsFocusable = false;
            stack = AddComponent<UIStackPanel>();

            RaycastTarget = true;

            selectable.FocusChanged += Selectable_FocusChanged;
            selectable.PointerEnter += Selectable_PointerEnter;
            selectable.PointerExit += Selectable_PointerExit;

            ChildAdded += UIMenu_ChildAdded;
            ChildRemoved += UIMenu_ChildRemoved;
        }

        private void UIMenu_ChildRemoved(UIObject obj)
        {
            UpdateTabbing();
        }

        private void UIMenu_ChildAdded(UIObject obj)
        {
            UpdateTabbing();
        }

        private void UpdateTabbing()
        {
            var children = Children.FindAll(m => m.HasComponent<UISelectable>());
            if (children.Count == 0)
            {
                selectable.Down = null;
                return;
            }

            var firstSelectable = children[0].GetComponent<UISelectable>();

            selectable.Down = firstSelectable;

            for (int i = 1; i < children.Count; ++i)
            {
                var c = children[i];
                var select = c.GetComponent<UISelectable>();

                var prevC = children[i - 1];
                var prevSelect = prevC.GetComponent<UISelectable>();

                switch (Direction)
                {
                    case Orientation.Horizontal:
                        prevSelect.Down = null;
                        prevSelect.Right = select;
                        select.Up = null;
                        select.Left = prevSelect;
                        break;
                    case Orientation.Vertical:
                        prevSelect.Right = null;
                        prevSelect.Down = select;
                        select.Left = null;
                        select.Up = prevSelect;
                        break;
                }
            }

            if (Parent is UIMenuItem)
            {
                var item = Parent as UIMenuItem;
                var pSelect = item.GetComponent<UISelectable>();
                switch (item.SubMenuAnchor)
                {
                    case Anchor.Bottom:
                        pSelect.Down = firstSelectable;
                        firstSelectable.Up = pSelect;
                        break;
                    case Anchor.Top:
                        pSelect.Up = firstSelectable;
                        firstSelectable.Down = pSelect;
                        break;
                    case Anchor.Left:
                        pSelect.Left = firstSelectable;
                        firstSelectable.Right = pSelect;
                        break;
                    case Anchor.Right:
                        pSelect.Right = firstSelectable;
                        firstSelectable.Left = pSelect;
                        break;
                }
            }
        }

        private void Selectable_PointerExit(UISelectable arg1, MouseEventArgs e)
        {
            PointerExit?.Invoke(this, e);
        }

        private void Selectable_PointerEnter(UISelectable arg1, MouseEventArgs e)
        {
            PointerEnter?.Invoke(this, e);
        } 

        private void Selectable_FocusChanged(UISelectable arg1, FocusEvent fv, bool focused)
        {
            if (focused)
            {
                Focused?.Invoke(this, fv);
            }
            else
            {
                Unfocused?.Invoke(this, fv);
            }
        }
    }
}
