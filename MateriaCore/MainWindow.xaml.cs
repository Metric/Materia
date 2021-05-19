using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
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

            Instance = this;

            //set TKGL as default GL Interface
            new TKGL();

            NativeWindowSettings settings = NativeWindowSettings.Default;
            settings.APIVersion = new Version(4, 6);
            settings.Title = "Materia";
            settings.WindowState = OpenTK.Windowing.Common.WindowState.Maximized;
            settings.Size = new OpenTK.Mathematics.Vector2i(1024, 768);

            glWindow = new MainGLWindow(settings);
            glWindow?.Show();

            //load fontmanager fonts here
            Materia.Rendering.Fonts.FontManager.GetAvailableFonts();

            updateTimer = new DispatcherTimer(DispatcherPriority.Normal)
            {
                Interval = new TimeSpan(0, 0, 0, 0, 1)
            };
            updateTimer.Tick += UpdateTimer_Tick;
            updateTimer.Start();

            Opened += MainWindow_Opened;
        }

        private void MainWindow_Opened(object sender, EventArgs e)
        {
            Task.Delay(1000).ContinueWith(t =>
            {
                Hide();
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        private void UpdateTimer_Tick(object sender, EventArgs e)
        {
            glWindow?.Process();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
