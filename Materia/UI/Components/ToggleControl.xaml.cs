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
using System.Reflection;

namespace Materia.UI.Components
{
    /// <summary>
    /// Interaction logic for Toggle.xaml
    /// </summary>
    public partial class ToggleControl : UserControl
    {
        PropertyInfo property;
        object propertyOwner;

        public ToggleControl()
        {
            InitializeComponent();
        }

        public ToggleControl(string name, PropertyInfo p, object owner)
        {
            InitializeComponent();
            property = p;
            propertyOwner = owner;
            Toggle.Content = name;
            Toggle.IsChecked = (bool)p.GetValue(owner);
        }

        private void Toggle_Click(object sender, RoutedEventArgs e)
        {
            property.SetValue(propertyOwner, Toggle.IsChecked);
        }
    }
}
