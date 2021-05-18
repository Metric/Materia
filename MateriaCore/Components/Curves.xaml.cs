using Avalonia;
using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Materia.Rendering.Imaging;
using Materia.Rendering.Mathematics;
using System.Reflection;
using System;
using System.Threading.Tasks;

namespace MateriaCore.Components
{
    public enum CurvesMode
    {
        RGB = 0,
        Red = 1,
        Green = 2,
        Blue = 3
    }

    public class Curves : UserControl
    {
        Button reset;
        Toggle toggleAll;
        ComboBox channels;
        Image curvePixels;
        RangeSlider valueRange;
        Canvas curveView;

        public Canvas View
        {
            get
            {
                return curveView;
            }
        }

        PropertyInfo property;
        object propertyOwner;

        PropertyInfo minProperty;
        PropertyInfo maxProperty;

        /// <summary>
        /// Note replace control with proper UserControl name
        /// </summary>
        Dictionary<int, List<CurvePoint>> Points { get; set; }
        Dictionary<int, List<PointD>> Normalized { get; set; }
        Dictionary<int, List<PointD>> Input { get; set; }

        RawBitmap display;

        CurvePoint target;

        CurvesMode mode;

        public bool ShowAllCurves { get; set; }

        public Curves()
        {
            this.InitializeComponent();
            InitDictionaries();

            toggleAll.ValueChanged += ToggleAll_ValueChanged;
            reset.Click += Reset_Click;
            channels.SelectionChanged += Channels_SelectionChanged;
            curveView.PointerPressed += CurveView_PointerPressed;
            valueRange.OnValueChanged += ValueRange_OnValueChanged;

            PropertyChanged += Curves_PropertyChanged;

            valueRange?.Set(0, 1);

            mode = CurvesMode.RGB;
            ResetCurve();
            mode = CurvesMode.Red;
            ResetCurve();
            mode = CurvesMode.Green;
            ResetCurve();
            mode = CurvesMode.Blue;
            ResetCurve();
            mode = CurvesMode.RGB;

            channels.SelectedIndex = 0;
        }

        private void ValueRange_OnValueChanged(RangeSlider slider, float min, float max)
        {
            UpdateValueRange(min, max);
        }

        private void Curves_PropertyChanged(object sender, AvaloniaPropertyChangedEventArgs e)
        {
            if (e.Property.Name == "Bounds")
            {
                OnSizeChanged();
            }
        }

        private void CurveView_PointerPressed(object sender, Avalonia.Input.PointerPressedEventArgs e)
        {
            if (e.KeyModifiers == Avalonia.Input.KeyModifiers.Control)
            {
                Point p = e.GetPosition(curveView);
                PointD pt = new PointD(p.X, p.Y);
                AddPoint(pt);
            }
        }

        public Curves(PropertyInfo p, object owner) : this()
        {
            property = p;
            propertyOwner = owner;
            mode = CurvesMode.RGB;

            try
            {
                minProperty = propertyOwner.GetType().GetProperty("MinValue");
            }
            catch (Exception e) { }

            try
            {
                maxProperty = propertyOwner.GetType().GetProperty("MaxValue");
            }
            catch (Exception e) { }

            Input = (Dictionary<int, List<PointD>>)p.GetValue(owner);

            Sort(Input[0]);
            Sort(Input[1]);
            Sort(Input[2]);
            Sort(Input[3]);

            float min = 0;
            float max = 1;

            if (minProperty != null)
            {
                min = Convert.ToSingle(minProperty.GetValue(propertyOwner));
            }
            if (maxProperty != null)
            {
                max = Convert.ToSingle(maxProperty.GetValue(propertyOwner));
            }

            InitPoints();

            valueRange?.Set(min, max);
        }

        void UpdateProperty()
        {
            Dictionary<int, List<PointD>> pts = new Dictionary<int, List<PointD>>();

            for (int i = 0; i < Points.Count; ++i)
            {
                var cps = Points[i];

                pts[i] = new List<PointD>();

                
                foreach (CurvePoint cp in cps)
                {
                    pts[i].Add(cp.Normalized);
                }
                
            }

            property?.SetValue(propertyOwner, pts);
        }

        void AddPoint(PointD p)
        {
            int m = (int)mode;

            CurvePoint cp = new CurvePoint(this);
            cp.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left;
            cp.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Top;
            cp.Width = 8;
            cp.Height = 8;
            cp.PointerPressed += Cp_PointerPressed;

            Points[m].Add(cp);
            curveView.Children.Add(cp);

            cp.Position = p;

            UpdatePath();
            UpdateProperty();
        }

        void AddPointNormalized(PointD p, bool update = true)
        {
            int m = (int)mode;

            CurvePoint cp = new CurvePoint(this);
            cp.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left;
            cp.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Top;
            cp.Width = 8;
            cp.Height = 8;
            cp.PointerPressed += Cp_PointerPressed;

            Points[m].Add(cp);
            curveView.Children.Add(cp);

            cp.Normalized = p;

            if (update)
            {
                UpdatePath();
                UpdateProperty();
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

                curveView.Children.Remove(p);
            }
        }

        void ShowPoints(CurvesMode mode, bool show = true)
        {
            int idx = 0;
            List<CurvePoint> pts = null;

            for (idx = 0; idx < Points.Count; ++idx)
            {
                pts = Points[idx];
                for (int i = 0; i < pts.Count; ++i)
                {
                    pts[i].IsVisible = false;
                }
            }

            idx = (int)mode;
            pts = Points[idx];

            for(int i = 0; i < pts.Count; ++i)
            {
                pts[i].IsVisible = show;
            }
        }

        void ResetCurve()
        {
            ClearCurve();

            AddPointNormalized(new PointD(0, 1));
            AddPointNormalized(new PointD(1, 0));

            valueRange?.Set(0, 1);
            UpdateValueRange(0, 1);

            UpdatePath();
            UpdateProperty();
        }

        void ClearCurve()
        {
            int idx = (int)mode;

            var pts = Points[idx];

            for (int i = 0; i < pts.Count; ++i)
            {
                curveView.Children.Remove(pts[i]);
            }

            pts.Clear();
        }

        private void Cp_PointerPressed(object sender, Avalonia.Input.PointerPressedEventArgs e)
        {
            if (e.KeyModifiers == Avalonia.Input.KeyModifiers.None)
            {
                target = sender as CurvePoint;

                if (target != null)
                {
                    SubscribeToWindowPointer();
                }
            }
            else if(e.KeyModifiers == Avalonia.Input.KeyModifiers.Alt)
            {
                CurvePoint point = sender as CurvePoint;

                if (point != null)
                {
                    RemovePoint(point);
                }
            }
        }

        void SubscribeToWindowPointer()
        {
            Window w = (Window)VisualRoot;

            if(w != null)
            {
                w.PointerReleased += W_PointerReleased;
                w.PointerMoved += W_PointerMoved;
            }
        }

        void UnsubscribeFromWindowPointer()
        {
            Window w = (Window)VisualRoot;

            if (w != null)
            {
                w.PointerReleased -= W_PointerReleased;
                w.PointerMoved -= W_PointerMoved;
            }
        }

        private void W_PointerMoved(object sender, Avalonia.Input.PointerEventArgs e)
        {
            if (target != null)
            {
                Point ep = e.GetPosition(curveView);

                PointD p = target.Normalized;

                p.x = ep.X / curveView.Bounds.Width;
                p.y = ep.Y / curveView.Bounds.Height;

                p.x = Math.Clamp(p.x, 0d, 1d);
                p.y = Math.Clamp(p.y, 0d, 1d);

                target.Normalized = p;

                UpdatePath();
                UpdateProperty();
            }
        }

        private void W_PointerReleased(object sender, Avalonia.Input.PointerReleasedEventArgs e)
        {
            if (target != null)
            {
                UnsubscribeFromWindowPointer();
            }

            target = null;
        }

        List<PointD> RenderPath(List<CurvePoint> path, RawBitmap buffer, byte r = 175, byte g = 175, byte b = 175)
        {
            List<PointD> points = new List<PointD>();
            List<PointD> curve = new List<PointD>();
            List<PointD> normalized = new List<PointD>();

            double width = curveView.Bounds.Width;
            double height = curveView.Bounds.Height;

            foreach (CurvePoint p in path)
            {
                PointD n = p.Normalized;
                points.Add(new PointD(n.x * width, n.y * height));
            }

            Sort(points);

            if (points.Count >= 2)
            {
                PointD f = points[0];

                if (f.x > 0)
                {
                    points.Insert(0, new PointD(0, f.y));
                }

                PointD l = points[points.Count - 1];

                if (l.x < width)
                {
                    points.Add(new PointD(width, l.y));
                }
            }

            double[] sd = Materia.Rendering.Mathematics.Curves.SecondDerivative(points.ToArray());

            for (int i = 0; i < points.Count - 1; ++i)
            {
                PointD cur = points[i];
                PointD next = points[i + 1];

                for (double x = cur.x; x < next.x; ++x)
                {
                    double t = (double)(x - cur.x) / (next.x - cur.x);

                    double a = 1 - t;
                    double bt = t;
                    double h = next.x - cur.x;

                    double y = a * cur.y + bt * next.y + (h * h / 6) * ((a * a * a - a) * sd[i] + (bt * bt * bt - bt) * sd[i + 1]);


                    if (y < 0) y = 0;
                    if (y > height - 1) y = height - 1;

                    curve.Add(new PointD(x, y));
                    normalized.Add(new PointD(x / width, y / height));
                }
            }

            if (points.Count == 0)
            {
                return normalized;
            }

            PointD lp = points[points.Count - 1];
            curve.Add(lp);
            normalized.Add(new PointD(lp.x / width, lp.y / height));

            GLPixel pixel = GLPixel.FromRGBA(r, g, b, 255);

            Parallel.For(0, curve.Count - 1, i =>
            {
                PointD p1 = curve[i];
                PointD p2 = curve[i + 1];
                buffer.DrawLine((int)p1.x, (int)p1.y, (int)p2.x, (int)p2.y, ref pixel);
            });

            return normalized;
        }

        void Sort(List<PointD> p)
        {
            p.Sort((a, b) =>
            {
                return (int)(a.x - b.x);
            });
        }

        void OnSizeChanged()
        {
            display = new RawBitmap((int)curveView.Bounds.Width, (int)curveView.Bounds.Height);

            for (int i = 0; i < Points.Count; ++i)
            {
                var pts = Points[i];

                for(int j = 0; j < pts.Count; ++j)
                {
                    pts[j].UpdatePosition();
                }
            }

            UpdatePath();
        }

        void UpdatePath()
        {
            List<PointD> normals = new List<PointD>();
            if (display == null || display.Width == 0 || display.Height == 0)
            {
                display = new RawBitmap((int)curveView.Bounds.Width, (int)curveView.Bounds.Height);
            }

            if (display.Width == 0 || display.Height == 0)
            {
                return;
            }

            display.Clear();

            if (ShowAllCurves)
            {
                for (int i = 0; i < Points.Count; ++i)
                {
                    switch (i)
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
                switch (mode)
                {
                    case CurvesMode.RGB:
                        normals = RenderPath(Points[0], display);
                        break;
                    case CurvesMode.Red:
                        normals = RenderPath(Points[1], display, 255, 0, 0);
                        break;
                    case CurvesMode.Green:
                        normals = RenderPath(Points[2], display, 0, 255, 0);
                        break;
                    case CurvesMode.Blue:
                        normals = RenderPath(Points[3], display, 0, 45, 255);
                        break;
                }

                Normalized[(int)mode] = normals;
            }

            curvePixels.Source = display.ToAvBitmap();
        }

        private void InitDictionaries()
        {
            Points = new Dictionary<int, List<CurvePoint>>();
            Normalized = new Dictionary<int, List<PointD>>();
            Input = new Dictionary<int, List<PointD>>();

            Points[0] = new List<CurvePoint>();
            Points[1] = new List<CurvePoint>();
            Points[2] = new List<CurvePoint>();
            Points[3] = new List<CurvePoint>();

            Normalized[0] = new List<PointD>();
            Normalized[1] = new List<PointD>();
            Normalized[2] = new List<PointD>();
            Normalized[3] = new List<PointD>();

            Input[0] = new List<PointD>();
            Input[1] = new List<PointD>();
            Input[2] = new List<PointD>();
            Input[3] = new List<PointD>();
        }

        private void InitPoints()
        {
            mode = CurvesMode.RGB;
            ClearCurve();
            foreach(PointD pt in Input[0])
            {
                AddPointNormalized(pt, false);
            }
            mode = CurvesMode.Red;
            ClearCurve();
            foreach(PointD pt in Input[1])
            {
                AddPointNormalized(pt, false);
            }
            ShowPoints(mode, false);
            mode = CurvesMode.Green;
            ClearCurve();
            foreach(PointD pt in Input[2])
            {
                AddPointNormalized(pt, false);
            }
            ShowPoints(mode, false);
            mode = CurvesMode.Blue;
            ClearCurve();
            foreach(PointD pt in Input[3])
            {
                AddPointNormalized(pt, false);
            }
            ShowPoints(mode, false);

            mode = CurvesMode.RGB;
            channels.SelectedIndex = 0;
        }
        
        private void Channels_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CurvesMode prev = mode;
            ShowPoints(prev, false);
            mode = (CurvesMode)channels.SelectedIndex;
            ShowPoints(mode);
            UpdatePath();
        }

        private void Reset_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            ResetCurve();
        }

        private void ToggleAll_ValueChanged(Toggle t, bool value)
        {
            ShowAllCurves = value;
            UpdatePath();
        }

        private void UpdateValueRange(float min, float max)
        {
            minProperty?.SetValue(propertyOwner, min);
            maxProperty?.SetValue(propertyOwner, max);
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            toggleAll = this.FindControl<Toggle>("ToggleAll");
            reset = this.FindControl<Button>("Reset");
            channels = this.FindControl<ComboBox>("Channels");
            curvePixels = this.FindControl<Image>("CurvePixels");
            curveView = this.FindControl<Canvas>("CurveView");
            valueRange = this.FindControl<RangeSlider>("ValueRange");
        }
    }
}
