using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Materia.Rendering.Extensions;
using System;

namespace MateriaCore.Components
{
    public class RangeSlider : UserControl
    {
        public delegate void ValueChange(RangeSlider slider, float min, float max);
        public event ValueChange OnValueChanged;

        Border minHandle;
        Border maxHandle;

        float min;
        float max;

        Point mouseStart;

        Border target;

        public float Min
        {
            get
            {
                return min;
            }
            set
            {
                min = value.Clamp(0,1);
                OnValueChanged?.Invoke(this, min, max);
            }
        }

        public float Max
        {
            get
            {
                return max;
            }
            set
            {
                max = value.Clamp(0,1);
                OnValueChanged?.Invoke(this, min, max);
            }
        }

        public RangeSlider()
        {
            this.InitializeComponent();

            minHandle.PointerPressed += MinHandle_PointerPressed;
            maxHandle.PointerPressed += MinHandle_PointerPressed;

            PropertyChanged += RangeSlider_PropertyChanged;

            Set(0, 1);
        }

        public RangeSlider(float min, float max) : this()
        {
            Set(min, max);
        }

        public void Set(float min, float max)
        {
            this.min = min.Clamp(0, 1);
            this.max = max.Clamp(0, 1);

            UpdateButtonPositions();
        }

        void UpdateButtonPositions()
        {
            double w = Bounds.Width;
            double bw = minHandle.Bounds.Width;

            double minx = Math.Min(w - bw, Math.Max(0, (w - bw) * min));
            double maxx = Math.Min(w - bw, Math.Max(0, (w - bw) * max));

            Canvas.SetLeft(minHandle, minx);
            Canvas.SetLeft(maxHandle, maxx);
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            minHandle = this.FindControl<Border>("Min");
            maxHandle = this.FindControl<Border>("Max");
        }

        private void RangeSlider_PropertyChanged(object sender, AvaloniaPropertyChangedEventArgs e)
        {
            if (e.Property.Name == "Bounds")
            {
                UpdateButtonPositions();
            }
        }

        private void MinHandle_PointerPressed(object sender, Avalonia.Input.PointerPressedEventArgs e)
        {
            OnPointerPressed(sender as Border, e.GetPosition(this));
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

        void UnsubscribeFromWindowPointer()
        {
            Window w = (Window)VisualRoot;
            if (w != null)
            {
                w.PointerMoved -= W_PointerMoved;
                w.PointerReleased -= W_PointerReleased;
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

        private void W_PointerMoved(object sender, Avalonia.Input.PointerEventArgs e)
        {
            if (target == null)
            {
                return;
            }

            double w = Bounds.Width;
            double bw = minHandle.Bounds.Width;

            Point p = e.GetPosition(this);

            double dx = p.X - mouseStart.X;
            double l = Canvas.GetLeft(target);

            l += dx;

            l = Math.Min(w - bw, Math.Max(0, l));

            if (target == minHandle)
            {
                Min = (float)Math.Max(0, l) / (float)(w - bw);
            }
            else
            {
                Max = (float)Math.Max(0, l) / (float)(w - bw);
            }

            Canvas.SetLeft(target, l);

            mouseStart = p;
        }

        void OnPointerPressed(Border b, Point p)
        {
            if(b == null || target != null)
            {
                return;
            }

            target = b;
            mouseStart = p;
            SubscribeToWindowPointer();
        }
    }
}
