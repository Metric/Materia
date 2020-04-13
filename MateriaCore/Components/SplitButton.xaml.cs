using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace MateriaCore.Components
{
    public class SplitButton : UserControl
    {
        public delegate void ClickEvent(object sender, RoutedEventArgs e);
        public event ClickEvent Click;

        Button primary;
        Button secondary;

        public static readonly AvaloniaProperty<object> PropertyDeclaration =
            AvaloniaProperty.Register<SplitButton, object>("InnerContent", inherits: true);

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

        public SplitButton()
        {
            this.InitializeComponent();
            primary.Click += Primary_Click;
            primary.DataContext = this;
            secondary.Click += Secondary_Click;
            secondary.DataContext = this;
        }

        private void Secondary_Click(object sender, RoutedEventArgs e)
        {
            ContextMenu?.Open(secondary);
        }

        private void Primary_Click(object sender, RoutedEventArgs e)
        {
            Click?.Invoke(this, e);
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            primary = this.FindControl<Button>("Primary");
            secondary = this.FindControl<Button>("Secondary");
        }
    }
}
