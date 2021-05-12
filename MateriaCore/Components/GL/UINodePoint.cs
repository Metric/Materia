using InfinityUI.Components;
using InfinityUI.Core;
using Materia.Nodes;
using Materia.Rendering.Attributes;
using Materia.Rendering.Mathematics;
using MateriaCore.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MateriaCore.Components.GL
{
    public class UINodePoint : UIObject
    {
        public const float DEFAULT_SIZE = 18;
        public const float DEFAULT_PADDING = 2;

        public static UINodePoint SelectedOrigin { get; set; } = null;

        protected static Vector4 White = new Vector4(1, 1, 1, 1);
        protected static Vector4 Red = new Vector4(1, 0.25f, 0.25f, 1);
        protected static Vector4 GrayInputOutput = new Vector4(0.5f, 0.5f, 0.5f, 1);
        protected static Vector4 ColorInputOutput = new Vector4(162f / 255f, 0, 238f / 255f, 1);
        protected static Vector4 FloatInputOutput = new Vector4(0, 1, 162f / 255f, 1);
        protected static Vector4 Float2InputOutput = new Vector4(2f / 255f, 216f / 255f, 138f / 255f, 1);
        protected static Vector4 Float3InputOutput = new Vector4(0, 170f / 255f, 108f / 255f, 1);
        protected static Vector4 Float4InputOutput = new Vector4(0, 149f / 255f, 93f / 255f, 1);
        protected static Vector4 MatrixInputOutput = new Vector4(191f / 255f, 131f / 255f, 43f / 255f, 1);
        protected static Vector4 BoolInputOutput = new Vector4(1, 145f / 255f, 250f / 255f, 1);
        protected static Vector4 AnyFloatInputOutput = new Vector4(80f / 255f, 148f / 255f, 123f / 255f, 1);

        public Vector4 Color
        {
            get
            {
                if (NodePoint == null) return White;
                NodeType type = NodePoint.Type;
                if ((type & NodeType.Color) != 0)
                {
                    return ColorInputOutput;
                }
                else if((type & NodeType.Gray) != 0)
                {
                    return GrayInputOutput;
                }
                else if((type & NodeType.Float) != 0 && (type & NodeType.Float2) != 0 
                    && (type & NodeType.Float3) != 0 && (type & NodeType.Float4) != 0)
                {
                    return AnyFloatInputOutput;
                }
                else if((type & NodeType.Float) != 0)
                {
                    return FloatInputOutput;
                }
                else if((type & NodeType.Float2) != 0)
                {
                    return Float2InputOutput;
                }
                else if((type & NodeType.Float3) != 0)
                {
                    return Float3InputOutput;
                }
                else if((type & NodeType.Float4) != 0)
                {
                    return Float4InputOutput;
                }
                else if((type & NodeType.Matrix) != 0)
                {
                    return MatrixInputOutput;
                }
                else if((type & NodeType.Execute) != 0)
                {
                    return Red;
                }
                else if((type & NodeType.Bool) != 0)
                {
                    return BoolInputOutput;
                }
                return White;
            }
        }

        public UINode Node { get; protected set; }
        public UINodePoint ParentNode { get; protected set; }

        public UIImage Background { get; protected set; }

        public INodePoint NodePoint { get; protected set; }

        public string Id { get; protected set; } = Guid.NewGuid().ToString();

        protected List<UINodePoint> to = new List<UINodePoint>();
        protected Dictionary<string, UINodePath> paths = new Dictionary<string, UINodePath>();

        //for debugging purposes only
        static ulong nodeCount = 0;

        #region Components
        protected UIObject nodeNameArea;
        protected UIText nodeName;
        protected UISelectable selectable;
        #endregion

        public UINodePoint() : base()
        {
            Size = new Vector2(DEFAULT_SIZE, DEFAULT_SIZE);
            Padding = new Box2(DEFAULT_PADDING, 
                                DEFAULT_PADDING, 
                                DEFAULT_PADDING, 
                                DEFAULT_PADDING);
        }

        public UINodePoint(UINode n, INodePoint point)
        {
            Name = "NodePoint" + nodeCount++;
            Node = n;
            NodePoint = point;

            Size = new Vector2(DEFAULT_SIZE, DEFAULT_SIZE);
            Padding = new Box2(DEFAULT_PADDING, 
                                DEFAULT_PADDING, 
                                DEFAULT_PADDING, 
                                DEFAULT_PADDING);

            InitializeComponents();

            if (point is NodeOutput)
            {
                var nout = point as NodeOutput;
                nout.OnOutputChanged += Nout_OnOutputChanged;
            }
        }

        private void Nout_OnOutputChanged(NodeOutput output)
        {
            if (selectable != null)
            {
                selectable.NormalColor = Color;
            }
        }

        public int GetOutIndex(UINodePoint p2)
        {
            return to.IndexOf(p2);
        }

        protected void InitializeComponents()
        {
            //todo: assign background image from embedded resource

            Background = AddComponent<UIImage>();
            Background.Texture = UI.GetEmbeddedImage(Icons.CIRCLE, typeof(UINodePoint));
            selectable = AddComponent<UISelectable>();
            selectable.TargetGraphic = Background;
            selectable.NormalColor = Color;
            selectable.PointerDown += Selectable_PointerDown;
            selectable.PointerEnter += Selectable_PointerEnter;
            selectable.PointerExit += Selectable_PointerExit;
            selectable.BeforeUpdateTarget += Selectable_BeforeUpdateTarget;
            selectable.BubbleEvents = false;

            nodeNameArea = new UIObject();
            nodeNameArea.RelativeTo = NodePoint is NodeInput ? Anchor.CenterLeft : Anchor.CenterRight; //this can change based on if input or output
            nodeNameArea.Position = new Vector2(18, 0);
            nodeName = nodeNameArea.AddComponent<UIText>();
            nodeName.Alignment = NodePoint is NodeInput ? TextAlignment.Right : TextAlignment.Left;
            nodeName.Text = NodePoint.Name;

            AddChild(nodeNameArea);
        }

        private void Selectable_BeforeUpdateTarget(UISelectable obj)
        {
            selectable.NormalColor = Color;
        }

        private void Selectable_PointerExit(UISelectable arg1, InfinityUI.Interfaces.MouseEventArgs e)
        {
            //update mouse cursor
            if (Node.Graph.ReadOnly) return;
        }

        private void Selectable_PointerEnter(UISelectable arg1, InfinityUI.Interfaces.MouseEventArgs e)
        {
            //update mouse cursor
            if (Node.Graph.ReadOnly) return;
        }

        private void Selectable_PointerDown(UISelectable arg1, InfinityUI.Interfaces.MouseEventArgs e)
        {
            if (Node.Graph.ReadOnly) return;
            if (e.Button.HasFlag(InfinityUI.Interfaces.MouseButton.Left) && !UI.IsAltPressed)
            {
                if (SelectedOrigin == this)
                {
                    SelectedOrigin = null;
                }
                else if(SelectedOrigin != this && SelectedOrigin != null)
                {
                    Connect(SelectedOrigin, true);
                    SelectedOrigin = null;
                }
                else
                {
                    SelectedOrigin = this;
                }
            }
            else if(e.Button.HasFlag(InfinityUI.Interfaces.MouseButton.Left) && UI.IsAltPressed)
            {
                Disconnect();
            }
        }

        public bool CanConnect(UINodePoint p)
        {
            if (p.NodePoint is NodeInput && NodePoint is NodeInput) return false;
            if (p.NodePoint is NodeOutput && NodePoint is NodeOutput) return false;
            return (p.NodePoint.Type & NodePoint.Type) != 0;
        }

        public bool IsCircular(UINodePoint p)
        {
            return p.Node == Node;
        }

        public void Disconnect(bool removeFromGraph = true)
        {
            ParentNode?.Disconnect(this, removeFromGraph);
            for (int i = 0; i < to.Count; ++i)
            {
                Disconnect(to[i], removeFromGraph);
            }
        }

        public void Disconnect(UINodePoint p, bool removeFromGraph = true)
        {
            if (p == null) return;

            //todo: reimplement node paths
            if (paths.TryGetValue(p.Id, out UINodePath path))
            {
                paths.Remove(p.Id);
                Node?.Graph?.RemoveChild(path);
                path?.Dispose();
            }

            if (p.ParentNode != this) return;
            if (p.NodePoint is NodeInput)
            {
                NodeInput input = p.NodePoint as NodeInput;
                if (removeFromGraph)
                {
                    input.Reference?.Remove(input);
                }
            }
            to.Remove(p);
            p.ParentNode = null;
        }

        public void Connect(UINodePoint p, bool applyToGraph = false)
        {
            if (p.ParentNode == this) return;
            p.ParentNode?.Disconnect(p);

            if (p.NodePoint is NodeOutput && NodePoint is NodeInput)
            {
                ParentNode = p;
                p.to.Add(this);
                UINodePath path = new UINodePath(p, this, NodePoint.Type == NodeType.Execute);
                Node?.Graph?.AddChild(path);
                p.paths[Id] = path;

                if (applyToGraph)
                {
                    (p.NodePoint as NodeOutput).Add(NodePoint as NodeInput);
                }
            }
            else if(p.NodePoint is NodeInput && NodePoint is NodeOutput)
            {
                p.ParentNode = this;
                to.Add(p);
                UINodePath path = new UINodePath(this, p, NodePoint.Type == NodeType.Execute);
                Node?.Graph?.AddChild(path);
                paths[p.Id] = path;

                if (applyToGraph)
                {
                    (NodePoint as NodeOutput).Add(p.NodePoint as NodeInput);
                }
            }
        }

        public override void Update()
        {
            base.Update();

            if (NodePoint is NodeOutput)
            {
                var activePaths = paths.Values.ToList();
                for (int i = 0; i < activePaths.Count; ++i)
                {
                    activePaths[i]?.Update();
                }
            }
            else
            {
                ParentNode?.Update();
            }
        }

        public override void Dispose(bool disposing = true)
        {
            if (NodePoint is NodeOutput)
            {
                var nout = NodePoint as NodeOutput;
                nout.OnOutputChanged -= Nout_OnOutputChanged;
            }

            Disconnect(false);
            base.Dispose(disposing);
        }
    }
}
