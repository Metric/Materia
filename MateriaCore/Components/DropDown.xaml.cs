using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System.Reflection;
using Avalonia.Media;
using System;
using MLog;
using Materia.Rendering.Extensions;

namespace MateriaCore.Components
{
    public class DropDown : UserControl
    {
        private ComboBox dropDown;
        private TextBox dropDownEdit;

        PropertyInfo property;
        object propertyOwner;
        string output;
        bool initing;


        public DropDown()
        {
            InitializeComponent();
            dropDown.SelectionChanged += DropDown_SelectionChanged;
            dropDownEdit.TextInput += DropDownEdit_TextInput;
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            dropDown = this.FindControl<ComboBox>("Dropdown");
            dropDownEdit = this.FindControl<TextBox>("DropdownEdit");
        }

        private void DropDownEdit_TextInput(object sender, Avalonia.Input.TextInputEventArgs e)
        {
            if (initing)
            {
                initing = false;
                return;
            }

            UpdateProperty(dropDownEdit.Text);
        }

        private void DropDown_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(initing)
            {
                initing = false;
                return;
            }

            dropDownEdit.Text = dropDown.SelectedItem.ToString();
            UpdateProperty(dropDown.SelectedItem);
        }

        public DropDown(object[] data, object owner, PropertyInfo p, string outputProperty = null, bool isEditable = false) : this()
        {
            Set(data, owner, p, outputProperty, isEditable);
        }

        public void Set(object[] data, object owner, PropertyInfo p, string outputProperty = null, bool isEditable = false)
        {
            property = p;
            propertyOwner = owner;
            output = outputProperty;
            dropDown.Items = data;
            dropDownEdit.IsVisible = isEditable;

            InitData(data, owner, p, outputProperty);
        }

        protected void InitData(object[] data, object owner, PropertyInfo p, string outputProperty = null)
        {
            output = outputProperty;

            initing = true;

            if (p.PropertyType.IsEnum)
            {
                dropDown.SelectedIndex = Array.IndexOf(data, p.GetValue(owner).ToString());
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
                    dropDown.SelectedIndex = 0;
                    initing = false;
                    return;
                }

                int k = Array.IndexOf(data, b);

                if (k > -1)
                {
                    dropDown.SelectedIndex = k;
                }
                else
                {
                    if (b.GetType().Equals(typeof(int)) || b.GetType().Equals(typeof(float)) || b.GetType().Equals(typeof(double)) || b.GetType().Equals(typeof(long)))
                    {
                        int g = (int)b.ToFloat();

                        if (g >= 0 && g < data.Length)
                        {
                            dropDown.SelectedIndex = g;
                        }
                    }
                    else if (b.GetType().Equals(typeof(string)))
                    {
                        dropDown.SelectedItem = b;
                    }
                    else
                    {
                        dropDown.SelectedIndex = 0;
                    }
                }
            }

            initing = false;
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
                        var index = dropDown.SelectedIndex;
                        var prop = propertyOwner.GetType().GetProperty(output);
                        if (prop == null) return;
                        if (prop.PropertyType.Equals(typeof(int)) || prop.PropertyType.Equals(typeof(float)))
                        {
                            prop.SetValue(propertyOwner, index);
                        }
                        else if (prop.PropertyType.Equals(typeof(string)))
                        {
                            if (dropDown.SelectedItem != null)
                            {
                                prop.SetValue(propertyOwner, dropDown.SelectedItem.ToString());
                            }
                            else if (s is string)
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
                    property.SetValue(propertyOwner, dropDown.SelectedIndex);
                }
                catch (Exception ex)
                {
                    Log.Error(ex);
                }
            }
        }
    }
}
