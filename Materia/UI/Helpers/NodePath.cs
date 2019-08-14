using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Shapes;
using System.Windows.Media;
using Materia.Nodes;

namespace Materia.UI.Helpers
{
    public class NodePath : IDisposable
    {
        private static SolidColorBrush RedColor = new SolidColorBrush(Colors.Red);

        protected Canvas view;
        protected UINodePoint point1;
        protected UINodePoint point2;
        protected Path path;
        protected TextBlock num;
        protected bool needsUpdate;

        protected bool selected;

        protected static NodePathType lastType;
        protected static NodePathType type;
        public static NodePathType Type
        {
            get
            {
                return type;
            }
            set
            {
                lastType = type;
                type = value;
            }
        }

        public bool Selected
        {
            get
            {
                return selected;
            }
            set
            {
                selected = value;
            }
        }

        static NodePath()
        {
            lastType = type = NodePathType.Line;
        }

        public NodePath(Canvas viewPort, UINodePoint p1, UINodePoint p2, bool execute = false)
        {
            needsUpdate = true;
            view = viewPort;
            point1 = p1;
            point2 = p2;

            //subscribe to view layout update
            view.LayoutUpdated += View_LayoutUpdated;

            path = new Path();
            path.Stroke = point1.ColorBrush;
            path.StrokeThickness = 2;

            if (execute)
            {
                num = new TextBlock();
                num.HorizontalAlignment = HorizontalAlignment.Left;
                num.VerticalAlignment = VerticalAlignment.Top;
                num.Foreground = new SolidColorBrush(Colors.LightGray);
                num.FontSize = 12;
            }

            view.Children.Add(path);

            if (num != null)
            {
                view.Children.Add(num);
            }
        }

        private void View_LayoutUpdated(object sender, EventArgs e)
        {
            if(needsUpdate)
            {
                needsUpdate = false;
                Update();
            }
        }

        public void Dispose()
        {
            if(view != null)
            {
                view.LayoutUpdated -= View_LayoutUpdated;
            }
            if(path != null && path.Parent != null && view != null)
            {
                view.Children.Remove(path);
                path = null;
            }
            if(num != null && num.Parent != null && view != null)
            {
                view.Children.Remove(num);
                num = null;
            }
            view = null;
        }

        private void Update()
        {
            try
            {
                switch(type)
                {
                    case NodePathType.Bezier:
                        DrawBezier();
                        break;
                    case NodePathType.Line:
                        DrawLines();
                        break;
                    default:
                        DrawLines();
                        break;
                }
            }
            catch { }

            needsUpdate = true;
        }

        private void InitLines(ref Point r1, double r1Extra, double r1y, double r2Extra, double r2y, ref Point r2)
        {
            path.VerticalAlignment = VerticalAlignment.Top;
            path.HorizontalAlignment = HorizontalAlignment.Left;
            PathGeometry p = new PathGeometry();
            PathFigure pf = new PathFigure();
            pf.IsClosed = false;
            pf.StartPoint = r1;

            LineSegment seg = new LineSegment(new Point(r1Extra, r1y), true);
            pf.Segments.Add(seg);
            LineSegment seg2 = new LineSegment(new Point(r2Extra, r2y), true);
            pf.Segments.Add(seg2);
            LineSegment seg3 = new LineSegment(r2, true);
            pf.Segments.Add(seg3);
            p.Figures.Add(pf);
            path.Data = p;
        }

        private void DrawLines()
        {
            if (point1.Parent == null || point2.Parent == null) return;
            if (!point1.HasAncestor(view) || !point2.HasAncestor(view)) return;

            Point r1 = point1.TransformToAncestor(view).Transform(new Point(point1.ActualWidth, 8f));
            Point r2 = point2.TransformToAncestor(view).Transform(new Point(0f, 8f));

            double midy = (r2.Y + r1.Y) * 0.5;
            double midx = (r2.X + r1.X) * 0.5;

            ///calculate distance
            double diffx = r2.X - r1.X;
            diffx *= diffx;
            double diffy = r2.Y - r1.Y;
            diffy *= diffy;

            double dist = Math.Sqrt(diffx + diffy);
            ///

            double distExtra = dist * 0.05;

            double r1Extra = r1.X + distExtra;
            double r2Extra = r2.X - distExtra;

            double r1y = midy;
            double r2y = midy;

            if(path != null)
            {
                if(selected)
                {
                    path.Stroke = RedColor;
                }
                else
                {
                    path.Stroke = point1.ColorBrush;
                }

                path.IsHitTestVisible = false;
                Canvas.SetZIndex(path, -1);
                if(path.Data  == null || lastType != type)
                {
                    lastType = type;
                    InitLines(ref r1, r1Extra, r1y, r2Extra, r2y, ref r2);
                }
                else
                {
                    PathGeometry p = (PathGeometry)path.Data;
                    PathFigure pf = p.Figures[0];
                    pf.StartPoint = r1;

                    if (pf.Segments.Count < 3)
                    {
                        InitLines(ref r1, r1Extra, r1y, r2Extra, r2y, ref r2);
                    }
                    else if(pf.Segments[0] is LineSegment && pf.Segments[1] is LineSegment && pf.Segments[2] is LineSegment)
                    {
                        LineSegment seg = (LineSegment)pf.Segments[0];
                        seg.Point = new Point(r1Extra, r1y);
                        LineSegment seg2 = (LineSegment)pf.Segments[1];
                        seg2.Point = new Point(r2Extra, r2y);
                        LineSegment seg3 = (LineSegment)pf.Segments[2];
                        seg3.Point = r2;
                    }
                    else
                    {
                        InitLines(ref r1, r1Extra, r1y, r2Extra, r2y, ref r2);
                    }
                }
            }

            if (num != null)
            {
                Point p = new Point(midx, midy);
                num.Text = (point1.GetOutIndex(point2) + 1).ToString();
                num.IsHitTestVisible = false;
                Canvas.SetZIndex(num, -1);
                Canvas.SetLeft(num, p.X);
                Canvas.SetTop(num, p.Y);
            }
        }

        private void InitBezier(ref Point r1, ref Point mid, ref Point r2)
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

        private void DrawBezier()
        {
            if (point1.Parent == null || point2.Parent == null) return;
            if (!point1.HasAncestor(view) || !point2.HasAncestor(view)) return;

            Point r1 = point1.TransformToAncestor(view).Transform(new Point(point1.ActualWidth, 8f));
            Point r2 = point2.TransformToAncestor(view).Transform(new Point(0f, 8f));

            double dy = r2.Y - r1.Y;

            Point mid = new Point((r2.X + r1.X) * 0.5f, (r2.Y + r1.Y) * 0.5f + dy * 0.5f);

            if (path != null)
            {
                if (selected)
                {
                    path.Stroke = RedColor;
                }
                else
                {
                    path.Stroke = point1.ColorBrush;
                }

                path.IsHitTestVisible = false;
                Canvas.SetZIndex(path, -1);
                if (path.Data == null || lastType != type)
                {
                    lastType = type;
                    InitBezier(ref r1, ref mid, ref r2);
                }
                else
                {
                    PathGeometry p = (PathGeometry)path.Data;
                    PathFigure pf = p.Figures[0];
                    pf.StartPoint = r1;

                    if (pf.Segments.Count > 1)
                    {
                        InitBezier(ref r1, ref mid, ref r2);
                    }
                    else if (pf.Segments[0] is BezierSegment)
                    {
                        BezierSegment seg = (BezierSegment)pf.Segments[0];
                        seg.Point1 = r1;
                        seg.Point2 = mid;
                        seg.Point3 = r2;
                    }
                    else
                    {
                        InitBezier(ref r1, ref mid, ref r2);
                    }
                }
            }

            if (num != null)
            {
                Point p = CatmullRomSpline.GetPointOnBezierCurve(r1, mid, r2, 0.5f);
                num.Text = (point1.GetOutIndex(point2) + 1).ToString();
                num.IsHitTestVisible = false;
                Canvas.SetZIndex(num, -1);
                Canvas.SetLeft(num, p.X);
                Canvas.SetTop(num, p.Y);
            }
        }
    }
}
