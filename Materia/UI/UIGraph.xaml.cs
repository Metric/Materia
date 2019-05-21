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
using Materia.Imaging;
using Materia.Nodes.Atomic;
using Materia.Nodes.Helpers;
using System.Threading;
using Newtonsoft.Json;
using Materia.Undo;
using Materia.UI.Components;
using Materia.Hdri;

namespace Materia
{
    public enum GraphStackType
    {
        Parameter,
        Pixel,
        FX,
        CustomFunction
    }
    /// <summary>
    /// Interaction logic for UIGraph.xaml
    /// </summary>
    public partial class UIGraph : UserControl
    {
        bool moving;
        Point start;

        public string Id { get; protected set; }

        public float Scale { get; protected set; }
        public float PrevScale { get; protected set; }

        public List<UINode> GraphNodes { get; protected set; }

        public Graph Graph { get; protected set; }

        protected Dictionary<string, UINode> lookup;

        public double XShift { get; protected set; }
        public double YShift { get; protected set; }

        public bool ReadOnly { get; protected set; }

        public bool Modified { get; protected set; }

        public List<UINode> SelectedNodes { get; protected set; }
        protected Rectangle selectionRect;

        protected Graph Original { get; set; }

        public const float GridSnap = 10;
        const float InvGridSnap = 1.0f / 10.0f;

        public string FilePath { get; protected set; }

        public string StoredGraph { get; protected set; }
        public string StoredGraphCWD { get; protected set; }
        public string[] StoredGraphStack { get; protected set; }
        protected string StoredGraphName { get; set; }

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

            public override bool Equals(object obj)
            {
                if(obj is GraphStackItem)
                {
                    GraphStackItem s = obj as GraphStackItem;
                    return s.id == id && s.type == type && s.parameter == parameter;
                }

                return false;
            }

            public override int GetHashCode()
            {
                return parameter.GetHashCode() ^ id.GetHashCode() ^ type.GetHashCode();
            }
        }

        public UIGraph()
        {
            InitializeComponent();

            Id = Guid.NewGuid().ToString();

            GraphStack = new Stack<GraphStackItem>();
            SelectedNodes = new List<UINode>();

            selectionRect = new Rectangle();
            selectionRect.Stroke = new SolidColorBrush(Colors.DarkRed);
            selectionRect.StrokeDashArray.Add(2);
            selectionRect.StrokeDashArray.Add(2);
            selectionRect.StrokeDashCap = PenLineCap.Round;
            selectionRect.StrokeEndLineCap = PenLineCap.Round;
            selectionRect.StrokeThickness = 1;
            selectionRect.HorizontalAlignment = HorizontalAlignment.Left;
            selectionRect.VerticalAlignment = VerticalAlignment.Top;

            selectionRect.Fill = null;
            selectionRect.RenderTransformOrigin = new Point(0, 0);

            lookup = new Dictionary<string, UINode>();
            Scale = 1;
            PrevScale = 1;

            moving = false;
            GraphNodes = new List<UINode>();
            Original = Graph = new ImageGraph("Untitled");

            UpdateGrid();

            Crumbs.Clear();

            ConnectionPointPreview.RadiusX = 8;
            ConnectionPointPreview.RadiusY = 8;

            ConnectionPointPreview.Width = 16;
            ConnectionPointPreview.Height = 16;

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

        public void MarkModified()
        {
            Modified = true;
        }

        public struct GraphCopyData
        {
            public List<string> nodes;
        }

        public void TryAndDelete()
        {
            foreach (UINode n in SelectedNodes)
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

                foreach (UINode n in SelectedNodes)
                {
                    nodes.Add(n.Node.GetJson());
                }

                GraphCopyData cd = new GraphCopyData()
                {
                    nodes = nodes,
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


                List<UINode> added = new List<UINode>();
                Dictionary<string, string> jsonContent = new Dictionary<string, string>();
                Dictionary<string, Node> realLookup = new Dictionary<string, Node>();

                for (int i = 0; i < cd.nodes.Count; i++)
                {
                    string json = cd.nodes[i];
                    var unode = AddNodeFromJson(json, realLookup);
                    if (unode != null)
                    {
                        added.Add(unode);
                        jsonContent[unode.Node.Id] = json;
                    }
                }

                double minX = float.MaxValue;
                double minY = float.MaxValue;

                //find minx and miny
                foreach (UINode n in added)
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
                foreach(UINode n in added)
                {
                    double dx = n.Node.ViewOriginX - minX;
                    double dy = n.Node.ViewOriginY - minY;

                    //also set node connections as needed
                    string json = null;
                    if (jsonContent.TryGetValue(n.Node.Id, out json))
                    {
                        Node.NodeData nd = JsonConvert.DeserializeObject<Node.NodeData>(json);
                        n.Node.SetConnections(realLookup, nd.outputs);
                    }

                    n.OffsetTo(mp.X + dx, mp.Y + dy);
                }

                Task.Delay(250).ContinueWith((Task t) =>
                {
                    App.Current.Dispatcher.Invoke(() =>
                    {
                        foreach (UINode n in added)
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
                Console.WriteLine(ex.StackTrace);
            }
        }

        public void CopyResources(string CWD)
        {
            Original.CopyResources(CWD);
        }

        public void LoadGraph(string path)
        {
            string directory = System.IO.Path.GetDirectoryName(path);
            FilePath = path;
            string data = System.IO.File.ReadAllText(path);
            LoadGraph(data, directory);
        }

        protected void LoadGraph(Graph g)
        {
            //no need to reload if it is the same graph already
            if (g == Graph) return;
            if (g == null) return;

            Graph.OnGraphUpdated -= Graph_OnGraphUpdated;

            ClearView();

            Graph = g;

            Scale = Graph.Zoom;
            XShift = Graph.ShiftX;
            YShift = Graph.ShiftY;

            ZoomLevel.Text = String.Format("{0:0}", Scale * 100);

            Graph.OnGraphUpdated += Graph_OnGraphUpdated;

            ReadOnly = Graph.ReadOnly;
            Graph.ReadOnly = false;
            LoadGraphUI();
        }

        private void Graph_OnGraphUpdated(Graph g)
        {
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
            else if(type == GraphStackType.FX)
            {

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
            }

            //no need to reload if it is the same graph already
            if (graph == Graph) return;
            if (graph == null) return;

            Graph.OnGraphUpdated -= Graph_OnGraphUpdated;

            ClearView();

            Graph = graph;

            Scale = Graph.Zoom;

            if(Scale <= 0)
            {
                Scale = 1;
            }

            XShift = Graph.ShiftX;
            YShift = Graph.ShiftY;

            ZoomLevel.Text = String.Format("{0:0}", Scale * 100);

            Graph.OnGraphUpdated += Graph_OnGraphUpdated;

            ReadOnly = Graph.ReadOnly;
            Graph.ReadOnly = false;
            LoadGraphUI();
        }

        protected void RestoreStack()
        {
            if(StoredGraphStack != null)
            {
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
                                graph = (n as PixelProcessorNode).Function;
                                item.graph = graph;

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
                                    item.graph = v.Value as Graph;

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
                        else if(item.type == GraphStackType.FX)
                        {
                            //do same as pixel basically
                        }
                        else if(item.type == GraphStackType.CustomFunction)
                        {
                            FunctionGraph fn = graph.CustomFunctions.Find(m => m.Name.Equals(item.id));

                            if(fn != null)
                            {
                                graph = item.graph = fn;

                                if(!GraphStack.Contains(item))
                                {
                                    GraphStack.Push(item);

                                    if(!Crumbs.Contains(item.id))
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

        public void LoadGraph(string data, string CWD, bool readOnly = false)
        {
            Release();

            Graph = new ImageGraph("Untitled");

            if (string.IsNullOrEmpty(data))
            {   
                return;
            }

            Original = Graph;

            Graph.CWD = CWD;
            Graph.FromJson(data);
            HdriManager.Selected = Graph.HdriIndex;

            Graph.OnGraphUpdated += Graph_OnGraphUpdated;

            Scale = Graph.Zoom;
            XShift = Graph.ShiftX;
            YShift = Graph.ShiftY;

            ZoomLevel.Text = String.Format("{0:0}", Scale * 100);

            LoadGraphUI();
            ReadOnly = readOnly;

            Crumbs.Clear();

            BreadCrumb cb = new BreadCrumb(Crumbs, "Root", this, null);
        }

        public string GetGraphData()
        {
            if(!string.IsNullOrEmpty(StoredGraph))
            {
                return StoredGraph;
            }

            return Original.GetJson();
        }

        protected void LoadGraphUI()
        {
            if(Graph is FunctionGraph)
            {
                FunctionGraph fg = (FunctionGraph)Graph;

                OutputRequirementsLabel.Text = "Required Output Node Type: " + fg.ExpectedOutput.ToString();
            }
            else
            {
                OutputRequirementsLabel.Text = "";
            }

            foreach (Node n in Graph.Nodes)
            {
                UINode unode = new UINode(n, this, n.ViewOriginX, n.ViewOriginY, XShift, YShift, Scale);

                unode.HorizontalAlignment = HorizontalAlignment.Left;
                unode.VerticalAlignment = VerticalAlignment.Top;
                ViewPort.Children.Add(unode);
                GraphNodes.Add(unode);

                lookup[n.Id] = unode;
            }

            Task.Delay(250).ContinueWith((Task t) =>
            {
                App.Current.Dispatcher.Invoke(() =>
                {
                    //foreach uinode connect up
                    foreach (UINode n in GraphNodes)
                    {
                        n.LoadConnections(lookup);
                    }

                    Graph.ReadOnly = ReadOnly;
                });
            });
        }

        public void Save(string f)
        {
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

            Original.Name = name;

            Original.CopyResources(cwd);

            System.IO.File.WriteAllText(f, GetGraphData());

            Modified = false;
        }

        public void Save()
        {
            if (string.IsNullOrEmpty(FilePath)) return;

            System.IO.File.WriteAllText(FilePath, GetGraphData());

            Modified = false;
        }

        public void SaveAs(string f)
        {
            if (Original == null) return;

            string cwd = System.IO.Path.GetDirectoryName(f);
            string name = System.IO.Path.GetFileNameWithoutExtension(f);

            Original.Name = name;

            FilePath = f;

            Original.CopyResources(cwd);

            System.IO.File.WriteAllText(f, GetGraphData());

            Modified = false;
        }

        public void ClearView()
        {
            //reset viewport etc
            XShift = 0;
            YShift = 0;
            Scale = 1;

            foreach(UINode n in GraphNodes)
            {
                ViewPort.Children.Remove(n);
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

            foreach (UINode n in GraphNodes)
            {
                ViewPort.Children.Remove(n);
            }

            lookup.Clear();
            GraphNodes.Clear();

            if (Graph != null)
            {
                Graph.OnGraphUpdated -= Graph_OnGraphUpdated;

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


        public UINode GetNode(string id)
        {
            UINode n = null;

            lookup.TryGetValue(id, out n);

            return n;
        }

        /// <summary>
        /// Helper for Undo Redo system. Does not trigger new undo added to stack
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Tuple<string, Point, List<Tuple<string, List<NodeOutputConnection>>>> RemoveNode(string id)
        {
            var n = GraphNodes.Find(m => m.Id.Equals(id));

            Tuple<string, Point, List<Tuple<string, List<NodeOutputConnection>>>> result = null;

            if(n != null)
            {
                result = new Tuple<string, Point, List<Tuple<string, List<NodeOutputConnection>>>>(n.Node.GetJson(), n.Origin, n.Node.GetParentsConnections());

                //to remove connections but not to remove from graph
                n.DisposeNoRemove();

                GraphNodes.Remove(n);
                ViewPort.Children.Remove(n);
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
        public UINode AddNodeFromJson(string json, Point p)
        {
            try
            {
                Node.NodeData nd = JsonConvert.DeserializeObject<Node.NodeData>(json);

                if (nd == null) return null;

                Node n = null;
                UINode unode = null;

                n = Graph.CreateNode(nd.type);

                if(n != null)
                {
                    n.Id = nd.id;

                    n.Width = nd.width;
                    n.Height = nd.height;

                    n.FromJson(Graph.NodeLookup, json);

                    Graph.Add(n);

                    n.SetConnections(Graph.NodeLookup, nd.outputs);

                    unode = new UINode(n, this, p.X, p.Y, XShift, YShift, Scale);
                    unode.HorizontalAlignment = HorizontalAlignment.Left;
                    unode.VerticalAlignment = VerticalAlignment.Top;
                    lookup[n.Id] = unode;
                    ViewPort.Children.Add(unode);
                    GraphNodes.Add(unode);

                    Modified = true;

                    Task.Delay(250).ContinueWith(t =>
                    {
                        unode.LoadConnections(lookup);
                    });
                }

                return unode;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
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
        protected UINode AddNodeFromJson(string json, Dictionary<string, Node> realLookup)
        {
            try
            {
                Node.NodeData nd = JsonConvert.DeserializeObject<Node.NodeData>(json);

                if (nd == null) return null;

                Node n = null;
                UINode unode = null;

                n = Graph.CreateNode(nd.type);

                if (n != null)
                {
                    realLookup[nd.id] = n;
                    n.FromJson(realLookup, json);

                    Graph.Add(n);

                    unode = new UINode(n, this, 0, 0, XShift, YShift, Scale);
                    unode.HorizontalAlignment = HorizontalAlignment.Left;
                    unode.VerticalAlignment = VerticalAlignment.Top;
                    lookup[n.Id] = unode;
                    ViewPort.Children.Add(unode);
                    GraphNodes.Add(unode);

                    UndoRedoManager.AddUndo(new UndoCreateNode(Id, unode.Id, this));

                    Modified = true;
                }

                return unode;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
                return null;
            }
        }

        protected void AddNode(string type, Point p)
        {
            Node n = null;
            UINode unode = null;

            n = Graph.CreateNode(type);

            if (n != null)
            {
                if (n is GraphInstanceNode)
                {
                    GraphInstanceNode gn = (GraphInstanceNode)n;
                    gn.Load(type);
                }

                Modified = true;

                n.ViewOriginX = p.X;
                n.ViewOriginY = p.Y;

                Graph.Add(n);
                unode = new UINode(n, this, p.X, p.Y, XShift, YShift, Scale);
                unode.HorizontalAlignment = HorizontalAlignment.Left;
                unode.VerticalAlignment = VerticalAlignment.Top;
                lookup[n.Id] = unode;
                ViewPort.Children.Add(unode);
                GraphNodes.Add(unode);

                UndoRedoManager.AddUndo(new UndoCreateNode(Id, unode.Id, this));
            }
        }

        public void RemoveNode(UINode n)
        {
            string json = n.Node.GetJson();
            Point p = n.Origin;

            UndoRedoManager.AddUndo(new UndoDeleteNode(Id, json, p, n.Node.GetParentsConnections(), this));

            GraphNodes.Remove(n);
            ViewPort.Children.Remove(n);
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
                    UINodePoint origin = UINodePoint.SelectOrigin;
                    UINodePoint dest = UINodePoint.SelectOver;

                    Point r1 = new Point();

                    if (origin.Output != null)
                    {
                        r1 = origin.TransformToAncestor(ViewPort).Transform(new Point(origin.ActualWidth, 8f));
                    }
                    else if (origin.Input != null)
                    {
                        r1 = origin.TransformToAncestor(ViewPort).Transform(new Point(0f, 8f));
                    }

                    Point r2 = Mouse.GetPosition(ViewPort);

                    if (dest != null)
                    {
                        if (origin.Output != null)
                        {
                            r2 = dest.TransformToAncestor(ViewPort).Transform(new Point(dest.ActualWidth, 8f));
                        }
                        else if (origin.Input != null)
                        {
                            r2 = dest.TransformToAncestor(ViewPort).Transform(new Point(0f, 8f));
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
                        r2.X -= 8;
                        r2.Y -= 8;
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

                //we don't want to start selection
                //if we are already over a node
                foreach(UINode n in GraphNodes)
                {
                    if(n.ContainsPoint(p))
                    {
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
                    ViewPort.Children.Add(ConnectionPointPreview);
                }

                if(ConnectionPathPreview.Parent == null)
                {
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

                foreach (UINode n in GraphNodes)
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
            foreach(UINode n in SelectedNodes)
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

            foreach (UINode n in GraphNodes)
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

            //find average position
            for (int i = 0; i < SelectedNodes.Count; i++)
            {
                midX += SelectedNodes[i].Origin.X;
            }

            midX /= SelectedNodes.Count;

            for (int i = 0; i < SelectedNodes.Count; i++)
            {
                Point p = SelectedNodes[i].Origin;
                SelectedNodes[i].OffsetTo(midX, p.Y);
            }
        }

        void AlignSelectedNodesHorizontal()
        {
            double midY = 0;

            if (SelectedNodes.Count <= 1) return;

            //find average position
            for(int i = 0; i < SelectedNodes.Count; i++)
            {
                midY += SelectedNodes[i].Origin.Y;
            }

            midY /= SelectedNodes.Count;

            for(int i = 0; i < SelectedNodes.Count; i++)
            {
                Point p = SelectedNodes[i].Origin;
                SelectedNodes[i].OffsetTo(p.X, midY);
            }
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

            foreach(UINode n in GraphNodes)
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

            foreach(UINode n in GraphNodes)
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

            foreach(UINode n in GraphNodes)
            {
                if(n.IsInRect(r))
                {
                    if(SelectedNodes.Contains(n))
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

            if(count == 0)
            {
                ClearMultiSelect();
            }
        }

        public void ToggleMultiSelect(UINode n)
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
        }

        public void ClearMultiSelect()
        {
            List<UINode> toRemove = SelectedNodes.ToList();

            SelectedNodes.Clear();

            foreach (UINode n in toRemove)
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

            foreach (UINode n in GraphNodes)
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

            foreach (UINode n in GraphNodes)
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
                LoadGraph(StoredGraph, StoredGraphCWD);
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
    }
}
