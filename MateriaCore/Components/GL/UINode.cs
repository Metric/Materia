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

namespace MateriaCore.Components.GL
{
    public class UINode : MovablePane
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
        #endregion

        #region Graph Details
        public Node Node { get; protected set; }

        public Graph Graph { get; protected set; }

        public string Id { get; protected set; } = Guid.NewGuid().ToString(); //assign a default guid to it

        //note add back in Input Output Nodes here

        protected InfinityUI.Interfaces.KeyboardEventArgs lastKeyboardEvent;

        #endregion

        public UINode() : base(new Vector2(DEFAULT_WIDTH, DEFAULT_HEIGHT))
        {
            InitializeComponents();
        }

        public UINode(Node n) : base(new Vector2(DEFAULT_WIDTH, DEFAULT_HEIGHT))
        {
            Position = new Vector2((float)n.ViewOriginX, (float)n.ViewOriginY);

            InitializeComponents();

            Node = n;
            Graph = n.ParentGraph;
            Id = n.Id;

            title.Text = n.Name;

            preview.Texture = n.GetActiveBuffer();

            //todo add in output and inputs areas

            //todo handle input and output node events
            //etc. they are currently commented out

            n.OnInputAddedToNode += N_OnInputAddedToNode;
            n.OnInputRemovedFromNode += N_OnInputRemovedFromNode;
            n.OnOutputAddedToNode += N_OnOutputAddedToNode;
            n.OnOutputRemovedFromNode += N_OnOutputRemovedFromNode;
            n.OnTextureChanged += N_OnTextureChanged;
            n.OnValueUpdated += N_OnValueUpdated;
            n.OnNameChanged += N_OnNameChanged;

            if (Graph is Function)
            {
                Function f = Graph as Function;
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

        private void N_OnOutputRemovedFromNode(Node n, NodeOutput inp, NodeOutput previous = null)
        {
            /*var uinp = OutputNodes.Find(m => m.Output == inp);

            if (uinp != null)
            {
                //whoops forgot to dispose
                //on the uinodepoint to remove previous connects
                //etc
                uinp.Dispose();
                OutputStack.Children.Remove(uinp);
                OutputNodes.Remove(uinp);
            }*/
        }

        private void N_OnOutputAddedToNode(Node n, NodeOutput inp, NodeOutput previous = null)
        {
            /*UINodePoint outpoint = new UINodePoint(this, Graph);
            outpoint.Output = inp;
            outpoint.VerticalAlignment = VerticalAlignment.Center;
            OutputNodes.Add(outpoint);
            OutputStack.Children.Add(outpoint);
            outpoint.UpdateColor();

            if (previous != null)
            {
                foreach (var cinp in inp.To)
                {
                    LoadConnection(cinp.Node.Id);
                }
            }*/
        }

        private void N_OnInputRemovedFromNode(Node n, NodeInput inp, NodeInput previous = null)
        {
            /*var uinp = InputNodes.Find(m => m.Input == inp);

            if (uinp != null)
            {
                //whoops forgot to dispose
                //on the uinodepoint to remove previous connects
                //etc
                uinp.Dispose();
                InputStack.Children.Remove(uinp);
                InputNodes.Remove(uinp);
            }*/
        }

        private void N_OnInputAddedToNode(Node n, NodeInput inp, NodeInput previous = null)
        {
            //need to take into account previous
            //aka we are just replacing the previous one
            /*UINodePoint previousNodePoint = null;
            UINodePoint previousNodePointParent = null;

            if (previous != null)
            {
                previousNodePoint = InputNodes.Find(m => m.Input == previous);
            }

            if (previousNodePoint != null)
            {
                previousNodePointParent = previousNodePoint.ParentNode;
            }

            UINodePoint inputpoint = new UINodePoint(this, Graph);
            inputpoint.Input = inp;
            inputpoint.VerticalAlignment = VerticalAlignment.Center;
            InputStack.Children.Add(inputpoint);
            InputNodes.Add(inputpoint);
            inputpoint.UpdateColor();

            //try and reconnect previous parent node to it graphically
            if (previousNodePointParent != null)
            {
                previousNodePointParent.ConnectToNode(inputpoint, true);
            }

            if (previous != null)
            {
                N_OnInputRemovedFromNode(n, previous);
            }*/
        }

        private void InitializeComponents()
        {
            SnapMode = MovablePaneSnapMode.Grid;
            SnapTolerance = 32;

            toggleable = AddComponent<UIToggleable>();

            selectable.Click += Selectable_Click;
            selectable.KeyDown += Selectable_KeyDown;
            selectable.KeyUp += Selectable_KeyUp;
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

            AddChild(previewArea);
            AddChild(descArea);
            AddChild(titleArea);
        }

        private void Selectable_PointerDown(UISelectable arg1, InfinityUI.Interfaces.MouseEventArgs arg2)
        {
            ZOrder = -1; //put on top of other nodes
        }

        private void Selectable_PointerUp(UISelectable arg1, InfinityUI.Interfaces.MouseEventArgs arg2)
        {
            ZOrder = 0; //restore z order to default
        }

        private void Selectable_KeyUp(UISelectable arg1, InfinityUI.Interfaces.KeyboardEventArgs arg2)
        {
            lastKeyboardEvent = arg2;
        }

        private void Selectable_KeyDown(UISelectable arg1, InfinityUI.Interfaces.KeyboardEventArgs arg2)
        {
            lastKeyboardEvent = arg2;
        }

        private void UINode_DoubleClick(MovablePane obj)
        {
            //set 2d preview node
        }

        private void UINode_Moved(MovablePane obj, Vector2 delta)
        {
            Node.ViewOriginX = Position.X;
            Node.ViewOriginY = Position.Y;

            //send event to other node that are multiselected for delta move
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
            if (lastKeyboardEvent != null && lastKeyboardEvent.IsCtrl)
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

            Parameters.Current?.Set(Node);
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

            if (Graph is Function)
            {
                Function f = Graph as Function;
                f.OnOutputSet -= F_OnOutputSet;
            }

            if (disposing)
            {
                Graph.Remove(Node);
            }

            //remove node input output node ui

            //update 3d and 2d preview
        }
    }
}
