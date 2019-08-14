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
using Materia.Nodes.Helpers;
using Materia.MathHelpers;

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

            object v = p.GetValue(owner);
            if (Utils.IsNumber(v) || v is bool)
            {
                Toggle.IsChecked = Convert.ToBoolean(v);
            }
            else if(v is MVector)
            {
                Toggle.IsChecked = Utils.VectorToBool(v);
            }
            else
            {
                Toggle.IsChecked = false;
            }
        }

        private void Toggle_Click(object sender, RoutedEventArgs e)
        {
            if (property == null) return;
            if (property.PropertyType.IsEnum)
            {
                int i = 0;
                if (Toggle.IsChecked != null)
                {
                    i = Toggle.IsChecked.Value ? 1 : 0;
                }
               
                property.SetValue(propertyOwner, i);
            }
            else if (property.PropertyType.Equals(typeof(float)) || property.PropertyType.Equals(typeof(double))
                || property.PropertyType.Equals(typeof(int)) || property.PropertyType.Equals(typeof(long)))
            {
                int i = 0;
                if (Toggle.IsChecked != null)
                {
                    i = Toggle.IsChecked.Value ? 1 : 0;
                }

                property.SetValue(propertyOwner, i);
            }
            else
            {
                property.SetValue(propertyOwner, Toggle.IsChecked);
            }
        }
    }
}
