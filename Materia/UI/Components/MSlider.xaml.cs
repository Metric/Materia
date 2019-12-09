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

namespace Materia.UI.Components
{
    /// <summary>
    /// Interaction logic for MSlider.xaml
    /// </summary>
    public partial class MSlider : UserControl
    {
        public delegate void ValueChanged(MSlider slider);
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

        bool mouseDown = false;

        public MSlider()
        {
            InitializeComponent();
            IsEnabledChanged += MSlider_IsEnabledChanged;
            maxValue = 1;
            minValue = 0;
            value = 0;
        }

        private void MSlider_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (!IsEnabled)
            {
                FillRect.Fill = (SolidColorBrush)Application.Current.Resources["Overlay11"];
            }
            else
            {
                FillRect.Fill = (SolidColorBrush)Application.Current.Resources["Primary"];
            }
        }

        private void Canvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            mouseDown = true;
            Point p = e.GetPosition(this);
            UpdateValueFromPoint(ref p);
            Materia.UI.Helpers.MateriaInputManager.OnMouseMove += MateriaInputManager_OnMouseMove;
            Materia.UI.Helpers.MateriaInputManager.OnMouseUp += MateriaInputManager_OnMouseUp;
        }

        private void MateriaInputManager_OnMouseUp(MouseButtonEventArgs e)
        {
            Materia.UI.Helpers.MateriaInputManager.OnMouseMove -= MateriaInputManager_OnMouseMove;
            Materia.UI.Helpers.MateriaInputManager.OnMouseUp -= MateriaInputManager_OnMouseUp;

            mouseDown = false;
        }

        private void MateriaInputManager_OnMouseMove(MouseEventArgs e)
        {
            if (mouseDown)
            {
                Point p = e.GetPosition(this);
                if (p.X >= 0 && p.X <= ActualWidth)
                {
                    UpdateValueFromPoint(ref p);
                }
            }
        }

        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (mouseDown)
            {
                Point p = e.GetPosition(this);
                UpdateValueFromPoint(ref p);
            }
        }

        protected void UpdateValueFromPoint(ref Point p)
        {
            double percent = p.X / ActualWidth;
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
                
                for(int i = 1; i < ticks.Length; i++)
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
            double w = ActualWidth * Math.Abs(p);
            FillRect.Width = Math.Max(w, 1);
        }

        private void Canvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (mouseDown)
            {
                Materia.UI.Helpers.MateriaInputManager.OnMouseMove -= MateriaInputManager_OnMouseMove;
                Materia.UI.Helpers.MateriaInputManager.OnMouseUp -= MateriaInputManager_OnMouseUp;
            }

            mouseDown = false;
        }

        private void Canvas_MouseEnter(object sender, MouseEventArgs e)
        {
            if (mouseDown && e.LeftButton == MouseButtonState.Released)
            {
                Materia.UI.Helpers.MateriaInputManager.OnMouseMove -= MateriaInputManager_OnMouseMove;
                Materia.UI.Helpers.MateriaInputManager.OnMouseUp -= MateriaInputManager_OnMouseUp;
                mouseDown = false;
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if(snapToTicks && ticks != null && ticks.Length > 0)
            {
                UpdateValueToNearestTick(value);
            }
            else
            {
                UpdateFillFromValue();
            }
        }
    }
}
