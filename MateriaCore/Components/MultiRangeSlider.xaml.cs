using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Materia.Rendering.Extensions;
using System;

namespace MateriaCore.Components
{
    public class MultiRangeSlider : UserControl
    {
        public delegate void ValueChange(MultiRangeSlider slider, float min, float mid, float max);
        public event ValueChange OnValueChanged;

        Border minHandle;
        Border midHandle;
        Border maxHandle;

        float min;
        float mid;
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
                min = value.Clamp(0, 1);
                OnValueChanged?.Invoke(this, min, mid, max);
            }
        }

        public float Mid
        {
            get
            {
                return mid;
            }
            set
            {
                mid = value.Clamp(0, 1);
                OnValueChanged?.Invoke(this, min, mid, max);
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
                max = value.Clamp(0, 1);
                OnValueChanged?.Invoke(this, min, mid, max);
            }
        }

        public MultiRangeSlider()
        {
            this.InitializeComponent();

            minHandle.PointerPressed += MinHandle_PointerPressed;
            midHandle.PointerPressed += MinHandle_PointerPressed;
            maxHandle.PointerPressed += MinHandle_PointerPressed;
 
            PropertyChanged += MultiRangeSlider_PropertyChanged;

            Set(0, 0.5f, 1);  
        }

        public MultiRangeSlider(float min, float mid, float max) : this()
        {
            Set(min, mid, max);
        }

        public void Set(float min, float mid, float max)
        {
            this.min = min.Clamp(0, 1);
            this.mid = mid.Clamp(0, 1);
            this.max = max.Clamp(0, 1);

            UpdateButtonPositions();
        }


        private void MinHandle_PointerPressed(object sender, Avalonia.Input.PointerPressedEventArgs e)
        {
            OnPointerPressed(sender as Border, e.GetPosition(this));
        }

        void UpdateButtonPositions()
        {
            double w = Bounds.Width;
            double bw = minHandle.Bounds.Width;

            double minx = Math.Max(0, (w - bw) * min);
            double midx = Math.Max(0, (w - bw) * mid);
            double maxx = Math.Max(0, (w - bw) * max);

            Canvas.SetLeft(minHandle, minx);
            Canvas.SetLeft(midHandle, midx);
            Canvas.SetLeft(maxHandle, maxx);
        }

        private void MultiRangeSlider_PropertyChanged(object sender, AvaloniaPropertyChangedEventArgs e)
        {
            if (e.Property.Name == "Bounds")
            {
                UpdateButtonPositions();
            }
        }

        void OnPointerPressed(Border b, Point p)
        {
            if (b == null || target != null)
            {
                return;
            }

            target = b;
            mouseStart = p;
            SubscribeToWindowPointer();
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
            else if(target == midHandle)
            {
                Mid = (float)(Math.Max(0, l) / (float)(w - bw));
            }
            else
            {
                Max = (float)Math.Max(0, l) / (float)(w - bw);
            }

            Canvas.SetLeft(target, l);

            mouseStart = p;
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            minHandle = this.FindControl<Border>("Min");
            midHandle = this.FindControl<Border>("Mid");
            maxHandle = this.FindControl<Border>("Max");
        }
    }
}
