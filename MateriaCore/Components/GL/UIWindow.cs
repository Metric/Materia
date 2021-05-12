using InfinityUI.Components;
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
            Sizing = SizeMode.Percent;
            InitializeComponents();
            title.Text = titleText;
        }

        private void InitializeComponents()
        {
            titleArea = new UIObject
            {
                RelativeTo = Anchor.Center,
                RaycastTarget = false
            };
            title = titleArea.AddComponent<UIText>();
            title.FontSize = 22;
            title.Alignment = TextAlignment.Center;

            titleBackgroundArea = new UIObject
            {
                Size = new Vector2(1, 32),
                RelativeTo = Anchor.TopHorizFill,
            };
            titleBackground = titleBackgroundArea.AddComponent<UIImage>();
            titleBackground.Color = new Vector4(0.1f, 0.1f, 0.1f, 1); //todo: add in a theme system
            titleBackgroundArea.AddChild(titleArea);

            content = new UIObject
            {
                RelativeTo = Anchor.Fill,
                Padding = new Box2(0, 32, 0, 0)
            };

            closeButton = new Button("", new Vector2(32, 32))
            {
                RelativeTo = Anchor.TopRight,
                Padding = new Box2(2, 2, 2, 2)
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

        /// <summary>
        /// We override this here to
        /// only test for if we are in the title
        /// background area for window dragging
        /// </summary>
        /// <param name="selectable">The selectable.</param>
        /// <param name="e">The <see cref="MouseEventArgs"/> instance containing the event data.</param>
        protected override void OnMouseMove(UISelectable selectable, MouseEventArgs e)
        {
            Vector2 p = e.Position;
            
            if (Canvas != null)
            {
                p = Canvas.ToCanvasSpace(p);
            }

            if (titleBackgroundArea.Contains(ref p))
            {
                base.OnMouseMove(selectable, e);
            }
        }
    }
}
