using InfinityUI.Components;
using InfinityUI.Controls;
using InfinityUI.Core;
using Materia.Graph;
using Materia.Nodes;
using Materia.Nodes.Atomic;
using Materia.Nodes.Items;
using Materia.Rendering.Mathematics;
using MateriaCore.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MateriaCore.Components.GL
{
    //move this to internal
    public struct UIGraphCopyData
    {
        public List<string> nodes;
        public Dictionary<string, string> parameters;
    }

    public enum GraphStackType
    {
        Parameter,
        Pixel,
        CustomFunction
    }

    public enum UIGraphState
    {
        None,
        Loading,
        LoadingWithTemplate
    }

    public class GraphStackItem
    {
        public string parameter;
        public Graph graph;
        public Node node;
        public string id;
        public GraphStackType type;

        internal class GraphStackItemData
        {
            public string id;
            public string parameter;
            public GraphStackType type;
        }

        public override string ToString()
        {
            GraphStackItemData d = new GraphStackItemData
            {
                type = type,
                id = id,
                parameter = parameter
            };
            return JsonConvert.SerializeObject(d);
        }

        public static GraphStackItem Get(string data)
        {
            GraphStackItemData d = JsonConvert.DeserializeObject<GraphStackItemData>(data);

            GraphStackItem i = new GraphStackItem
            {
                id = d.id,
                parameter = d.parameter,
                type = d.type
            };

            return i;
        }
    }

    public class UIGraph : MovablePane
    {
        public event Action<UIGraph> NameChanged;

        protected enum UIGraphMouseMode
        {
            Normal,
            Select
        }

        protected const float ZOOM_SPEED = 1.0f / 10f;

        #region Subviews
        #endregion

        #region Components
        protected UICanvas canvas;
        protected UIImage background;
        #endregion

        #region General
        protected UIGraphMouseMode mouseMode = UIGraphMouseMode.Normal;

        public string Id { get; protected set; } = Guid.NewGuid().ToString();

        public Graph Root { get; protected set; }

        public Graph Current { get; protected set; }

        protected float zoom = 1;
        protected float invZoom = 1;
        public float Zoom
        {
            get => zoom;
        }

        public bool ReadOnly
        {
            get => Current != null && Current.ReadOnly;
        }

        public bool Modified
        {
            get
            {
                if (Root != null)
                {
                    return Root.Modified;
                }

                return false;
            }
        }

        public string GraphName
        {
            get
            {
                if (Root != null)
                {
                    return Root.Name;
                }

                return RawRootName;
            }
        }

        protected UIGraphState graphState = UIGraphState.None;

        public string Filename { get; protected set; }
        #endregion

        #region Selection
        protected HashSet<string> selectionStartedIn = new HashSet<string>();
        public List<IGraphNode> Selected { get; protected set; } = new List<IGraphNode>();
        public HashSet<string> SelectedIds { get; protected set; } = new HashSet<string>();
        protected Box2 selectionRect = new Box2(0,0,0,0);
        #endregion

        #region Graph Stack
        public string RawRootName { get; protected set; }
        public string RawRoot { get; protected set; }
        public string RawRootCWD { get; protected set; }
        public string[] RawStack { get; protected set; }

        protected Stack<GraphStackItem> graphStack = new Stack<GraphStackItem>();
        #endregion

        #region Pins
        protected int pinIndex = 0;
        //todo: replace IGraphNode with UIPinNode once implemented
        protected List<IGraphNode> pins = new List<IGraphNode>();
        #endregion

        #region Archive Details
        public bool FromArchive { get; protected set; }
        public string FromArchivePath { get; protected set; }
        #endregion

        protected Dictionary<string, IGraphNode> nodes = new Dictionary<string, IGraphNode>();

        public UIGraph(Vector2 size, Graph template = null) : base(size) 
        {
            InitializeComponents();

            if (template != null)
            {
                graphState = UIGraphState.LoadingWithTemplate;
                RawRoot = template.GetJson();
            }

            //handle bread crumbs init here sort of
            //hide mouse connection preview
        }

        protected void AddGlobalEvents()
        {
            GlobalEvents.On(GlobalEvent.MoveSelected, OnMoveSelected);
        }

        protected void RemoveGlobalEvents()
        {
            GlobalEvents.Off(GlobalEvent.MoveSelected, OnMoveSelected);
        }

        protected void InitializeComponents()
        {
            //don't actually allow drag / snapping
            //we just want to be able to accept the Moved Event
            //for delta stuff
            //why reinvent the wheel right?
            MoveAxis = Axis.None;
            SnapMode = MovablePaneSnapMode.None;

            canvas = AddComponent<UICanvas>();
            canvas.Resize(Size.X, Size.Y);

            background = AddComponent<UIImage>();
            background.Color = new Vector4(0.25f, 0.25f, 0.25f, 1);

            selectable.Wheel += Selectable_Wheel;

            Moved += UIGraph_Moved;
        }

        #region Loading Handlers
        public void Load(string path)
        {
            if (!System.IO.File.Exists(path)) return;

            string directory = System.IO.Path.GetDirectoryName(path);
            Filename = path;
            string data = System.IO.File.ReadAllText(path);
            Load(data, directory);
        }

        public void Load(string data, string cwd, bool readOnly = false)
        {
            InternalDispose();

            Graph g = new Image("Untitled", 256, 256);
            if (string.IsNullOrEmpty(data))
            {
                graphState = UIGraphState.None;
                return;
            }

            g.CWD = cwd;

            long stamp = Environment.TickCount;
            g.FromJson(data);
            long stampDiff = Environment.TickCount - stamp;
            MLog.Log.Debug(string.Format("Graph data loaded in {0:0}ms", stampDiff));
            Root = g;
            g.ReadOnly = readOnly;

            Load(g);
        }

        public void Load(Graph g)
        {
            if (g == Current) return;
            if (g == null) return;

            pinIndex = 0;

            if (Current != null)
            {
                Current.OnGraphUpdated -= Current_OnGraphUpdated;
                Current.OnGraphNameChanged -= Current_OnGraphNameChanged;
                Current.OnHdriChanged -= Current_OnHdriChanged;
            }

            Clear();

            Current = g;

            zoom = g.Zoom;
            invZoom = 1.0f / zoom;

            canvas.Cam.LocalPosition = new Vector3((float)g.ShiftX, (float)g.ShiftY, 0);
            canvas.Scale = zoom;

            //update zoom text etc

            //todo: reimplement HdriManager
            //Current.HdriImages = HdriManager.Available.ToArray();
            
            Current.OnGraphUpdated += Current_OnGraphUpdated;
            Current.OnGraphNameChanged += Current_OnGraphNameChanged;
            Current.OnHdriChanged += Current_OnHdriChanged;

            //clear crumbs etc

            InitializeNodes();
            Current.TryAndProcess();
        }
        #endregion

        #region Node Loading
        protected void InitializeNodes()
        {
            if (Current == null) return;

            if (Current is Function)
            {
                Function fg = (Function)Current;
                fg.SetAllVars();

                //update output requirements text
                //hide node resize ui

                //update context menu
            }
            else
            {
                //show node resize ui
                //update output requirement label / hide it
                if (Current.ParentGraph == null)
                {
                    //todo: reimplement layers
                }

                //update context menu
            }

            for (int i = 0; i < Current.Nodes.Count; ++i)
            {
                Node n = Current.Nodes[i];
                IGraphNode unode = null;

                //handle node type
                //comment etc
                //otherwise just do
                if (n is CommentNode)
                {

                }
                else if (n is PinNode)
                {

                }
                else
                {
                    unode = new UINode(this, n);
                    if (graphState == UIGraphState.LoadingWithTemplate)
                    {
                        (unode as MovablePane)?.Move(Size * 0.5f);
                    }
                }

                nodes[n.Id] = unode;

                //handle output preview linking

                if (unode is UINode)
                {
                    TryAndLinkOutputPreview(unode as UINode);
                }

                //add to view
                AddChild(unode as UIObject);
            }

            graphState = UIGraphState.None;

            var allNodes = nodes.Values.ToList();
            for (int i = 0; i < allNodes.Count; ++i)
            {
                var unode = allNodes[i];
                unode.LoadConnections();
            }
        }

        #endregion

        #region Graph Events
        private void Current_OnHdriChanged(Graph g)
        {
            //HdriManager.Selected = g.HdriIndex;
        }

        private void Current_OnGraphNameChanged(Graph g)
        {
            NameChanged?.Invoke(this);
        }

        private void Current_OnGraphUpdated(Graph g)
        {
            
        }

        private void OnMoveSelected(object sender, object arg)
        {
            if (sender is IGraphNode)
            {
                IGraphNode unode = sender as IGraphNode;

                if (!nodes.ContainsKey(unode.Id)) return;

                Vector2 delta = (Vector2)arg;

                for (int i = 0; i < Selected.Count; ++i)
                {
                    var n = Selected[i];
                    if (n == unode) continue;
                    (n as MovablePane)?.Move(delta, false);
                }
            }
        }

        #endregion

        #region Stack & Data Related
        public void Store()
        {
            if (Root == null) return;
            CaptureStack();
            InternalDispose();
        }

        public void Restore()
        {
            if (string.IsNullOrEmpty(RawRoot)) return;
            Load(RawRoot, RawRootCWD, false);
            RawRoot = null;
            RestoreStack();
        }

        public void TryAndLoadGraphStack(string[] stack)
        {
            if (stack == null) return;
            RawStack = stack;
            graphStack.Clear();
            //clear crumbs
            //reinit crumbs
            RestoreStack();
        }

        public string GetRawData()
        {
            if (!string.IsNullOrEmpty(RawRoot))
            {
                return RawRoot;
            }

            return Root?.GetJson();
        }

        public void Push(Node n, GraphStackType type = GraphStackType.Pixel)
        {
            Graph graph = null;

            if (type == GraphStackType.Pixel)
            {
                if (n is PixelProcessorNode)
                {
                    graph = (n as PixelProcessorNode).Function;
                }
            }

            Push(n, graph, type);
        }

        public void Push(Node n, Graph g, GraphStackType type, string param = null)
        {
            if (g == null) return;
            if (n != null)
            {
                GraphStackItem item = new GraphStackItem()
                {
                    id = n.Id,
                    node = n,
                    graph = g,
                    type = type,
                    parameter = param
                };

                graphStack.Push(item);
                //update crumbs
            }
            else
            {
                GraphStackItem item = new GraphStackItem()
                {
                    id = g.Name,
                    node = null,
                    graph = g,
                    type = type,
                    parameter = param
                };

                graphStack.Push(item);
                //update crumb
            }

            Load(g);
        }

        public void PopTo(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                graphStack.Clear();
                Load(Root);
                return;
            }

            var found = graphStack.FirstOrDefault(m => m.id.Equals(id));
            if (found == null) return;
            var peek = graphStack.Peek();
            if (peek.id.Equals(id))
            {
                return;
            }

            var g = graphStack.Pop();
            while (graphStack.Count > 0 && !g.id.Equals(id))
            {
                g = graphStack.Pop();
            }

            Push(g.node, g.graph, g.type, g.parameter);
        }

        public string[] GetRawStack()
        {
            string[] n = new string[graphStack.Count];
            var stack = graphStack.ToArray();
            for (int i = 0; i < stack.Length; ++i)
            {
                n[i] = stack[i].ToString();
            }
            return n;
        }

        protected void CaptureStack()
        {
            RawStack = GetRawStack();

            if (Root == null) return;
            RawRootName = Root.Name;
            RawRoot = Root.GetJson();
            RawRootCWD = Root.CWD;
        }

        public void RestoreStack()
        {
            if (RawStack == null) return;
            graphStack.Clear();
            var g = Root;
            Node n;
            for (int i = 0; i < RawStack.Length; ++i)
            {
                GraphStackItem item = GraphStackItem.Get(RawStack[i]);
                if (g.NodeLookup.TryGetValue(item.id, out n))
                {
                    item.node = n;

                    switch(item.type)
                    {
                        case GraphStackType.Pixel:
                            if (n is PixelProcessorNode)
                            {
                                item.graph = g = (n as PixelProcessorNode).Function;
                                graphStack.Push(item);
                                //update crumbs
                            }
                            else
                            {
                                RawStack = null;
                                return;
                            }
                            break;
                        case GraphStackType.Parameter:
                            if (!string.IsNullOrEmpty(item.parameter))
                            {
                                if (g.HasParameterValue(item.id, item.parameter))
                                {
                                    if (g.IsParameterValueFunction(item.id, item.parameter))
                                    {
                                        var v = g.GetParameterRaw(item.id, item.parameter);
                                        g = item.graph = v.Value as Graph;
                                        graphStack.Push(item);
                                        //update crumbs
                                    }
                                }
                            }
                            else
                            {
                                RawStack = null;
                                return;
                            }
                            break;
                    }
                }
                else if(item.type == GraphStackType.CustomFunction)
                {
                    Function fn = g.CustomFunctions.Find(m => m.Name.Equals(item.id));
                    if (fn != null)
                    {
                        g = item.graph = fn;
                        graphStack.Push(item);
                        //update crumbs
                    }
                    else
                    {
                        RawStack = null;
                        return;
                    }
                }
                else
                {
                    RawStack = null;
                    return;
                }
            }

            Load(g);
            RawStack = null;
        }
        #endregion

        #region Helpers
        public void Clear()
        {
            //send parameter view reset
            GlobalEvents.Emit(GlobalEvent.ViewParameters, this, null);

            Selected.Clear();
            SelectedIds.Clear();
            pins.Clear();

            //reset camera origin
            canvas.Cam.LocalPosition = new Vector3(0, 0, 0);

            var allNodes = nodes.Values.ToList();
            for (int i = 0; i < allNodes.Count; ++i)
            {
                var uinode = allNodes[i] as UIObject;
                RemoveChild(uinode);
                uinode?.Dispose();
            }

            nodes.Clear();
        }

        protected void InternalDispose()
        {
            Clear();

            //dispose layers here

            if (Current != null)
            {
                Current.OnGraphUpdated -= Current_OnGraphUpdated;
                Current.OnHdriChanged -= Current_OnHdriChanged;
                Current.OnGraphNameChanged -= Current_OnGraphNameChanged;

                Current.Dispose();
                Current = null;
            }

            Root?.Dispose();
            Root = null;
        }

        protected void TryAndLinkOutputPreview(UINode node)
        {
            if (node.Node is OutputNode)
            {
                OutputNode n = node.Node as OutputNode;

                if (n.OutType == OutputType.basecolor)
                {
                    GlobalEvents.Emit(GlobalEvent.Preview3DColor, this, n);
                }
                else if (n.OutType == OutputType.metallic)
                {
                    GlobalEvents.Emit(GlobalEvent.Preview3DMetallic, this, n);
                }
                else if (n.OutType == OutputType.roughness)
                {
                    GlobalEvents.Emit(GlobalEvent.Preview3DRoughness, this, n);
                }
                else if (n.OutType == OutputType.normal)
                {
                    GlobalEvents.Emit(GlobalEvent.Preview3DNormal, this, n);
                }
                else if (n.OutType == OutputType.occlusion)
                {
                    GlobalEvents.Emit(GlobalEvent.Preview3DOcclusion, this, n);
                }
                else if (n.OutType == OutputType.height)
                {
                    GlobalEvents.Emit(GlobalEvent.Preview3DHeight, this, n);
                }
                else if (n.OutType == OutputType.thickness)
                {
                    GlobalEvents.Emit(GlobalEvent.Preview3DThickness, this, n);
                }
            }
        }

        #endregion

        public IGraphNode GetNode(string id)
        {
            nodes.TryGetValue(id, out IGraphNode n);
            return n;
        }

        private void Selectable_Wheel(UISelectable arg1, InfinityUI.Interfaces.MouseWheelArgs e)
        {
            zoom += e.Delta.Y * ZOOM_SPEED; //change speedd by division here
            zoom = zoom.Clamp(0.03f, 3f);

            invZoom = 1.0f / zoom;
            canvas.Scale = zoom;
        }

        private void UIGraph_Moved(MovablePane arg1, Vector2 delta)
        {
            switch (mouseMode) 
            {
                case UIGraphMouseMode.Normal:
                    if (canvas != null)
                    {
                        canvas.Cam.LocalPosition += new Vector3(delta * invZoom); 
                    }
                    if (Current != null)
                    {
                        Current.ShiftX += delta.X * invZoom;
                        Current.ShiftY += delta.Y * invZoom;
                    }
                    break;
            }
        }

        public override void Dispose(bool disposing = true)
        {
            RawStack = null;
            RawRoot = null;
            InternalDispose();
            base.Dispose(disposing);
        }
    }
}
