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
using Materia.Nodes.Helpers;

namespace Materia
{
    /// <summary>
    /// Interaction logic for EnumDropDown.xaml
    /// </summary>
    public partial class DropDown : UserControl
    {
        private static ILogger Log = LogManager.GetCurrentClassLogger();

        private static SolidColorBrush LightGrayColor = new SolidColorBrush(Colors.LightGray);

        PropertyInfo property;
        object propertyOwner;
        string output;

        bool isIniting;

        public DropDown()
        {
            InitializeComponent();
        }

        public DropDown(object[] data, object owner, PropertyInfo p, string outputProperty = null, bool isEditable = false)
        {
            InitializeComponent();
            property = p;
            propertyOwner = owner;
            Dropdown.ItemsSource = data;

            Dropdown.IsEditable = isEditable;

            InitData(data, owner, p, outputProperty);
        }

        public void Set(object[] data, object owner, PropertyInfo p, string outputProperty = null, bool isEditable = false)
        {
            property = p;
            propertyOwner = owner;
            Dropdown.ItemsSource = data;

            Dropdown.IsEditable = isEditable;

            InitData(data, owner, p, outputProperty);
        }

        protected void InitData(object[] data, object owner, PropertyInfo p, string outputProperty = null)
        {
            output = outputProperty;

            isIniting = true;

            if (p.PropertyType.IsEnum)
            {
                Dropdown.SelectedIndex = Array.IndexOf(data, p.GetValue(owner).ToString());
            }
            else
            {
                object b = property.GetValue(owner);

                if (data == null && b is object[])
                {
                    data = (object[])b;
                }

                if (!string.IsNullOrEmpty(output))
                {
                    try
                    {
                        var prop = propertyOwner.GetType().GetProperty(output);
                        if (prop == null) return;
                        if (prop.PropertyType.Equals(typeof(int)) || prop.PropertyType.Equals(typeof(float)) || prop.PropertyType.Equals(typeof(string)))
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
                    Dropdown.SelectedIndex = 0;
                    isIniting = false;
                    return;
                }

                int k = Array.IndexOf(data, b);

                if (k > -1)
                {
                    Dropdown.SelectedIndex = k;
                }
                else
                {
                    if (b.GetType().Equals(typeof(int)) || b.GetType().Equals(typeof(float)) || b.GetType().Equals(typeof(double)) || b.GetType().Equals(typeof(long)))
                    {
                        int g = (int)Utils.ConvertToFloat(b);

                        if (g >= 0 && g < data.Length)
                        {
                            Dropdown.SelectedIndex = g;
                        }
                    }
                    else if (b.GetType().Equals(typeof(string)))
                    {
                        Dropdown.SelectedItem = b;
                    }
                    else
                    {
                        Dropdown.SelectedIndex = 0;
                    }
                }
            }

            isIniting = false;
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            try
            { 
                Dropdown.ApplyTemplate();
                TextBox tb = Dropdown.Template.FindName("PART_EditableTextBox", Dropdown) as TextBox;
                if (tb != null)
                {
                    tb.CaretBrush = (SolidColorBrush)Application.Current.Resources["Primary"];
                    tb.Foreground = Dropdown.Foreground;
                    tb.Background = Dropdown.Background;
                    tb.TextChanged += Tb_TextChanged;
                }
            }
            catch {}
        }

        private void Tb_TextChanged(object sender, TextChangedEventArgs e)
        {
            if(isIniting)
            {
                return;
            }

            TextBox tb = sender as TextBox;
            UpdateProperty(tb.Text);
        }

        private void Dropdown_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(isIniting)
            {
                return;
            }

            UpdateProperty(Dropdown.SelectedValue);
        }

        private void UpdateProperty(object s)
        {
            if (!IsEnabled) return;

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
            else if (property.PropertyType.Equals(typeof(string[])))
            {
                if (!string.IsNullOrEmpty(output))
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
                        else if (prop.PropertyType.Equals(typeof(string)))
                        {
                            if (Dropdown.SelectedItem != null)
                            {
                                prop.SetValue(propertyOwner, Dropdown.SelectedItem.ToString());
                            }
                            else if(s is string)
                            {
                                prop.SetValue(propertyOwner, s);
                            }
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
