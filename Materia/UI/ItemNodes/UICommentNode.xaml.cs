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

namespace Materia.UI.ItemNodes
{
    /// <summary>
    /// Interaction logic for UICommentNode.xaml
    /// </summary>
    public partial class UICommentNode : UserControl, IUIGraphNode
    {
        static SolidColorBrush HighlightBrush = (SolidColorBrush) new BrushConverter().ConvertFrom("#0087e5");
        static SolidColorBrush DefaultBrush = (SolidColorBrush)new BrushConverter().ConvertFrom("#22ffffff");

        const double MIN_HEIGHT = 38;
        const double MIN_WIDTH = 64;

        double xShift;
        double yShift;

        double originX;
        double originY;

        double scale;

        bool loading;

        Point start;

        bool mouseDown;

        List<IUIGraphNode> contained;

        public double Scale
        {
            get
            {
                return scale;
            }
        }

        protected CommentNode commentNode;
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

        public UICommentNode()
        {
            InitializeComponent();
            xShift = 0;
            yShift = 0;
            Focusable = true;
        }

        public UICommentNode(Node n, UIGraph graph, double ox, double oy, double xs, double xy, double sc = 1) 
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

            contained = new List<IUIGraphNode>();
            OutputNodes = new List<UINodePoint>();
            InputNodes = new List<UINodePoint>();

            commentNode = n as CommentNode;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Width = Math.Max(Node.Width, MIN_WIDTH);
            Height = Math.Max(Node.Height, MIN_HEIGHT);

            loading = true;
            CommentText.Text = commentNode.Content;

            UpdateViewArea();

            Canvas.SetZIndex(this, -1);
        }

        private void UserControl_MouseMove(object sender, MouseEventArgs e)
        {
            if(mouseDown && e.LeftButton == MouseButtonState.Pressed)
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

                if (e.ClickCount > 1)
                {
                    Holder.Visibility = Visibility.Collapsed;

                    CommentText.SelectAll();
                    Keyboard.Focus(CommentText);
                }
                else
                {
                    Focus();
                    Keyboard.Focus(this);

                    //cache contained nodes
                    //except other comment nodes
                    //if we don't filter out other comment nodes
                    //it then will create a stack overflow
                    //if two or more comment nodes are on top of each other
                    //and you try to move one of them
                    //ignore nodes that may already be selected as part of multiselect
                    contained = Graph.GraphNodes.FindAll(m => !(m is UICommentNode) && m.IsInRect(this.Bounds) && !Graph.SelectedIds.Contains(m.Id));

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
        }

        //this is used for copy pasting!
        public List<IUIGraphNode> GetContained()
        {
            return Graph.GraphNodes.FindAll(m => m != this && m.IsInRect(this.Bounds) && !Graph.SelectedIds.Contains(m.Id));
        }

        private void UserControl_MouseLeave(object sender, MouseEventArgs e)
        {
            
        }

        private void UserControl_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Released)
            {
                mouseDown = false;
                Canvas.SetZIndex(this, -1);
            }
        }

        private void Corner_DragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            Node.Width += (int)e.HorizontalChange;
            Node.Width = (int)Math.Max(MIN_WIDTH, Node.Width);
            Node.Height += (int)e.VerticalChange;
            Node.Height = (int)Math.Max(MIN_HEIGHT, Node.Height);

            Node.Height = (int)(Math.Round(Node.Height / Graph.ScaledGridSnap) * Graph.ScaledGridSnap);
            Node.Width = (int)(Math.Round(Node.Width / Graph.ScaledGridSnap) * Graph.ScaledGridSnap);

            Width = Node.Width;
            Height = Node.Height;

            UpdateViewArea();
        }

        private void Right_DragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            Node.Width += (int)e.HorizontalChange;
            Node.Width = (int)Math.Max(MIN_WIDTH, Node.Width);

            Node.Width = (int)(Math.Round(Node.Width / Graph.ScaledGridSnap) * Graph.ScaledGridSnap);

            Width = Node.Width;

            UpdateViewArea();
        }

        private void Bottom_DragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            Node.Height += (int)e.VerticalChange;
            Node.Height = (int)Math.Max(MIN_HEIGHT, Node.Height);

            Node.Height = (int)(Math.Round(Node.Height / Graph.ScaledGridSnap) * Graph.ScaledGridSnap);

            Height = Node.Height;

            UpdateViewArea();
        }

        private void Top_DragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            originY += (int)e.VerticalChange;
            Node.Height -= (int)e.VerticalChange;
            Node.Height = (int)Math.Max(MIN_HEIGHT, Node.Height);

            Node.Height = (int)(Math.Round(Node.Height / Graph.ScaledGridSnap) * Graph.ScaledGridSnap);

            Height = Node.Height;

            UpdateViewArea();
        }


        private void Left_DragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            originX += (int)e.HorizontalChange;
            Node.Width -= (int)e.HorizontalChange;
            Node.Width = (int)Math.Max(MIN_WIDTH, Node.Width);

            Node.Width = (int)(Math.Round(Node.Width / Graph.ScaledGridSnap) * Graph.ScaledGridSnap);

            Width = Node.Width;

            UpdateViewArea();
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
            //do nothing in this node
        }

        public void LoadConnections(Dictionary<string, IUIGraphNode> lookup)
        {
            //do nothing in this node
        }

        public bool ContainsPoint(Point p)
        {
            return Bounds.Contains(p);
        }

        public bool IsInRect(Rect r)
        {
            if (r.IntersectsWith(Bounds))
            {
                return true;
            }

            return false;
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

            //also update contained nodes
            foreach(IUIGraphNode n in contained)
            {
                n.Offset(dx, dy);
            }
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
            //do nothing
        }

        public void Dispose()
        {
            Graph.RemoveNode(this);
        }

        private void CommentText_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter || e.Key == Key.Escape)
            {
                //clear select
                CommentText.SelectionLength = 0;
                //clear focus
                Keyboard.ClearFocus();
                //reapply focus to this
                Focus();
                Keyboard.Focus(this);
                Holder.Visibility = Visibility.Visible;
            }
        }

        private void CommentText_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (loading)
            {
                loading = false;
                return;
            }

            if (!IsLoaded) return;

            commentNode.SetContent(CommentText.Text);
        }

        private void CommentText_LostFocus(object sender, RoutedEventArgs e)
        {
            CommentText.SelectionLength = 0;
            //show holder
            Holder.Visibility = Visibility.Visible;
        }
    }
}
