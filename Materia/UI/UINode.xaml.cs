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
using Materia.UI;
using Materia.Nodes;
using Materia.Nodes.Atomic;
using Materia.Imaging;
using Materia.UI.Helpers;
using System.IO;
using NLog;

namespace Materia
{
    /// <summary>
    /// Interaction logic for UINode.xaml
    /// </summary>
    public partial class UINode : UserControl, IUIGraphNode
    {
        private static ILogger Log = LogManager.GetCurrentClassLogger();

        double xShift;
        double yShift;

        double originX;
        double originY;

        double scale;
        public double Scale
        {
            get
            {
                return scale;
            }
        }

        Point start;

        bool mouseDown;

        public Node Node { get; set; }

        public UIGraph Graph { get; set; }

        public string Id { get; set; }

        public delegate void NodeUIEvent();
        public event NodeUIEvent OnNodeUIChanged;

        public List<UINodePoint> InputNodes { get; set; }
        public List<UINodePoint> OutputNodes { get; set; }

        public const double defaultHeight = 50;
        public const double defaultWidth = 120;

        public Rect UnscaledBounds
        {
            get
            {
                return new Rect(originX, originY, ActualWidth, ActualHeight);
            }
        }

        public Rect Bounds
        {
            get
            {

                double w2 = Graph.ViewPort.ActualWidth * 0.5;
                double h2 = Graph.ViewPort.ActualHeight * 0.5;

                double x = (originX - w2) * scale + w2;
                double y = (originY - h2) * scale + h2;

                double xpos = Math.Round(x / Graph.ScaledGridSnap) * Graph.ScaledGridSnap + xShift;
                double ypos = Math.Round(y / Graph.ScaledGridSnap) * Graph.ScaledGridSnap + yShift;

                Rect t = new Rect(xpos, ypos, ActualWidth * scale, ActualHeight * scale);

                return t;
            }
        }

        public Point Origin
        {
            get
            {
                return new Point(originX, originY);
            }
        }

        public UINode()
        {
            InitializeComponent();
            xShift = 0;
            yShift = 0;
            Focusable = true;
        }

        public UINode(Node n, UIGraph graph, double ox, double oy, double xs, double xy, double sc = 1)
        {
            InitializeComponent();

            Focusable = true;

            InputNodes = new List<UINodePoint>();
            OutputNodes = new List<UINodePoint>();

            Graph = graph;

            xShift = xs;
            yShift = xy;

            scale = sc;

            Node = n;

            Id = n.Id;

            originX = ox;
            originY = oy;

            NodeName.Text = n.Name;

            for(int i = 0; i < n.Outputs.Count; ++i)
            {
                NodeOutput op = n.Outputs[i];
                UINodePoint outpoint = new UINodePoint(this, graph);
                outpoint.Output = op;
                outpoint.VerticalAlignment = VerticalAlignment.Center;
                OutputNodes.Add(outpoint);
                OutputStack.Children.Add(outpoint);
            }

            for(int i = 0; i < n.Inputs.Count; ++i)
            {
                NodeInput inp = n.Inputs[i];
                UINodePoint inputpoint = new UINodePoint(this, graph);
                inputpoint.Input = inp;
                inputpoint.VerticalAlignment = VerticalAlignment.Center;
                InputStack.Children.Add(inputpoint);
                InputNodes.Add(inputpoint);
            }

            n.OnInputAddedToNode += N_OnInputAddedToNode;
            n.OnInputRemovedFromNode += N_OnInputRemovedFromNode;
            n.OnOutputAddedToNode += N_OnOutputAddedToNode;
            n.OnOutputRemovedFromNode += N_OnOutputRemovedFromNode;
            n.OnTextureChanged += N_OnTextureChanged;
            n.OnValueUpdated += N_OnValueUpdated;

            if(n is MathNode)
            {
                Desc.Visibility = Visibility.Visible;
                Desc.Text = n.GetDescription();

                if(string.IsNullOrEmpty(Desc.Text))
                {
                    Desc.Visibility = Visibility.Collapsed;
                }
            }
            else
            {
                Desc.Visibility = Visibility.Collapsed;
            }

            if(graph.Graph is FunctionGraph)
            {
                FunctionGraph f = graph.Graph as FunctionGraph;
                f.OnOutputSet += F_OnOutputSet;

                if(n == f.OutputNode)
                {
                    OutputIcon.Visibility = Visibility.Visible;
                }
                else
                {
                    OutputIcon.Visibility = Visibility.Collapsed;
                }

                if(n == f.Execute)
                {
                    InputIcon.Visibility = Visibility.Visible;
                }
                else
                {
                    InputIcon.Visibility = Visibility.Collapsed;
                }
            }
            else
            {
                if(n is OutputNode)
                {
                    OutputIcon.Visibility = Visibility.Visible;
                }
                else
                {
                    OutputIcon.Visibility = Visibility.Collapsed;
                }

                if(n is InputNode)
                {
                    InputIcon.Visibility = Visibility.Visible;
                }
                else
                {
                    InputIcon.Visibility = Visibility.Collapsed;
                }
            }
        }

        private void N_OnValueUpdated(Node n)
        {
            if (n is MathNode)
            {
                Desc.Text = n.GetDescription();

                if (string.IsNullOrEmpty(Desc.Text))
                {
                    Desc.Visibility = Visibility.Collapsed;
                }
                else
                {
                    Desc.Visibility = Visibility.Visible;
                }
            }
        }

        private void F_OnOutputSet(Node n)
        {
            if(n == Node)
            {
                OutputIcon.Visibility = Visibility.Visible;
            }
            else
            {
                OutputIcon.Visibility = Visibility.Collapsed;
            }
        }

        private void N_OnOutputRemovedFromNode(Node n, NodeOutput inp, NodeOutput previous = null)
        {
            var uinp = OutputNodes.Find(m => m.Output == inp);

            if (uinp != null)
            {
                //whoops forgot to dispose
                //on the uinodepoint to remove previous connects
                //etc
                uinp.Dispose();
                OutputStack.Children.Remove(uinp);
                OutputNodes.Remove(uinp);
            }
        }

        private void N_OnOutputAddedToNode(Node n, NodeOutput inp, NodeOutput previous = null)
        {
            UINodePoint outpoint = new UINodePoint(this, Graph);
            outpoint.Output = inp;
            outpoint.VerticalAlignment = VerticalAlignment.Center;
            OutputNodes.Add(outpoint);
            OutputStack.Children.Add(outpoint);
            outpoint.UpdateColor();

            if(previous != null)
            {
                foreach(var cinp in inp.To)
                {
                    LoadConnection(cinp.Node.Id);
                }
            }
        }

        private void N_OnInputRemovedFromNode(Node n, NodeInput inp, NodeInput previous = null)
        {
            var uinp = InputNodes.Find(m => m.Input == inp);

            if (uinp != null)
            {
                //whoops forgot to dispose
                //on the uinodepoint to remove previous connects
                //etc
                uinp.Dispose();
                InputStack.Children.Remove(uinp);
                InputNodes.Remove(uinp);
            }
        }

        private void N_OnInputAddedToNode(Node n, NodeInput inp, NodeInput previous = null)
        {
            //need to take into account previous
            //aka we are just replacing the previous one
            UINodePoint previousNodePoint = null;
            UINodePoint previousNodePointParent = null;

            if (previous != null)
            {
                previousNodePoint = InputNodes.Find(m => m.Input == previous);
            }

            if(previousNodePoint != null)
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
            }
        }

        void UpdateViewArea()
        {
            Rect t = Bounds;
            TransformGroup g = new TransformGroup();
            g.Children.Add(new ScaleTransform(scale, scale));
            g.Children.Add(new TranslateTransform(t.Left, t.Top));

            RenderTransform = g;
        }

        public void LoadConnection(string id)
        {
            var connections = Node.GetConnections();

            foreach(var con in connections)
            {
                if(con.node.Equals(id))
                {
                    string nid = con.node;
                    int idx = con.index;
                    int odx = con.outIndex;

                    IUIGraphNode on = Graph.GetNode(nid);

                    if (on != null)
                    {
                        if (idx >= 0 && idx < on.InputNodes.Count)
                        {
                            UINodePoint p = on.InputNodes[idx];
                            //connect

                            if (odx >= 0 && odx < OutputNodes.Count)
                            {
                                OutputNodes[odx].ConnectToNode(p, true);
                            }
                        }
                        else
                        {
                            //log error
                            Log.Warn("Failed to connect a node's output to input");
                        }
                    }
                    else
                    {
                        //log error
                        Log.Warn("Node does not exist for connection");
                    }

                    break;
                }
            }
        }

        public void LoadConnections(Dictionary<string, IUIGraphNode> lookup)
        {
            var connections = Node.GetConnections();

            foreach(var con in connections)
            {
                string nid = con.node;
                int idx = con.index;
                int odx = con.outIndex;

                IUIGraphNode on = null;

                if (lookup.TryGetValue(nid, out on))
                {
                    if (idx >= 0 && idx < on.InputNodes.Count)
                    {
                        UINodePoint p = on.InputNodes[idx];
                        //connect

                        if (odx >= 0 && odx < OutputNodes.Count)
                        {
                            OutputNodes[odx].ConnectToNode(p, true);
                        }
                    }
                    else
                    {
                        //log error
                        Log.Warn("Failed to connect a node's output to input");
                    }
                }
                else
                {
                    //log error
                    Log.Warn("Node does not exist for connection");
                }
            }
        }

        private void N_OnTextureChanged(Node n)
        {
            try
            {
                if (!IsLoaded) return;
                if (PreviewWrapper.Visibility == Visibility.Collapsed) return;
                if (PreviewWrapper.ActualHeight == 0 || PreviewWrapper.ActualWidth == 0) return;
                if (!n.CanPreview) return;

                NodeName.Text = n.Name;

                int pw = (int)Math.Max(PreviewWrapper.ActualWidth, 100);
                int ph = (int)Math.Max(PreviewWrapper.ActualHeight, 100);

                //we transform the preview size based
                //based on actual size
                if(n.Width > n.Height)
                {
                    ph = (int)Math.Min(PreviewWrapper.ActualHeight, (ph * ((float)n.Height / (float)n.Width)));
                }
                else if(n.Height > n.Width)
                {
                    pw = (int)Math.Min(PreviewWrapper.ActualWidth, (pw * ((float)n.Width / (float)n.Height)));
                }

                ph = Math.Max(ph, 100);
                pw = Math.Max(pw, 100);

                //this is pretty expensive
                byte[] small = n.GetPreview(pw, ph);

                if (small != null && pw > 0 && ph > 0 && small.Length > 0) {
                    Preview.Source = BitmapSource.Create(pw, ph, 72, 72, PixelFormats.Bgra32, null, small, pw * 4);
                }
            }
            catch (Exception e) 
            {
                Log.Error(e);
            }
        }

        public bool ContainsPoint(Point p)
        {
            return Bounds.Contains(p);
        }

        public bool IsInRect(Rect r)
        {
            if(r.IntersectsWith(Bounds))
            {
                return true;
            }

            return false;
        }

        public void UpdateScale(double sc)
        {
            scale = sc;

            UpdateViewArea();

            if(OnNodeUIChanged != null)
            {
                OnNodeUIChanged.Invoke();
            }
        }

        public void MoveTo(double sx, double sy)
        {
            xShift = sx;
            yShift = sy;

            UpdateViewArea();

            if (OnNodeUIChanged != null)
            {
                OnNodeUIChanged.Invoke();
            }
        }

        public void Move(double deltax, double deltay)
        {
            xShift += deltax;
            yShift += deltay;

            UpdateViewArea();

            if (OnNodeUIChanged != null)
            {
                OnNodeUIChanged.Invoke();
            }
        }

        /// <summary>
        /// deltaX and deltaY should be unscaled
        /// </summary>
        /// <param name="deltaX"></param>
        /// <param name="deltaY"></param>
        public void Offset(double deltaX, double deltaY)
        {
            originX += deltaX / scale;
            originY += deltaY / scale;

            Node.ViewOriginX = originX;
            Node.ViewOriginY = originY;

            UpdateViewArea();

            if (OnNodeUIChanged != null)
            {
                OnNodeUIChanged.Invoke();
            }
        }

        public void OffsetTo(double sx, double sy)
        {
            originX = sx;
            originY = sy;

            Node.ViewOriginX = originX;
            Node.ViewOriginY = originY;

            UpdateViewArea();

            if (OnNodeUIChanged != null)
            {
                OnNodeUIChanged.Invoke();
            }
        }

        public void ResetPosition()
        {
            xShift = 0;
            yShift = 0;

            UpdateViewArea();

            if (OnNodeUIChanged != null)
            {
                OnNodeUIChanged.Invoke();
            }
        }

        public void HideBorder()
        {
            BorderGrid.Background = (SolidColorBrush)Application.Current.Resources["Solid16"];

            foreach(UINodePoint n in InputStack.Children)
            {
                n.ShowName = false;
            }

            foreach(UINodePoint n in OutputStack.Children)
            {
                n.ShowName = false;
            }

            foreach(UINodePoint p in OutputNodes)
            {
                p.UpdateSelected(false);
            }
        }

        public void ShowBorder()
        {
            BorderGrid.Background = (SolidColorBrush)Application.Current.Resources["Solid20"];

            Focus();

            foreach (UINodePoint n in InputStack.Children)
            {
                n.ShowName = true;
            }

            foreach (UINodePoint n in OutputStack.Children)
            {
                n.ShowName = true;
            }

            foreach (UINodePoint p in OutputNodes)
            {
                p.UpdateSelected(true);
            }
        }


        private void Preview_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Focus();
            Keyboard.Focus(this);

            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if(e.ClickCount > 1)
                {
                    if (Node.CanPreview)
                    {
                        if (UIPreviewPane.Instance != null)
                        {
                            UIPreviewPane.Instance.SetPreviewNode(this);
                        }
                    }
                    return;
                }

                if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
                {
                    Graph.ToggleMultiSelect(this);
                    return;
                }

                mouseDown = true;

                start = e.GetPosition(Graph);

                bool selected = Graph.SelectedNodes.Contains(this);

                if (selected && Graph.SelectedNodes.Count > 1)
                {
                    return;
                }
                else if(selected && Graph.SelectedNodes.Count == 1)
                {
                    return;
                }

                if (UINodeParameters.Instance != null)
                {
                    UINodeParameters.Instance.SetActive(Node);
                }

                Graph.ClearMultiSelect();
                Graph.ToggleMultiSelect(this);
                Canvas.SetZIndex(this, 1);
            }
            else if(e.RightButton == MouseButtonState.Pressed)
            {
                if (Node is PixelProcessorNode)
                {
                    ContextMenu ctx = (ContextMenu)Resources["PixelProcContext"];
                    ContextMenu = ctx;
                    ContextMenu.PlacementTarget = this;
                    ContextMenu.IsOpen = true;
                }
                else if (Node is GraphInstanceNode)
                {
                    ContextMenu ctx = (ContextMenu)Resources["ImageInstContext"];
                    ContextMenu = ctx;
                    ContextMenu.PlacementTarget = this;
                    ContextMenu.IsOpen = true;
                }
                else if(Node is ImageNode)
                {
                    ContextMenu ctx = (ContextMenu)Resources["ImageContext"];
                    ContextMenu = ctx;
                    ContextMenu.PlacementTarget = this;
                    ContextMenu.IsOpen = true;
                }
                else if(Node is MathNode)
                {
                    ContextMenu ctx = (ContextMenu)Resources["MathContext"];
                    ContextMenu = ctx;
                    ContextMenu.PlacementTarget = this;
                    ContextMenu.IsOpen = true;
                }
            }
        }

        private void Preview_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && mouseDown)
            {
                Point g = e.GetPosition(Graph);

                double dx = g.X - start.X;
                double dy = g.Y - start.Y;

                Graph.MoveMultiSelect(dx, dy);

                start = g;
            }
        }

        public void DisposeNoRemove()
        {
            Node.OnInputAddedToNode -= N_OnInputAddedToNode;
            Node.OnInputRemovedFromNode -= N_OnInputRemovedFromNode;
            Node.OnOutputAddedToNode -= N_OnOutputAddedToNode;
            Node.OnOutputRemovedFromNode -= N_OnOutputRemovedFromNode;
            Node.OnTextureChanged -= N_OnTextureChanged;

            if (Graph.Graph is FunctionGraph)
            {
                FunctionGraph f = Graph.Graph as FunctionGraph;
                f.OnOutputSet -= F_OnOutputSet;
            }

            foreach (UINodePoint p in OutputStack.Children)
            {
                p.DisposeNoRemove();
            }

            foreach (UINodePoint p in InputStack.Children)
            {
                p.DisposeNoRemove();
            }

            /*if (UI3DPreview.Instance != null)
            {
                UI3DPreview.Instance.TryAndRemovePreviewNode(this);
            }

            if (UIPreviewPane.Instance != null)
            {
                UIPreviewPane.Instance.TryAndRemovePreviewNode(this);
            }*/
        }

        public void Dispose()
        {
            Node.OnInputAddedToNode -= N_OnInputAddedToNode;
            Node.OnInputRemovedFromNode -= N_OnInputRemovedFromNode;
            Node.OnOutputAddedToNode -= N_OnOutputAddedToNode;
            Node.OnOutputRemovedFromNode -= N_OnOutputRemovedFromNode;
            Node.OnTextureChanged -= N_OnTextureChanged;

            if (Graph.Graph is FunctionGraph)
            {
                FunctionGraph f = Graph.Graph as FunctionGraph;
                f.OnOutputSet -= F_OnOutputSet;
            }

            //register remove node first
            //for undo
            Graph.RemoveNode(this);

            foreach (UINodePoint p in OutputStack.Children)
            {
                p.Dispose();
            }

            foreach(UINodePoint p in InputStack.Children)
            {
                p.Dispose();
            }

            if(UI3DPreview.Instance != null)
            {
                UI3DPreview.Instance.TryAndRemovePreviewNode(this);
            }

            if(UIPreviewPane.Instance != null)
            {
                UIPreviewPane.Instance.TryAndRemovePreviewNode(this);
            }
        }

        private void Grid_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if(e.LeftButton == MouseButtonState.Released)
            {
                mouseDown = false;
                Canvas.SetZIndex(this, 0);
            }
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            var prev = UI3DPreview.Instance;
            MenuItem item = sender as MenuItem;

            if(item.Header.ToString().ToLower().Contains("base color"))
            {
                if (prev != null)
                {
                    prev.SetAlbedoNode(this);
                }
            }
            else if(item.Header.ToString().ToLower().Contains("height"))
            {
                if(prev != null)
                {
                    prev.SetHeightNode(this);
                }
            }
            else if(item.Header.ToString().ToLower().Contains("normal"))
            {
                if(prev != null)
                {
                    prev.SetNormalNode(this);
                }
            }
            else if (item.Header.ToString().ToLower().Contains("metallic"))
            {
                if (prev != null)
                {
                    prev.SetMetallicNode(this);
                }
            }
            else if (item.Header.ToString().ToLower().Contains("roughness"))
            {
                if (prev != null)
                {
                    prev.SetRoughnessNode(this);
                }
            }
            else if (item.Header.ToString().ToLower().Contains("occlusion"))
            {
                if (prev != null)
                {
                    prev.SetOcclusionNode(this);
                }
            }
            else if(item.Header.ToString().ToLower().Contains("thickness"))
            {
                if(prev != null)
                {
                    prev.SetThicknessNode(this);
                }
            }
            else if(item.Header.ToString().ToLower().Contains("emission"))
            {
                if(prev != null)
                {
                    prev.SetEmissionNode(this);
                }
            }
            else if(item.Header.ToString().ToLower().Contains("edit"))
            {
                if(Node is PixelProcessorNode && Graph.Graph is ImageGraph)
                {
                    Graph.Push(Node);

                    if(UIPreviewPane.Instance != null)
                    {
                        UIPreviewPane.Instance.SetPreviewNode(this);
                    }
                }
            }
            else if(item.Header.ToString().ToLower().Contains("set as out"))
            {
                if (Node is MathNode && Graph.Graph is FunctionGraph)
                {
                    FunctionGraph g = Graph.Graph as FunctionGraph;
                    g.SetOutputNode(Node.Id);
                }
            }
            else if(item.Header.ToString().ToLower().Contains("export"))
            {
                ExportAsPng();
            }
            else if(item.Header.ToString().ToLower().Contains("refresh"))
            {
                PreviewWrapper.Visibility = Visibility.Visible;
                //we delay to make sure the previewwrapper is fully
                //loaded and has its width and height set
                Task.Delay(25).ContinueWith(t =>
                {
                    N_OnTextureChanged(Node);
                }, TaskScheduler.FromCurrentSynchronizationContext());
            }
            else if(item.Header.ToString().ToLower().Contains("reload inst"))
            {
                if (Node is GraphInstanceNode)
                {
                    (Node as GraphInstanceNode).Reload();
                    Graph?.Graph?.TryAndProcess();
                }
            }
        }

        private void ExportAsPng()
        {
            var dialog = new Microsoft.Win32.SaveFileDialog();
            dialog.Filter = "PNG|*.png";
            dialog.CheckFileExists = false;
            dialog.CheckPathExists = true;

            if(dialog.ShowDialog() == true)
            {
                string path = dialog.FileName;
                byte[] bits = Node.GetPreview(Node.Width, Node.Height);

                Task.Run(() =>
                {
                    try
                    {
                        RawBitmap bmp = null;
                        if (bits != null)
                        {
                            bmp = new RawBitmap(Node.Width, Node.Height, bits);
                            var src = bmp.ToImageSource();
                            PngBitmapEncoder encoder = new PngBitmapEncoder();
                            BitmapFrame frame = BitmapFrame.Create(src);
                            encoder.Frames.Add(frame);

                            using (FileStream fs = new FileStream(path, FileMode.OpenOrCreate))
                            {
                                encoder.Save(fs);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Log.Error(e);
                    }
                });
            }
        }

        private void UserControl_MouseLeave(object sender, MouseEventArgs e)
        {
            mouseDown = false;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Width = defaultWidth;
            UpdateViewArea();
        }
    }
}
