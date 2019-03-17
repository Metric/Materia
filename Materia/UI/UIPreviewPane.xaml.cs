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
using static Materia.UILevels;
using Materia.Imaging;
using RSMI.Containers;

namespace Materia
{
    /// <summary>
    /// Interaction logic for UIPreviewPane.xaml
    /// </summary>
    public partial class UIPreviewPane : UserControl
    {
        float scale;

        UINode current;
        Point pan;

        Point start;

        double vw;
        double vh;

        public static UIPreviewPane Instance { get; protected set; }

        public UIPreviewPane()
        {
            InitializeComponent();
            pan = new Point(0, 0);
            Instance = this;
            scale = 0.5f;
        }

        public void SetMesh(Mesh m)
        {
            UVs.SetMesh(m);
        }

        //this is to make sure references are cleared
        //and released for memory purposes
        public void TryAndRemovePreviewNode(UINode n)
        {
            if(current == n)
            {
                current.Node.OnUpdate -= Node_OnUpdate;
                current = null;
            }

            PreviewView.Source = null;
        }

        public void SetPreviewNode(UINode n)
        {
            if(current != null)
            {
                current.Node.OnUpdate -= Node_OnUpdate;
            }

            current = n;
            current.Node.OnUpdate += Node_OnUpdate;

            Node_OnUpdate(n.Node);
        }

        private void Node_OnUpdate(Nodes.Node n)
        {
            int pw = Math.Min(n.Width, 4096);
            int ph = Math.Min(n.Height, 4096);

            if (n.Brush != null)
            {
                Histogram.GenerateHistograph(n.Brush);
                PreviewView.Source = n.Brush.ToImageSource();
            }
            else
            {
                byte[] src = n.GetPreview(pw, ph);

                if (src != null)
                {
                    RawBitmap bitmap = new RawBitmap(pw, ph, src);
 
                    Histogram.GenerateHistograph(bitmap);

                    PreviewView.Source = BitmapSource.Create(pw, ph, 72, 72, PixelFormats.Bgra32, null, src, pw * 4);
                }
            }

            vw = pw;
            vh = ph;

            Update();
        }

        private void ZoomHandler_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta < 0)
            {
                scale -= 0.02f;

                if(scale <= 0.1)
                {
                    scale = 0.1f;
                }
            }
            else if(e.Delta > 0)
            {
                scale += 0.02f;

                if(scale > 3)
                {
                    scale = 3.0f;
                }
            }

            UpdateZoomText();
            Update();
        }

        private void ZoomHandler_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.MiddleButton == MouseButtonState.Pressed)
            {
                Cursor = Cursors.ScrollAll;

                Point p = e.GetPosition(ZoomHandler);
                double dx = p.X - start.X;
                double dy = p.Y - start.Y;

                start = p;

                pan.X += dx;
                pan.Y += dy;

                Update();
            }
            else
            {
                Cursor = Cursors.Arrow;
            }
        }

        void Update()
        {
            double cx = ZoomHandler.ActualWidth;
            double cy = ZoomHandler.ActualHeight;
            double cx2 = Math.Min(4096, Math.Max(16, vw * scale));
            double cy2 = Math.Min(4096, Math.Max(16, vh * scale));

            PreviewView.Width = TransformArea.Width = cx2;
            PreviewView.Height = TransformArea.Height = cy2;

            Canvas.SetLeft(TransformArea, pan.X + cx * 0.5 - cx2 * 0.5);
            Canvas.SetTop(TransformArea, pan.Y + cy * 0.5 - cy2 * 0.5);

            UVArea.Width = UVs.Width = cx2;
            UVArea.Height = UVs.Height = cy2;

            Canvas.SetLeft(UVArea, pan.X + cx * 0.5 - cx2 * 0.5);
            Canvas.SetTop(UVArea, pan.Y + cy * 0.5 - cy2 * 0.5);
        }

        void UpdateZoomText()
        {
            float p = scale * 100;
            ZoomLevel.Text = String.Format("{0:0}", p) + "%";
        }

        private void ZoomHandler_MouseDown(object sender, MouseButtonEventArgs e)
        {
            start = e.GetPosition(ZoomHandler);
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            vw = 512;
            vh = 512;

            HistMode.SelectedIndex = 0;

            UpdateZoomText();
            Update();
        }

        private void FitIntoView_Click(object sender, RoutedEventArgs e)
        {
            pan.X = 0;
            pan.Y = 0;

            float minViewArea = (float)Math.Min(ZoomHandler.ActualWidth, ZoomHandler.ActualHeight);

            if(vw >= vh)
            {
                scale = minViewArea / (float)vw;
            }
            else
            {
                scale = minViewArea / (float)vh;
            }

            UpdateZoomText();
            Update();
        }

        private void ZoomOut_Click(object sender, RoutedEventArgs e)
        {
            scale -= 0.02f;

            if (scale <= 0.1)
            {
                scale = 0.1f;
            }

            UpdateZoomText();
            Update();
        }

        private void ZoomIn_Click(object sender, RoutedEventArgs e)
        {
            scale += 0.02f;

            if (scale > 3)
            {
                scale = 3.0f;
            }

            UpdateZoomText();
            Update();
        }

        private void Ratio1_Click(object sender, RoutedEventArgs e)
        {
            pan.X = 0;
            pan.Y = 0;

            scale = 1;

            UpdateZoomText();
            Update();
        }

        private void ToggleHistogram_Click(object sender, RoutedEventArgs e)
        {
            if (HistogramArea.Visibility != Visibility.Visible)
            {
                HistogramArea.Visibility = Visibility.Visible;
            }
            else
            {
                HistogramArea.Visibility = Visibility.Collapsed;
            }
        }

        private void HistMode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBoxItem item = (ComboBoxItem)HistMode.SelectedItem;
            string c = (string)item.Content;
            var mode = (LevelMode)Enum.Parse(typeof(LevelMode), c);

            Histogram.Mode = mode;
        }

        private void ToggleUV_Click(object sender, RoutedEventArgs e)
        {
            if(UVArea.Visibility != Visibility.Visible)
            {
                UVArea.Visibility = Visibility.Visible;
            }
            else
            {
                UVArea.Visibility = Visibility.Collapsed;
            }
        }

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Update();
        }
    }
}
