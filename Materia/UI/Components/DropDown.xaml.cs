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

namespace Materia
{
    /// <summary>
    /// Interaction logic for EnumDropDown.xaml
    /// </summary>
    public partial class DropDown : UserControl, IParameter
    {
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
                        Console.WriteLine(ex.StackTrace);
                    }
                }

                if (b == null || data == null)
                {
                    Dropdown.SelectedIndex = 0;
                    return;
                }

                int k = Array.IndexOf(data, b);

                if(k > -1)
                {
                    Dropdown.SelectedIndex = k;
                }
                else
                {
                    if(b.GetType().Equals(typeof(int)))
                    {
                        int g = (int)b;

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
                    Console.WriteLine(ex.StackTrace);
                }
            }
            else if(property.PropertyType.Equals(typeof(float)))
            {
                if (s.GetType().Equals(typeof(float)))
                {
                    try
                    {
                        property.SetValue(propertyOwner, s);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.StackTrace);
                    }
                }
                else
                {
                    float v = 0;
                    if (float.TryParse(s.ToString(), out v))
                    {
                        try
                        {
                            property.SetValue(propertyOwner, v);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.StackTrace);
                        }
                    }
                    else
                    {
                        try
                        {
                            var index = Dropdown.SelectedIndex;
                            property.SetValue(propertyOwner, (float)index);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.StackTrace);
                        }
                    }
                }
            }
            else if(property.PropertyType.Equals(typeof(int)))
            {
                if (s.GetType().Equals(typeof(int)))
                {
                    try
                    {
                        property.SetValue(propertyOwner, s);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.StackTrace);
                    }
                }
                else
                {
                    int t = 0;
                    if (int.TryParse(s.ToString(), out t))
                    { 
                        try
                        {
                            property.SetValue(propertyOwner, t);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.StackTrace);
                        }
                    }
                    else
                    {
                        try
                        {
                            var index = Dropdown.SelectedIndex;
                            property.SetValue(propertyOwner, index);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.StackTrace);
                        }
                    }
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
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.StackTrace);
                    }

                }
            }
        }
    }
}
