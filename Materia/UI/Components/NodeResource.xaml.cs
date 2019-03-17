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
            }
        }

        public NodeResource()
        {
            InitializeComponent();
        }

        private void Grid_MouseMove(object sender, MouseEventArgs e)
        {
            if(e.LeftButton == MouseButtonState.Pressed)
            {
                DragDrop.DoDragDrop(this, this, DragDropEffects.Copy);
            }
        }
    }
}
