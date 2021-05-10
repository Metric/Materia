using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using MateriaCore.Components.Dialogs;
using OpenTK.Windowing.Desktop;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace MateriaCore
{
    public class MainWindow : Window
    {
        public static MainWindow Instance { get; protected set; }

        protected MainGLWindow glWindow;

        public MainWindow()
        {
            InitializeComponent();

            MLog.Log.File = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "log.txt");

            Closing += MainWindow_Closing;
            Instance = this;

            NativeWindowSettings settings = NativeWindowSettings.Default;
            settings.APIVersion = new Version(4, 6);
            settings.Title = "Materia";
            settings.WindowState = OpenTK.Windowing.Common.WindowState.Normal;
            settings.Size = new OpenTK.Mathematics.Vector2i(1024, 768);
            
            glWindow = new MainGLWindow(settings);
            glWindow?.Show();
        }

        public override void Render(DrawingContext context)
        {
            base.Render(context);
            glWindow?.Invalidate();
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            glWindow?.Close();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
