using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using System.Reflection;
using Materia.Rendering.Extensions;

namespace MateriaCore.Components
{
    public class Toggle : UserControl
    {
        public delegate void Checked(Toggle t, bool value);
        public event Checked ValueChanged;

        Button toggle;

        PropertyInfo property;
        object propertyOwner;

        public static readonly AvaloniaProperty<object> PropertyDeclaration =
            AvaloniaProperty.Register<Toggle, object>("InnerContent", inherits: true);

        public object InnerContent
        {
            get
            {
                return GetValue(PropertyDeclaration);
            }
            set
            {
                SetValue(PropertyDeclaration, value);
            }
        }

        protected bool isChecked;
        public bool IsChecked
        {
            get
            {
                return isChecked;
            }
            set
            {
                if (value != IsChecked)
                {
                    isChecked = value;
                    UpdateBrush();
                    ValueChanged?.Invoke(this, value);
                }
            }
        }

        public Toggle()
        {
            this.InitializeComponent();
            toggle.Click += Toggle_Click;
            toggle.DataContext = this;
            UpdateBrush();
        }

        public Toggle(string name, PropertyInfo p, object owner) : this()
        {
            property = p;
            propertyOwner = owner;
            toggle.Content = name;

            object v = p.GetValue(owner);
            IsChecked = v.ToBool();
        }

        private void Toggle_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            IsChecked = !isChecked;
            if (property == null)
            {
                return;
            }

            if (property.PropertyType.IsEnum)
            {
                int i = isChecked ? 1 : 0;
                property.SetValue(propertyOwner, i);
            }
            else if (property.PropertyType.Equals(typeof(float)) || property.PropertyType.Equals(typeof(double))
            || property.PropertyType.Equals(typeof(int)) || property.PropertyType.Equals(typeof(long)))
            {
                int i = isChecked ? 1 : 0;
                property.SetValue(propertyOwner, i);
            }
            else
            {
                property.SetValue(propertyOwner, isChecked);
            }
        }

        private void UpdateBrush()
        {
            if (!isChecked)
            {
                toggle.Background = (SolidColorBrush)Application.Current.Resources["Overlay5"];
            }
            else
            {
                toggle.Background = (SolidColorBrush)Application.Current.Resources["Primary"];
            }
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            toggle = this.FindControl<Button>("Toggle");
        }


    }
}
