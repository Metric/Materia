﻿using InfinityUI.Components;
using InfinityUI.Controls;
using InfinityUI.Core;
using InfinityUI.Interfaces;
using Materia.Rendering.Mathematics;
using MateriaCore.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace MateriaCore.Components.GL
{
    public class UIWindow : MovablePane
    {
        public event Action<UIWindow> Closing;

        #region Components
        protected UIObject titleArea;
        protected UIText title;

        protected UIObject titleBackgroundArea;
        protected UIImage titleBackground;

        protected Button closeButton;

        protected UIObject content;
        #endregion

        public string Title
        {
            get
            {
                return title == null ? "" : title.Text;
            }
            set
            {
                if (title == null) return;
                title.Text = value;
            }
        }

        public UIWindow(Vector2 size, string titleText = "") : base(size)
        {
            selectable.IsFocusable = false;
            InitializeComponents();
            title.Text = titleText;
        }

        private void InitializeComponents()
        {
            titleArea = new UIObject
            {
                RaycastTarget = false,
                RelativeTo = Anchor.Center
            };
            title = titleArea.AddComponent<UIText>();
            title.FontSize = 22;
            title.Alignment = TextAlignment.Center;

            titleBackgroundArea = new UIObject
            {
                Size = new Vector2(1, 48),
                RelativeTo = Anchor.TopHorizFill,
            };
            titleBackground = titleBackgroundArea.AddComponent<UIImage>();
            titleBackground.Color = new Vector4(0.1f, 0.1f, 0.1f, 1); //todo: add in a theme system
            titleBackgroundArea.AddChild(titleArea);
            titleBackgroundArea.RaycastTarget = false;

            content = new UIObject
            {
                RelativeTo = Anchor.Fill,
                Margin = new Box2(0, 48, 0, 0),
                RaycastTarget = true
            };

            closeButton = new Button("", new Vector2(48, 48))
            {
                Margin = new Box2(2, 2, 2, 2),
                RelativeTo = Anchor.TopRight
            };

            //todo: load close button icon here
            closeButton.Background.Texture = UI.GetEmbeddedImage(Icons.CLOSE, typeof(UIWindow));
            closeButton.Submit += CloseButton_Submit;

            Background.Clip = true;

            //specific add order
            AddChild(titleBackgroundArea);
            AddChild(closeButton);
            AddChild(content);
        }

        private void CloseButton_Submit(Button obj)
        {
            Closing?.Invoke(this);
            Visible = false;
        }
    }
}
