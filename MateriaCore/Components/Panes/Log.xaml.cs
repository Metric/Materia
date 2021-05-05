using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace MateriaCore.Components.Panes
{
    public class Log : Window
    {
        public Log()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
