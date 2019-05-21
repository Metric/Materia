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
using Materia.MathHelpers;
using Materia.UI.Components;

namespace Materia.UI.Helpers
{
    /// <summary>
    /// Interaction logic for GradientHandle.xaml
    /// </summary>
    public partial class GradientHandle : UserControl
    {
        public delegate void ColorChanged();
        public event ColorChanged OnColorChanged;

        public float Position { get; set; }

        protected MVector color;
        public MVector SColor
        {
            get
            {
                return color;
            }
            set
            {
                color = value;
            }
        }

        public GradientHandle()
        {
            InitializeComponent();
        }

        public void SetColor(MVector c)
        {
            color = c;
            SelectedColor.Background = new SolidColorBrush(Color.FromArgb((byte)(c.W * 255), (byte)(c.X * 255), (byte)(c.Y * 255), (byte)(c.Z * 255)));
        }

        public void SetColor(System.Drawing.Color c)
        {
            color.X = c.R / 255.0f;
            color.Y = c.G / 255.0f;
            color.Z = c.B / 255.0f;
            color.W = c.A / 255.0f;
        }

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if(e.ClickCount > 1)
            {
                e.Handled = true;

                ColorPicker cp = new ColorPicker(System.Drawing.Color.FromArgb((int)(color.W * 255), (int)(color.X * 255), (int)(color.Y * 255), (int)(color.Z * 255)));
                cp.ShowDialog();

                if(cp.DialogResult == true)
                {
                    var c = cp.Selected;
                    SetColor(c);

                    if(OnColorChanged != null)
                    {
                        OnColorChanged.Invoke();
                    }
                }
            }
        }
    }
}
