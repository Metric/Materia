using InfinityUI.Components;
using InfinityUI.Controls;
using InfinityUI.Core;
using Materia.Rendering.Mathematics;
using MateriaCore.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace MateriaCore.Components.GL
{
    public class UIDocumentTab : ToggleButton
    {
        //todo: add in drag / drop reorder support

        public event Action<UIDocumentTab> Close;
        public UIGraph Graph { get; protected set; }

        #region components
        protected Button closeButton;
        #endregion
        
        public UIDocumentTab(UIGraph graph) : base(graph?.GraphName, new Vector2(128,32))
        {
            Graph = graph;
            Graph.NameChanged += Graph_NameChanged;
            InitializeComponents();
        }

        private void Graph_NameChanged(UIGraph obj)
        {
            if (Graph == null) return;
            Text = Graph.GraphName;
        }

        private void InitializeComponents()
        {
            RelativeTo = Anchor.TopLeft;

            textContainer.Margin = new Box2(4, 4, 0, 0);
            textContainer.RelativeTo = Anchor.Left;
            textView.Alignment = TextAlignment.Left;

            closeButton = new Button("", new Vector2(16, 16))
            {
                Padding = new Box2(2, 2, 2, 2),
                RelativeTo = Anchor.Right
            };
            closeButton.Background.Texture = UI.GetEmbeddedImage(Icons.CLOSE, typeof(UIDocumentTab));

            closeButton.Submit += CloseButton_Submit;

            AddChild(closeButton);

            toggleState.ValueChanged += ToggleState_ValueChanged;
        }

        private void ToggleState_ValueChanged(UIToggleable arg1, bool arg2)
        {
            //store / restore graph from GPU
            //and hide / show ui
            if (toggleState.IsToggled && Graph != null && Graph.Root == null)
            {
                Graph.Restore();
            }
            else if(!toggleState.IsToggled && Graph != null && Graph.Root != null)
            {
                Graph.Store();
            }

            Graph.Visible = toggleState.IsToggled;
        }

        private void CloseButton_Submit(Button obj)
        {
            Close?.Invoke(this);
        }

        public override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (Graph != null)
            {
                Graph.NameChanged -= Graph_NameChanged;
                Graph = null;
            }
        }
    }
}
