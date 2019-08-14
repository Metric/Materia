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
using NLog;

namespace Materia
{
    /// <summary>
    /// Interaction logic for UINodePoint.xaml
    /// </summary>
    public partial class UINodePoint : UserControl, IDisposable
    {
        private static ILogger Log = LogManager.GetCurrentClassLogger();

        private static SolidColorBrush RedColor = new SolidColorBrush(Colors.Red);
        private static SolidColorBrush WhiteColor = new SolidColorBrush(Colors.White);

        private Dictionary<UINodePoint, NodePath> paths;

        public static UINodePoint SelectOrigin
        {
            get; set;
        }

        public static UINodePoint SelectOver
        {
            get; protected set;
        }


        private List<UINodePoint> to;
        public List<UINodePoint> To
        {
            get
            {
                return to;
            }
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

        public IEnumerable<NodePath> Paths
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

        public Brush ColorBrush
        {
            get
            {
                return node.Background;
            }
        }

        UIGraph graph;

        bool loaded = false;

        //a node point should only have one of these set
        //not both
        //as a node point can either be an input or an output
        public NodeInput Input { get; set; }

        protected NodeOutput output;
        public NodeOutput Output
        {
            get
            {
                return output;
            }
            set
            {
                if(output != null)
                {
                    output.OnTypeChanged -= Output_OnTypeChanged;
                }

                output = value;

                output.OnTypeChanged += Output_OnTypeChanged;

                if (loaded)
                {
                    UpdateColor();
                }
            }
        }

        private void Output_OnTypeChanged(NodeOutput inp)
        {
            UpdateColor();
        }

        public UINodePoint()
        {
            InitializeComponent();
            loaded = false;
            Name = "NodePoint" + nodeCount++;
            paths = new Dictionary<UINodePoint, NodePath>();
            to = new List<UINodePoint>();
        }

        public UINodePoint(UINode n, UIGraph pc)
        {
            InitializeComponent();
            loaded = false;
            graph = pc;
            Name = "NodePoint" + nodeCount++;
            paths = new Dictionary<UINodePoint, NodePath>();
            to = new List<UINodePoint>();
            Node = n;
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
                else if((Input.Type & NodeType.Float2) != 0
                    && (Input.Type & NodeType.Float3) != 0 && (Input.Type & NodeType.Float4) != 0)
                {
                    node.Background = (SolidColorBrush)Resources["AnyFloatInputOutput"];
                }
                else if((Input.Type & NodeType.Float2) != 0 && (Input.Type & NodeType.Float) != 0)
                {
                    node.Background = (SolidColorBrush)Resources["AnyFloatInputOutput"];
                }
                else if(Input.Type == NodeType.Execute)
                {
                    node.Background = RedColor;
                }
                else if(Resources.Contains(Input.Type.ToString() + "InputOutput"))
                {
                    node.Background = (SolidColorBrush)Resources[Input.Type.ToString() + "InputOutput"];
                }
                else
                {
                    node.Background = WhiteColor;
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
                else if((Output.Type & NodeType.Float2) != 0
                    && (Output.Type & NodeType.Float3) != 0 && (Output.Type & NodeType.Float4) != 0)
                {
                    //need to update this color
                    node.Background = (SolidColorBrush)Resources["AnyFloatInputOutput"];
                }
                else if((Output.Type & NodeType.Float2) != 0 && (Output.Type & NodeType.Float) != 0)
                {
                    //really need to update this color
                    node.Background = (SolidColorBrush)Resources["AnyFloatInputOutput"];
                }
                else if(Output.Type == NodeType.Execute)
                {
                    node.Background = RedColor;
                }
                else if(Resources.Contains(Output.Type.ToString() + "InputOutput"))
                {
                    node.Background = (SolidColorBrush)Resources[Output.Type.ToString() + "InputOutput"];
                }
                else
                {
                    node.Background = WhiteColor;
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
            return p.Node == Node;
        }

        public void ConnectToNode(UINodePoint p, bool loading = false)
        {
            if(p.ParentNode == this)
            {
                return;
            }

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

            if (!loading)
            {
                Output.Add(p.Input);
            }

            NodePath path = new NodePath(graph.ViewPort, this, p, output != null && output.Type == NodeType.Execute);
            paths[p] = path;
            to.Add(p);
            p.ParentNode = this;
        }

        public int GetOutIndex(UINodePoint p)
        {
            return to.IndexOf(p);
        }

        public void RemoveNode(UINodePoint p, bool removeFromGraph = true)
        {
            NodePath path = null;
            if(paths.TryGetValue(p, out path))
            {
                path.Dispose();
            }

            paths.Remove(p);
            to.Remove(p);

            if (p.ParentNode == this)
            {
                if(p.Input != null && p.Input.Input != null && removeFromGraph)
                {
                    p.Input.Input.Remove(p.Input);
                }

                p.ParentNode = null;
            }
        }

        public void UpdateSelected(bool selected)
        {
            try
            {
                if (paths != null)
                {
                    foreach (NodePath p in paths.Values)
                    {
                        p.Selected = selected;
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

        public void DisposeNoRemove()
        {
            if (output != null)
            {
                output.OnTypeChanged -= Output_OnTypeChanged;

                List<UINodePoint> toRemove = new List<UINodePoint>();

                try
                {
                    foreach (UINodePoint p in paths.Keys)
                    {
                        toRemove.Add(p);
                    }

                }
                catch (Exception e)
                {
                    //it is possible to hit here
                    //in certain situations on deleting a node
                    //as the iterator is deleted
                    //while traversing it in either toRemove
                    //or path.keys
                    Log.Error(e);
                }

                try
                {
                    foreach (UINodePoint p in toRemove)
                    {
                        RemoveNode(p, false);
                    }
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }
            else
            {
                if (ParentNode != null)
                {
                    ParentNode.RemoveNode(this, false);
                }
            }
        }

        public void Dispose()
        {
            if (output != null)
            {
                output.OnTypeChanged -= Output_OnTypeChanged;

                List<UINodePoint> toRemove = new List<UINodePoint>();

                try
                {
                    foreach (UINodePoint p in paths.Keys)
                    {
                        toRemove.Add(p);
                    }

                }
                catch (Exception e)
                {
                    //it is possible to hit here
                    //in certain situations on deleting a node
                    //as the iterator is deleted
                    //while traversing it in either toRemove
                    //or path.keys
                    Log.Error(e);
                }

                try
                {
                    foreach (UINodePoint p in toRemove)
                    {
                        RemoveNode(p);
                    }
                }
                catch (Exception e)
                {
                    Log.Error(e);
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
           
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateColor();
            loaded = true;
        }
    }
}
