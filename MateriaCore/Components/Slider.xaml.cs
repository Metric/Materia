using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System;
using Avalonia.Controls.Shapes;
using Avalonia.Media;

namespace MateriaCore.Components
{
    public class Slider : UserControl
    {
        private Rectangle fillRect;
        private Canvas sliderArea;

        public delegate void ValueChanged(Slider slider);
        public event ValueChanged OnValueChanged;

        protected bool snapToTicks;
        public bool SnapToTicks
        {
            get
            {
                return snapToTicks;
            }
            set
            {
                snapToTicks = value;
                if (snapToTicks && ticks != null && ticks.Length > 0)
                {
                    UpdateValueToNearestTick(Value);
                }
            }
        }

        protected float[] ticks;
        public float[] Ticks
        {
            get
            {
                return ticks;
            }
            set
            {
                ticks = value;
                //sort ticks low to high
                if (ticks != null && ticks.Length > 1)
                {
                    Array.Sort(ticks);
                }
                if (snapToTicks && ticks != null && ticks.Length > 0)
                {
                    UpdateValueToNearestTick(Value);
                }
            }
        }

        protected float minValue;
        public float MinValue
        {
            get
            {
                return minValue;
            }
            set
            {
                minValue = value;
                UpdateFillFromValue();
            }
        }

        protected float maxValue;
        public float MaxValue
        {
            get
            {
                return maxValue;
            }
            set
            {
                maxValue = value;
                UpdateFillFromValue();
            }
        }

        protected float value;
        public float Value
        {
            get
            {
                return value;
            }
            set
            {
                this.value = value;
                UpdateFillFromValue();
            }
        }

        public bool ClampToMinMax { get; set; } = false;

        bool mouseDown = false;

        public Slider()
        {
            this.InitializeComponent();
            PropertyChanged += Slider_PropertyChanged;
            if (sliderArea != null)
            {
                sliderArea.PointerPressed += OnPointerPressed;
                sliderArea.PointerReleased += OnPointerReleased;
            }
            maxValue = 1;
            minValue = 0;
            value = 0.25f;
            UpdateFillFromValue();
        }

        private void OnPointerMoved(object sender, Avalonia.Input.PointerEventArgs e)
        { 
            if (mouseDown)
            {
                Point p = e.GetPosition(sliderArea);
                UpdateValueFromPoint(ref p);
            }
        }

        private void OnPointerReleased(object sender, Avalonia.Input.PointerReleasedEventArgs e)
        {
            if (mouseDown)
            {
                UnsubscribeFromWindowPointer();
            }

            mouseDown = false;
        }

        private void OnPointerPressed(object sender, Avalonia.Input.PointerPressedEventArgs e)
        {
            if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            {
                mouseDown = true;
                SubscribeToWindowPointer();
                Point p = e.GetPosition(sliderArea);
                UpdateValueFromPoint(ref p);
            }
        }

        private void SubscribeToWindowPointer()
        {
            Window w = (Window)VisualRoot;
            if (w != null)
            {
                w.PointerMoved += OnPointerMoved;
                w.PointerReleased += OnPointerReleased;
            }
        }

        private void UnsubscribeFromWindowPointer()
        {
            Window w = (Window)VisualRoot;
            if (w != null)
            {
                w.PointerMoved -= OnPointerMoved;
                w.PointerReleased -= OnPointerReleased;
            }
        }

        private void Slider_PropertyChanged(object sender, AvaloniaPropertyChangedEventArgs e)
        {
            if (e.Property.Name == "IsEnabled" && fillRect != null)
            {
                if (!IsEnabled)
                {
                    fillRect.Fill = (SolidColorBrush)Application.Current.Resources["Overlay11"];
                }
                else
                {
                    fillRect.Fill = (SolidColorBrush)Application.Current.Resources["Primary"];
                }
            }
        }

        protected void UpdateValueFromPoint(ref Point p)
        {
            double percent = p.X / sliderArea.Bounds.Width;
            
            if (ClampToMinMax)
            {
                percent = Math.Clamp(percent, 0d, 1d);
            }

            if (snapToTicks && ticks != null && ticks.Length > 0)
            {
                float temp = (float)percent * (maxValue - minValue) + minValue;
                UpdateValueToNearestTick(temp);
                OnValueChanged?.Invoke(this);
            }
            else
            {
                value = (float)percent * (maxValue - minValue) + minValue;
                UpdateFillFromValue();
                OnValueChanged?.Invoke(this);
            }
        }

        protected void UpdateValueToNearestTick(float v)
        {
            if (ticks != null && ticks.Length > 0)
            {
                float closet = ticks[0];
                float mindist = Math.Abs(closet - v);

                for (int i = 1; i < ticks.Length; i++)
                {
                    float t = ticks[i];
                    float d = Math.Abs(t - v);
                    if (d < mindist)
                    {
                        mindist = d;
                        closet = t;
                    }
                }

                value = closet;
                UpdateFillFromValue();
            }
        }

        protected void UpdateFillFromValue()
        {
            float p = (value - minValue) / (maxValue - minValue);
            double w = sliderArea.Bounds.Width * p; //slideArea.Bounds.Width is also not working in preview for VS2019
            if (fillRect != null)
            {
                fillRect.Width = Math.Min(sliderArea.Bounds.Width, Math.Max(w, 0));
            }
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            fillRect = this.FindControl<Rectangle>("FillRect");
            sliderArea = this.FindControl<Canvas>("SliderArea");
        }
    }
}
