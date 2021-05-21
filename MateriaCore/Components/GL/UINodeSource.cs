using InfinityUI.Controls;
using InfinityUI.Core;
using InfinityUI.Components;
using Materia.Rendering.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;
using MateriaCore.Utils;

namespace MateriaCore.Components.GL
{
    public class UINodeSource : Button
    {
        public string Type { get; set; }
        public string Path { get; set; }
        public string Title
        {
            get
            {
                if (textView == null) return "";
                return textView.Text;
            }
            set
            {
                if (textView == null) return;
                textView.Text = value;
                if (string.IsNullOrEmpty(textView.Text)) return;
                LoadIcon();
            }
        }

        protected UIObject iconArea;
        protected UIImage icon;

        public bool AllowDrag { get; set; } = true;

        bool isMouseDown = false;

        public UINodeSource(string name) : base(name, new Vector2(128,32))
        {
            InitializeComponents();
            Title = name;
            Name = name;
        }

        public UINodeSource(UINodeSource s) : base(s.Title, new Vector2(128,32))
        {
            InitializeComponents();
            Name = s.Name;
            Title = s.Title;
            Path = s.Path;
            Type = s.Type;
            Tooltip = s.Tooltip;
        }

        public UINodeSource Copy()
        {
            return new UINodeSource(this);
        }

        private void InitializeComponents()
        {
            RelativeTo = Anchor.TopHorizFill;

            textContainer.Margin = new Box2(42, 0, 0, 0);
            textContainer.Padding = new Box2(0, 0, 42, 0);
            textView.Color = new Vector4(1, 1, 1, 1);
            selectable.NormalColor = new Vector4(0.05f, 0.05f, 0.05f, 1);

            textContainer.RelativeTo = Anchor.Left;
            textView.Alignment = TextAlignment.Left;

            iconArea = new UIObject
            {
                Margin = new Box2(2,2,2,2),
                Size = new Vector2(32, 32),
                RelativeTo = Anchor.TopLeft
            };
            icon = iconArea.AddComponent<UIImage>();
            iconArea.RaycastTarget = false;

            //set default node icon
            icon.Texture = UI.GetEmbeddedImage(Icons.NODE, typeof(UINodeSource));

            selectable.PointerMove += Selectable_PointerMove;
            selectable.PointerDown += Selectable_PointerDown;
            selectable.PointerUp += Selectable_PointerUp;
            selectable.PointerEnter += Selectable_PointerEnter;

            AddChild(iconArea);
        }

        private void Selectable_PointerEnter(UISelectable arg1, InfinityUI.Interfaces.MouseEventArgs arg2)
        {
            isMouseDown = false;
        }

        private void Selectable_PointerUp(UISelectable arg1, InfinityUI.Interfaces.MouseEventArgs e)
        {
            if (e.Button.HasFlag(InfinityUI.Interfaces.MouseButton.Left))
            {
                isMouseDown = false;
            }
        }

        private void Selectable_PointerDown(UISelectable arg1, InfinityUI.Interfaces.MouseEventArgs e)
        {
            if (e.Button.HasFlag(InfinityUI.Interfaces.MouseButton.Left))
            {
                isMouseDown = true;
            }
        }

        private void LoadIcon()
        {
            string path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Icons", "Shelf", $"{Title}.png");
            if (System.IO.File.Exists(path) && icon != null)
            {
                icon.Texture = UI.GetImage(path);
            }
        }

        private void Selectable_PointerMove(UISelectable arg1, InfinityUI.Interfaces.MouseEventArgs e)
        {
            if (isMouseDown && AllowDrag)
            { 
                Canvas?.DragDrop?.Begin(this, this);
            }
        }
    }
}
