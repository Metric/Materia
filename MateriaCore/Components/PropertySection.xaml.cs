using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;

namespace MateriaCore.Components
{
    public class PropertySection : UserControl
    {
        Button collapsedButton;
        StackPanel panelItems;
        TextBlock labelContent;

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
                UpdateVisibility();
            }
        }

        public string Title
        {
            get
            {
                return labelContent.Text;
            }
            set
            {
                labelContent.Text = value;
            }
        }

        public PropertySection()
        {
            this.InitializeComponent();
            Collapsed = false;
            collapsedButton.Click += CollapsedButton_Click;
        }

        private void CollapsedButton_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            Collapsed = !Collapsed;
        }

        public void Insert(int index, Control c)
        {
            panelItems.Children.Insert(index, c);
        }

        public void Add(Control c)
        {
            panelItems.Children.Add(c);
        }

        void UpdateVisibility()
        {
            RotateTransform rotate = (RotateTransform)collapsedButton.RenderTransform;
            if (collapsed)
            {
                rotate.Angle = 0;
                panelItems.IsVisible = false;
            }
            else
            {
                rotate.Angle = 90;
                panelItems.IsVisible = true;
            }
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            collapsedButton = this.FindControl<Button>("CollapsedButton");
            panelItems = this.FindControl<StackPanel>("PanelItems");
            labelContent = this.FindControl<TextBlock>("LabelContent");
        }
    }
}
