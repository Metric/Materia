using System;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace Materia
{
    /// <summary>
    /// Interaction logic for NodeResource.xaml
    /// </summary>
    public partial class NodeResource : UserControl
    {
        [Category("My Props")]
        [Description("Node type that this resource represents")]
        [DisplayName("Type")]
        public string Type
        {
            get; set;
        }

        [Category("My Props")]
        [Description("Title of the Resource")]
        [DisplayName("Title")]
        public string Title
        {
            get
            {
                return NodeName.Text;
            }
            set
            {
                NodeName.Text = value;
                LoadIcon();
            }
        }

        [Category("My Props")]
        [Description("The path structure")]
        [DisplayName("Path")]
        public string Path
        {
            get; set;
        }

        public NodeResource()
        {
            InitializeComponent();
        }

        public NodeResource(NodeResource r)
        {
            InitializeComponent();
            Type = r.Type;
            Title = r.Title;
            Path = r.Path;
            ToolTip = r.ToolTip;
        }

        public NodeResource Clone()
        {
            return new NodeResource(this);
        }

        private void Grid_MouseMove(object sender, MouseEventArgs e)
        {
            if(e.LeftButton == MouseButtonState.Pressed)
            {
                DragDrop.DoDragDrop(this, this, DragDropEffects.Copy);
            }
        }

        private void UserControl_MouseEnter(object sender, MouseEventArgs e)
        {
            GridView.Background = new SolidColorBrush(Color.FromArgb(255, 32, 32, 32));
        }

        private void UserControl_MouseLeave(object sender, MouseEventArgs e)
        {
            GridView.Background = new SolidColorBrush(Colors.Transparent);
        }

        private void LoadIcon()
        {
            string path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Icons", "Shelf", Title + ".png");
            if(System.IO.File.Exists(path))
            {
                Icon.Source = new BitmapImage(new Uri(path));
            }
        }
    }
}
