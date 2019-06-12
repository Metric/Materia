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
using NLog;

namespace Materia
{
    /// <summary>
    /// Interaction logic for EnumDropDown.xaml
    /// </summary>
    public partial class DropDown : UserControl, IParameter
    {
        private static ILogger Log = LogManager.GetCurrentClassLogger();

        PropertyInfo property;
        object propertyOwner;
        string output;

        bool isIniting;

        public DropDown()
        {
            InitializeComponent();
        }

        public DropDown(object[] data, object owner, PropertyInfo p, string outputProperty = null)
        {
            InitializeComponent();
            property = p;
            propertyOwner = owner;
            Dropdown.ItemsSource = data;

            output = outputProperty;

            isIniting = true;

            if(p.PropertyType.IsEnum)
            {
                Dropdown.SelectedIndex = Array.IndexOf(data, p.GetValue(owner).ToString());
            }
            else
            {
                
                object b = property.GetValue(owner);

                if(!string.IsNullOrEmpty(output))
                {
                    try
                    {
                        var prop = propertyOwner.GetType().GetProperty(output);
                        if (prop == null) return;
                        if (prop.PropertyType.Equals(typeof(int)) || prop.PropertyType.Equals(typeof(float)))
                        {
                            b = prop.GetValue(propertyOwner);
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex);
                    }
                }

                if (b == null || data == null)
                {
                    return;
                }

                int k = Array.IndexOf(data, b);

                if(k > -1)
                {
                    Dropdown.SelectedIndex = k;
                }
                else
                {
                    if(b.GetType().Equals(typeof(int)) || b.GetType().Equals(typeof(float)) || b.GetType().Equals(typeof(double)) || b.GetType().Equals(typeof(long)))
                    {
                        int g = (int)Convert.ToSingle(b);

                        if(g >= 0 && g < data.Length)
                        {
                            Dropdown.SelectedIndex = g;
                        }
                    }
                    else
                    {
                        Dropdown.SelectedIndex = 0;
                    }
                }
            }
        }

        public void OnUpdate(object obj)
        {
            
        }

        private void Dropdown_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(isIniting)
            {
                isIniting = false;
                return;
            }

            object s = Dropdown.SelectedValue;

            if (property.PropertyType.IsEnum)
            {
                try
                {
                    property.SetValue(propertyOwner, Enum.Parse(property.PropertyType, (string)s));
                }
                catch (Exception ex)
                {
                    Log.Error(ex);
                }
            }
            else if(property.PropertyType.Equals(typeof(string[])))
            {
                if(!string.IsNullOrEmpty(output))
                {
                    try
                    {
                        var index = Dropdown.SelectedIndex;
                        var prop = propertyOwner.GetType().GetProperty(output);
                        if (prop == null) return;
                        if (prop.PropertyType.Equals(typeof(int)) || prop.PropertyType.Equals(typeof(float)))
                        {
                            prop.SetValue(propertyOwner, index);
                        }
                        else if(prop.PropertyType.Equals(typeof(string)))
                        {
                            prop.SetValue(propertyOwner, Dropdown.SelectedItem);
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex);
                    }

                }
            }
            else
            {
                try
                {
                    property.SetValue(propertyOwner, Dropdown.SelectedIndex);
                }
                catch (Exception ex)
                {
                    Log.Error(ex);
                }
            }
        }
    }
}
