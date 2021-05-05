using InfinityUI.Components;
using InfinityUI.Core;
using InfinityUI.Interfaces;
using Materia.Rendering.Mathematics;
using System;

namespace InfinityUI.Controls
{
    public class Button : UIObject
    {
        public event Action<Button> Submit;
        public event Action<Button> Focused;
        public event Action<Button> Unfocused;

        protected UIImage background;

        public UIImage Background
        {
            get => background;
        }

        protected UIObject textContainer;
        protected UIText textView;
        protected UISelectable selectable;

        public UIObject TextContainer
        {
            get => textContainer;
        }

        public TextAlignment TextAlignment
        {
            get
            {
                if (textView == null) return TextAlignment.Left;
                return textView.Alignment;
            }
            set
            {
                if (textView == null) return;
                textView.Alignment = value;
            }
        }

        public string Text
        {
            get
            {
                if (textView == null) return string.Empty;
                return textView.Text;
            }
            set
            {
                if (textView == null) return;
                textView.Text = value;
            }
        }

        public Button() : this("button")
        {

        }

        public Button(string text) : this(text, new Vector2(100,24))
        {

        }

        public Button(string text, Vector2 size)
        {
            Size = size;

            InitComponents();
            InitEvents();

            Text = text;
        }

        protected virtual void InitComponents()
        {
            textContainer = new UIObject();
            textContainer.Size = Size;
            textContainer.RelativeTo = Anchor.CenterHorizFill;
            AddChild(textContainer);

            textView = textContainer.AddComponent<UIText>();
            textView.Alignment = TextAlignment.Center;

            background = AddComponent<UIImage>();
            selectable = AddComponent<UISelectable>();
            selectable.TargetGraphic = background;
        }

        protected virtual void InitEvents()
        {
            if (selectable == null) return;
            selectable.Submit += Selectable_Submit;
            selectable.Click += Selectable_Click;
            selectable.FocusChanged += Selectable_FocusChanged;
        }

        private void Selectable_FocusChanged(UISelectable arg1, bool arg2)
        {
            if (arg2)
            {
                Focused?.Invoke(this);
            }
            else
            {
                Unfocused?.Invoke(this);
            }
        }

        private void Selectable_Click(UISelectable arg1, MouseEventArgs arg2)
        {
            Submit?.Invoke(this);
        }

        private void Selectable_Submit(UISelectable obj)
        {
            Submit?.Invoke(this);
        }

        public override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (selectable == null) return;
            selectable.Submit -= Selectable_Submit;
            selectable.Click -= Selectable_Click;
            selectable.FocusChanged -= Selectable_FocusChanged;
        }
    }
}
