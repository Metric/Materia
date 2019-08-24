using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Materia.Nodes;
using Materia.Nodes.Items;
using Materia.Imaging;
using Materia.Nodes.Atomic;
using Materia.Nodes.Helpers;
using System.Threading;
using Newtonsoft.Json;
using Materia.Undo;
using Materia.UI.Components;
using Materia.Hdri;
using Materia.UI;
using Materia.UI.ItemNodes;
using NLog;
using Materia.UI.Helpers;
using Materia.Archive;

namespace Materia
{
    public enum GraphStackType
    {
        Parameter,
        Pixel,
        CustomFunction
    }

    /// <summary>
    /// Interaction logic for UIGraph.xaml
    /// </summary>
    public partial class UIGraph : UserControl, IDisposable
    {
        private static ILogger Log = LogManager.GetCurrentClassLogger();

        private const float CONNECTION_POINT_SIZE = 16;

        public struct GraphCopyData
        {
            public List<string> nodes;
            public Dictionary<string, string> parameters;
        }

        bool moving;
        Point start;
        Point insertPos;

        public delegate void GraphUpdate(UIGraph graph);
        public static event GraphUpdate OnGraphNameChanged;

        public string Id { get; protected set; }

        public float Scale { get; protected set; }
        public float PrevScale { get; protected set; }

        public List<IUIGraphNode> GraphNodes { get; protected set; }

        public Graph Graph { get; protected set; }

        protected Dictionary<string, IUIGraphNode> lookup;

        public double XShift { get; protected set; }
        public double YShift { get; protected set; }

        public bool ReadOnly { get; protected set; }

        public bool Modified { get; protected set; }

        public List<IUIGraphNode> SelectedNodes { get; protected set; }
        public HashSet<string> SelectedIds { get; protected set; }
        protected Rectangle selectionRect;

        protected Graph Original { get; set; }

        public const float GridSnap = 10;
        const float InvGridSnap = 1.0f / 10.0f;

        public string FilePath { get; protected set; }

        public string StoredGraph { get; protected set; }
        public string StoredGraphCWD { get; protected set; }
        public string[] StoredGraphStack { get; protected set; }
        protected string StoredGraphName { get; set; }

        protected int pinIndex;
        protected List<UIPinNode> Pins;

        private bool isDisposed;

        public bool FromArchive { get; set; }
        public string FromArchivePath { get; set; }

        public string GraphName
        {
            get
            {
                if(Original != null)
                {
                    return Original.Name;
                }
                else
                {
                    return StoredGraphName;
                }
            }
        }

        public float ScaledGridSnap
        {
            get
            {
                return GridSnap * Scale;
            }
        }


        protected Path ConnectionPathPreview = new Path();
        protected Rectangle ConnectionPointPreview = new Rectangle();

        protected Stack<GraphStackItem> GraphStack;

        protected bool graphIsLoadingSizeSelect;
        protected bool graphInitedWithTemplate;
        protected bool isLoadingGraph;

        protected HashSet<string> selectedStartedIn;

        public class GraphStackItem
        {
            public string parameter;
            public Graph graph;
            public Node node;
            public string id;
            public GraphStackType type;

            public class GraphStackItemData
            {
                public string id;
                public string parameter;
                public GraphStackType type;
            }

            public string GetJson()
            {
                GraphStackItemData d = new GraphStackItemData();
                d.type = type;
                d.id = id;
                d.parameter = parameter;

                return JsonConvert.SerializeObject(d);
            }

            public static GraphStackItem FromJson(string data)
            {
                GraphStackItemData d = JsonConvert.DeserializeObject<GraphStackItemData>(data);

                GraphStackItem i = new GraphStackItem();
                i.id = d.id;
                i.parameter = d.parameter;
                i.type = d.type;

                return i;
            }
        }

        public UIGraph(Graph template = null)
        {
            InitializeComponent();

            isDisposed = false;

            Id = Guid.NewGuid().ToString();

            GraphStack = new Stack<GraphStackItem>();
            SelectedNodes = new List<IUIGraphNode>();
            SelectedIds = new HashSet<string>();
            selectedStartedIn = new HashSet<string>();
            Pins = new List<UIPinNode>();
            pinIndex = 0;

            selectionRect = new Rectangle();
            selectionRect.Stroke = (SolidColorBrush)new BrushConverter().ConvertFrom("#0087e5");
            selectionRect.StrokeDashArray.Add(2);
            selectionRect.StrokeDashArray.Add(2);
            selectionRect.StrokeDashCap = PenLineCap.Round;
            selectionRect.StrokeEndLineCap = PenLineCap.Round;
            selectionRect.StrokeThickness = 1;
            selectionRect.HorizontalAlignment = HorizontalAlignment.Left;
            selectionRect.VerticalAlignment = VerticalAlignment.Top;

            selectionRect.Fill = null;
            selectionRect.RenderTransformOrigin = new Point(0, 0);

            lookup = new Dictionary<string, IUIGraphNode>();
            Scale = 1;
            PrevScale = 1;

            moving = false;
            GraphNodes = new List<IUIGraphNode>();

            if (template != null)
            {
                graphInitedWithTemplate = true;
                StoredGraph = template.GetJson();
            }

            Original = Graph = new ImageGraph("Untitled");

            UpdateGrid();

            Crumbs.Clear();

            ConnectionPointPreview.RadiusX = CONNECTION_POINT_SIZE * 0.5;
            ConnectionPointPreview.RadiusY = CONNECTION_POINT_SIZE * 0.5;

            ConnectionPointPreview.Width = CONNECTION_POINT_SIZE;
            ConnectionPointPreview.Height = CONNECTION_POINT_SIZE;

            ConnectionPointPreview.Fill = new SolidColorBrush(Colors.DarkRed);

            ConnectionPointPreview.HorizontalAlignment = HorizontalAlignment.Left;
            ConnectionPointPreview.VerticalAlignment = VerticalAlignment.Top;

            ConnectionPathPreview.HorizontalAlignment = HorizontalAlignment.Left;
            ConnectionPathPreview.VerticalAlignment = VerticalAlignment.Top;

            ConnectionPathPreview.Stroke = new SolidColorBrush(Colors.DarkRed);

            ConnectionPointPreview.IsHitTestVisible = false;
            ConnectionPathPreview.IsHitTestVisible = false;

            BreadCrumb cb = new BreadCrumb(Crumbs, "Root", this, null);
        }

        public void TryAndLoadGraphStack(string[] stack)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                if (stack == null) return;

                StoredGraphStack = stack;
                GraphStack.Clear();
                Crumbs.Clear();

                BreadCrumb cb = new BreadCrumb(Crumbs, "Root", this, null);
                RestoreStack();
            });
        }

        public void MarkModified()
        {
            Modified = true;
        }

        public void TryAndPin()
        {
            Point m = Mouse.GetPosition(ViewPort);
            ToWorld(ref m);

            Node node = Graph.CreateNode(typeof(PinNode).ToString());
            if(node == null)
            {
                return;
            }

            node.ViewOriginX = m.X - 32;
            node.ViewOriginY = m.Y - 32;

            if (!Graph.Add(node)) return;

            UIPinNode unode = new UIPinNode(node, this, node.ViewOriginX, node.ViewOriginY, XShift, YShift, Scale);
            unode.HorizontalAlignment = HorizontalAlignment.Left;
            unode.VerticalAlignment = VerticalAlignment.Top;
            lookup[node.Id] = unode;
            ViewPort.Children.Add(unode);
            GraphNodes.Add(unode);

            Pins.Add(unode);

            UndoRedoManager.AddUndo(new CreateNode(Id, unode.Id, this));
        }

        public void TryAndComment()
        {
            Rect area = GetSelectedNodeBounds();

            if(area.Width <= 0 || area.Height <= 0)
            {
                Point m = Mouse.GetPosition(ViewPort);
                ToWorld(ref m);

                area.X = m.X;
                area.Y = m.Y;
                area.Width = 256;
                area.Height = 256;
            }

            Node node = Graph.CreateNode(typeof(CommentNode).ToString());
            if (node == null)
            {
                return;
            }

            //add padding
            node.Width = (int)area.Width + 64;
            node.Height = (int)area.Height + 90;

            node.ViewOriginX = area.Left - 32;
            node.ViewOriginY = area.Top - 52;

            if (!Graph.Add(node)) return;

            UICommentNode unode = new UICommentNode(node, this, area.Left - 32, area.Top - 52, XShift, YShift, Scale);
            unode.HorizontalAlignment = HorizontalAlignment.Left;
            unode.VerticalAlignment = VerticalAlignment.Top;
            lookup[node.Id] = unode;
            ViewPort.Children.Add(unode);
            GraphNodes.Add(unode);

            UndoRedoManager.AddUndo(new CreateNode(Id, unode.Id, this));
        }

        public void TryAndUndo()
        {
            UndoRedoManager.Undo(Id);
        }

        public void TryAndRedo()
        {
            UndoRedoManager.Redo(Id);
        }

        public void TryAndDelete()
        {
            foreach (IUIGraphNode n in SelectedNodes)
            {
                if(UINodeParameters.Instance != null)
                {
                    if(UINodeParameters.Instance.node == n.Node)
                    {
                        UINodeParameters.Instance.ClearView();
                    }
                }

                n.Dispose();
            }

            SelectedNodes.Clear();
        }

        public void TryAndCopy()
        {
            if (SelectedNodes.Count > 0)
            {
                List<string> nodes = new List<string>();

                HashSet<IUIGraphNode> copied = new HashSet<IUIGraphNode>();

                List<IUIGraphNode> comments = new List<IUIGraphNode>();
                List<IUIGraphNode> regular = new List<IUIGraphNode>();
                Dictionary<string, string> copiedParams = new Dictionary<string, string>();

                //splitting up comment nodes
                //and non comment nodes
                //so we can test for duplicates!
                foreach (IUIGraphNode n in SelectedNodes)
                {
                    if(n is UICommentNode)
                    {
                        comments.Add(n);
                    }
                    else
                    {
                        regular.Add(n);
                    }
                }

                foreach(IUIGraphNode n in comments)
                {
                    nodes.Add(n.Node.GetJson());
                    copied.Add(n);

                    UICommentNode cm = n as UICommentNode;

                    var contained = cm.GetContained();

                    foreach(IUIGraphNode cn in contained)
                    {
                        //there is a possibility more
                        //than one comment node share
                        //the same node!
                        if(copied.Contains(cn))
                        {
                            continue;
                        }

                        var cparams = Graph.CopyParameters(cn.Node);

                        foreach (string k in cparams.Keys)
                        {
                            copiedParams[k] = cparams[k];
                        }

                        copied.Add(cn);
                        nodes.Add(cn.Node.GetJson());
                    }
                }

                foreach (IUIGraphNode n in regular)
                {
                    if (copied.Contains(n))
                    {
                        continue;
                    }

                    var cparams = Graph.CopyParameters(n.Node);

                    foreach(string k in cparams.Keys)
                    {
                        copiedParams[k] = cparams[k];
                    }

                    nodes.Add(n.Node.GetJson());
                    copied.Add(n);
                }

                GraphCopyData cd = new GraphCopyData()
                {
                    nodes = nodes,
                    parameters = copiedParams
                };

                //copy to clipboard
                Clipboard.SetText(JsonConvert.SerializeObject(cd));
            }
        }

        public void TryAndPaste()
        {
            try
            {
                if (Graph.ReadOnly) return;
               
                Point mp = Mouse.GetPosition(ViewPort);

                mp.X = Math.Max(0, mp.X);
                mp.Y = Math.Max(0, mp.Y);
                mp.X = mp.X > ViewPort.ActualWidth ? 0 : mp.X;
                mp.Y = mp.Y > ViewPort.ActualHeight ? 0 : mp.Y;

                ToWorld(ref mp);

                string data = Clipboard.GetText();

                if (string.IsNullOrEmpty(data)) return;
                GraphCopyData cd;

                //apparently deserialize will throw an exception
                //it the content isn't json at atll
                try
                {
                   cd = JsonConvert.DeserializeObject<GraphCopyData>(data);
                }
                catch
                {
                    return;
                }

                if (cd.nodes == null || cd.nodes.Count == 0) return;


                List<IUIGraphNode> added = new List<IUIGraphNode>();
                Dictionary<string, Node.NodeData> jsonContent = new Dictionary<string, Node.NodeData>();
                Dictionary<string, Node> realLookup = new Dictionary<string, Node>();

                for (int i = 0; i < cd.nodes.Count; i++)
                {
                    string json = cd.nodes[i];
                    Node.NodeData ndata = null;
                    var unode = AddNodeFromJson(json, realLookup, cd.parameters, out ndata);
                    if (unode != null)
                    {
                        added.Add(unode);
                        jsonContent[unode.Node.Id] = ndata;
                    }
                }

                double minX = float.MaxValue;
                double minY = float.MaxValue;

                //find minx and miny
                foreach (IUIGraphNode n in added)
                {
                    if(minX > n.Node.ViewOriginX)
                    {
                        minX = n.Node.ViewOriginX;
                    }
                    if(minY > n.Node.ViewOriginY)
                    {
                        minY = n.Node.ViewOriginY;
                    }
                }

                //offset nodes origin by mouse point position
                foreach(IUIGraphNode n in added)
                {
                    double dx = n.Node.ViewOriginX - minX;
                    double dy = n.Node.ViewOriginY - minY;

                    //also set node connections as needed
                    Node.NodeData json = null;
                    if (jsonContent.TryGetValue(n.Node.Id, out json))
                    {
                        n.Node.SetConnections(realLookup, json.outputs);
                    }

                    n.OffsetTo(mp.X + dx, mp.Y + dy);
                }

                Task.Delay(25).ContinueWith((Task t) =>
                {
                    App.Current.Dispatcher.Invoke(() =>
                    {
                        foreach (IUIGraphNode n in added)
                        {
                            //finally load visual connections
                            n.LoadConnections(lookup);
                        }

                        if (added.Count > 0)
                        {
                            Modified = true;
                        }
                    });
                });

            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
        }

        public void CopyResources(string CWD)
        {
            if (Original != null)
            {
                Original.CopyResources(CWD);
            }
        }

        public void LoadGraph(string path)
        {
            string directory = System.IO.Path.GetDirectoryName(path);
            FilePath = path;
            string data = System.IO.File.ReadAllText(path);
            LoadGraph(data, directory);
        }

        protected void LoadGraph(Graph g, bool loadUI = true)
        {
            //no need to reload if it is the same graph already
            if (g == Graph) return;
            if (g == null) return;

            pinIndex = 0;

            if (Graph != null)
            {
                Graph.OnGraphUpdated -= Graph_OnGraphUpdated;
                Graph.OnGraphNameChanged -= Graph_OnGraphNameChanged;
                Graph.OnHdriChanged -= Graph_OnHdriChanged;
                Graph.OnGraphLinesChanged -= Graph_OnGraphLinesChanged;
            }

            ClearView();

            Graph = g;

            Scale = Graph.Zoom;

            if(Scale <= 0)
            {
                Scale = 1;
            }

            XShift = Graph.ShiftX;
            YShift = Graph.ShiftY;

            ZoomLevel.Text = String.Format("{0:0}", Scale * 100);

            //whoops forgot to add this line back in
            //when I separated the core to it's own project
            Graph.HdriImages = HdriManager.Available.ToArray();

            Graph.OnGraphUpdated += Graph_OnGraphUpdated;
            Graph.OnGraphNameChanged += Graph_OnGraphNameChanged;
            Graph.OnHdriChanged += Graph_OnHdriChanged;
            Graph.OnGraphLinesChanged += Graph_OnGraphLinesChanged;

            NodePath.Type = Graph.GraphLinesDisplay;

            ReadOnly = Graph.ReadOnly;
            Graph.ReadOnly = false;

            if (loadUI)
            {
                LoadGraphUI();
            }
        }

        private void Graph_OnGraphLinesChanged(Graph g)
        {
            NodePath.Type = Graph.GraphLinesDisplay;
        }

        private void Graph_OnHdriChanged(Graph g)
        {
            HdriManager.Selected = g.HdriIndex;
        }

        private void Graph_OnGraphNameChanged(Graph g)
        {
            Modified = true;
        }

        private void Graph_OnGraphUpdated(Graph g)
        {
            if(isLoadingGraph)
            {
                if (Graph is ImageGraph && !Graph.Synchronized)
                {
                    if (Graph.IsProcessing) return;
                    else isLoadingGraph = false;
                }

                return;
            }
            Modified = true;
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

        public void Push(Node n, Graph graph, GraphStackType type, string param = null)
        {
            if (graph != null)
            {
                if (n != null)
                {
                    GraphStackItem item = new GraphStackItem();
                    item.id = n.Id;
                    item.node = n;
                    item.graph = graph;
                    item.type = type;
                    item.parameter = param;


                    if (!GraphStack.Contains(item))
                    {
                        GraphStack.Push(item);
                        if (!Crumbs.Contains(n.Id))
                        {
                            BreadCrumb c = new BreadCrumb(Crumbs, graph.Name, this, n.Id);
                        }
                    }
                }
                else
                {
                    GraphStackItem item = new GraphStackItem();
                    item.id = graph.Name;
                    item.node = null;
                    item.graph = graph;
                    item.type = type;
                    item.parameter = param;
                    if(!GraphStack.Contains(item))
                    {
                        GraphStack.Push(item);
                        if(!Crumbs.Contains(item.id))
                        {
                            BreadCrumb c = new BreadCrumb(Crumbs, graph.Name, this, item.id);
                        }
                    }
                }

                LoadGraph(graph);
            }
        }

        protected void RestoreStack()
        {
            if(StoredGraphStack != null)
            {
                //need to clear it since we are restoring
                GraphStack.Clear();

                var graph = Original;
                Node n;
                for(int i = 0; i < StoredGraphStack.Length; i++)
                {
                    GraphStackItem item = GraphStackItem.FromJson(StoredGraphStack[i]);
                    if (graph.NodeLookup.TryGetValue(item.id, out n))
                    {
                        item.node = n;

                        if(item.type == GraphStackType.Pixel)
                        {
                            if(n is PixelProcessorNode)
                            {
                                item.graph = graph = (n as PixelProcessorNode).Function;

                                if(!GraphStack.Contains(item))
                                {
                                    GraphStack.Push(item);

                                    if(!Crumbs.Contains(n.Id))
                                    {
                                        BreadCrumb c = new BreadCrumb(Crumbs, graph.Name, this, n.Id);
                                    }
                                }
                            }
                            else
                            {
                                //we return as the stack does not match
                                StoredGraphStack = null;
                                return;
                            }
                        }
                        else if(item.type == GraphStackType.Parameter && !string.IsNullOrEmpty(item.parameter))
                        {
                            if(graph.HasParameterValue(item.id, item.parameter))
                            {
                                if(graph.IsParameterValueFunction(item.id, item.parameter))
                                {
                                    var v = graph.GetParameterRaw(item.id, item.parameter);
                                    graph = item.graph = v.Value as Graph;

                                    if (!GraphStack.Contains(item))
                                    {
                                        GraphStack.Push(item);

                                        if (!Crumbs.Contains(n.Id))
                                        {
                                            BreadCrumb c = new BreadCrumb(Crumbs, graph.Name, this, n.Id);
                                        }
                                    }
                                }
                                else
                                {
                                    StoredGraphStack = null;
                                    return;
                                }
                            }
                            else
                            {
                                StoredGraphStack = null;
                                return;
                            }
                        }
                        else
                        {
                            StoredGraphStack = null;
                            return;
                        }
                    }
                    else if (item.type == GraphStackType.CustomFunction)
                    {
                        FunctionGraph fn = graph.CustomFunctions.Find(m => m.Name.Equals(item.id));

                        if (fn != null)
                        {
                            graph = item.graph = fn;

                            if (!GraphStack.Contains(item))
                            {
                                GraphStack.Push(item);

                                if (!Crumbs.Contains(item.id))
                                {
                                    BreadCrumb c = new BreadCrumb(Crumbs, fn.Name, this, item.id);
                                }
                            }
                        }
                        else
                        {
                            StoredGraphStack = null;
                            return;
                        }
                    }
                    else
                    {
                        StoredGraphStack = null;
                        return;
                    }
                }

                if(graph != null)
                {
                    LoadGraph(graph);
                }

                StoredGraphStack = null;
            }
        }

        public string[] GetGraphStack()
        {
            string[] n = new string[GraphStack.Count];
            GraphStackItem[] stack = GraphStack.ToArray();

            for (int i = 0; i < stack.Length; i++)
            {
                n[i] = stack[i].GetJson();
            }

            return n;
        }

        protected void CaptureStack()
        {
            StoredGraphStack = new string[GraphStack.Count];
            GraphStackItem[] stack = GraphStack.ToArray();

            for(int i = 0; i < stack.Length; i++)
            {
                StoredGraphStack[i] = stack[i].GetJson();
            }
        }

        public void PopTo(string id)
        {
            //if null then we just load the original root
            if(string.IsNullOrEmpty(id))
            {
                GraphStack.Clear();
                LoadGraph(Original);
            }

            if (GraphStack.Count > 0)
            {
                var found = GraphStack.FirstOrDefault(m => m.id.Equals(id));

                if (found != null)
                {
                    var peek = GraphStack.Peek();

                    //already last one so ignore
                    if(peek.id.Equals(id))
                    {
                        return;
                    }

                    var g = GraphStack.Pop();

                    while (GraphStack.Count > 0 && !g.id.Equals(id))
                    {
                        g = GraphStack.Pop();
                    }

                    //since we popped it out
                    //we push it back
                    Push(g.node, g.graph, g.type, g.parameter);
                }
            }
        }

        public void LoadGraph(string data, string CWD, bool readOnly = false, bool loadUI = true)
        {
            Release();

            Graph g  = new ImageGraph("Untitled");

            if (string.IsNullOrEmpty(data))
            {   
                return;
            }

            pinIndex = 0;

            g.CWD = CWD;
            long ms = Environment.TickCount;
            g.FromJson(data);
            
            Original = g;

            Log.Info("Loaded graph in {0:0}ms", Environment.TickCount - ms);

            g.ReadOnly = ReadOnly;
            Crumbs.Clear();
            BreadCrumb cb = new BreadCrumb(Crumbs, "Root", this, null);
            LoadGraph(g, loadUI);
        }

        public string GetGraphData()
        {
            if(!string.IsNullOrEmpty(StoredGraph))
            {
                return StoredGraph;
            }

            return Original.GetJson();
        }

        protected void LoadGraphSizeSelect()
        {
            graphIsLoadingSizeSelect = true;

            int index = Array.IndexOf(Graph.GRAPH_SIZES, Math.Max(Graph.Width, Graph.Height));
            if(index == -1)
            {
                index = Array.IndexOf(Graph.GRAPH_SIZES, Graph.DEFAULT_SIZE);
            }
           
            if(index > -1)
            {
                DefaultNodeSize.SelectedIndex = index;
            }
        }

        protected void LoadGraphUI()
        {
            if (Graph is FunctionGraph)
            {
                FunctionGraph fg = (FunctionGraph)Graph;

                if ((fg.ExpectedOutput & NodeType.Float) != 0 && (fg.ExpectedOutput & NodeType.Float4) != 0)
                {
                    OutputRequirementsLabel.Text = "Required Output Node Type: Float or Float4";
                }
                else if ((int)fg.ExpectedOutput == 0)
                {
                    OutputRequirementsLabel.Text = "Required Output Node Type: Any";
                }
                else
                {
                    OutputRequirementsLabel.Text = "Required Output Node Type: " + fg.ExpectedOutput.ToString();
                }

                ApplySize.Visibility = Visibility.Collapsed;
                DefaultNodeSize.Visibility = Visibility.Collapsed;
                ApplySizeLabel.Visibility = Visibility.Collapsed;

                ContextMenu = (ContextMenu)Resources["FunctionContextMenu"];
            }
            else
            {
                ApplySize.Visibility = Visibility.Visible;
                DefaultNodeSize.Visibility = Visibility.Visible;
                ApplySizeLabel.Visibility = Visibility.Visible;

                LoadGraphSizeSelect();

                OutputRequirementsLabel.Text = "";

                ContextMenu = null;
            }

            for(int i = 0; i < Graph.Nodes.Count; i++)
            {
                Node n = Graph.Nodes[i];

                if (n is CommentNode)
                {
                    UICommentNode unode = new UICommentNode(n, this, n.ViewOriginX, n.ViewOriginY, XShift, YShift, Scale);
                    unode.HorizontalAlignment = HorizontalAlignment.Left;
                    unode.VerticalAlignment = VerticalAlignment.Top;
                    ViewPort.Children.Add(unode);
                    GraphNodes.Add(unode);
                    lookup[n.Id] = unode;
                }
                else if (n is PinNode)
                {
                    UIPinNode unode = new UIPinNode(n, this, n.ViewOriginX, n.ViewOriginY, XShift, YShift, Scale);
                    unode.HorizontalAlignment = HorizontalAlignment.Left;
                    unode.VerticalAlignment = VerticalAlignment.Top;
                    ViewPort.Children.Add(unode);
                    GraphNodes.Add(unode);
                    lookup[n.Id] = unode;
                    Pins.Add(unode);
                }
                else
                {
                    UINode unode = new UINode(n, this, n.ViewOriginX, n.ViewOriginY, XShift, YShift, Scale);

                    unode.HorizontalAlignment = HorizontalAlignment.Left;
                    unode.VerticalAlignment = VerticalAlignment.Top;
                    ViewPort.Children.Add(unode);
                    GraphNodes.Add(unode);

                    if (graphInitedWithTemplate)
                    {
                        unode.Offset(ViewPort.ActualWidth * 0.5, ViewPort.ActualHeight * 0.5);
                    }

                    TryAndLinkOutputPreview(unode);

                    lookup[n.Id] = unode;
                }
            }

            graphInitedWithTemplate = false;

            Task.Delay(25).ContinueWith((Task t) =>
            {
                App.Current.Dispatcher.Invoke(() =>
                {
                    //foreach uinode connect up
                    for(int i = 0; i < GraphNodes.Count; i++)
                    {
                        IUIGraphNode n = GraphNodes[i];

                        n.LoadConnections(lookup);
                    }

                    Graph.ReadOnly = ReadOnly;

                    isLoadingGraph = true;
                    Graph.TryAndProcess();
                    if(Graph is FunctionGraph || Graph.Synchronized)
                    {
                        isLoadingGraph = false;
                    }
                });
            });
        }

        protected void TryAndLinkOutputPreview(UINode node)
        {
            UI3DPreview preview = UI3DPreview.Instance;
            if(node.Node is OutputNode && preview != null)
            {
                OutputNode n = node.Node as OutputNode;

                if(n.OutType == OutputType.basecolor)
                {
                    preview.SetAlbedoNode(node);
                }
                else if(n.OutType == OutputType.metallic)
                {
                    preview.SetMetallicNode(node);
                }
                else if(n.OutType == OutputType.roughness)
                {
                    preview.SetRoughnessNode(node);
                }
                else if(n.OutType == OutputType.normal)
                {
                    preview.SetNormalNode(node);
                }
                else if(n.OutType == OutputType.occlusion)
                {
                    preview.SetOcclusionNode(node);
                }
                else if(n.OutType == OutputType.height)
                {
                    preview.SetHeightNode(node);
                }
                else if(n.OutType == OutputType.thickness)
                {
                    preview.SetThicknessNode(node);
                }
            }
        }

        public void Save(string f, bool saveAs = false)
        {
            bool saveAsArchive = System.IO.Path.GetExtension(f).Contains("mtga");
            string archivePath = null;

            if(saveAsArchive)
            {
                archivePath = f;
                f = f.Replace(".mtga", ".mtg");
            }

            if(Original == null)
            {
                if (!string.IsNullOrEmpty(StoredGraph))
                {
                    Original = new Graph("temp");
                    Original.FromJson(StoredGraph);
                }
                else
                {
                    return;
                }
            }

            string cwd = System.IO.Path.GetDirectoryName(f);
            string name = System.IO.Path.GetFileNameWithoutExtension(f);

            if (Original.Name.Equals("Untitled") || saveAs)
            {
                StoredGraphName = Original.Name = name;
            }

            FilePath = f;

            Original.CopyResources(cwd, saveAs && !saveAsArchive && !FromArchive);

            System.IO.File.WriteAllText(f, GetGraphData());

            Modified = false;

            ///save as archive and remove temporary files
            if (saveAsArchive && !string.IsNullOrEmpty(archivePath))
            {
                MTGArchive archive = new MTGArchive(archivePath);
                if(!archive.Create(f))
                {
                    Log.Error("Failed to create materia graph archive file");
                    return;
                }

                FromArchive = true;
                FromArchivePath = archivePath;
            }
        }

        public void Save()
        {
            if (string.IsNullOrEmpty(FilePath)) return;

            if (FromArchive)
            {
                Save(FromArchivePath, false);
            }
            else
            {
                System.IO.File.WriteAllText(FilePath, GetGraphData());
            }

            Modified = false;
        }

        public void DeleteTempArchiveData()
        {
            if(FromArchive && !string.IsNullOrEmpty(FromArchivePath))
            {
                string tempdir = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "temp", System.IO.Path.GetFileNameWithoutExtension(FromArchivePath));

                if(System.IO.Directory.Exists(tempdir))
                {
                    System.IO.Directory.Delete(tempdir, true);
                }
            }
        }

        public void ClearView()
        {
            SelectedNodes.Clear();
            SelectedIds.Clear();

            //reset viewport etc
            XShift = 0;
            YShift = 0;
            Scale = 1;

            foreach(IUIGraphNode n in GraphNodes)
            {
                //remove UI components only
                n.DisposeNoRemove();
                ViewPort.Children.Remove(n as UIElement);
            }

            lookup.Clear();
            GraphNodes.Clear();
            ViewPort.Children.Clear();
        }

        public void Release()
        {
            if(UINodeParameters.Instance != null)
            {
                UINodeParameters.Instance.ClearView();
            }

            foreach (IUIGraphNode n in GraphNodes)
            {
                //whoops forgot to properly dispose
                //the nodes but not do an actual remove undo
                n.DisposeNoRemove();
                ViewPort.Children.Remove(n as UIElement);
            }

            lookup.Clear();
            GraphNodes.Clear();
            SelectedIds.Clear();
            SelectedNodes.Clear();
            Pins.Clear();

            if (Graph != null)
            {
                Graph.OnGraphUpdated -= Graph_OnGraphUpdated;
                Graph.OnHdriChanged -= Graph_OnHdriChanged;
                Graph.OnGraphNameChanged -= Graph_OnGraphNameChanged;
                Graph.OnGraphLinesChanged -= Graph_OnGraphLinesChanged;

                Graph.Dispose();
                Graph = null;
            }

            if(Original != null)
            {
                Original.Dispose();
                Original = null;
            }

            ViewPort.Children.Clear();
        }


        public IUIGraphNode GetNode(string id)
        {
            IUIGraphNode n = null;

            lookup.TryGetValue(id, out n);

            return n;
        }

        /// <summary>
        /// Helper for Undo Redo system. Does not trigger new undo added to stack
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Tuple<string, Point, List<NodeConnection>> RemoveNode(string id)
        {
            var n = GraphNodes.Find(m => m.Id.Equals(id));

            Tuple<string, Point, List<NodeConnection>> result = null;

            if(n != null)
            {
                result = new Tuple<string, Point, List<NodeConnection>>(n.Node.GetJson(), n.Origin, n.Node.GetParentConnections());

                if(n is UIPinNode)
                {
                    Pins.Remove(n as UIPinNode);
                    if(pinIndex >= Pins.Count)
                    {
                        pinIndex = Pins.Count - 1;
                    }
                }

                //to remove connections but not to remove from graph
                n.DisposeNoRemove();

                GraphNodes.Remove(n);
                ViewPort.Children.Remove(n as UIElement);
                lookup.Remove(n.Id);

                //remove from underlying graph
                Graph.Remove(n.Node);

                Modified = true;
            }

            return result;
        }

        /// <summary>
        /// Helper for Undeo Redo System. Does not trigger new undo added to stack.
        /// </summary>
        /// <param name="json"></param>
        /// <param name="p"></param>
        /// <returns></returns>
        public IUIGraphNode AddNodeFromJson(string json, Point p)
        {
            try
            {
                Node.NodeData nd = JsonConvert.DeserializeObject<Node.NodeData>(json);

                if (nd == null) return null;

                Node n = null;
                IUIGraphNode unode = null;

                n = Graph.CreateNode(nd.type);

                if(n != null)
                {
                    n.Id = nd.id;
                    n.FromJson(json);
                    if (!Graph.Add(n)) return unode;

                    n.SetConnections(Graph.NodeLookup, nd.outputs);

                    if (n is CommentNode)
                    {
                        unode = new UICommentNode(n, this, p.X, p.Y, XShift, YShift, Scale);
                    }
                    else if(n is PinNode)
                    {
                        unode = new UIPinNode(n, this, p.X, p.Y, XShift, YShift, Scale);
                        Pins.Add(unode as UIPinNode);
                    }
                    else
                    {
                        unode = new UINode(n, this, p.X, p.Y, XShift, YShift, Scale);
                    }

                    (unode as UserControl).HorizontalAlignment = HorizontalAlignment.Left;
                    (unode as UserControl).VerticalAlignment = VerticalAlignment.Top;
                    lookup[n.Id] = unode;
                    ViewPort.Children.Add(unode as UIElement);
                    GraphNodes.Add(unode);

                    n.TryAndProcess();

                    Modified = true;

                    Task.Delay(25).ContinueWith(t =>
                    {
                        App.Current.Dispatcher.Invoke(() =>
                        {
                            unode.LoadConnections(lookup);
                        });
                    });
                }

                return unode;
            }
            catch (Exception e)
            {
                Log.Error(e);
            }

            return null;
        }

        /// <summary>
        /// real lookup is required as the nodes added from json
        /// from a paste command will have new ids
        /// but will need to lookup the old ids
        /// to reconnect any that may have been connected when pasted
        /// </summary>
        /// <param name="json"></param>
        /// <param name="realLookup"></param>
        /// <returns></returns>
        protected IUIGraphNode AddNodeFromJson(string json, Dictionary<string, Node> realLookup, Dictionary<string, string> cparams, out Node.NodeData data)
        {
            try
            {
                Node.NodeData nd = data = JsonConvert.DeserializeObject<Node.NodeData>(json);
                if (nd == null) return null;

                Node n = null;
                IUIGraphNode unode = null;

                n = Graph.CreateNode(nd.type);

                if (n != null)
                { 
                    n.GetType();
                    if (!Graph.Add(n)) return unode;

                    realLookup[nd.id] = n;
                    n.FromJson(json);

                    Graph.PasteParameters(cparams, nd, n);

                    if (n is CommentNode)
                    {
                        unode = new UICommentNode(n, this, 0, 0, XShift, YShift, Scale);
                    }
                    else if(n is PinNode)
                    {
                        unode = new UIPinNode(n, this, 0, 0, XShift, YShift, Scale);
                        Pins.Add(unode as UIPinNode);
                    }
                    else
                    {
                        unode = new UINode(n, this, 0, 0, XShift, YShift, Scale);
                    }

                    (unode as UserControl).HorizontalAlignment = HorizontalAlignment.Left;
                    (unode as UserControl).VerticalAlignment = VerticalAlignment.Top;
                    lookup[n.Id] = unode;
                    ViewPort.Children.Add(unode as UIElement);
                    GraphNodes.Add(unode);

                    n.TryAndProcess();

                    UndoRedoManager.AddUndo(new CreateNode(Id, unode.Id, this));

                    Modified = true;
                }

                return unode;
            }
            catch (Exception e)
            {
                Log.Error(e);
                data = null;
                return null;
            }
        }

        protected IUIGraphNode AddNode(string type, Point p)
        {
            Node n = null;
            IUIGraphNode unode = null;

            n = Graph.CreateNode(type);

            if (n != null)
            {
                if (n is GraphInstanceNode)
                {
                    GraphInstanceNode gn = (GraphInstanceNode)n;
                    gn.Load(type);
                }
                else if(n is CommentNode)
                {
                    //add padding
                    n.Width = 256 + 16;
                    n.Height = 256 + 38;
                }

                Modified = true;

                n.ViewOriginX = p.X;
                n.ViewOriginY = p.Y;

                if (!Graph.Add(n)) return unode;

                if (n is CommentNode)
                {
                    unode = new UICommentNode(n, this, p.X, p.Y, XShift, YShift, Scale);
                }
                else if(n is PinNode)
                {
                    unode = new UIPinNode(n, this, p.X, p.Y, XShift, YShift, Scale);
                    Pins.Add(unode as UIPinNode);
                }
                else
                {
                    unode = new UINode(n, this, p.X, p.Y, XShift, YShift, Scale);
                }

                (unode as UserControl).HorizontalAlignment = HorizontalAlignment.Left;
                (unode as UserControl).VerticalAlignment = VerticalAlignment.Top;
                lookup[n.Id] = unode;
                ViewPort.Children.Add(unode as UIElement);
                GraphNodes.Add(unode);

                n.TryAndProcess();

                UndoRedoManager.AddUndo(new CreateNode(Id, unode.Id, this));

                return unode;
            }

            return null;
        }

        public void PrepareInsert()
        {
            insertPos = Mouse.GetPosition(ViewPort);
            ToWorld(ref insertPos);
        }

        public void Insert(string type)
        {
            IUIGraphNode n = AddNode(type, insertPos);

            if (n == null) return;

            if(UINodePoint.SelectOrigin != null)
            {
                UINodePoint p = UINodePoint.SelectOrigin;

                if(p.To != null && p.Output != null)
                {
                    if(p.To.Count == 1)
                    {
                        var input = p.To[0];

                        //try and find similar connection point
                        //the input will disconnect from parent
                        foreach(var opoint in n.OutputNodes)
                        {
                            if(opoint.CanConnect(input))
                            {
                                opoint.ConnectToNode(input);
                                break;
                            }
                        }
                    }

                    //try and find similar connection point
                    foreach(var ipoint in n.InputNodes)
                    {
                        if(p.CanConnect(ipoint))
                        {
                            p.ConnectToNode(ipoint);
                            break;
                        }
                    }
                }
                else if(p.Input != null)
                {
                    UINodePoint origin = null;
                    if (p.ParentNode != null)
                    {
                        origin = p.ParentNode;
                    }

                    //try and find similar
                    //don't worry it will disconnect
                    //from parent if there is one
                    foreach (var opoint in n.OutputNodes)
                    {
                        if(opoint.CanConnect(p))
                        {
                            opoint.ConnectToNode(p);
                            break;
                        }
                    }

                    if(origin != null)
                    {
                        foreach(var ipoint in n.InputNodes)
                        {
                            if(origin.CanConnect(ipoint))
                            {
                                origin.ConnectToNode(ipoint);
                                break;
                            }
                        }
                    }
                }

                UINodePoint.SelectOrigin = null;
            }
        }

        public void RemoveNode(IUIGraphNode n)
        {
            string json = n.Node.GetJson();
            Point p = n.Origin;

            UndoRedoManager.AddUndo(new DeleteNode(Id, json, n.Node.Id, p, n.Node.GetParentConnections(), this));

            //update pin references
            if(n is UIPinNode)
            {
                Pins.Remove(n as UIPinNode);
                if(pinIndex >= Pins.Count)
                {
                    pinIndex = Pins.Count - 1;
                }
            }

            GraphNodes.Remove(n);
            ViewPort.Children.Remove(n as UIElement);
            lookup.Remove(n.Id);

            //remove from underlying graph
            Graph.Remove(n.Node);

            Modified = true;
        }

        protected override void OnDrop(DragEventArgs e)
        {
            base.OnDrop(e);

            if(e.Data.GetDataPresent(typeof(NodeResource)))
            {
                NodeResource res = (NodeResource)e.Data.GetData(typeof(NodeResource));

                if(res != null)
                {
                    Point p = e.GetPosition(ViewPort);

                    ToWorld(ref p);
 
                    AddNode(res.Type, p);
                }
            }
            else if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] path = (string[])e.Data.GetData(DataFormats.FileDrop);

                foreach (string p in path)
                {
                    string fname = System.IO.Path.GetFileNameWithoutExtension(p);
                    string ext = System.IO.Path.GetExtension(p);

                    if (ext.Equals(".mtg") || ext.Equals(".mtga"))
                    {
                        Point pt = e.GetPosition(ViewPort);
                        ToWorld(ref pt);
                        AddNode(p, pt);
                    }
                }
            }
        }

        public void ToLocal(ref Point p)
        {
            double w3 = ViewPort.ActualWidth * 0.5;
            double h3 = ViewPort.ActualHeight * 0.5;

            p.X = (p.X - w3) * Scale + w3 + XShift;
            p.Y = (p.Y - h3) * Scale + h3 + YShift;
        }

        public void ToWorld(ref Point p)
        {
            //convert to proper space
            //which is the reverse shift and reverse scale
            //applied from the center of the viewport
            //of the actual position in each node
            // (node.originX - (viewport / 2)) * Scale + (viewport / 2) + xShift
            // however we must also scale shift by the reverse

            double w3 = ViewPort.ActualWidth * 0.5;
            double h3 = ViewPort.ActualHeight * 0.5;

            p.X = (p.X - w3) / Scale + w3 - XShift / Scale;
            p.Y = (p.Y - h3) / Scale + h3 - YShift / Scale;
        }

        private void UpdateConnectionPreview()
        {
            if(UINodePoint.SelectOrigin != null)
            {
                //catch for when the node is removed and layout update is still triggered
                try
                {
                    ConnectionPointPreview.Width = CONNECTION_POINT_SIZE * Scale;
                    ConnectionPointPreview.Height = CONNECTION_POINT_SIZE * Scale;
                    ConnectionPointPreview.RadiusX = CONNECTION_POINT_SIZE * Scale * 0.5;
                    ConnectionPointPreview.RadiusY = CONNECTION_POINT_SIZE * Scale * 0.5;

                    UINodePoint origin = UINodePoint.SelectOrigin;
                    UINodePoint dest = UINodePoint.SelectOver;

                    double w2 = CONNECTION_POINT_SIZE * 0.5;
                    double w2s = w2 * Scale;

                    Point r1 = new Point();

                    if (origin.Parent != null)
                    {
                        if (origin.Output != null)
                        {
                            r1 = origin.TransformToAncestor(ViewPort).Transform(new Point(origin.ActualWidth, w2));
                        }
                        else if (origin.Input != null)
                        {
                            r1 = origin.TransformToAncestor(ViewPort).Transform(new Point(0f, w2));
                        }
                    }

                    Point r2 = Mouse.GetPosition(ViewPort);
  
                    if (dest != null && origin.Parent != null)
                    {
                        if (origin.Output != null)
                        {
                            r2 = dest.TransformToAncestor(ViewPort).Transform(new Point(dest.ActualWidth, w2));
                        }
                        else if (origin.Input != null)
                        {
                            r2 = dest.TransformToAncestor(ViewPort).Transform(new Point(0f, w2));
                        }
                    }

                    Path path = ConnectionPathPreview;

                    double dy = r2.Y - r1.Y;

                    Point mid = new Point((r2.X + r1.X) * 0.5f, (r2.Y + r1.Y) * 0.5f + dy * 0.5f);

                    if (path != null)
                    {
                        if (path.Data == null)
                        {
                            path.VerticalAlignment = VerticalAlignment.Top;
                            path.HorizontalAlignment = HorizontalAlignment.Left;
                            PathGeometry p = new PathGeometry();
                            PathFigure pf = new PathFigure();
                            pf.IsClosed = false;
                            pf.StartPoint = r1;

                            BezierSegment seg = new BezierSegment(r1, mid, r2, true);
                            pf.Segments.Add(seg);
                            p.Figures.Add(pf);
                            path.Data = p;
                        }
                        else
                        {
                            PathGeometry p = (PathGeometry)path.Data;
                            PathFigure pf = p.Figures[0];
                            pf.StartPoint = r1;
                            BezierSegment seg = (BezierSegment)pf.Segments[0];
                            seg.Point1 = r1;
                            seg.Point2 = mid;
                            seg.Point3 = r2;
                        }
                    }

                    ConnectionPathPreview = path;

                    if(dest != null)
                    {
                        r2 = dest.TransformToAncestor(ViewPort).Transform(new Point());
                    }
                    else
                    {
                        r2.X -= w2s;
                        r2.Y -= w2s;
                    }

                    Canvas.SetLeft(ConnectionPointPreview, r2.X);
                    Canvas.SetTop(ConnectionPointPreview, r2.Y);
                }
                catch { }
            }
        }

        private void ViewPort_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.MiddleButton == MouseButtonState.Pressed)
            {
                Grid g = sender as Grid;
                moving = true;
                start = e.GetPosition(g);
            }
            else if (e.LeftButton == MouseButtonState.Pressed)
            {
                UINodePoint.SelectOrigin = null;

                Grid g = sender as Grid;
                Point p = e.GetPosition(g);

                selectedStartedIn.Clear();

                //we don't want to start selection
                //if we are already over a node
                //except if we are in a comment node
                foreach(IUIGraphNode n in GraphNodes)
                {
                    if (!(n is UICommentNode))
                    {
                        if (n.ContainsPoint(p))
                        {
                            return;
                        }
                    }
                    else
                    {
                        if (n.ContainsPoint(p))
                        {
                            //this is for handling
                            //where we should only select
                            //the nodes inside of comment block
                            //if the selection point was in a comment block
                            //to start with
                            //and not to select the comment block itself
                            selectedStartedIn.Add(n.Id);
                        }
                    }
                }

                //handle a quick shortcut to load graph settings
                if(selectedStartedIn.Count == 0 && e.ClickCount > 1)
                {
                    if(UINodeParameters.Instance != null)
                    {
                        UINodeParameters.Instance.SetActive(Graph);
                        return;
                    }
                }

                selectionRect.Width = 0;
                selectionRect.Height = 0;

                ViewPort.Children.Add(selectionRect);
                selectionRect.Margin = new Thickness(p.X, p.Y, 0, 0);

                start = p;

                ViewPort.CaptureMouse();
            }
        }

        private void ViewPort_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if(e.MiddleButton == MouseButtonState.Released)
            {
                moving = false;
            }

            if(ViewPort.Children.Contains(selectionRect))
            {
                //determine nodes to add to selection
                DetermineSelection();
            }

            ViewPort.ReleaseMouseCapture();
            ViewPort.Children.Remove(selectionRect);
        }

        private void ViewPort_MouseMove(object sender, MouseEventArgs e)
        {
            //handle preview of node point selection
            if(UINodePoint.SelectOrigin != null)
            {
                if(ConnectionPointPreview.Parent == null)
                {
                    ConnectionPointPreview.Fill = UINodePoint.SelectOrigin.ColorBrush;
                    ViewPort.Children.Add(ConnectionPointPreview);
                }

                if(ConnectionPathPreview.Parent == null)
                {
                    ConnectionPathPreview.Stroke = UINodePoint.SelectOrigin.ColorBrush;
                    ViewPort.Children.Add(ConnectionPathPreview);
                }

                UpdateConnectionPreview();
            }
            else
            {
                if(ConnectionPathPreview.Parent != null)
                {
                    ViewPort.Children.Remove(ConnectionPathPreview);
                }

                if(ConnectionPointPreview.Parent != null)
                {
                    ViewPort.Children.Remove(ConnectionPointPreview);
                }
            }

            if(moving)
            {
                Grid g = sender as Grid;
                Point p = e.GetPosition(g);

                Point diff = new Point(p.X - start.X, p.Y - start.Y);

                XShift += diff.X;
                YShift += diff.Y;

                Graph.ShiftX = XShift;
                Graph.ShiftY = YShift;

                foreach (IUIGraphNode n in GraphNodes)
                {
                    n.Move(diff.X, diff.Y);
                }

                start = p;

                UpdateGrid();
            }
            else if(e.LeftButton == MouseButtonState.Pressed)
            {
                Grid g = sender as Grid;
                double sx = selectionRect.Margin.Left;
                double sy = selectionRect.Margin.Top;
                Point p = e.GetPosition(g);

                double diffx = p.X - start.X;
                double diffy = p.Y - start.Y;

                if(diffx < 0 && diffy < 0)
                {
                    selectionRect.Margin = new Thickness(start.X + diffx, start.Y + diffy, 0, 0);
                }
                else if(diffx < 0)
                {
                    selectionRect.Margin = new Thickness(start.X + diffx, start.Y, 0, 0);
                }
                else if(diffy < 0)
                {
                    selectionRect.Margin = new Thickness(start.X, start.Y + diffy, 0, 0);
                }

                selectionRect.Width = Math.Abs(diffx);
                selectionRect.Height = Math.Abs(diffy);
            }
        }

        public void MoveMultiSelect(double diffx, double diffy)
        {
            foreach(IUIGraphNode n in SelectedNodes)
            {
                n.Offset(diffx, diffy);
            }
        }

        void ResetView()
        {
            XShift = 0;
            YShift = 0;
            Scale = 1;

            Graph.ShiftX = XShift;
            Graph.ShiftY = YShift;
            Graph.Zoom = Scale;

            foreach (IUIGraphNode n in GraphNodes)
            {
                n.MoveTo(XShift, YShift);
                n.UpdateScale(Scale);
            }

            ZoomLevel.Text = String.Format("{0:0}", Scale * 100);

            UpdateGrid();
        }
        
        void AlignSelectedNodesVertical()
        {
            double midX = 0;

            if (SelectedNodes.Count <= 1) return;

            List<IUIGraphNode> selected = SelectedNodes.FindAll(m => m is UINode || m is UIPinNode);

            //find average position
            for (int i = 0; i < selected.Count; i++)
            {
                midX += selected[i].Origin.X;
            }

            midX /= selected.Count;

            for (int i = 0; i < selected.Count; i++)
            {
                Point p = selected[i].Origin;
                selected[i].OffsetTo(midX, p.Y);
            }
        }

        void AlignSelectedNodesHorizontal()
        {
            double midY = 0;

            if (SelectedNodes.Count <= 1) return;

            List<IUIGraphNode> selected = SelectedNodes.FindAll(m => m is UINode || m is UIPinNode);

            //find average position
            for (int i = 0; i < selected.Count; i++)
            {
                midY += selected[i].Origin.Y;
            }

            midY /= selected.Count;

            for(int i = 0; i < selected.Count; i++)
            {
                Point p = selected[i].Origin;
                selected[i].OffsetTo(p.X, midY);
            }
        }

        public void NextPin()
        {
            if (Pins.Count == 0) return;
            if(pinIndex >= Pins.Count)
            {
                pinIndex = 0;
            }
            UIPinNode n = Pins[pinIndex++];
            ShiftToNode(n);
            n.Focus();
            Keyboard.Focus(n);
        }

        private void ShiftToNode(IUIGraphNode node)
        {
            XShift = 0;
            YShift = 0;

            Rect b = node.UnscaledBounds;

            double w2 = ViewPort.ActualWidth * 0.5;
            double h2 = ViewPort.ActualHeight * 0.5;

            XShift = -(b.Left - w2) * Scale - (b.Width * 0.5 * Scale);
            YShift = -(b.Top - h2) * Scale - (b.Height * 0.5 * Scale);

            foreach(IUIGraphNode n in GraphNodes)
            {
                n.MoveTo(XShift, YShift);
            }

            Graph.ShiftX = XShift;
            Graph.ShiftY = YShift;

            UpdateGrid();
        }

        void FitNodesIntoView()
        {
            XShift = 0;
            YShift = 0;

            double minViewArea = (double)Math.Min(ViewPort.ActualWidth, ViewPort.ActualHeight);

            Rect b = GetNodeBounds();

            if(b.Width >= b.Height)
            {
                Scale = (float)(minViewArea / b.Width);
            }
            else
            {
                Scale = (float)(minViewArea / b.Height);
            }

            Scale = Math.Max(0.1f, Scale);
            Scale = Math.Min(3.0f, Scale);

            double w2 = ViewPort.ActualWidth * 0.5;
            double h2 = ViewPort.ActualHeight * 0.5;

            XShift = -(b.Left - w2) * Scale - (b.Width * 0.5 * Scale);
            YShift = -(b.Top - h2) * Scale - (b.Height * 0.5 * Scale);

            foreach(IUIGraphNode n in GraphNodes)
            {
                n.MoveTo(XShift, YShift);
                n.UpdateScale(Scale);
            }

            Graph.ShiftX = XShift;
            Graph.ShiftY = YShift;
            Graph.Zoom = Scale;

            ZoomLevel.Text = String.Format("{0:0}", Scale * 100);

            UpdateGrid();
        }

        Rect GetSelectedNodeBounds()
        {
            if(SelectedNodes.Count == 0)
            {
                return new Rect(0, 0, 0, 0);
            }

            double minX = double.PositiveInfinity;
            double minY = double.PositiveInfinity;
            double maxX = double.NegativeInfinity;
            double maxY = double.NegativeInfinity;

            foreach (IUIGraphNode n in SelectedNodes)
            {
                Rect b = n.UnscaledBounds;

                if (b.Left < minX)
                {
                    minX = b.Left;
                }
                if (b.Right > maxX)
                {
                    maxX = b.Right;
                }

                if (b.Top < minY)
                {
                    minY = b.Top;
                }
                if (b.Bottom > maxY)
                {
                    maxY = b.Bottom;
                }
            }

            return new Rect(minX, minY, Math.Abs(maxX - minX), Math.Abs(maxY - minY));
        }

        Rect GetNodeBounds()
        {
            if(GraphNodes.Count == 0)
            {
                return new Rect(ViewPort.ActualWidth * 0.5, ViewPort.Height * 0.5, 0, 0);
            }

            double minX = double.PositiveInfinity;
            double minY = double.PositiveInfinity;
            double maxX = double.NegativeInfinity;
            double maxY = double.NegativeInfinity;

            foreach(IUIGraphNode n in GraphNodes)
            {
                Rect b = n.UnscaledBounds;

                if(b.Left < minX)
                {
                    minX = b.Left;
                }
                if(b.Right > maxX)
                {
                    maxX = b.Right;
                }

                if(b.Top < minY)
                {
                    minY = b.Top;
                }
                if(b.Bottom > maxY)
                {
                    maxY = b.Bottom;
                }
            }

            return new Rect(minX, minY, Math.Abs(maxX - minX), Math.Abs(maxY - minY));
        }

        void DetermineSelection()
        {
            Rect r = new Rect(selectionRect.Margin.Left, selectionRect.Margin.Top, selectionRect.Width, selectionRect.Height);

            int count = 0;

            foreach(IUIGraphNode n in GraphNodes)
            {
                if (n.IsInRect(r) && !selectedStartedIn.Contains(n.Id))
                {
                    if (SelectedNodes.Contains(n))
                    {
                        SelectedNodes.Remove(n);
                        n.HideBorder();
                    }
                    else
                    {
                        SelectedNodes.Add(n);
                        n.ShowBorder();
                    }

                    count++;
                }
            }

            selectedStartedIn.Clear();

            if(count == 0)
            {
                ClearMultiSelect();
            }
        }

        public void ToggleMultiSelect(IUIGraphNode n)
        {
            if (SelectedNodes.Contains(n))
            {
                SelectedIds.Remove(n.Id);
                SelectedNodes.Remove(n);
                n.HideBorder();           
            }
            else
            {
                SelectedIds.Add(n.Id);
                SelectedNodes.Add(n);
                n.ShowBorder();
            }
        }

        public void ClearMultiSelect()
        {
            List<IUIGraphNode> toRemove = SelectedNodes.ToList();

            SelectedNodes.Clear();
            SelectedIds.Clear();

            foreach (IUIGraphNode n in toRemove)
            {
                n.HideBorder();
            }
        }

        private void ViewPort_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            PrevScale = Scale;

            if(e.Delta < 0)
            {
                ZoomNodesOut();
            }
            else
            {
                ZoomNodesIn();
            }
        }

        void ZoomNodesOut()
        {
            Scale -= InvGridSnap;
            if(Scale < InvGridSnap)
            {
                Scale = InvGridSnap;
            }

            ZoomLevel.Text = String.Format("{0:0}", Scale * 100);

            foreach (IUIGraphNode n in GraphNodes)
            {
                n.UpdateScale(Scale);
            }

            Graph.Zoom = Scale;

            UpdateGrid();

            UpdateConnectionPreview();
        }

        void ZoomNodesIn()
        {
            Scale += InvGridSnap;
            if (Scale > 3.0f)
            {
                Scale = 3.0f;
            }

            ZoomLevel.Text = String.Format("{0:0}", Scale * 100);

            foreach (IUIGraphNode n in GraphNodes)
            {
                n.UpdateScale(Scale);
            }

            Graph.Zoom = Scale;

            UpdateGrid();

            UpdateConnectionPreview();
        }

        void UpdateGrid()
        {
            DrawingBrush db = new DrawingBrush();
            db.TileMode = TileMode.Tile;
            db.Viewbox = new Rect(XShift, YShift, GridSnap * Scale, GridSnap * Scale);
            db.Viewport = new Rect(XShift , YShift, GridSnap * Scale, GridSnap * Scale);
            db.ViewportUnits = BrushMappingMode.Absolute;
            db.ViewboxUnits = BrushMappingMode.Absolute;

            GeometryDrawing geom = new GeometryDrawing(null, new Pen(new SolidColorBrush(Color.FromRgb(22, 22, 22)), 0.5), new RectangleGeometry(new Rect(XShift,  YShift, GridSnap * Scale, GridSnap * Scale)));
            db.Drawing = geom;
            ViewPort.Background = db;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if(!string.IsNullOrEmpty(StoredGraph))
            {
                LoadGraph(StoredGraph, StoredGraphCWD, false, StoredGraphStack == null || StoredGraphStack.Length == 0);
                StoredGraph = null;

                RestoreStack();
            }
        }

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
           
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            if (Original != null)
            {
                CaptureStack();

                StoredGraphName = Original.Name;
                StoredGraph = Original.GetJson();
                StoredGraphCWD = Original.CWD;

                Release();
            }
        }

        private void Ratio1_Click(object sender, RoutedEventArgs e)
        {
            ResetView();
        }

        private void FitIntoView_Click(object sender, RoutedEventArgs e)
        {
            FitNodesIntoView();
        }

        private void ZoomOut_Click(object sender, RoutedEventArgs e)
        {
            ZoomNodesOut();
        }

        private void ZoomIn_Click(object sender, RoutedEventArgs e)
        {
            ZoomNodesIn();
        }

        private void AlignHoriz_Click(object sender, RoutedEventArgs e)
        {
            AlignSelectedNodesHorizontal();
        }

        private void AlignVert_Click(object sender, RoutedEventArgs e)
        {
            AlignSelectedNodesVertical();
        }

        private void DefaultNodeSize_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(graphIsLoadingSizeSelect)
            {
                graphIsLoadingSizeSelect = false;
                return;
            }

            if (!IsLoaded || Graph == null) return;
            
            int index = DefaultNodeSize.SelectedIndex;

            if(index > -1)
            {
                int size = Graph.GRAPH_SIZES[index];

                Graph.Width = Graph.Height = size;
            }
        }

        private void ApplySize_Click(object sender, RoutedEventArgs e)
        {
            for(int i = 0; i < SelectedNodes.Count; i++)
            {
                IUIGraphNode n = SelectedNodes[i];
                if (n is UINode)
                {
                    //do not update bitmap nodes
                    //as they are controlled by
                    //the loaded image
                    if (!(n.Node is BitmapNode))
                    {
                        //slight optimization
                        //so only OnWidthHeightSet is triggered once
                        //instead of twice
                        n.Node.SetSize(Graph.Width, Graph.Height);
                    }
                }
            }
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            MenuItem item = sender as MenuItem;

            if (Graph is FunctionGraph)
            {
                if (item.Header.ToString().ToLower().Contains("import func"))
                {
                    HandleFunctionImport();
                }
                else if (item.Header.ToString().ToLower().Contains("export func"))
                {
                    HandleFunctionExport();
                }
            }
        }

        private void HandleFunctionExport()
        {
            System.Windows.Forms.SaveFileDialog dialog = new System.Windows.Forms.SaveFileDialog();
            dialog.CheckPathExists = true;
            dialog.CheckFileExists = false;
            dialog.Filter = "Materia Function Graph (*.mtfg)|*.mtfg";

            if(dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                try
                {
                    string data = Graph.GetJson();
                    System.IO.File.WriteAllText(dialog.FileName, data);
                    Log.Info("Function Graph Exported: {0}", System.IO.Path.GetFileNameWithoutExtension(dialog.FileName));
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }
        }

        private void HandleFunctionImport()
        {
            if (MessageBox.Show("All current nodes will be replaced, continue with import?", "", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                System.Windows.Forms.OpenFileDialog dialog = new System.Windows.Forms.OpenFileDialog();
                dialog.CheckFileExists = true;
                dialog.CheckPathExists = true;
                dialog.Filter = "Materia Function Graph (*.mtfg)|*.mtfg";
                dialog.Multiselect = false;

                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    try
                    {
                        string txt = System.IO.File.ReadAllText(dialog.FileName);

                        if (string.IsNullOrEmpty(txt)) return;

                        ClearView();

                        string oldName = Graph.Name;

                        Graph.Dispose();
                        Graph.FromJson(txt);
                        Graph.SetConnections();

                        Graph.Name = oldName;

                        LoadGraphUI();

                        FitNodesIntoView();

                        Log.Info("Function Graph Imported: {0}", System.IO.Path.GetFileNameWithoutExtension(dialog.FileName));
                    }
                    catch (Exception e)
                    {
                        Log.Error(e);
                    }
                }
            }
        }

        public void Dispose()
        {
            if (isDisposed) return;

            isDisposed = true;
            Release();
        }
    }
}
