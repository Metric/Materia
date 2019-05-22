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
using OpenTK;
using Materia.Nodes.Atomic;
using Materia.UI.Components;
using Materia.Imaging;
using System.IO;

namespace Materia
{
    /// <summary>
    /// Interaction logic for UINode.xaml
    /// </summary>
    public partial class UINode : UserControl
    {
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

        public Node Node { get; protected set; }

        public UIGraph Graph { get; protected set; }

        public string Id { get; protected set; }

        public delegate void NodeUIEvent();
        public event NodeUIEvent OnNodeUIChanged;
        public List<UINodePoint> InputNodes { get; protected set; }
        public List<UINodePoint> OutputNodes { get; protected set; }

        int clickCount = 0;
        long lastClickTime = 0;

        public const double defaultSize = 100;

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

            Width = defaultSize;
            Height = defaultSize;

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

            Margin = new Thickness(0, 0, 0, 0);

            NodeName.Text = n.Name;

            foreach (NodeOutput op in n.Outputs)
            {
                UINodePoint outpoint = new UINodePoint(this, graph);
                outpoint.Output = op;
                outpoint.VerticalAlignment = VerticalAlignment.Center;
                OutputNodes.Add(outpoint);
                OutputStack.Children.Add(outpoint);
                outpoint.UpdateColor();
            }

            foreach(NodeInput i in n.Inputs)
            {
                UINodePoint inputpoint = new UINodePoint(this, graph);
                inputpoint.Input = i;
                inputpoint.VerticalAlignment = VerticalAlignment.Center;
                InputStack.Children.Add(inputpoint);
                InputNodes.Add(inputpoint);
                inputpoint.UpdateColor();
            }

            n.OnUpdate += N_OnUpdate;
            n.OnNameUpdate += N_OnNameUpdate;
            n.OnInputAddedToNode += N_OnInputAddedToNode;
            n.OnInputRemovedFromNode += N_OnInputRemovedFromNode;
            n.OnOutputAddedToNode += N_OnOutputAddedToNode;
            n.OnOutputRemovedFromNode += N_OnOutputRemovedFromNode;
            N_OnUpdate(n);
        }

        private void N_OnOutputRemovedFromNode(Node n, NodeOutput inp)
        {
            var uinp = OutputNodes.Find(m => m.Output == inp);

            if(uinp != null)
            {
                OutputStack.Children.Remove(uinp);
                OutputNodes.Remove(uinp);
            }

            ResizeHeight();
        }

        private void N_OnOutputAddedToNode(Node n, NodeOutput inp)
        {
            UINodePoint outpoint = new UINodePoint(this, Graph);
            outpoint.Output = inp;
            outpoint.VerticalAlignment = VerticalAlignment.Center;
            OutputNodes.Add(outpoint);
            OutputStack.Children.Add(outpoint);
            outpoint.UpdateColor();

            ResizeHeight();
        }

        private void N_OnInputRemovedFromNode(Node n, NodeInput inp)
        {
            var uinp = InputNodes.Find(m => m.Input == inp);

            if(uinp != null)
            {
                InputStack.Children.Remove(uinp);
                InputNodes.Remove(uinp);
            }

            ResizeHeight();
        }

        private void N_OnInputAddedToNode(Node n, NodeInput inp)
        {
            UINodePoint inputpoint = new UINodePoint(this, Graph);
            inputpoint.Input = inp;
            inputpoint.VerticalAlignment = VerticalAlignment.Center;
            InputStack.Children.Add(inputpoint);
            InputNodes.Add(inputpoint);
            inputpoint.UpdateColor();

            ResizeHeight();
        }

        private void N_OnNameUpdate(Node n)
        {
            NodeName.Text = n.Name;
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

                    UINode on = Graph.GetNode(nid);

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
                        }
                    }
                    else
                    {
                        //log error
                    }
                }
            }
        }

        public void LoadConnections(Dictionary<string, UINode> lookup)
        {
            var connections = Node.GetConnections();

            foreach(var con in connections)
            {
                string nid = con.node;
                int idx = con.index;
                int odx = con.outIndex;

                UINode on = null;

                if (lookup.TryGetValue(nid, out on))
                {
                    if (idx >= 0 && idx < on.InputNodes.Count)
                    {
                        UINodePoint p = on.InputNodes[idx];
                        //connect

                        if (odx >= 0 && odx < OutputNodes.Count)
                        {
                            OutputNodes[odx].ConnectToNode(p);
                        }
                    }
                    else
                    {
                        //log error
                    }
                }
                else
                {
                    //log error
                }
            }
        }

        private void N_OnUpdate(Node n)
        {
            try
            {
                NodeName.Text = n.Name;

                int pw = 100;
                int ph = 100;

                //we transform the preview size based
                //based on actual size
                if(n.Width > n.Height)
                {
                    ph = (int)Math.Min(100, (ph * ((float)n.Height / (float)n.Width)));
                }
                else if(n.Height > n.Width)
                {
                    pw = (int)Math.Min(100, (pw * ((float)n.Width / (float)n.Height)));
                }

                byte[] small = n.GetPreview(pw, ph);

                if (small != null) {
                    Preview.Source = BitmapSource.Create(pw, ph, 72, 72, PixelFormats.Bgr32, null, small, pw * 4);
                }
            }
            catch (Exception e) 
            {
                Console.WriteLine(e.StackTrace);
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
            BorderThickness = new Thickness(0, 0, 0, 0);

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
            Keyboard.ClearFocus();

            BorderBrush = new SolidColorBrush(Colors.White);
            BorderThickness = new Thickness(2, 2, 2, 2);

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

        private void ResizeHeight()
        {
            double newHeight = Math.Max(Node.Outputs.Count, Node.Inputs.Count) * 20 + 20;

            if (newHeight > defaultSize)
            {
                Height = newHeight;
            }
            else
            {
                Height = defaultSize;
            }
        }

        private void Preview_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Focus();

            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if(new TimeSpan(DateTime.Now.Ticks - lastClickTime).TotalMilliseconds > 200)
                {
                    clickCount = 0;
                }

                clickCount++;
                lastClickTime = DateTime.Now.Ticks;

                if(clickCount > 1)
                {
                    clickCount = 0;
                    if (Node.CanPreview)
                    {
                        if (UIPreviewPane.Instance != null)
                        {
                            UIPreviewPane.Instance.SetPreviewNode(this);
                        }
                    }
                    return;
                }

                if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.LeftCtrl))
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
            foreach (UINodePoint p in OutputStack.Children)
            {
                p.Dispose();
            }

            foreach (UINodePoint p in InputStack.Children)
            {
                p.Dispose();
            }

            if (UI3DPreview.Instance != null)
            {
                UI3DPreview.Instance.TryAndRemovePreviewNode(this);
            }

            if (UIPreviewPane.Instance != null)
            {
                UIPreviewPane.Instance.TryAndRemovePreviewNode(this);
            }
        }

        public void Dispose()
        {
            foreach(UINodePoint p in OutputStack.Children)
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

            Graph.RemoveNode(this);
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

            if(item.Header.ToString().ToLower().Contains("albedo"))
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
                });
            }
        }

        private void UserControl_MouseLeave(object sender, MouseEventArgs e)
        {
            mouseDown = false;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Width = defaultSize;
            Height = defaultSize;

            ResizeHeight();

            UpdateViewArea();
        }
    }
}
