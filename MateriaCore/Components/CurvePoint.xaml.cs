using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Materia.Rendering.Mathematics;

namespace MateriaCore.Components
{
    public class CurvePoint : UserControl
    {

        protected PointD normalized;
        public PointD Normalized
        {
            get
            {
                return normalized;
            }
            set
            {
                normalized = value;
                UpdatePosition();
            }
        }

        protected PointD position;
        public PointD Position
        {
            get
            {
                return position;
            }
            set
            {
                position = value;
                UpdateNormalized();
                UpdatePosition();
            }
        }

        Curves CurveView { get; set; }

        public CurvePoint()
        {
            this.InitializeComponent();
        }

        public CurvePoint(Curves parent) : this()
        {
            CurveView = parent;
        }

        void UpdateNormalized()
        {
            if (CurveView == null)
            {
                return;
            }

            normalized.x = ((position.x + 2) / (CurveView.View.Bounds.Width - 1));
            normalized.y = ((position.y + 2) / (CurveView.View.Bounds.Height - 1));
        }

        public void UpdatePosition()
        {
            if(CurveView == null)
            {
                return;
            }

            position = new PointD(normalized.x * (CurveView.View.Bounds.Width - 1) - 4, normalized.y * (CurveView.View.Bounds.Height - 1) - 4);
            Canvas.SetLeft(this, position.x);
            Canvas.SetTop(this, position.y);
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
