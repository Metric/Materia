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
using System.Reflection;
using Materia.Imaging;
using Materia.Nodes.Helpers;
using System.Threading;
using Materia.UI.Helpers;

namespace Materia
{
    /// <summary>
    /// Interaction logic for UICurves.xaml
    /// </summary>
    public partial class UICurves : UserControl
    {
        PropertyInfo property;
        PropertyInfo outputProperty;
        object propertyOwner;

        Dictionary<int, List<CurvePoint>> Points { get; set; }
        Dictionary<int, List<Point>> Normalized { get; set; }
        Dictionary<int, List<Point>> Input { get; set; }

        RawBitmap display;

        object target;
        Point mouseStart;

        public enum CurveMode
        {
            RGB = 0,
            Red = 1,
            Green = 2,
            Blue = 3
        }

        CurveMode mode;

        public bool ShowAllCurves { get; set; }
        bool inited;

        public UICurves()
        {
            InitializeComponent();
            mode = CurveMode.RGB;
            Points = new Dictionary<int, List<CurvePoint>>();
            Points[0] = new List<CurvePoint>();
            Points[1] = new List<CurvePoint>();
            Points[2] = new List<CurvePoint>();
            Points[3] = new List<CurvePoint>();

            Normalized = new Dictionary<int, List<Point>>();
            Normalized[0] = new List<Point>();
            Normalized[1] = new List<Point>();
            Normalized[2] = new List<Point>();
            Normalized[3] = new List<Point>();

            inited = false;

            Input = new Dictionary<int, List<Point>>();
            Input[0] = new List<Point>();
            Input[1] = new List<Point>();
            Input[2] = new List<Point>();
            Input[3] = new List<Point>();
        }

        public UICurves(PropertyInfo p, PropertyInfo output, object owner)
        {
            InitializeComponent();
            mode = CurveMode.RGB;
            outputProperty = output;
            property = p;
            propertyOwner = owner;

            Points = new Dictionary<int, List<CurvePoint>>();
            Points[0] = new List<CurvePoint>();
            Points[1] = new List<CurvePoint>();
            Points[2] = new List<CurvePoint>();
            Points[3] = new List<CurvePoint>();

            inited = false;

            Input = (Dictionary<int, List<Point>>)p.GetValue(owner);
            Normalized = (Dictionary<int, List<Point>>)output.GetValue(owner);

            Sort(Input[0]);
            Sort(Input[1]);
            Sort(Input[2]);
            Sort(Input[3]);
        }

        void UpdateProperty()
        {
            Dictionary<int, List<Point>> pts = new Dictionary<int, List<Point>>();

            for(int i = 0; i < 4; i++)
            {
                var cps = Points[i];

                pts[i] = new List<Point>();

                foreach(CurvePoint cp in cps)
                {
                    pts[i].Add(cp.Normalized);
                }
            }

            property.SetValue(propertyOwner, pts);
        }

        void AddPoint(Point p)
        {
            int m = (int)mode;

            CurvePoint cp = new CurvePoint(this);
            cp.HorizontalAlignment = HorizontalAlignment.Left;
            cp.VerticalAlignment = VerticalAlignment.Top;
            cp.Width = 8;
            cp.Height = 8;
            cp.MouseDown += Point_MouseDown;
            cp.MouseUp += Point_MouseUp;
            cp.MouseMove += Point_MouseMove;
            cp.Position = p;

            Points[m].Add(cp);
            CurveView.Children.Add(cp);

            UpdatePath();
            UpdateProperty();
        }

        void AddPointNormalized(Point p)
        {
            int m = (int)mode;

            CurvePoint cp = new CurvePoint(this);
            cp.HorizontalAlignment = HorizontalAlignment.Left;
            cp.VerticalAlignment = VerticalAlignment.Top;
            cp.Width = 8;
            cp.Height = 8;
            cp.MouseDown += Point_MouseDown;
            cp.MouseUp += Point_MouseUp;
            cp.MouseMove += Point_MouseMove;
            cp.Normalized = p;

            Points[m].Add(cp);
            CurveView.Children.Add(cp);
        }

        private void Point_MouseUp(object sender, MouseButtonEventArgs e)
        {
            target = null;
        }

        private void Point_MouseMove(object sender, MouseEventArgs e)
        {
            if(target == sender)
            {
                e.Handled = true;
                Point ep = e.GetPosition(CurveView);
                double dx = ep.X - mouseStart.X;
                double dy = ep.Y - mouseStart.Y;

                CurvePoint cp = target as CurvePoint;
                Point p = cp.Position;
                p.X += dx;
                p.Y += dy;
                cp.Position = p;

                mouseStart = ep;
                //update curve
                UpdatePath();
                UpdateProperty();
            }
        }

        void UpdatePointsForResize()
        {
            if (!inited) return;

            display = new RawBitmap((int)CurveView.ActualWidth, (int)CurveView.ActualHeight);

            for(int i = 0; i < 3; i++)
            {
                var pts = Points[i];

                foreach(CurvePoint cp in pts)
                {
                    cp.UpdateViewPosition();
                    cp.Relayout();
                }
            }

            UpdatePath();
        }

        /// <summary>
        /// Renderes the path to the buffer
        /// and returns the normalized curve data
        /// the path should be a normalized set of points from 0 - 1
        /// </summary>
        /// <param name="path"></param>
        /// <param name="buffer"></param>
        /// <returns></returns>
        protected List<Point> RenderPath(List<CurvePoint> path, RawBitmap buffer, byte r = 175, byte g = 175, byte b = 175)
        {
            List<Point> points = new List<Point>();
            List<Point> curve = new List<Point>();
            List<Point> normalized = new List<Point>();
            double width = CurveView.ActualWidth;
            double height = CurveView.ActualHeight - 1;

            foreach (CurvePoint p in path)
            {
                Point n = p.Normalized;
                points.Add(new Point(n.X * width, n.Y * height));
            }

            Sort(points);

            //make sure we have x points on edges
            if (points.Count >= 2)
            {
                Point f = points[0];

                if (f.X > 0)
                {
                    points.Insert(0, new Point(0, f.Y));
                }

                Point l = points[points.Count - 1];

                if (l.X < width)
                {
                    points.Add(new Point(width, l.Y));
                }
            }

            double[] sd = Curves.SecondDerivative(points.ToArray());

            for (int i = 0; i < points.Count - 1; i++)
            {
                Point cur = points[i];
                Point next = points[i + 1];

                for (double x = cur.X; x < next.X; x++)
                {
                    double t = (double)(x - cur.X) / (next.X - cur.X);

                    double a = 1 - t;
                    double bt = t;
                    double h = next.X - cur.X;

                    double y = a * cur.Y + bt * next.Y + (h * h / 6) * ((a * a * a - a) * sd[i] + (bt * bt * bt - bt) * sd[i + 1]);


                    if (y < 0) y = 0;
                    if (y > height - 1) y = height - 1;

                    curve.Add(new Point(x, y));
                    normalized.Add(new Point(x / width, y / height));
                }
            }

            Point lp = points[points.Count - 1];
            curve.Add(lp);
            normalized.Add(new Point(lp.X / width, lp.Y / height));

            Parallel.For(0, curve.Count - 1, i =>
            {
                Point p1 = curve[i];
                Point p2 = curve[i + 1];
                buffer.DrawLine((int)p1.X, (int)p1.Y, (int)p2.X, (int)p2.Y, r, g, b, 255);
            });

            return normalized;
        }

        protected void UpdatePath(bool updateProperty = true)
        {
            List<Point> normals = new List<Point>();
            if(display == null)
            {
                display = new RawBitmap((int)CurveView.ActualWidth, (int)CurveView.ActualHeight);
            }

            Utils.Clear(display);

            if(ShowAllCurves)
            {
                for(int i = 0; i < 4; i++)
                {
                    switch(i)
                    {
                        case 0:
                            normals = RenderPath(Points[i], display);
                            break;
                        case 1:
                            normals = RenderPath(Points[i], display, 255, 0, 0);
                            break;
                        case 2:
                            normals = RenderPath(Points[i], display, 0, 255, 0);
                            break;
                        case 3:
                            normals = RenderPath(Points[i], display, 0, 45, 255);
                            break;
                    }

                    Normalized[i] = normals;
                }
            }
            else
            {
                switch(mode)
                {
                    case CurveMode.RGB:
                        normals = RenderPath(Points[0], display);
                        break;
                    case CurveMode.Red:
                        normals = RenderPath(Points[1], display, 255, 0, 0);
                        break;
                    case CurveMode.Green:
                        normals = RenderPath(Points[2], display, 0, 255, 0);
                        break;
                    case CurveMode.Blue:
                        normals = RenderPath(Points[3], display, 0, 45, 255);
                        break;
                }

                Normalized[(int)mode] = normals;
            }

            CurvePixels.Source = display.ToImageSource();

            if (updateProperty)
            {
                outputProperty.SetValue(propertyOwner, Normalized);
            }
        }

        private void Point_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && !Keyboard.IsKeyDown(Key.LeftAlt) && !Keyboard.IsKeyDown(Key.RightAlt))
            {
                mouseStart = e.GetPosition(CurveView);
                target = sender;
            }
            else if(e.LeftButton == MouseButtonState.Pressed && (Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt)))
            {
                RemovePoint(sender as CurvePoint);
            }
        }

        void Sort(List<Point> p)
        {
            //sort by x
            p.Sort((Point p1, Point p2) =>
            {
                return (int)(p1.X - p2.X);
            });
        }

        private void CurveView_MouseUp(object sender, MouseButtonEventArgs e)
        {
            target = null;
        }

        private void CurveView_MouseMove(object sender, MouseEventArgs e)
        {
            if(target != null)
            {
                Point_MouseMove(target, e);
            }
        }

        private void CurveView_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if(e.ClickCount > 1)
            {
                AddPoint(e.GetPosition(CurveView));
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {

            Points[0] = new List<CurvePoint>();
            Points[1] = new List<CurvePoint>();
            Points[2] = new List<CurvePoint>();
            Points[3] = new List<CurvePoint>();

            foreach (Point pt in Input[0])
            {
                CurvePoint cp = new CurvePoint(this);
                cp.HorizontalAlignment = HorizontalAlignment.Left;
                cp.VerticalAlignment = VerticalAlignment.Top;
                cp.Width = 8;
                cp.Height = 8;
                cp.MouseDown += Point_MouseDown;
                cp.MouseUp += Point_MouseUp;
                cp.MouseMove += Point_MouseMove;
                cp.Normalized = pt;

                Points[0].Add(cp);
                CurveView.Children.Add(cp);
            }

            foreach (Point pt in Input[1])
            {
                CurvePoint cp = new CurvePoint(this);
                cp.HorizontalAlignment = HorizontalAlignment.Left;
                cp.VerticalAlignment = VerticalAlignment.Top;
                cp.Width = 8;
                cp.Height = 8;
                cp.MouseDown += Point_MouseDown;
                cp.MouseUp += Point_MouseUp;
                cp.MouseMove += Point_MouseMove;
                cp.Normalized = pt;

                Points[1].Add(cp);
            }

            foreach (Point pt in Input[2])
            {
                CurvePoint cp = new CurvePoint(this);
                cp.HorizontalAlignment = HorizontalAlignment.Left;
                cp.VerticalAlignment = VerticalAlignment.Top;
                cp.Width = 8;
                cp.Height = 8;
                cp.MouseDown += Point_MouseDown;
                cp.MouseUp += Point_MouseUp;
                cp.MouseMove += Point_MouseMove;
                cp.Normalized = pt;

                Points[2].Add(cp);
            }

            foreach(Point pt in Input[3])
            {
                CurvePoint cp = new CurvePoint(this);
                cp.HorizontalAlignment = HorizontalAlignment.Left;
                cp.VerticalAlignment = VerticalAlignment.Top;
                cp.Width = 8;
                cp.Height = 8;
                cp.MouseDown += Point_MouseDown;
                cp.MouseUp += Point_MouseUp;
                cp.MouseMove += Point_MouseMove;
                cp.Normalized = pt;

                Points[3].Add(cp);
            }

            inited = true;

            UpdatePath();

            Channels.SelectedIndex = 0;
        }

        void HidePoints(CurveMode m)
        {
            int idx = (int)m;

            var pts = Points[idx];

            foreach(CurvePoint p in pts)
            {
                CurveView.Children.Remove(p);
            }
        }

        void ShowPoints(CurveMode m)
        {
            int idx = (int)m;

            var pts = Points[idx];

            foreach(CurvePoint p in pts)
            {
                CurveView.Children.Add(p);
            }
        }

        void RemovePoint(CurvePoint p)
        {
            int idx = (int)mode;
            var pts = Points[idx];

            if (pts.Count > 2)
            {
                pts.Remove(p);

                UpdatePath();
                UpdateProperty();

                CurveView.Children.Remove(p);
            }
        }

        void ResetCurve()
        {
            int idx = (int)mode;

            var pts = Points[idx];

            foreach(CurvePoint p in pts)
            {
                CurveView.Children.Remove(p);
            }

            pts.Clear();

            AddPointNormalized(new Point(0, 1));
            AddPointNormalized(new Point(1, 0));

            UpdatePath();
            UpdateProperty();
        }

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdatePointsForResize();
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CurveMode prev = mode;

            HidePoints(prev);

            mode = (CurveMode)Channels.SelectedIndex;

            ShowPoints(mode);

            UpdatePath();
        }

        private void CurveView_MouseLeave(object sender, MouseEventArgs e)
        {
            target = null;
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            ResetCurve();
        }

        private void ToggleAll_Click(object sender, RoutedEventArgs e)
        {
            ShowAllCurves = ToggleAll.IsChecked.Value;
            UpdatePath(false);
        }
    }
}
