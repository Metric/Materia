using InfinityUI.Components;
using InfinityUI.Components.Layout;
using InfinityUI.Controls;
using InfinityUI.Core;
using InfinityUI.Interfaces;
using Materia.Rendering.Mathematics;
using MateriaCore.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace MateriaCore.Components.GL.Menu
{
    public class UIMenuItem : Button
    {
        public bool IsSubMenuActive { get; protected set; }
        protected bool showSubMenuArrow = true; 
        public bool ShowSubMenuArrow
        {
            get => showSubMenuArrow;
            set
            {
                showSubMenuArrow = value;
                UpdateSubMenu();
            }
        }

        protected Anchor submenuAnchor = Anchor.Bottom;
        public Anchor SubMenuAnchor
        {
            get => submenuAnchor;
            set
            {
                submenuAnchor = value;
                UpdateSubMenu();
            }
        }

        protected UIMenu submenu = null;
        public UIMenu SubMenu 
        { 
            get => submenu;
            set
            {
                RemoveChild(submenu);
                submenu = value;
                AddChild(submenu);
                UpdateSubMenu();
            }
        }

        #region internal components
        protected UIImage submenuArrow;
        protected UIObject submenuArrowArea;
        #endregion

        public UIMenuItem(string title) : base(title)
        {
            selectable.NormalColor = new Vector4(0.25f, 0.25f, 0.25f, 1);
            InitializeComponents();
            InitializeEvents();
        }

        private void InitializeComponents()
        {
            var fitter = AddComponent<UIContentFitter>();
            fitter.Ignore(typeof(UIMenu));

            textView.Alignment = TextAlignment.Left;

            Padding = new Box2(2, 2, 2, 2);

            submenuArrowArea = new UIObject()
            {
                Size = new Vector2(16,16),
                Margin = new Box2(0, 0, 2, 0),
                Visible = false
            };

            submenuArrow = submenuArrowArea.AddComponent<UIImage>();
            submenuArrowArea.RaycastTarget = false;
            submenuArrow.Texture = UI.GetEmbeddedImage(Icons.CHEVRON_RIGHT, typeof(UIMenuItem));

            AddChild(submenuArrowArea);
        }

        private void InitializeEvents()
        {
            Focused += UIMenuItem_Focused;
            Unfocused += UIMenuItem_Unfocused;
            Resize += UIMenuItem_Resize;
        }

        private void UIMenuItem_Resize(UIObject obj)
        {
            UpdateSubMenu();
        }


        private void UIMenuItem_Unfocused(Button obj, FocusEvent fv)
        {
            HideMenu();
        }

        private void UIMenuItem_Focused(Button obj, FocusEvent fv)
        {
            ShowMenu();
        }


        private void UpdateSubMenu()
        {
            if (submenuArrowArea != null)
            {
                submenuArrowArea.Visible = submenu != null && showSubMenuArrow;
            }

            if (submenu == null) return;

            switch (submenuAnchor)
            {
                case Anchor.Bottom:
                    submenu.RelativeTo = Anchor.BottomLeft;
                    selectable.Down = submenu.GetComponent<UISelectable>().Down;
                    submenu.Margin = new Box2(-Padding.Left, 0, 0, -(Padding.Bottom - 1));
                    submenu.Position = new Vector2(0, AnchorSize.Y);
                    break;
                case Anchor.Top:
                    submenu.RelativeTo = Anchor.TopLeft;
                    selectable.Up = submenu.GetComponent<UISelectable>().Down;
                    break;
                case Anchor.Left:
                    submenu.RelativeTo = Anchor.TopLeft;
                    selectable.Left = submenu.GetComponent<UISelectable>().Down;
                    break;
                case Anchor.Right:
                    submenu.RelativeTo = Anchor.TopRight;
                    selectable.Right = submenu.GetComponent<UISelectable>().Down;
                    break;
            }
        }

        public virtual void HideMenu()
        {
            if (submenu == null) return;
            submenu.Visible = false;
            IsSubMenuActive = false;
        }

        public virtual void ShowMenu()
        {
            if (submenu == null) return;
            submenu.Visible = true;
            IsSubMenuActive = true;
        }
    }
}
