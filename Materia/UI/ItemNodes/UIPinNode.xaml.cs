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
using Materia.UI.Components;

namespace Materia.UI.ItemNodes
{
    /// <summary>
    /// Interaction logic for UIPinNode.xaml
    /// </summary>
    public partial class UIPinNode : UserControl, IUIGraphNode
    {
        static SolidColorBrush HighlightBrush = (SolidColorBrush)new BrushConverter().ConvertFrom("#0087e5");
        static SolidColorBrush DefaultBrush = new SolidColorBrush(Colors.Transparent);

        protected Color Color { get; set; }

        protected PinNode pinNode;

        double xShift;
        double yShift;

        double originX;
        double originY;

        double scale;

        Point start;

        bool mouseDown;

        public double Scale
        {
            get
            {
                return scale;
            }
        }

        public Node Node { get; set; }
        public UIGraph Graph { get; set; }
        public string Id { get; set; }
        public List<UINodePoint> InputNodes { get; set; }
        public List<UINodePoint> OutputNodes { get; set; }

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

        public UIPinNode()
        {
            InitializeComponent();
            xShift = 0;
            yShift = 0;
            Color = Colors.White;
        }

        public UIPinNode(Node n, UIGraph graph, double ox, double oy, double xs, double xy, double sc = 1)
        {
            InitializeComponent();
            Graph = graph;
            Node = n;
            xShift = xs;
            yShift = xy;
            originX = ox;
            originY = oy;
            scale = sc;

            Id = n.Id;

            Margin = new Thickness(0);

            pinNode = n as PinNode;

            Color = pinNode.GetColor();
            PinColorBrush.Color = Color;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateViewArea();
            Canvas.SetZIndex(this, 99);
        }

        private void UserControl_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ColorPicker cp = new ColorPicker(pinNode.GetSystemColor());
            cp.Owner = MainWindow.Instance;

            if(cp.ShowDialog() == true)
            {
                var nc = cp.Selected;
                pinNode.SetSystemColor(nc);
                Color = pinNode.GetColor();
                PinColorBrush.Color = Color;
            }
        }


        private void UserControl_MouseMove(object sender, MouseEventArgs e)
        {
            if (mouseDown && e.LeftButton == MouseButtonState.Pressed)
            {
                Point g = e.GetPosition(Graph);

                double dx = g.X - start.X;
                double dy = g.Y - start.Y;

                Graph.MoveMultiSelect(dx, dy);

                start = g;
            }
        }

        private void UserControl_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if(e.LeftButton == MouseButtonState.Pressed)
            {
                e.Handled = true;

                Focus();
                Keyboard.Focus(this);

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
                else if (selected && Graph.SelectedNodes.Count == 1)
                {
                    return;
                }

                Graph.ClearMultiSelect();
                Graph.ToggleMultiSelect(this);
            }
        }

        private void UserControl_MouseLeave(object sender, MouseEventArgs e)
        {
            mouseDown = false;
        }

        private void UserControl_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Released)
            {
                mouseDown = false;
                Canvas.SetZIndex(this, -1);
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
            //do nothing
        }

        public void LoadConnections(Dictionary<string, IUIGraphNode> lookup)
        {
            //do nothing
        }

        public bool ContainsPoint(Point p)
        {
            return Bounds.Contains(p);
        }

        public bool IsInRect(Rect r)
        {
            return r.IntersectsWith(Bounds);
        }

        public void UpdateScale(double sc)
        {
            scale = sc;

            UpdateViewArea();
        }

        public void MoveTo(double sx, double sy)
        {
            xShift = sx;
            yShift = sy;

            UpdateViewArea();
        }

        public void Move(double dx, double dy)
        {
            xShift += dx;
            yShift += dy;

            UpdateViewArea();
        }

        public void Offset(double dx, double dy)
        {
            originX += dx / scale;
            originY += dy / scale;

            Node.ViewOriginX = originX;
            Node.ViewOriginY = originY;

            UpdateViewArea();
        }

        public void OffsetTo(double sx, double sy)
        {
            originX = sx;
            originY = sy;

            Node.ViewOriginX = originX;
            Node.ViewOriginY = originY;

            UpdateViewArea();
        }

        public void ResetPosition()
        {
            xShift = 0;
            yShift = 0;

            UpdateViewArea();
        }

        public void HideBorder()
        {
            //do nothing
            BorderArea.BorderBrush = DefaultBrush;
        }

        public void ShowBorder()
        {
            //do nothing
            BorderArea.BorderBrush = HighlightBrush;
        }

        public void DisposeNoRemove()
        {
            
        }

        public void Dispose()
        {
            Graph.RemoveNode(this);
        }
    }
}
