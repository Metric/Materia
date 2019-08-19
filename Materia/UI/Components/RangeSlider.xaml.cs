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
    /// Interaction logic for RangeSlider.xaml
    /// </summary>
    public partial class RangeSlider : UserControl
    {
        float minValue;
        float midValue;
        float maxValue;

        public delegate void ValueChangeEvent(object sender, float min, float max);
        public event ValueChangeEvent OnValueChanged;

        Point start;

        double midx = 0;
        double minx = 0;
        double maxx = 0;

        object target;

        public float MinValue
        {
            get
            {
                return minValue;
            }
            set
            {
                minValue = Math.Min(1, Math.Max(0, value));

                if (OnValueChanged != null)
                {
                    OnValueChanged.Invoke(this, minValue, maxValue);
                }
            }
        }

        public float MaxValue
        {
            get
            {
                return maxValue;
            }
            set
            {
                maxValue = Math.Min(1, Math.Max(0, value));

                if (OnValueChanged != null)
                {
                    OnValueChanged.Invoke(this, minValue, maxValue);
                }
            }
        }

        public RangeSlider()
        {
            InitializeComponent();
            minValue = 0;
            maxValue = 1;
            midValue = 0.5f;
            SetButtonPositions();
        }

        public RangeSlider(float min, float max)
        {
            InitializeComponent();
            minValue = Math.Min(1, Math.Max(0, min));
            maxValue = Math.Min(1, Math.Max(0, max));
            SetButtonPositions();
        }

        public void Set(float min, float max)
        {
            minValue = Math.Min(1, Math.Max(0, min));
            maxValue = Math.Min(1, Math.Max(0, max));

            SetButtonPositions();
        }

        public void SetButtonPositions()
        {
            double w = GridView.ActualWidth;
            double bw = Min.ActualWidth;

            minx = Math.Max(0, (w - bw) * minValue);
            maxx = Math.Max(0, (w - bw) * maxValue);

            Min.RenderTransform = new TranslateTransform(minx, 0);
            Max.RenderTransform = new TranslateTransform(maxx, 0);
        }

        private void Min_MouseMove(object sender, MouseEventArgs e)
        {
            if (target == Min)
            {
                e.Handled = true;
                double w = GridView.ActualWidth;
                double bw = Min.ActualWidth;
                Point p = e.GetPosition(GridView);
                double dx = p.X - start.X;
                double l = minx;

                l += dx;

                l = Math.Min(w - bw, Math.Max(0, l));

                MinValue = (float)Math.Max(0, l) / (float)(w - bw);

                minx = l;

                Min.RenderTransform = new TranslateTransform(minx, 0);

                start = p;
            }
        }

        private void Max_MouseMove(object sender, MouseEventArgs e)
        {
            if (target == Max)
            {
                e.Handled = true;
                double w = GridView.ActualWidth;
                double bw = Min.ActualWidth;
                Point p = e.GetPosition(GridView);
                double dx = p.X - start.X;
                double l = maxx;

                l += dx;

                l = Math.Min(w - bw, Math.Max(0, l));

                MaxValue = (float)Math.Max(0, l) / (float)(w - bw);

                maxx = l;

                Max.RenderTransform = new TranslateTransform(maxx, 0);

                start = p;
            }
        }

        private void Min_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                start = e.GetPosition(GridView);
                target = sender;
            }
        }

        private void GridView_MouseUp(object sender, MouseButtonEventArgs e)
        {
            target = null;
        }

        private void GridView_MouseMove(object sender, MouseEventArgs e)
        {
            if (target == Min)
            {
                Min_MouseMove(sender, e);
            }
            else if (target == Max)
            {
                Max_MouseMove(sender, e);
            }
        }

        private void GridView_MouseLeave(object sender, MouseEventArgs e)
        {
            target = null;
        }
    }
}
