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
using System.Threading;
using Materia.UI.Helpers;

namespace Materia
{
    /// <summary>
    /// Interaction logic for UINodePoint.xaml
    /// </summary>
    public partial class UINodePoint : UserControl
    {
        private List<UINodePoint> to;

        private Dictionary<UINodePoint, Path> paths;

        public static UINodePoint SelectOrigin
        {
            get; set;
        }

        public static UINodePoint SelectOver
        {
            get; protected set;
        }

        public UINode Node { get; protected set; }
        public UINodePoint ParentNode { get; set; }

        bool prevShowName;
        bool showName;
        public bool ShowName
        {
            get
            {
                return showName;
            }
            set
            {
                prevShowName = showName;
                showName = value;
                ToggleName();
            }
        }

        public IEnumerable<Path> Paths
        {
            get
            {
                return paths.Values;
            }
        }

        static int nodeCount = 0;

        public int PathCount
        {
            get
            {
                return paths.Count;
            }
        }

        UIGraph graph;

        //a node point should only have one of these set
        //not both
        //as a node point can either be an input or an output
        public NodeInput Input { get; set; }
        public NodeOutput Output { get; set; }

        public UINodePoint()
        {
            InitializeComponent();
            Name = "NodePoint" + nodeCount++;
            paths = new Dictionary<UINodePoint, Path>();
            to = new List<UINodePoint>();

            LayoutUpdated += UINodePoint_LayoutUpdated;
        }

        private void UINodePoint_LayoutUpdated(object sender, EventArgs e)
        {
            UpdatePaths();
        }

        public UINodePoint(UINode n, UIGraph pc)
        {
            InitializeComponent();
            graph = pc;
            Name = "NodePoint" + nodeCount++;
            paths = new Dictionary<UINodePoint, Path>();
            to = new List<UINodePoint>();
            Node = n;

            LayoutUpdated += UINodePoint_LayoutUpdated;
        }

        public void UpdateColor()
        {
            if(Input != null)
            {
                if((Input.Type & NodeType.Color) != 0 && (Input.Type & NodeType.Gray) != 0)
                {
                    node.Background = (LinearGradientBrush)Resources["GrayColorInputOutput"];
                }
                else if((Input.Type & NodeType.Float) != 0 && (Input.Type & NodeType.Float2) != 0
                    && (Input.Type & NodeType.Float3) != 0 && (Input.Type & NodeType.Float4) != 0)
                {
                    node.Background = (SolidColorBrush)Resources["AnyFloatInputOutput"];
                }
                else
                {
                    node.Background = (SolidColorBrush)Resources[Input.Type.ToString() + "InputOutput"];
                }
            }
            else if(Output != null)
            {
                if ((Output.Type & NodeType.Color) != 0 && (Output.Type & NodeType.Gray) != 0)
                {
                    node.Background = (LinearGradientBrush)Resources["GrayColorInputOutput"];
                }
                else if ((Output.Type & NodeType.Float) != 0 && (Output.Type & NodeType.Float2) != 0
                    && (Output.Type & NodeType.Float3) != 0 && (Output.Type & NodeType.Float4) != 0)
                {
                    node.Background = (SolidColorBrush)Resources["AnyFloatInputOutput"];
                }
                else
                {
                    node.Background = (SolidColorBrush)Resources[Output.Type.ToString() + "InputOutput"];
                }
            }
        }

        void ToggleName()
        {
            if(showName)
            {
                if(Input != null)
                {
                    InputName.Text = Input.Name;
                }
                else if(Output != null)
                {
                    OutputName.Text = Output.Name;
                }
            }
            else
            {
                InputName.Text = "";
                OutputName.Text = "";
            }
        }

        public bool CanConnect(UINodePoint p)
        {
            if (Output == null) return false;
            if (p.Input == null) return false;
            return (Output.Type & p.Input.Type) != 0;
        }

        public bool IsCircular(UINodePoint p)
        {
            if (p.Node == Node) return true;

            Stack<UINodePoint> stack = new Stack<UINodePoint>();

            stack.Push(p);

            bool circ = false;
            while(stack.Count > 0)
            {
                UINodePoint pt = stack.Pop();

                bool shouldBreak = false;
                UINode node = pt.Node;

                foreach (UINodePoint output in node.OutputNodes)
                {
                    if (output != null && output.to != null)
                    {
                        foreach (UINodePoint n in output.to)
                        {
                            if (n.Node == Node)
                            {
                                shouldBreak = true;
                                circ = true;
                                break;
                            }
                            else
                            {
                                stack.Push(n);
                            }
                        }
                    }

                    if(shouldBreak)
                    {
                        break;
                    }
                }

                if(shouldBreak)
                {
                    break;
                }
            }

            return circ;
        }

        public void ConnectToNode(UINodePoint p)
        {
            if (!CanConnect(p))
            {
                return;
            }

            if (IsCircular(p))
            {
                return;
            }

            if (p.ParentNode != null)
            {
                p.ParentNode.RemoveNode(p);
            }

            Output.Add(p.Input);

            Path path = new Path();
            path.Stroke = new SolidColorBrush(Colors.Gray);
            path.StrokeThickness = 2;
            paths[p] = path;
            to.Add(p);
            p.ParentNode = this;
            UpdatePaths();
            graph.ViewPort.Children.Add(path);
        }

        public void RemoveNode(UINodePoint p)
        {
            Path path = null;
            if(paths.TryGetValue(p, out path))
            {
                graph.ViewPort.Children.Remove(path);
            }

            paths.Remove(p);

            to.Remove(p);

            if (p.ParentNode == this)
            {
                if(p.Input != null && p.Input.Input != null)
                {
                    p.Input.Input.Remove(p.Input);
                }

                p.ParentNode = null;
            }
        }

        public void UpdatePaths()
        {
            //catch for when the node is removed and layout update is still triggered
            try
            {
                if (this.Parent == null) return;

                if(!this.HasAncestor(graph.ViewPort))
                {
                    return;
                }

                Point r1 = this.TransformToAncestor(graph.ViewPort).Transform(new Point(ActualWidth, 8f));

                foreach (UINodePoint n in to)
                {
                    if (n.Parent == null) continue;

                    if(!n.HasAncestor(graph.ViewPort))
                    {
                        continue;
                    }

                    Point r2 = n.TransformToAncestor(graph.ViewPort).Transform(new Point(0f, 8f));

                    Path path = null;

                    paths.TryGetValue(n, out path);

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
                }
            }
            catch { }
        }

        private void UserControl_MouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            if (Node.Graph.ReadOnly) return;

            if (e.LeftButton == MouseButtonState.Pressed && !Keyboard.IsKeyDown(Key.LeftAlt) && !Keyboard.IsKeyDown(Key.RightAlt))
            {
                if (SelectOrigin == this)
                {
                    SelectOrigin = null;
                }
                else if (SelectOrigin != this && SelectOrigin != null)
                {
                    if (Output != null && SelectOrigin.Output == null)
                    {
                        ConnectToNode(SelectOrigin);
                    }
                    else
                    {
                        SelectOrigin.ConnectToNode(this);
                    }

                    SelectOrigin = null;
                }
                else
                {
                    SelectOrigin = this;
                }
            }
            else if(e.LeftButton == MouseButtonState.Pressed && (Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt)))
            {
                Dispose();
            }
        }

        private void Node_MouseEnter(object sender, MouseEventArgs e)
        {
            ShowName = true;

            SelectOver = this;

            if(SelectOrigin != null)
            { 
                if(((SelectOrigin.CanConnect(this) && !SelectOrigin.IsCircular(this)) 
                    || (CanConnect(SelectOrigin) && !IsCircular(SelectOrigin))) && !Node.Graph.ReadOnly)
                {
                    Cursor = Cursors.Arrow;
                }
                else
                {
                    Cursor = Cursors.No;
                }
            }
            else
            {
                Cursor = Cursors.Arrow;
            }
        }

        public void Dispose()
        {
            if (Output != null)
            {
                List<UINodePoint> toRemove = new List<UINodePoint>();
                foreach (UINodePoint p in paths.Keys)
                {
                    toRemove.Add(p);
                }

                foreach (UINodePoint p in toRemove)
                {
                    RemoveNode(p);
                }
            }
            else
            {
                if (ParentNode != null)
                {
                    ParentNode.RemoveNode(this);
                }
            }
        }

        private void Node_MouseLeave(object sender, MouseEventArgs e)
        {
            SelectOver = null;
            ShowName = prevShowName;
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            LayoutUpdated -= UINodePoint_LayoutUpdated;
        }
    }
}
