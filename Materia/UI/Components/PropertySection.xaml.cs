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
    /// Interaction logic for PropertySection.xaml
    /// </summary>
    public partial class PropertySection : UserControl
    {
        protected bool collapsed;
        public bool Collapsed
        {
            get
            {
                return collapsed;
            }
            set
            {
                collapsed = value;
                if(collapsed)
                {
                    CollapseButtonRotation.Angle = -90;
                    PanelItems.Visibility = Visibility.Collapsed;
                }
                else
                {
                    CollapseButtonRotation.Angle = 90;
                    PanelItems.Visibility = Visibility.Visible;
                }
            }
        }

        public string Title
        {
            get
            {
                return LabelContent.Text;
            }
            set
            {
                LabelContent.Text = value;
            }
        }

        public PropertySection()
        {
            InitializeComponent();
            PanelItems.DataContext = this;
            Collapsed = false;
        }

        public void Insert(int index, UIElement e)
        {
            PanelItems.Children.Insert(index, e);
        }

        public void Add(UIElement e)
        {
            PanelItems.Children.Add(e);
        }

        private void CollapsedButton_Click(object sender, RoutedEventArgs e)
        {
            Collapsed = !Collapsed;
        }
    }
}
