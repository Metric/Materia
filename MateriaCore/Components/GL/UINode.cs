using System;
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

namespace MateriaCore.Components.GL
{
    public class UINode : MovablePane, IGraphNode
    {
        public const int DEFAULT_HEIGHT = 50;
        public const int DEFAULT_WIDTH = 120;

        #region UI Components  
        protected UIObject titleArea;
        protected UIText title;

        protected UIObject descArea;
        protected UIText desc;

        protected UIObject previewArea;
        protected UIImage preview;

        protected UIToggleable toggleable;

        protected UIObject inputsArea;
        protected UIObject outputsArea;

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

        public UINode() : base(new Vector2(DEFAULT_WIDTH, DEFAULT_HEIGHT))
        {
            RelativeTo = Anchor.TopLeft;
            InitializeComponents();
        }

        public UINode(UIGraph g, Node n) : base(new Vector2(DEFAULT_WIDTH, DEFAULT_HEIGHT))
        {
            RelativeTo = Anchor.TopLeft;
            Position = new Vector2((float)n.ViewOriginX, (float)n.ViewOriginY);

            InitializeComponents();

            Node = n;
            Graph = g;
            Id = n.Id;

            title.Text = n.Name;

            preview.Texture = n.GetActiveBuffer();

            //todo add in output and inputs areas

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

            //todo handle input and output node events
            //etc. they are currently commented out

            n.OnInputAddedToNode += N_OnInputAddedToNode;
            n.OnInputRemovedFromNode += N_OnInputRemovedFromNode;
            n.OnOutputAddedToNode += N_OnOutputAddedToNode;
            n.OnOutputRemovedFromNode += N_OnOutputRemovedFromNode;
            n.OnTextureChanged += N_OnTextureChanged;
            n.OnValueUpdated += N_OnValueUpdated;
            n.OnNameChanged += N_OnNameChanged;

            var graph = Graph.Current;
            if (graph is Function)
            {
                Function f = graph as Function;
                f.OnOutputSet += F_OnOutputSet;

                //todo rework this area
                /*
                if (n == f.OutputNode)
                {
                    OutputIcon.Visibility = Visibility.Visible;
                }
                else
                {
                    OutputIcon.Visibility = Visibility.Collapsed;
                }

                if (n == f.Execute)
                {
                    InputIcon.Visibility = Visibility.Visible;
                }
                else
                {
                    InputIcon.Visibility = Visibility.Collapsed;
                }*/
            }
            else
            {
                //todo rework this area
                /*
                if (n is OutputNode)
                {
                    OutputIcon.Visibility = Visibility.Visible;
                }
                else
                {
                    OutputIcon.Visibility = Visibility.Collapsed;
                }

                if (n is InputNode)
                {
                    InputIcon.Visibility = Visibility.Visible;
                }
                else
                {
                    InputIcon.Visibility = Visibility.Collapsed;
                }*/
            }

            if (n is MathNode)
            {
                descArea.Visible = true;
                desc.Text = n.GetDescription();

                if (string.IsNullOrEmpty(desc.Text))
                {
                    descArea.Visible = false;
                }
            }
            else
            {
                descArea.Visible = false;
            }
        }

        private void N_OnTextureChanged(Node n)
        {
            preview.Texture = n.GetActiveBuffer();
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
        }

        private void F_OnOutputSet(Node n)
        {
            /*if (n == Node)
            {
                OutputIcon.Visibility = Visibility.Visible;
            }
            else
            {
                OutputIcon.Visibility = Visibility.Collapsed;
            }*/
        }

        public void LoadConnection(UINodePoint output, NodeInput inp)
        {
            var unode = Graph?.GetNode(inp.Node.Id);
            UINode uinode = unode as UINode;
            if (uinode == null) return;
            var input = uinode.inputs.Find(m => m.NodePoint == inp);
            if (input == null) return;
            output?.Connect(input);
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
                previousNodePointParent.Connect(inputpoint);
            }

            if (previous != null)
            {
                N_OnInputRemovedFromNode(n, previous);
            }
        }

        private void InitializeComponents()
        {
            SnapMode = MovablePaneSnapMode.Grid;
            SnapTolerance = 32;

            toggleable = AddComponent<UIToggleable>();

            selectable.Click += Selectable_Click;
            selectable.PointerUp += Selectable_PointerUp;
            selectable.PointerDown += Selectable_PointerDown;
            selectable.TargetGraphic = Background;

            DoubleClick += UINode_DoubleClick;

            Moved += UINode_Moved;

            titleArea = new UIObject();
            titleArea.RelativeTo = Anchor.TopHorizFill;
            title = titleArea.AddComponent<UIText>();
            title.Alignment = InfinityUI.Components.TextAlignment.Center;

            descArea = new UIObject();
            descArea.RelativeTo = Anchor.Fill;
            descArea.Padding = new Box2(10, 20, 10, 10);
            descArea.Visible = false;
            desc = descArea.AddComponent<UIText>();
            desc.Alignment = InfinityUI.Components.TextAlignment.Center;

            previewArea = new UIObject();
            previewArea.RelativeTo = Anchor.Fill;
            previewArea.Padding = new Box2(10, 20, 10, 10);
            preview = previewArea.AddComponent<UIImage>();

            outputsArea = new UIObject();
            outputsArea.RelativeTo = Anchor.CenterLeft;
            var stack = outputsArea.AddComponent<UIStackPanel>();
            stack.Direction = Orientation.Vertical;

            inputsArea = new UIObject();
            inputsArea.RelativeTo = Anchor.CenterRight;
            stack = inputsArea.AddComponent<UIStackPanel>();
            stack.Direction = Orientation.Vertical;

            AddChild(previewArea);
            AddChild(descArea);
            AddChild(titleArea);
            AddChild(outputsArea);
            AddChild(inputsArea);

            fitter = AddComponent<UIContentFitter>();
            fitter.Axis = Axis.Vertical;
        }

        private void Selectable_PointerDown(UISelectable arg1, InfinityUI.Interfaces.MouseEventArgs arg2)
        {
            ZOrder = -1; //put on top of other nodes
        }

        private void Selectable_PointerUp(UISelectable arg1, InfinityUI.Interfaces.MouseEventArgs arg2)
        {
            ZOrder = 0; //restore z order to default
        }

        private void UINode_DoubleClick(MovablePane obj)
        {
            //set 2d preview node
            GlobalEvents.Emit(GlobalEvent.Preview2D, this, Node);
        }

        private void UINode_Moved(MovablePane obj, Vector2 delta)
        {
            Node.ViewOriginX = Position.X;
            Node.ViewOriginY = Position.Y;

            //update node point paths
            UpdateNodePoints();

            //send event to other node that are multiselected for delta move
            GlobalEvents.Emit(GlobalEvent.MoveSelected, this, delta);
        }

        private void UpdateNodePoints()
        {
            for (int i = 0; i < outputs.Count; ++i)
            {
                outputs[i]?.Update();
            }
            for (int i = 0; i < inputs.Count; ++i)
            {
                inputs[i]?.Update();
            }
        }

        private void Selectable_Click(UISelectable arg1, InfinityUI.Interfaces.MouseEventArgs arg2)
        {
            if (arg2.Button.HasFlag(InfinityUI.Interfaces.MouseButton.Left))
            {
                TryAndSelect();
            }
            else if(arg2.Button.HasFlag(InfinityUI.Interfaces.MouseButton.Right))
            {
                ShowContextMenu();
            }
        }

        protected void TryAndSelect()
        {
            if (UI.IsCtrlPressed)
            {
                //add node to multi select
                return;
            }

            bool selected = false;

            //try and see if ui node is already in graph select
            if (selected)
            {
                return;
            }

            //clear multiselect
            //toggle graph select this

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

        protected void Export()
        {
            var dialog = new SaveFileDialog();
            FileDialogFilter filter = new FileDialogFilter();
            filter.Extensions.Add("png");
            dialog.Filters.Add(filter);
            dialog.Title = "Export Image";

            string path = null;

            Task.Run(async () =>
            {
                path = await dialog.ShowAsync(null); //note need to assign actual main window here
            }).ContinueWith(t =>
            {
                if (string.IsNullOrEmpty(path)) return;

                byte[] bits = Node?.GetPreview(Node.Width, Node.Height);

                if (bits == null) return;

                Task.Run(() =>
                {
                    try
                    {
                        RawBitmap bmp = new RawBitmap(Node.Width, Node.Height, bits);
                        var src = bmp.ToAvBitmap();
                        src.Save(path);
                    }
                    catch (Exception e)
                    {
                        MLog.Log.Error(e);
                    }
                });
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        public override void Dispose(bool disposing = true)
        {
            base.Dispose(disposing);

            if (Node == null) return;

            Node.OnInputAddedToNode -= N_OnInputAddedToNode;
            Node.OnInputRemovedFromNode -= N_OnInputRemovedFromNode;
            Node.OnOutputAddedToNode -= N_OnOutputAddedToNode;
            Node.OnOutputRemovedFromNode -= N_OnOutputRemovedFromNode;
            Node.OnTextureChanged -= N_OnTextureChanged;
            Node.OnNameChanged -= N_OnNameChanged;
            Node.OnValueUpdated -= N_OnValueUpdated;

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
