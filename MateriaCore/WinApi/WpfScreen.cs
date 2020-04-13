using Avalonia.Controls;
using Avalonia.Platform;
using Avalonia;

namespace Materia.WinApi
{
    public class WpfScreen
    {
        public static WpfScreen GetScreenFrom(Window w, PixelPoint point)
        {
            int x = point.X;
            int y = point.Y;

            Screen screen = null;

            for(int i = 0; i < w.Screens.All.Count; ++i)
            {
                Screen s = w.Screens.All[i];

                if(x >= s.WorkingArea.X && y >= s.WorkingArea.Y && x <= s.WorkingArea.Width && y <= s.WorkingArea.Height)
                {
                    screen = s;
                    break;
                }
            }

            WpfScreen wpfScreen = new WpfScreen(screen);
            return wpfScreen;
        }

        private readonly Screen screen;

        internal WpfScreen(Screen screen)
        {
            this.screen = screen;
        }

        public PixelRect DeviceBounds
        {
            get
            {
                if (screen != null)
                {
                    return screen.Bounds;
                }

                return new PixelRect();
            }
        }

        public PixelRect WorkingArea
        {
            get
            {
                if (screen != null)
                {
                    return screen.WorkingArea;
                }

                return new PixelRect();
            }
        }

        public bool IsPrimary
        {
            get { return this.screen.Primary; }
        }
    }
}
