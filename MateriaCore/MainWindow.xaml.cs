using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Threading;
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

        protected DispatcherTimer updateTimer;

        public MainWindow()
        {
            InitializeComponent();
            MLog.Log.File = "log.txt";

            Closing += MainWindow_Closing;
            Instance = this;

            //set TKGL as default GL Interface
            new TKGL();

            NativeWindowSettings settings = NativeWindowSettings.Default;
            settings.APIVersion = new Version(4, 6);
            settings.Title = "Materia";
            settings.WindowState = OpenTK.Windowing.Common.WindowState.Normal;
            settings.Size = new OpenTK.Mathematics.Vector2i(1024, 768);

            glWindow = new MainGLWindow(settings);
            glWindow?.Show();

            updateTimer = new DispatcherTimer(DispatcherPriority.Render)
            {
                Interval = new TimeSpan(0, 0, 0, 0, 16)
            };
            updateTimer.Tick += UpdateTimer_Tick;
            updateTimer.Start();
        }

        private void UpdateTimer_Tick(object sender, EventArgs e)
        {
            Dispatcher.UIThread.Post(() =>
            {
                if (glWindow == null || glWindow.IsExiting) return;
                glWindow.Process();
            });
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            glWindow?.Close();
            glWindow?.Dispose();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
