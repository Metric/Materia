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
        const double HALF_HALF = 4;

        Image gradientImage;
        Canvas handleHolder;
        Grid imageWrapper;

        List<GradientHandle> handles;

        PropertyInfo property;
        object propertyOwner;

        RawBitmap bmp;

        Point mousePosition;
        GradientHandle target;
        int clickCount = 0;
        ulong clickTimeStamp = 0;

        public GradientEditor()
        {
            this.InitializeComponent();
            handles = new List<GradientHandle>();
            PointerPressed += GradientEditor_PointerPressed;
            PointerMoved += GradientEditor_PointerMoved;
            DoubleTapped += GradientEditor_DoubleTapped;
            PropertyChanged += GradientEditor_PropertyChanged;
            InitBase();
        }

        private void GradientEditor_PointerPressed(object sender, PointerPressedEventArgs e)
        {
            mousePosition = e.GetPosition(handleHolder);
        }

        private void GradientEditor_PointerMoved(object sender, PointerEventArgs e)
        {
            mousePosition = e.GetPosition(handleHolder);
        }

        private void GradientEditor_DoubleTapped(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            GradientHandle handle = null;
            Point p = mousePosition;
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

            if (handle == null)
            {
                AddHandle(p, new MVector(0, 0, 0, 1), true);
            }
            else
            {
                AddHandle(p, handle.SelectedColor, true);
            }
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
                UpdatePreview(true);
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
            double n = (p.X + HALF_HALF) / handleHolder.Bounds.Width;
            n = Math.Clamp(n, 0d, 1d);
            double real = n * handleHolder.Bounds.Width - HANDLE_HALF_WIDTH;
            Canvas.SetLeft(target, real);
            target.Position = (float)n;
            UpdatePreview(false);
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

                if (obj is Materia.Nodes.Containers.Gradient)
                {
                    Materia.Nodes.Containers.Gradient g = (Materia.Nodes.Containers.Gradient)obj;

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
            h.Position = MathF.Min(1, MathF.Max(0, (float)((p.X + HALF_HALF) / handleHolder.Bounds.Width)));
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

                    Materia.Rendering.Imaging.Gradient.Fill(bmp, pos, cols);
                }).ContinueWith(t =>
                {
                    if (updateProperty)
                    {
                        Materia.Nodes.Containers.Gradient grad = new Materia.Nodes.Containers.Gradient();
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
