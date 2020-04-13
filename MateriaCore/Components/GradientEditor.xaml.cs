using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Materia.Rendering.Imaging;
using Materia.Rendering.Mathematics;
using MLog;
using System;
using System.Collections.Generic;
using System.Reflection;
using Materia.Nodes.Containers;
using System.Threading.Tasks;

namespace MateriaCore.Components
{
    public class GradientEditor : UserControl
    {
        const double HANDLE_HALF_WIDTH = 8;

        Image gradientImage;
        Canvas handleHolder;
        Grid imageWrapper;

        List<GradientHandle> handles;

        PropertyInfo property;
        object propertyOwner;

        RawBitmap bmp;

        GradientHandle target;
        Point mouseStart;
        int clickCount = 0;
        ulong clickTimeStamp = 0;

        public GradientEditor()
        {
            this.InitializeComponent();
            handles = new List<GradientHandle>();
            PointerPressed += GradientEditor_PointerPressed;
            PropertyChanged += GradientEditor_PropertyChanged;
            InitBase();
        }

        private void GradientEditor_PointerPressed(object sender, PointerPressedEventArgs e)
        {
            if (e.Timestamp - clickTimeStamp > 500)
            {
                clickCount = 0;
            }

            if (clickCount > 1)
            {    
                GradientHandle handle = null;
                Point p = e.GetPosition(handleHolder);
                double min = double.PositiveInfinity;
                for (int i = 0; i < handles.Count; ++i)
                {
                    double x = Canvas.GetLeft(handles[i]);
                    double dist = Math.Abs(p.X - x);

                    if (dist < min)
                    {
                        min = dist;
                        handle = handles[i];
                    }
                }

                Point realPoint = new Point(p.X - HANDLE_HALF_WIDTH, p.Y);

                if (handle == null)
                {
                    AddHandle(realPoint, new MVector(0, 0, 0, 1), true);
                }
                else
                {
                    AddHandle(realPoint, handle.SelectedColor, true);
                }

                clickCount = 0;
            }

            clickTimeStamp = e.Timestamp;
            ++clickCount;
        }

        private void GradientEditor_PropertyChanged(object sender, AvaloniaPropertyChangedEventArgs e)
        {
            if (e.Property.Name == "Bounds")
            {
                OnSizeChanged();
            }
        }

        public GradientEditor(PropertyInfo p, object owner) : this()
        {
            handles = new List<GradientHandle>();
            property = p;
            propertyOwner = owner;
            InitHandles();
        }

        void SubscribeToWindowPointer()
        {
            Window w = (Window)VisualRoot;
            if (w != null)
            {
                w.PointerMoved += W_PointerMoved;
                w.PointerReleased += W_PointerReleased;
            }
        }

        private void W_PointerReleased(object sender, PointerReleasedEventArgs e)
        {
            if (target != null)
            {
                UnsubscribeFromWindowPointer();
            }

            target = null;
        }

        private void W_PointerMoved(object sender, PointerEventArgs e)
        {
            if(target == null)
            {
                return;
            }

            Point p = e.GetPosition(handleHolder);
            double delta = p.X - mouseStart.X;
            double pos = UpdateHandlePosition(target, delta);
            float normal = MathF.Min(1, MathF.Min(0, (float)(pos / handleHolder.Bounds.Width - HANDLE_HALF_WIDTH)));
            target.Position = normal;
            mouseStart = p;
        }

        void UnsubscribeFromWindowPointer()
        {
            Window w = (Window)VisualRoot;
            if (w != null)
            {
                w.PointerMoved -= W_PointerMoved;
                w.PointerReleased -= W_PointerReleased;
            }
        }

        double UpdateHandlePosition(GradientHandle h, double dx)
        {
            double c = Canvas.GetLeft(h);
            c += dx;
            c = Math.Min(h.Bounds.Width - HANDLE_HALF_WIDTH, Math.Max(-HANDLE_HALF_WIDTH, c));
            Canvas.SetLeft(h, c);
            return c;
        }

        void InitBase()
        {
            handles = new List<GradientHandle>();
            AddHandle(0, new MVector(0, 0, 0, 1));
            AddHandle(1, new MVector(1, 1, 1, 1));
        }

        void InitHandles()
        {
            try
            {
                handleHolder.Children.Clear();
                handles = new List<GradientHandle>();

                object obj = property?.GetValue(propertyOwner);

                if (obj is Gradient)
                {
                    Gradient g = (Gradient)obj;

                    if (g != null && g.positions != null && g.colors != null && g.positions.Length == g.colors.Length)
                    {
                        for (int i = 0; i < g.positions.Length; ++i)
                        {
                            AddHandle(g.positions[i], g.colors[i]);
                        }
                    }
                    else
                    {
                        AddHandle(0, new MVector(0, 0, 0, 1));
                        AddHandle(1, new MVector(1, 1, 1, 1));
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }

        void AddHandle(float p, MVector c, bool updateProperty = false)
        {
            GradientHandle h = new GradientHandle();
            h.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left;
            h.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Top;
            h.Position = p;
            h.SetColor(c);
            h.PointerPressed += H_PointerPressed;
            h.OnColorChanged += H_OnColorChanged;
            handles.Add(h);
            handleHolder.Children.Add(h);
            UpdatePreview(updateProperty);
        }

        void AddHandle(Point p, MVector c, bool updateProperty = false)
        {
            GradientHandle h = new GradientHandle();
            h.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left;
            h.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Top;
            h.Position = MathF.Min(1, MathF.Max(0, (float)((p.X + HANDLE_HALF_WIDTH) / handleHolder.Bounds.Width)));
            h.SetColor(c);
            h.PointerPressed += H_PointerPressed;
            h.OnColorChanged += H_OnColorChanged;
            handles.Add(h);
            handleHolder.Children.Add(h);
            Canvas.SetLeft(h, h.Position * handleHolder.Bounds.Width - HANDLE_HALF_WIDTH);
            UpdatePreview(updateProperty);
        }

        void OnSizeChanged()
        {
            for(int i = 0; i < handles.Count; ++i)
            {
                GradientHandle h = handles[i];
                Canvas.SetLeft(h, h.Position * handleHolder.Bounds.Width - HANDLE_HALF_WIDTH);
            }

            bmp = new RawBitmap((int)imageWrapper.Bounds.Width, (int)imageWrapper.Bounds.Height);
            UpdatePreview(false);
        }

        private void H_OnColorChanged(GradientHandle handle)
        {
            UpdatePreview(true);
        }

        private void H_PointerPressed(object sender, PointerPressedEventArgs e)
        {
            GradientHandle h = sender as GradientHandle;
            if(target != null)
            {
                return;
            }

            if (e.KeyModifiers == KeyModifiers.Alt)
            {
                if (handles.Count > 2)
                {
                    handleHolder.Children.Remove(h);
                    handles.Remove(h);
                    UpdatePreview(true);
                }
            }
            else
            {
                target = h;
                mouseStart = e.GetPosition(handleHolder);
                SubscribeToWindowPointer();
            }
        }

        void UpdatePreview(bool updateProperty = false)
        {
            if (handles.Count < 2)
            {
                return;
            }

            if (bmp == null || bmp.Width == 0 || bmp.Height == 0)
            {
                bmp = new RawBitmap((int)imageWrapper.Bounds.Width, (int)imageWrapper.Bounds.Height);
            }

            if (bmp.Width == 0 || bmp.Height == 0)
            {
                return;
            }

            handles.Sort((a, b) =>
            {
                if (a.Position < b.Position)
                {
                    return -1;
                }
                else if (a.Position > b.Position)
                {
                    return 1;
                }

                return 0;
            });

            try
            {
                float[] pos = new float[handles.Count];
                MVector[] cols = new MVector[handles.Count];

                Task.Run(() =>
                {
                    for (int i = 0; i < handles.Count; ++i)
                    {
                        pos[i] = handles[i].Position;
                        cols[i] = handles[i].SelectedColor;
                    }

                    Materia.Nodes.Helpers.Utils.CreateGradient(bmp, pos, cols);
                }).ContinueWith(t =>
                {
                    if (updateProperty)
                    {
                        Gradient grad = new Gradient();
                        grad.colors = cols;
                        grad.positions = pos;

                        property?.SetValue(propertyOwner, grad);
                    }
                    gradientImage.Source = bmp.ToAvBitmap();
                }, TaskScheduler.FromCurrentSynchronizationContext());
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            gradientImage = this.FindControl<Image>("GradientImage");
            handleHolder = this.FindControl<Canvas>("HandleHolder");
            imageWrapper = this.FindControl<Grid>("ImageWrapper");
        }
    }
}
