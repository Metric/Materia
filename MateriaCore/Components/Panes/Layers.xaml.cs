using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace MateriaCore.Components.Panes
{
    public class Layers : Window
    {
        public Layers()
        {
            this.InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
