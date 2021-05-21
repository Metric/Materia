using InfinityUI.Components;
using InfinityUI.Controls;
using InfinityUI.Core;
using InfinityUI.Interfaces;
using Materia.Graph;
using Materia.Nodes;
using Materia.Nodes.Atomic;
using Materia.Nodes.Items;
using Materia.Rendering.Mathematics;
using Materia.Rendering.Spatial;
using MateriaCore.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

    public class UIGraph : MovablePane, IDropTarget
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
        protected UIObject gridArea;
        protected UIImage grid;
        #endregion

        #region General
        //do we really need this?
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

        #region Pins & Comments
        protected int pinIndex = 0;
        //todo: replace IGraphNode with UIPinNode once implemented
        protected List<IGraphNode> pins = new List<IGraphNode>();
        protected List<IGraphNode> comments = new List<IGraphNode>();
        #endregion

        #region Archive Details
        public bool FromArchive { get; protected set; }
        public string FromArchivePath { get; protected set; }
        #endregion

        protected Dictionary<string, IGraphNode> nodes = new Dictionary<string, IGraphNode>();

        public UIGraph() : base(Vector2.Zero) 
        {
            InitializeComponents();
        }

        protected void AddGlobalEvents()
        {
            GlobalEvents.On(GlobalEvent.MoveSelected, OnMoveSelected);
            GlobalEvents.On(GlobalEvent.MoveComplete, OnMoveComplete);
        }

        protected void RemoveGlobalEvents()
        {
            GlobalEvents.Off(GlobalEvent.MoveSelected, OnMoveSelected);
            GlobalEvents.Off(GlobalEvent.MoveComplete, OnMoveComplete);
        }

        protected void InitializeComponents()
        {
            selectable.IsFocusable = false;

            RelativeTo = Anchor.Fill;

            //set it so children can always be raycast to
            //even outside bounds
            RaycastAlways = true;

            //don't actually allow drag / snapping
            //we just want to be able to accept the Moved Event
            //for delta stuff
            //why reinvent the wheel right?
            MoveAxis = Axis.None;
            SnapMode = MovablePaneSnapMode.None;

            Background.Color = new Vector4(0.1f, 0.1f, 0.1f, 1);
            gridArea = new UIObject()
            {
                RelativeTo = Anchor.Fill
            };

            //disable raycastTarget for grid area
            grid = gridArea.AddComponent<UIImage>();
            grid.Texture = GridGenerator.CreateBasic(64,64);
            grid.Color = Vector4.One;
            grid.Tiling = new Vector2(16, 16);
            grid.BeforeDraw += Grid_BeforeDraw;
            gridArea.RaycastTarget = false;

            selectable.Wheel += Selectable_Wheel;
            selectable.PointerUp += Selectable_PointerUp;

            Moved += UIGraph_Moved;

            AddChild(gridArea);
        }

        private void Selectable_PointerUp(UISelectable arg1, MouseEventArgs arg2)
        {
            ClearSelection();
        }

        private void Grid_BeforeDraw(UIDrawable obj)
        {
            Vector2 size = WorldSize; 
            float newSize = MathF.Max(size.X, size.Y) / 64;
            float aspect = size.X / size.Y;
            grid.Tiling = new Vector2(newSize, newSize / aspect) * zoom;

            //calculate grid offset
            Vector2 gpos = gridArea.WorldPosition;

            //whoops forgot to multiply by invZoom
            Vector2 fpos = new Vector2(gpos.X / size.X, gpos.Y / size.Y) * invZoom;
            grid.Offset = fpos % 1.0f; //we modulo by 1.0f to keep it within -1f to 1f
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

            AddGlobalEvents();

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

            RemoveCurrentEvents();

            Clear();

            Current = g;

            zoom = g.Zoom;
            invZoom = 1.0f / zoom;

            UpdateZoomDetails();

            if (Canvas != null)
            {
                Canvas.Cam.LocalPosition = new Vector3((float)g.ShiftX, (float)g.ShiftY, 0);
            }

            if (gridArea != null)
            {
                gridArea.Position = new Vector2((float)g.ShiftX, (float)g.ShiftY);
            }

            //todo: reimplement HdriManager
            //Current.HdriImages = HdriManager.Available.ToArray();

            AddCurrentEvents();

            //clear crumbs etc

            InitializeNodes();
            Current.TryAndProcess();
        }

        #endregion

        #region Input Commands
        public void TryAndPin()
        {
            if (Current == null || Canvas == null) return;

            Vector2 m = UI.MousePosition;
            Vector2 wp = Canvas.ToCanvasSpace(m);

            Node n = Current.CreateNode(typeof(PinNode));
            if (n == null) return;

            n.ViewOriginX = wp.X - 32; //todo: make this a constant
            n.ViewOriginY = wp.Y - 32;

            if (!Current.Add(n))
            {
                n?.Dispose();
                return;
            }

            IGraphNode unode = CreateUINode(n);
            pins.Add(unode);
            Current?.Snapshot();
        }

        public void TryAndComment()
        {
            if (Current == null || Canvas == null) return;
            Box2 bounds = GetSelectedBounds();
            if (bounds.Width <= 0 || bounds.Height <= 0)
            {
                Vector2 m = UI.MousePosition;
                Vector2 wp = Canvas.ToCanvasSpace(m);
                bounds = new Box2(wp, 256, 256); //todo: make comment constant size accessors
            }

            Node n = Current.CreateNode(typeof(CommentNode));
            if (n == null) return;

            n.Width = (int)(bounds.Width + 64); //todo: make a constant
            n.Height = (int)(bounds.Height + 64);
            n.ViewOriginX = bounds.Left - 32; //todo: make a constant
            n.ViewOriginY = bounds.Top - 52;

            if (!Current.Add(n))
            {
                n.Dispose();
                return;
            }

            IGraphNode unode = CreateUINode(n);
            comments.Add(unode);
            Current?.Snapshot();
        }

        public void TryAndDelete()
        {
            if (Current == null) return;

            for (int i= 0; i < Selected.Count; ++i)
            {
                var n = Selected[i];

                GlobalEvents.Emit(GlobalEvent.ClearViewParameters, this, n.Node);

                Current.Remove(n.Node);
                var unode = n as UIObject;
                RemoveChild(unode);
                unode?.Dispose();
            }

            Current?.Snapshot();
            Current?.TryAndProcess();

            Selected.Clear();
            SelectedIds.Clear();
        }

        public string TryAndCopy()
        {
            if (Current == null) return null;
            if (Selected.Count == 0) return null;

            List<string> nodeData = new List<string>();
            List<IGraphNode> selectedComments = new List<IGraphNode>();
            List<IGraphNode> selectedNormal = new List<IGraphNode>();
            Dictionary<string, string> copiedParams = new Dictionary<string, string>();
            HashSet<string> copied = new HashSet<string>();

            for (int i = 0; i < Selected.Count; ++i)
            {
                var n = Selected[i];
                if (n.Node is CommentNode)
                {
                    selectedComments.Add(n);
                }
                else
                {
                    selectedNormal.Add(n);
                }
            }

            for (int i = 0; i < selectedComments.Count; ++i)
            {
                var n = selectedComments[i];
                nodeData.Add(n.Node.GetJson());
                copied.Add(n.Id);

                var internalNodes = GetNodesIn((n as UIObject).Rect);
                for (int k = 0; k < internalNodes.Count; ++k)
                {
                    var inode = internalNodes[k];
                    if (copied.Contains(inode.Id)) continue;
                    var cparams = Current.CopyParameters(inode.Node);
                    foreach (string key in cparams.Keys)
                    {
                        copiedParams[key] = cparams[key];
                    }
                    copied.Add(inode.Id);
                    nodeData.Add(inode.Node.GetJson());
                }
            }

            for (int i = 0; i < selectedNormal.Count; ++i)
            {
                var n = selectedNormal[i];
                if (copied.Contains(n.Id)) continue;
                copied.Add(n.Id);
                nodeData.Add(n.Node.GetJson());
                var cparams = Current.CopyParameters(n.Node);
                foreach(string key in cparams.Keys)
                {
                    copiedParams[key] = cparams[key];
                }
            }

            if (nodeData.Count == 0) return null;

            UIGraphCopyData copyData = new UIGraphCopyData()
            {
                nodes = nodeData,
                parameters = copiedParams
            };

            return JsonConvert.SerializeObject(copyData);
        }

        public void TryAndPaste(string data)
        {
            try
            {
                if (Current == null || Canvas == null) return;
                if (string.IsNullOrEmpty(data)) return;
                if (ReadOnly) return;

                Vector2 m = UI.MousePosition;
                Vector2 wp = Canvas.ToCanvasSpace(m);
                UIGraphCopyData cd;
                try
                {
                   cd = JsonConvert.DeserializeObject<UIGraphCopyData>(data);
                }
                catch
                {
                    return;
                }

                if (cd.nodes == null || cd.nodes.Count == 0) return;

                List<Node> addedNodes = new List<Node>();
                List<IGraphNode> addedUINodes = new List<IGraphNode>();
                Dictionary<string, Node> lookup = new Dictionary<string, Node>();
                for (int i = 0; i < cd.nodes.Count; ++i)
                {
                    var nodeData = cd.nodes[i];
                    var node = CreateNodeFromData(nodeData, out string oldId, cd.parameters);
                    if (node == null) continue;
                    if (string.IsNullOrEmpty(oldId))
                    {
                        Current.Remove(node);
                        node.Dispose();
                        continue;
                    }
                    lookup[oldId] = node; //map old id to new node
                    addedNodes.Add(node);
                }

                float minX = float.MaxValue;
                float minY = float.MaxValue;

                //restore previous connections and get minX and minY
                for (int i = 0; i < addedNodes.Count; ++i)
                {
                    var n = addedNodes[i];
                    minX = MathF.Min((float)n.ViewOriginX, minX);
                    minY = MathF.Min((float)n.ViewOriginY, minY);
                    n.RestoreConnections(lookup, true); //restore old connections to new node lookup
                }

                //reset node view origin to new paste location
                //and create ui
                for (int i = 0; i < addedNodes.Count; ++i)
                {
                    var n = addedNodes[i];
                    float dx = (float)n.ViewOriginX - minX;
                    float dy = (float)n.ViewOriginY - minY;
                    n.ViewOriginX = wp.X + dx;
                    n.ViewOriginY = wp.Y + dy;
                    var unode = CreateUINode(n);
                    addedUINodes.Add(unode);
                }

                //load ui connections
                for (int i = 0; i < addedUINodes.Count; ++i)
                {
                    addedUINodes[i]?.LoadConnections();
                }

                Current?.Snapshot();

                //try and process graph
                Current?.TryAndProcess();
            }
            catch (Exception e)
            {
                MLog.Log.Error(e);
            }
        }

        public void TryAndInsertNode(string type)
        {
            if (Current == null || Canvas == null) return;
            Vector2 m = UI.MousePosition;
            Vector2 wp = Canvas.ToCanvasSpace(m);
            Node n = CreateNode(type);
            if (n == null) return;
            
            n.ViewOriginX = wp.X;
            n.ViewOriginY = wp.Y;

            IGraphNode unode;

            if (n is PinNode || n is CommentNode)
            {
                CreateUINode(n);
                Current?.Snapshot();
                return;
            }

            //connect up internally first at the data level
            if (UINodePoint.SelectedOrigin != null)
            {
                var p = UINodePoint.SelectedOrigin;
                var nodePoint = p.NodePoint;

                if (nodePoint is NodeOutput && n.Inputs.Count > 0)
                {
                    var nout = nodePoint as NodeOutput;
                    var inp = n.Inputs.Find(m => (m.Type & nout.Type) != 0);
                    if (inp != null)
                    {
                        nout.Add(inp);
                        UINodePoint.SelectedOrigin = null;
                    }
                }
                else if(nodePoint is NodeInput && n.Outputs.Count > 0)
                {
                    var inp = nodePoint as NodeInput;
                    var nout = n.Outputs.Find(m => (m.Type & inp.Type) != 0);
                    if (nout != null)
                    {
                        nout.Add(inp);
                        UINodePoint.SelectedOrigin = null;
                    }
                }
            }

            unode = CreateUINode(n);
            unode.LoadConnections();
            Current?.Snapshot();
            Current?.TryAndProcess();
        }

        public void GotoNextPin()
        {
            if (pins.Count == 0) return;
            if (pinIndex >= pins.Count)
            {
                pinIndex = 0;
            }
            IGraphNode n = pins[pinIndex++];
            UIObject unode = n as UIObject;
            if (Canvas != null)
            {
                Vector2 pos = unode.Position;
                Canvas.Cam.LocalPosition = new Vector3(pos.X, pos.Y, 0);
            }
            unode.GetComponent<UISelectable>()?.OnFocus(new FocusEvent());
        }

        public void TryAndCopyResources(string cwd)
        {
            Root?.CopyResources(cwd);
        }
        #endregion

        #region Node Bound Helpers
        protected List<IGraphNode> GetNodesIn(Box2 r)
        {
            List<IGraphNode> found = new List<IGraphNode>();
            List<IGraphNode> allNodes = nodes.Values.ToList();
            for (int i = 0; i < allNodes.Count; ++i)
            {
                UIObject unode = allNodes[i] as UIObject;
                if (r.Intersects(unode.Rect))
                {
                    found.Add(allNodes[i]);
                }
            }
            return found;
        }

        protected Box2 GetNodeBounds()
        {
            Box2 bounds = new Box2(0, 0, 0, 0);
            List<IGraphNode> allNodes = nodes.Values.ToList();
            for (int i = 0; i < allNodes.Count; ++i)
            {
                UIObject unode = allNodes[i] as UIObject;
                bounds.Encapsulate(unode.Rect);
            }
            return bounds;
        }

        protected Box2 GetSelectedBounds()
        {
            Box2 bounds = new Box2(0, 0, 0, 0);
            for (int i = 0; i < Selected.Count; ++i)
            {
                bounds.Encapsulate((Selected[i] as UIObject).Rect);
            }
            return bounds;
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
                CreateUINode(n);
            }

            graphState = UIGraphState.None;

            var allNodes = nodes.Values.ToList();
            for (int i = 0; i < allNodes.Count; ++i)
            {
                var unode = allNodes[i];
                unode.LoadConnections();
            }
        }

        protected Node CreateNode(string type)
        {
            try
            {
                if (Current == null) return null;
                Node n = Current.CreateNode(type);
                if (n == null) return null;
                
                if (!Current.Add(n))
                {
                    n.Dispose();
                    return null;
                }

                if (n is GraphInstanceNode)
                {
                    GraphInstanceNode gn = n as GraphInstanceNode;
                    gn.Load(type);
                }
                else if(n is CommentNode)
                {
                    n.Width = 256 + 16; //todo: setup constanst for comment node size
                    n.Height = 256 + 38;
                }

                return n;
            }
            catch (Exception e)
            {
                MLog.Log.Error(e);
            }

            return null;
        }

        protected Node CreateNodeFromData(string data, out string oldId, Dictionary<string, string> cparams = null)
        {
            oldId = null;

            try
            {
                if (Current == null) return null;

                Node.NodeData nd = JsonConvert.DeserializeObject<Node.NodeData>(data);
                if (nd == null) return null;

                oldId = nd.id;

                Node n = Current.CreateNode(nd.type);
                if (n == null) return null;

                string newId = n.Id;

                if (!Current.Add(n))
                {
                    n.Dispose();
                    return null;
                }

                n.FromJson(data);

                //restore new id
                //since n.FromJson restores the old id
                n.Id = newId;

                if (cparams == null) return n;

                Current.PasteParameters(cparams, nd, n);

                return n;
            }
            catch (Exception e)
            {
                MLog.Log.Error(e);
            }

            return null;
        }

        protected IGraphNode CreateUINode(Node n)
        {
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
                if (graphState == UIGraphState.LoadingWithTemplate)
                {
                    n.ViewOriginX = Size.X * 0.5f;
                    n.ViewOriginY = Size.Y * 0.5f;
                }

                unode = new UINode(this, n);
            }

            nodes[unode.Id] = unode;

            //handle output preview linking

            if (unode is UINode)
            {
                TryAndLinkOutputPreview(unode as UINode);
            }

            //add to view
            AddChild(unode as UIObject);
            unode.Snap();
            return unode;
        }

        protected IGraphNode CreateUINode(Node n, Vector2 pos)
        {
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
                if (graphState == UIGraphState.LoadingWithTemplate)
                {
                    n.ViewOriginX = Size.X * 0.5f;
                    n.ViewOriginY = Size.Y * 0.5f;
                }

                unode = new UINode(this, n);
            }

            nodes[unode.Id] = unode;

            //handle output preview linking

            if (unode is UINode)
            {
                TryAndLinkOutputPreview(unode as UINode);
            }

            var uobj = unode as UIObject;
            uobj.Position = pos;

            //add to view
            AddChild(unode as UIObject);
            unode.Snap();
            return unode;
        }
        #endregion

        #region Graph Events
        protected void AddCurrentEvents()
        {
            if (Current == null) return;
            Current.OnUndo += Current_OnUndo;
            Current.OnRedo += Current_OnRedo;
            Current.OnUpdate += Current_OnGraphUpdated;
            Current.OnNameChange += Current_OnGraphNameChanged;
        }

        protected void RemoveCurrentEvents()
        {
            if (Current == null) return;
            Current.OnUndo -= Current_OnUndo;
            Current.OnRedo -= Current_OnRedo;
            Current.OnUpdate -= Current_OnGraphUpdated;
            Current.OnNameChange -= Current_OnGraphNameChanged;
        }

        private void Current_OnRedo(Graph g)
        {
            if (g != Current) return;
            MergeUndoRedo();
        }

        private void Current_OnUndo(Graph g)
        {
            if (g != Current) return;
            MergeUndoRedo();
        }

        private void Current_OnGraphNameChanged(Graph g)
        {
            NameChanged?.Invoke(this);
        }

        private void Current_OnGraphUpdated(Graph g)
        {
            
        }

        private void OnMoveComplete(object sender, object arg)
        {
            if (sender is IGraphNode)
            {
                IGraphNode unode = sender as IGraphNode;
                if (!nodes.ContainsKey(unode.Id)) return;
                for (int i = 0; i < Selected.Count; ++i)
                {
                    var n = Selected[i];
                    if (n == unode) continue;
                    var pane = n as MovablePane;
                    if (pane == null) continue;
                    UI.SnapToGrid(pane, (int)pane.SnapTolerance);
                }
            }
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

            if (Canvas != null && Current != null)
            {
                Canvas.Cam.LocalPosition = new Vector3((float)Current.ShiftX, (float)Current.ShiftY, 0);
            }

            Current?.TryAndProcess();
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

        #region Multiselect
        public bool IsSelected(string id)
        {
            return SelectedIds.Contains(id);
        } 
        public bool IsSelected(IGraphNode n)
        {
            return SelectedIds.Contains(n.Id);
        }

        public void Select(IGraphNode n)
        {
            if (SelectedIds.Contains(n.Id)) return;

            //todo: add in set toggle for nodes

            Selected.Add(n);
            SelectedIds.Add(n.Id);
        }
        public void Unselect(IGraphNode n)
        {
            //todo: add in clear toggle for nodes
            Selected.Remove(n);
            SelectedIds.Remove(n.Id);
        }
        public void ToggleSelect(IGraphNode n)
        {
            if (SelectedIds.Contains(n.Id))
            {
                //todo: add in clear toggle for nodes

                Selected.Remove(n);
                SelectedIds.Remove(n.Id);
            }
            else
            {
                //todo: add in set toggle for nodes

                Selected.Add(n);
                SelectedIds.Add(n.Id);
            }
        }

        public void ClearSelection()
        {
            //todo: add back in clear toggle for nodes

            SelectedIds.Clear();
            Selected.Clear();
        }
        #endregion

        #region Helpers

        /// <summary>
        /// Merges the undo redo changes for the UI
        /// It determines removed nodes and missing nodes
        /// also clears previous UI connections
        /// </summary>
        protected void MergeUndoRedo()
        {
            List<IGraphNode> toDispose = new List<IGraphNode>();
            List<IGraphNode> toRestore = new List<IGraphNode>();

            //find removed nodes
            //and prepare to remove the ui for them
            //otherwise prepare to restore the node
            //to the new node from grpah data
            foreach(string k in nodes.Keys)
            {
                if (!Current.NodeLookup.TryGetValue(k, out Node n))
                { 
                    toDispose.Add(nodes[k]);
                }
                else
                {
                    nodes[k]?.Restore();
                    toRestore.Add(nodes[k]);
                }
            }

            //create missing ui nodes and schedule for restore
            for(int i = 0; i < Current.Nodes.Count; ++i)
            {
                Node n = Current.Nodes[i];
                if(!nodes.TryGetValue(n.Id, out IGraphNode unode))
                {
                    unode = CreateUINode(n);
                    toRestore.Add(unode);
                }
            }

            for (int i = 0; i < toDispose.Count; ++i)
            {
                var n = toDispose[i];
                if (n == null) continue;

                Selected.Remove(n);
                SelectedIds.Remove(n.Id);
                pins.Remove(n);
                comments.Remove(n);
                nodes.Remove(n.Id);

                UIObject unode = n as UIObject;
                RemoveChild(unode);
                unode?.Dispose();
            }

            for (int i = 0; i < toRestore.Count; ++i)
            {
                toRestore[i]?.LoadConnections();
            }
        }

        private void UpdateZoomDetails()
        {
            //update zoom text etc here

            if (Canvas != null)
            {
                Canvas.Scale = zoom;
            }

            gridArea.Scale = new Vector2(zoom);
        }

        /// <summary>
        /// Clears the view of all nodes.
        /// Resets zoom and camera position
        /// </summary>
        public void Clear()
        {
            //send parameter view reset
            GlobalEvents.Emit(GlobalEvent.ViewParameters, this, null);

            Selected.Clear();
            SelectedIds.Clear();
            pins.Clear();
            comments.Clear();

            //reset zoom
            zoom = invZoom = 1.0f;

            UpdateZoomDetails();

            //reset camera origin
            if (Canvas != null)
            {
                Canvas.Cam.LocalPosition = Vector3.Zero;
            }

            if (gridArea != null)
            {
                gridArea.Position = Vector2.Zero;
            }

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
            RemoveGlobalEvents();
            RemoveCurrentEvents();

            Clear();

            //dispose layers here

            Current?.Dispose();
            Current = null;

            Root?.Dispose();
            Root = null;
        }

        protected void TryAndLinkOutputPreview(UINode node)
        {
            if (node.Node is OutputNode)
            {
                OutputNode n = node.Node as OutputNode;

                switch(n.OutType)
                {
                    case OutputType.basecolor:
                        GlobalEvents.Emit(GlobalEvent.Preview3DColor, this, node);
                        break;
                    case OutputType.metallic:
                        GlobalEvents.Emit(GlobalEvent.Preview3DMetallic, this, node);
                        break;
                    case OutputType.roughness:
                        GlobalEvents.Emit(GlobalEvent.Preview3DRoughness, this, node);
                        break;
                    case OutputType.normal:
                        GlobalEvents.Emit(GlobalEvent.Preview3DNormal, this, node);
                        break;
                    case OutputType.occlusion:
                        GlobalEvents.Emit(GlobalEvent.Preview3DOcclusion, this, node);
                        break;
                    case OutputType.height:
                        GlobalEvents.Emit(GlobalEvent.Preview3DHeight, this, node);
                        break;
                    case OutputType.thickness:
                        GlobalEvents.Emit(GlobalEvent.Preview3DThickness, this, node);
                        break;
                    case OutputType.emission:
                        GlobalEvents.Emit(GlobalEvent.Preview3DEmission, this, node);
                        break;
                }
            }
        }

        #endregion

        public IGraphNode GetNode(string id)
        {
            nodes.TryGetValue(id, out IGraphNode n);
            return n;
        }

        private void Selectable_Wheel(UISelectable arg1, MouseWheelArgs e)
        {
            zoom += e.Delta.Y * ZOOM_SPEED;
            zoom = zoom.Clamp(0.03f, 3f);

            invZoom = 1.0f / zoom;

            UpdateZoomDetails();
        }

        private void UIGraph_Moved(MovablePane arg1, Vector2 delta, MouseEventArgs e)
        {
            //must reverse the delta as
            //otherwise camera pans in opposite direction
            Vector2 scaledDelta = -delta * invZoom;

            switch (mouseMode) 
            {
                case UIGraphMouseMode.Normal:
                    if (Canvas != null)
                    {
                        Canvas.Cam.LocalPosition += new Vector3(scaledDelta); 
                    }
                    if (Current != null)
                    {
                        Current.ShiftX += scaledDelta.X;
                        Current.ShiftY += scaledDelta.Y;
                    }
                    gridArea.Position += scaledDelta;
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

        public void OnFileDrop(UIFileDropEvent e)
        {
            e.IsHandled = true;
            var data = e.files;

            if (data == null) return;

            int totalValid = 0;
            for (int i = 0; i < data.Length; ++i)
            {
                string f = data[i];
                if (string.IsNullOrEmpty(f)) continue;
                string ext = System.IO.Path.GetExtension(f);
                if (ext.ToLower().Equals(".mtg") || ext.ToLower().Equals(".mtga"))
                {
                    var n = CreateNode(f);
                    if (n == null) return;

                    Vector2 pos = UI.MousePosition;

                    if (Canvas != null)
                    {
                        pos = Canvas.ToCanvasSpace(pos);
                    }

                    pos += new Vector2(totalValid * UINode.DEFAULT_WIDTH, 0);

                    CreateUINode(n, pos);
                    ++totalValid;
                }
            }

            if (totalValid > 0)
            {
                Current?.Snapshot();
                Current?.TryAndProcess();
            }
        }

        public void OnDrop(UIDropEvent e)
        {
            var data = e.dragDrop.DropData;
            if (data is UINodeSource) //todo: check for string type file stuff
            {
                e.IsHandled = true;
                UINodeSource src = data as UINodeSource;
                var n = CreateNode(src.Type);
                if (n == null) return;
                
                Vector2 pos = UI.MousePosition;
                
                if (Canvas != null)
                {
                    pos = Canvas.ToCanvasSpace(pos);
                }

                CreateUINode(n, pos);
                Current?.Snapshot();
                Current?.TryAndProcess();
            }
            else if (data is string)
            {
                e.IsHandled = true;
                string v = (string)data;
                if (!string.IsNullOrEmpty(v))
                {
                    var n = CreateNode(v);
                    if (n == null) return;

                    Vector2 pos = UI.MousePosition;

                    if (Canvas != null)
                    {
                        pos = Canvas.ToCanvasSpace(pos);
                    }

                    CreateUINode(n, pos);
                    Current?.Snapshot();
                    Current?.TryAndProcess();
                }
            }
        }
    }
}
