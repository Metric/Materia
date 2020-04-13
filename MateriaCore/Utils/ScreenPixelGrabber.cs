using Materia.WinApi;
using Avalonia.Threading;
using D = System.Drawing;
using Avalonia;
using System;

namespace MateriaCore.Utils
{
    public class ScreenPixelGrabber : IDisposable
    {
        public delegate void Grabbed(ref D.Color c);
        public event Grabbed OnGrabbed;
        public event Action OnClick;

        DispatcherTimer pickingTimer;
        MouseHook hook;
        
        public bool IsGrabbing
        {
            get; protected set;
        }

        /// <summary>
        /// Delay is in milliseconds
        /// </summary>
        /// <param name="delay"></param>
        public ScreenPixelGrabber(int delay = 25)
        {
            pickingTimer = new DispatcherTimer();
            pickingTimer.Interval = new TimeSpan(0, 0, 0, 0, delay);
            pickingTimer.Tick += PickingTimer_Tick;
            pickingTimer.Stop();

            hook = new MouseHook();
            hook.MouseClickEvent += Hook_MouseClickEvent;
        }

        private void Hook_MouseClickEvent(object sender, MouseEventArgs e)
        {
            Pick(ref e.point);
            Stop();
            OnClick?.Invoke();
        }

        private void PickingTimer_Tick(object sender, EventArgs e)
        {
            PixelPoint p = hook.Point;
            Pick(ref p);
        }

        public void Start()
        {
            if (IsGrabbing)
            {
                return;
            }

            IsGrabbing = true;
            pickingTimer?.Start();
            hook?.SetHook();
        }

        public void Stop()
        {
            if(!IsGrabbing)
            {
                return;
            }

            IsGrabbing = false;
            pickingTimer?.Stop();
            hook?.UnHook();
        }

        protected void Pick(ref PixelPoint p)
        {
            D.Color c = GetColorAt(p.X, p.Y);
            OnGrabbed?.Invoke(ref c);
        }

        D.Bitmap bmp = new D.Bitmap(16, 16, D.Imaging.PixelFormat.Format32bppArgb);
        protected D.Color GetColorAt(int x, int y)
        {
            D.Rectangle bounds = new D.Rectangle(x - 8, y - 8, 16, 16);

            using (D.Graphics gdest = D.Graphics.FromImage(bmp))
            {
                gdest.CopyFromScreen(bounds.Location, D.Point.Empty, bounds.Size);
            }

            return bmp.GetPixel(7, 7);
        }

        public void Dispose()
        {
            Stop();

            if(bmp != null)
            {
                bmp.Dispose();
                bmp = null;
            }
        }
    }
}
