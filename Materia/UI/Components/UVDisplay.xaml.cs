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
using RSMI.Containers;
using OpenTK;
using Materia.Imaging;

namespace Materia.UI.Components
{
    /// <summary>
    /// Interaction logic for UVDisplay.xaml
    /// </summary>
    public partial class UVDisplay : UserControl
    {
        Mesh mesh;

        public UVDisplay()
        {
            InitializeComponent();
        }

        public void SetMesh(Mesh m)
        {
            mesh = m;
            UpdatePath();
        }

        public void UpdatePath()
        {
            var bitmap = new RawBitmap(1024, 1024);
            if (mesh != null)
            {
                if (mesh.uv != null && mesh.uv.Count == mesh.vertices.Count) {

                    Parallel.For(0, mesh.triangles.Count, i =>
                    {
                        Triangle t = mesh.triangles[i];

                        Vector2 uv0 = mesh.uv[t.u0];
                        Vector2 uv1 = mesh.uv[t.u1];
                        Vector2 uv2 = mesh.uv[t.u2];

                        Point p = new Point(uv0.X * 1024, uv0.Y * 1024);
                        Point p2 = new Point(uv1.X * 1024, uv1.Y * 1024);
                        Point p3 = new Point(uv2.X * 1024, uv2.Y * 1024);

                        bitmap.DrawLine((int)p.X, (int)p.Y, (int)p2.X, (int)p2.Y, 0, 255, 255, 255);
                        bitmap.DrawLine((int)p2.X, (int)p2.Y, (int)p3.X, (int)p3.Y, 0, 255, 255, 255);
                        bitmap.DrawLine((int)p3.X, (int)p3.Y, (int)p.X, (int)p.Y, 0, 255, 255, 255);
                    });
                }
            }
            Preview.Source = bitmap.ToImageSource();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {

        }
    }
}
