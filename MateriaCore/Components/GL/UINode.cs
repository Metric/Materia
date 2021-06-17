﻿using System;
using System.Collections.Generic;
using System.Text;
using InfinityUI.Core;
using InfinityUI.Components;
using Materia.Nodes;
using Materia.Graph;
using Materia.Rendering.Mathematics;
using InfinityUI.Components.Layout;
using MateriaCore.Components.Panes;
using InfinityUI.Controls;
using Materia.Nodes.Atomic;
using Avalonia.Controls;
using System.Threading.Tasks;
using Materia.Rendering.Imaging;
using MateriaCore.Utils;
using InfinityUI.Interfaces;
using Materia.Rendering.Textures;
using System.Diagnostics;
using Materia.Graph.Exporters;

namespace MateriaCore.Components.GL
{
    public class UINode : MovablePane, IGraphNode
    {
        public event Action<UINode> Restored;
        public event Action<UINode> PreviewUpdated;

        public const int DEFAULT_HEIGHT = 128;
        public const int DEFAULT_WIDTH = 128;

        #region UI Components  
        protected UIObject titleArea;
        protected UIText title;

        protected UIObject descArea;
        protected UIText desc;

        protected UIObject previewArea;
        protected UIImage preview;

        protected UIObject previewTransparency;

        protected UIToggleable toggleable;

        protected UIObject inputsArea;
        protected UIObject outputsArea;

        protected UIObject iconsArea;
        protected UIObject inputOutputIcon;

        protected UIContentFitter fitter;
        #endregion

        #region Graph Details
        public Node Node { get; protected set; }

        public UIGraph Graph { get; protected set; }

        public string Id { get; protected set; } = Guid.NewGuid().ToString(); //assign a default guid to it

        //note add back in Input Output Nodes here
        protected List<UINodePoint> inputs = new List<UINodePoint>();
        protected List<UINodePoint> outputs = new List<UINodePoint>();
        #endregion

        #region States
        protected bool isMoving = false;
        #endregion

        public UINode() : base(new Vector2(DEFAULT_WIDTH, DEFAULT_HEIGHT))
        {
            InitializeComponents();
        }

        public UINode(UIGraph g, Node n) : base(new Vector2(DEFAULT_WIDTH, DEFAULT_HEIGHT))
        {
            Position = new Vector2((float)n.ViewOriginX, (float)n.ViewOriginY);

            InitializeComponents();

            Node = n;
            Graph = g;
            Id = n.Id;

            N_OnNameChanged(Node);
            N_OnTextureChanged(Node);
            N_OnValueUpdated(Node);

            AddNodeEvents();
            AddNodePoints();

            var graph = Graph.Current;
            if (graph is Function)
            {
                Function f = graph as Function;
                f.OnOutputSet += F_OnOutputSet;        
            }

            //todo: add in output / input icons
            if (Node is OutputNode)
            {
                inputOutputIcon.Scale = Vector2.One;
            }
            else if (Node is InputNode)
            {
                inputOutputIcon.Scale = new Vector2(-1, 0);
            }
            else
            {
                inputOutputIcon.Visible = false;
            }
        }

        public Box2 GetViewSpaceRect()
        {
            Box2 world = AnchoredRect;
            if (Canvas == null || Graph == null) return world;
            Vector2 pos = Position * Graph.InverseZoom - Canvas.Cam.LocalPosition.Xy;
            return new Box2(pos, Size.X * Graph.InverseZoom, Size.Y * Graph.InverseZoom);
        }

        public GLTexture2D GetActiveBuffer()
        {
            return Node?.GetActiveBuffer();
        }

        #region Events
        private void AddNodeEvents()
        {
            var n = Node;
            if (n == null) return;
            n.OnInputAddedToNode += N_OnInputAddedToNode;
            n.OnInputRemovedFromNode += N_OnInputRemovedFromNode;
            n.OnOutputAddedToNode += N_OnOutputAddedToNode;
            n.OnOutputRemovedFromNode += N_OnOutputRemovedFromNode;
            n.OnTextureChanged += N_OnTextureChanged;
            n.OnValueUpdated += N_OnValueUpdated;
            n.OnNameChanged += N_OnNameChanged;
            n.OnTextureRebuilt += N_OnTextureRebuilt;
        }

        private void RemoveNodeEvents()
        {
            var n = Node;
            if (n == null) return;
            n.OnInputAddedToNode -= N_OnInputAddedToNode;
            n.OnInputRemovedFromNode -= N_OnInputRemovedFromNode;
            n.OnOutputAddedToNode -= N_OnOutputAddedToNode;
            n.OnOutputRemovedFromNode -= N_OnOutputRemovedFromNode;
            n.OnTextureChanged -= N_OnTextureChanged;
            n.OnValueUpdated -= N_OnValueUpdated;
            n.OnNameChanged -= N_OnNameChanged;
            n.OnTextureRebuilt -= N_OnTextureRebuilt;
        }

        private void N_OnTextureRebuilt(Node n)
        {
            preview.Texture = n.GetActiveBuffer();
            PreviewUpdated?.Invoke(this);
        }

        private void N_OnTextureChanged(Node n)
        {
            preview.Texture = n.GetActiveBuffer();
            PreviewUpdated?.Invoke(this);
        }

        private void N_OnNameChanged(Node n)
        {
            title.Text = n.Name;
        }

        private void N_OnValueUpdated(Node n)
        {
            if (n is MathNode)
            {
                desc.Text = n.GetDescription();

                if (string.IsNullOrEmpty(desc.Text))
                {
                    descArea.Visible = false;
                }
                else
                {
                    descArea.Visible = true;
                }
            }
            else
            {
                descArea.Visible = false;
            }
        }

        private void F_OnOutputSet(Node n)
        {
            //todo: add in output icon
        }

        private void N_OnOutputRemovedFromNode(Node n, NodeOutput inp, NodeOutput previous = null)
        {
            var uinp = outputs.Find(m => m.NodePoint == inp);
            if (uinp == null) return;
            outputs.Remove(uinp);
            outputsArea.RemoveChild(uinp);
            uinp.Dispose();
        }

        private void N_OnOutputAddedToNode(Node n, NodeOutput inp, NodeOutput previous = null)
        {
            UINodePoint outpoint = new UINodePoint(this, inp);
            outputs.Add(outpoint);
            outputsArea.AddChild(outpoint);

            if (previous == null) return;
            foreach (var cinp in previous.To)
            {
                //load connection
                LoadConnection(outpoint, cinp);
            }
            N_OnOutputRemovedFromNode(n, previous);
        }

        private void N_OnInputRemovedFromNode(Node n, NodeInput inp, NodeInput previous = null)
        {
            var uinp = inputs.Find(m => m.NodePoint == inp);
            if (uinp == null) return;
            inputs.Remove(uinp);
            inputsArea.RemoveChild(uinp);
            uinp.Dispose();
        }

        private void N_OnInputAddedToNode(Node n, NodeInput inp, NodeInput previous = null)
        {
            UINodePoint previousNodePoint = null;
            UINodePoint previousNodePointParent = null;

            if (previous != null)
            {
                previousNodePoint = inputs.Find(m => m.NodePoint == previous);
            }

            if (previousNodePoint != null)
            {
                previousNodePointParent = previousNodePoint.ParentNode;
            }

            UINodePoint inputpoint = new UINodePoint(this, inp);
            inputs.Add(inputpoint);
            inputsArea.AddChild(inputpoint);

            if (previousNodePointParent != null)
            {
                UINodePoint.Connect(previousNodePointParent, inputpoint);
            }

            if (previous != null)
            {
                N_OnInputRemovedFromNode(n, previous);
            }
        }
        #endregion

        #region Connection Handling
        public void LoadConnection(UINodePoint output, NodeInput inp)
        {
            var unode = Graph?.GetNode(inp.Node.Id);
            UINode uinode = unode as UINode;
            if (uinode == null) return;
            var input = uinode.inputs.Find(m => m.NodePoint == inp);
            if (input == null) return;
            UINodePoint.Connect(output, input);
        }

        public void LoadConnections()
        {
            for (int i = 0; i < outputs.Count; ++i)
            {
                var op = outputs[i].NodePoint as NodeOutput;
                if (op == null) continue;
                for (int k = 0; k < op.To.Count; ++k)
                {
                    var inp = op.To[k];
                    LoadConnection(outputs[i], inp);
                }
            }
        }
        #endregion

        /// <summary>
        /// Restores this instance from underlying graph data
        /// that may been updated for the specified node id
        /// </summary>
        public void Restore()
        {
            if (Graph == null) return;
            if (Graph.Current == null) return;
            if (!Graph.Current.NodeLookup.TryGetValue(Node.Id, out Node n))
            {
                return;
            }

            RemoveNodePoints();
            RemoveNodeEvents();

            Node = n;

            AddNodeEvents();
            AddNodePoints();

            N_OnValueUpdated(Node);
            N_OnNameChanged(Node);
            N_OnTextureChanged(Node);

            Restored?.Invoke(this);
        }

        #region Node Point Setup, Removal, Updates
        protected void AddNodePoints()
        {
            var n = Node;
            for (int i = 0; i < n.Outputs.Count; ++i)
            {
                var op = n.Outputs[i];
                UINodePoint outpoint = new UINodePoint(this, op);
                outputs.Add(outpoint);
                outputsArea.AddChild(outpoint);
            }

            for (int i = 0; i < n.Inputs.Count; ++i)
            {
                var op = n.Inputs[i];
                UINodePoint inputpoint = new UINodePoint(this, op);
                inputs.Add(inputpoint);
                inputsArea.AddChild(inputpoint);
            }
        }

        protected void RemoveNodePoints()
        {
            for (int i = 0; i < outputs.Count; ++i)
            {
                outputsArea.RemoveChild(outputs[i]);
                outputs[i]?.Dispose();
            }
            for (int i = 0; i < inputs.Count; ++i)
            {
                inputsArea.RemoveChild(inputs[i]);
                inputs[i]?.Dispose();
            }

            outputs.Clear();
            inputs.Clear();
        }
        #endregion

        private void InitializeComponents()
        {
            RelativeTo = Anchor.TopLeft; //set as topleft by default

            SnapMode = MovablePaneSnapMode.Grid;
            SnapTolerance = 32;

            selectable.BubbleEvents = false;

            selectable.NormalColor = new Vector4(0.15f, 0.15f, 0.15f, 1);
           
            selectable.Click += Selectable_Click;
            selectable.PointerUp += Selectable_PointerUp;
            selectable.PointerDown += Selectable_PointerDown;
            selectable.TargetGraphic = Background;

            DoubleClick += UINode_DoubleClick;

            Moved += UINode_Moved;
            MovedTo += UINode_MovedTo;

            //todo: make a container area for the center part
            //the container area will hold the following
            //preview, desc, inputs, outputs
            //the container area will be a contentSizeFitter
            //the node itself will become a stackpanel
            //with the title area first
            //followed by the container

            float nodePointSizePadding = UINodePoint.DEFAULT_SIZE + UINodePoint.DEFAULT_PADDING + UINodePoint.DEFAULT_PADDING;

            titleArea = new UIObject
            {
                RelativeTo = Anchor.Top
            };
            title = titleArea.AddComponent<UIText>();
            title.Alignment = InfinityUI.Components.TextAlignment.Center;

            descArea = new UIObject
            {
                Margin = new Box2(nodePointSizePadding, 26, nodePointSizePadding, nodePointSizePadding - 4),
                Visible = false,
                RelativeTo = Anchor.Fill
            };
            desc = descArea.AddComponent<UIText>();
            desc.Alignment = InfinityUI.Components.TextAlignment.Center;

            previewArea = new UIObject
            {
                Margin = new Box2(nodePointSizePadding, 26, nodePointSizePadding, nodePointSizePadding - 4),
                RelativeTo = Anchor.Fill,
            };
            preview = previewArea.AddComponent<UIImage>();
            previewArea.RaycastTarget = false;

            previewTransparency = new UIObject
            {
                Margin = new Box2(nodePointSizePadding, 26, nodePointSizePadding, nodePointSizePadding - 4),
                RelativeTo = Anchor.Fill
            };
            var transparencyBG = previewTransparency.AddComponent<UIImage>();
            transparencyBG.Texture = GridGenerator.CreateTransparent(32, 32);
            transparencyBG.Tiling = new Vector2(4, 4);
            previewTransparency.RaycastTarget = false;

            outputsArea = new UIObject
            {
                RaycastTarget = true,
                RelativeTo = Anchor.Right,
            };
            var stack = outputsArea.AddComponent<UIStackPanel>();
            stack.Direction = Orientation.Vertical;

            inputsArea = new UIObject
            {
                RaycastTarget = true,
                RelativeTo = Anchor.Left,
            };
            stack = inputsArea.AddComponent<UIStackPanel>();
            stack.Direction = Orientation.Vertical;

            iconsArea = new UIObject
            {
                Size = new Vector2(128,32),
                Position = new Vector2(0,-32),
                RaycastTarget = false,
            };
            var iconStack = iconsArea.AddComponent<UIStackPanel>();
            iconStack.Direction = Orientation.Horizontal;

            inputOutputIcon = new UIObject
            {
                Size = new Vector2(32, 32),
                RaycastTarget = false
            };
            var ioimg = inputOutputIcon.AddComponent<UIImage>();
            ioimg.Texture = UI.GetEmbeddedImage(Icons.OUTPUT, typeof(UINode));

            iconsArea.AddChild(inputOutputIcon);

            AddChild(previewTransparency);
            AddChild(previewArea);
            AddChild(descArea);
            AddChild(titleArea);
            AddChild(outputsArea);
            AddChild(inputsArea);
            AddChild(iconsArea);
        }

        #region User Input Events
        private void Selectable_PointerDown(UISelectable arg1, MouseEventArgs e)
        {
            if (e.Button.HasFlag(MouseButton.Left))
            {
                TryAndSelect();
            }
        }

        private void Selectable_PointerUp(UISelectable arg1, MouseEventArgs e)
        {
            if (isMoving)
            {
                GlobalEvents.Emit(GlobalEvent.MoveComplete, this, this);
                isMoving = false;
            }
        }

        private void Selectable_Click(UISelectable arg1, MouseEventArgs e)
        {
            if (e.Button.HasFlag(MouseButton.Right))
            {
                ShowContextMenu();
            }
        }

        private void UINode_DoubleClick(MovablePane obj)
        {
            //set 2d preview node
            GlobalEvents.Emit(GlobalEvent.Preview2D, this, Node);
        }
        private void UINode_Moved(MovablePane obj, Vector2 delta, MouseEventArgs e)
        {
            isMoving = true;

            Node.ViewOriginX = Position.X;
            Node.ViewOriginY = Position.Y;

            //send event to other node that are multiselected for delta move
            GlobalEvents.Emit(GlobalEvent.MoveSelected, this, delta);
        }
        private void UINode_MovedTo(MovablePane arg1, Vector2 pos)
        {
            Node.ViewOriginX = Position.X;
            Node.ViewOriginY = Position.Y;
        }

        protected void TryAndSelect()
        {
            if (Graph == null) return;

            if (UI.IsCtrlPressed)
            {
                //add node or remove from multi select
                Graph.ToggleSelect(this);
                return;
            }

            bool selected = Graph.IsSelected(Id);
            if (selected && Graph.Selected.Count > 1)
            {
                return;
            }

            //clear multiselect
            Graph.ClearSelection();

            //toggle graph select this
            Graph.Select(this);

            GlobalEvents.Emit(GlobalEvent.ViewParameters, this, Node);
        }

        protected void ShowContextMenu()
        {
            //setup context menu and show it
            if (Node is PixelProcessorNode)
            {

            }
            else if (Node is GraphInstanceNode)
            {

            }
            else if (Node is ImageNode)
            {

            }
            else if (Node is MathNode)
            {

            }
        }
        #endregion;

        protected void Export()
        {
            string path = null;

            Task.Run(async () =>
            {
                try
                {
                    var dialog = new SaveFileDialog();
                    FileDialogFilter filter = new FileDialogFilter();
                    filter.Name = "PNG";
                    filter.Extensions.Add("png");
                    dialog.Filters.Add(filter);
                    dialog.Title = "Export Image";
                    path = await dialog.ShowAsync(MainWindow.Instance);
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.StackTrace);
                }
            }).ContinueWith(t =>
            {
                if (string.IsNullOrEmpty(path)) return;
                GlobalEvents.Emit(GlobalEvent.ScheduleExport, this, new SingleExporter(Node, path));
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        public override void Dispose(bool disposing = true)
        {
            base.Dispose(disposing);

            RemoveNodeEvents();

            if (Graph == null) return;
            var graph = Graph.Current;
            if (graph != null)
            {
                if (graph is Function)
                {
                    Function f = graph as Function;
                    f.OnOutputSet -= F_OnOutputSet;
                }
            }
        }
    }
}
