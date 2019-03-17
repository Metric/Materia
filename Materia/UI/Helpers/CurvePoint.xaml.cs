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

namespace Materia.UI.Helpers
{
    /// <summary>
    /// Interaction logic for CurvePoint.xaml
    /// </summary>
    public partial class CurvePoint : UserControl
    {
        protected Point normalized;
        public Point Normalized
        {
            get
            {
                return normalized;
            }
            set
            {
                normalized = value;
                //update position
                UpdateViewPosition();
                Relayout();
            }
        }

        protected Point position;
        public Point Position
        {
            get
            {
                return position;
            }
            set
            {
                position = value;
                UpdateNormalized();
                Relayout();
            }
        }

        UICurves CurveView { get; set; }

        public CurvePoint()
        {
            InitializeComponent();
        }

        public CurvePoint(UICurves parent)
        {
            InitializeComponent();
            CurveView = parent;
        }

        void UpdateNormalized()
        {
            normalized.X = ((position.X + 2) / (CurveView.CurveView.ActualWidth - 1));
            normalized.Y = ((position.Y + 2) / (CurveView.CurveView.ActualHeight - 1));
        }

        public void UpdateViewPosition()
        {
            position = new Point(normalized.X * (CurveView.CurveView.ActualWidth - 1) - 4, normalized.Y * (CurveView.CurveView.ActualHeight - 1) - 4);
        }

        public void Relayout()
        {
            TranslateTransform tt = new TranslateTransform(position.X, position.Y);
            RenderTransform = tt;
        }
    }
}
