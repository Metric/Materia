using InfinityUI.Controls;
using InfinityUI.Core;
using Materia.Nodes;
using Materia.Nodes.Items;
using Materia.Rendering.Mathematics;
using MateriaCore.Components.Dialogs;
using MateriaCore.Utils;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MateriaCore.Components.GL.ItemNodes
{
    public class UIPinNode : UINodeBase
    {
        public const int DEFAULT_HEIGHT = 64;
        public const int DEFAULT_WIDTH = 64;

        protected Vector4 color;

        public UIPinNode() : base(new Vector2(DEFAULT_WIDTH, DEFAULT_HEIGHT)) 
        {
            color = new Vector4(1, 1, 1, 1);
            InitializeComponents();
        }

        public UIPinNode(UIGraph g, PinNode n) : base(new Vector2(DEFAULT_WIDTH, DEFAULT_HEIGHT))
        {
            Graph = g;
            Node = n;
            Id = n.Id;

            color = n.GetColor();

            InitializeComponents();
        }

        protected override void OnRestored()
        {
            if (Node == null || !(Node is PinNode)) return;
            color = (Node as PinNode).GetColor();
            if (selectable == null) return;
            selectable.NormalColor = color;
        }

        private void InitializeComponents()
        {
            RelativeTo = Anchor.TopLeft; //set as topleft by default

            SnapMode = MovablePaneSnapMode.Grid;
            SnapTolerance = 32;

            selectable.BubbleEvents = false;

            selectable.NormalColor = color;

            selectable.Click += Selectable_Click;
            selectable.PointerUp += Selectable_PointerUp;
            selectable.PointerDown += Selectable_PointerDown;
            selectable.TargetGraphic = Background;

            DoubleClick += UINode_DoubleClick;

            Moved += UINode_Moved;
            MovedTo += UINode_MovedTo;

            Background.Texture = UI.GetEmbeddedImage(Icons.PIN, GetType());
        }

        protected void UpdateColor(MVector v)
        {
            if (selectable == null) return;
            color = v.ToVector4();
            selectable.NormalColor = color;
        }

        protected override void UINode_DoubleClick(MovablePane obj)
        {
            bool success = false;
            MVector newColor = new MVector(0);
            Task.Run(async () =>
            {
                try
                {
                    var dialog = new ColorPicker(new MVector(color));
                    success = await dialog.ShowDialog<bool>(MainWindow.Instance);
                    newColor = dialog.SelectedVector;
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.StackTrace);
                }
            }).ContinueWith(t =>
            {
                if (success) UpdateColor(newColor);
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }
    }
}
