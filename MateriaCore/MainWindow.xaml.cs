using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using MateriaCore.Components.Dialogs;
using System.Diagnostics;
using System.Threading.Tasks;

namespace MateriaCore
{
    public class MainWindow : Window
    {
        public static MainWindow Instance { get; protected set; }

        public MainWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
            Instance = this;
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
