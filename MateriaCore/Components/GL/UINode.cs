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
using InfinityUI.Interfaces;
using Materia.Rendering.Textures;
using System.Diagnostics;
using Materia.Graph.Exporters;

namespace MateriaCore.Components.GL
{
    public class UINode :  UINodeBase
    {
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

        protected UIObject fitterArea;
        protected UIContentFitter fitter;
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
            OnPreviewUpdated();
        }

        private void N_OnTextureChanged(Node n)
        {
            preview.Texture = n.GetActiveBuffer();
            OnPreviewUpdated();
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
            if (inputOutputIcon != null)
            {
                inputOutputIcon.Visible = true;
                inputOutputIcon.Scale = Vector2.One;
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
                UINodePoint.Connect(previousNodePointParent, inputpoint);
            }

            if (previous != null)
            {
                N_OnInputRemovedFromNode(n, previous);
            }
        }
        #endregion

        #region Connection Handling
        public override void LoadConnection(UINodePoint output, NodeInput inp)
        {
            var unode = Graph?.GetNode(inp.Node.Id);
            UINode uinode = unode as UINode;
            if (uinode == null) return;
            var input = uinode.inputs.Find(m => m.NodePoint == inp);
            if (input == null) return;
            UINodePoint.Connect(output, input);
        }

        public override void LoadConnections()
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

        protected override void OnBeforeRestored()
        {
            RemoveNodePoints();
            RemoveNodeEvents();
        }

        protected override void OnRestored()
        {
            AddNodeEvents();
            AddNodePoints();

            N_OnValueUpdated(Node);
            N_OnNameChanged(Node);
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

            float nodePointSizePadding = UINodePoint.DEFAULT_SIZE + UINodePoint.DEFAULT_PADDING + UINodePoint.DEFAULT_PADDING;

            fitterArea = new UIObject()
            {
                Name = "NodeContentFitter",
                RelativeTo = Anchor.TopLeft,
                RaycastTarget = true,
            };
            fitter = fitterArea.AddComponent<UIContentFitter>();


            titleArea = new UIObject
            {
                Name = "NodeTitle",
                RelativeTo = Anchor.Top
            };
            title = titleArea.AddComponent<UIText>();
            title.Alignment = InfinityUI.Components.TextAlignment.Center;

            descArea = new UIObject
            {
                Name = "DescArea",
                Margin = new Box2(nodePointSizePadding, 26, nodePointSizePadding, nodePointSizePadding - 4),
                Visible = false,
                RelativeTo = Anchor.Fill
            };
            desc = descArea.AddComponent<UIText>();
            desc.Alignment = InfinityUI.Components.TextAlignment.Center;

            previewArea = new UIObject
            {
                Name = "PreviewArea",
                Margin = new Box2(nodePointSizePadding, 26, nodePointSizePadding, nodePointSizePadding - 4),
                RelativeTo = Anchor.Fill,
            };
            preview = previewArea.AddComponent<UIImage>();
            previewArea.RaycastTarget = false;

            previewTransparency = new UIObject
            {
                Name = "PreviewTransparency",
                Margin = new Box2(nodePointSizePadding, 26, nodePointSizePadding, nodePointSizePadding - 4),
                RelativeTo = Anchor.Fill
            };


            var transparencyBG = previewTransparency.AddComponent<UIImage>();
            transparencyBG.Texture = GridGenerator.CreateTransparent(32, 32);
            transparencyBG.Tiling = new Vector2(4, 4);
            previewTransparency.RaycastTarget = false;

            outputsArea = new UIObject
            {
                Name = "NodeOutputs",
                RaycastTarget = true,
                RelativeTo = Anchor.Right,
            };
            var stack = outputsArea.AddComponent<UIStackPanel>();
            stack.Direction = Orientation.Vertical;

            inputsArea = new UIObject
            {
                Name = "NodeInputs",
                RaycastTarget = true,
                RelativeTo = Anchor.Left,
            };
            stack = inputsArea.AddComponent<UIStackPanel>();
            stack.Direction = Orientation.Vertical;

            iconsArea = new UIObject
            {
                Name = "NodeIcons",
                Size = new Vector2(128,32),
                RaycastTarget = false,
            };
            var iconStack = iconsArea.AddComponent<UIStackPanel>();
            iconStack.Direction = Orientation.Horizontal;

            inputOutputIcon = new UIObject
            {
                Name = "InputOutputIcon",
                Size = new Vector2(32, 32),
                RaycastTarget = false
            };
            var ioimg = inputOutputIcon.AddComponent<UIImage>();
            ioimg.Texture = UI.GetEmbeddedImage(Icons.OUTPUT, typeof(UINode));

            iconsArea.AddChild(inputOutputIcon);

            previewTransparency.AddChild(previewArea);

            fitterArea.AddChild(previewTransparency);
            fitterArea.AddChild(descArea);
            fitterArea.AddChild(outputsArea);
            fitterArea.AddChild(inputsArea);

            stack = AddComponent<UIStackPanel>();
            stack.Direction = Orientation.Vertical;

            AddChild(iconsArea);
            AddChild(titleArea);
            AddChild(fitterArea);
        }

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
