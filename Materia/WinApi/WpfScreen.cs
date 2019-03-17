using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using System.Windows;

namespace Materia.WinApi
{
    public class WpfScreen
    {
        public static IEnumerable<WpfScreen> AllScreens()
        {
            foreach (Screen screen in System.Windows.Forms.Screen.AllScreens)
            {
                yield return new WpfScreen(screen);
            }
        }

        public static WpfScreen GetScreenFrom(System.Windows.Point point)
        {
            int x = (int)Math.Round(point.X);
            int y = (int)Math.Round(point.Y);

            // are x,y device-independent-pixels ??
            System.Drawing.Point drawingPoint = new System.Drawing.Point(x, y);
            Screen screen = System.Windows.Forms.Screen.FromPoint(drawingPoint);
            WpfScreen wpfScreen = new WpfScreen(screen);

            return wpfScreen;
        }

        public static Screen Primary
        {
            get { return Screen.PrimaryScreen; }
        }

        private readonly Screen screen;

        internal WpfScreen(System.Windows.Forms.Screen screen)
        {
            this.screen = screen;
        }

        public Rectangle DeviceBounds
        {
            get { return this.screen.Bounds; }
        }

        public Rectangle WorkingArea
        {
            get { return this.screen.WorkingArea; }
        }

        //get device independent rect from given rect
        public Rectangle GetRect(Rectangle value)
        {
            double wf = (double)WorkingArea.Width / Primary.WorkingArea.Width;
            double hf = (double)WorkingArea.Height / Primary.WorkingArea.Height;

            // should x, y, width, height be device-independent-pixels ??
            return new Rectangle
            {
                X = (int)(value.X * wf),
                Y = (int)(value.Y * hf),
                Width = (int)(value.Width * wf),
                Height = (int)(value.Height * hf)
            };
        }

        public bool IsPrimary
        {
            get { return this.screen.Primary; }
        }

        public string DeviceName
        {
            get { return this.screen.DeviceName; }
        }
    }
}
