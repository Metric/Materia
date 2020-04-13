using System;
using Avalonia;
using System.Runtime.InteropServices;

namespace Materia.WinApi
{
    public enum MouseButtons
    {
        None,
        Left,
        Right,
        Middle
    }

    public struct MouseEventArgs
    {
        public MouseButtons button;
        public int x;
        public int y;
        public PixelPoint point;
        public int clickCount;

        public MouseEventArgs(MouseButtons btn, int clicks, int px, int py)
        {
            clickCount = clicks;
            x = px;
            y = py;
            point = new PixelPoint(x, y);
            button = btn;
        }
    }

    public class MouseHook
    {
        private PixelPoint point;
        public PixelPoint Point
        {
            get { return point; }
            protected set
            {
                if (point != value)
                {
                    point = value;
                    if (MouseMoveEvent != null)
                    {
                        var e = new MouseEventArgs(MouseButtons.None, 0, point.X, point.Y);
                        MouseMoveEvent(this, e);
                    }
                }
            }
        }
        private int hHook;
        private const int WM_MOUSEMOVE = 0x200;
        private const int WM_LBUTTONDOWN = 0x201;
        private const int WM_RBUTTONDOWN = 0x204;
        private const int WM_MBUTTONDOWN = 0x207;
        private const int WM_LBUTTONUP = 0x202;
        private const int WM_RBUTTONUP = 0x205;
        private const int WM_MBUTTONUP = 0x208;
        private const int WM_LBUTTONDBLCLK = 0x203;
        private const int WM_RBUTTONDBLCLK = 0x206;
        private const int WM_MBUTTONDBLCLK = 0x209;
        public const int WH_MOUSE_LL = 14;

        bool wasDown;

        public Win32Api.HookProc hProc;
        public MouseHook()
        {
            this.Point = new PixelPoint();
        }
        public int SetHook()
        {
            hProc = new Win32Api.HookProc(MouseHookProc);
            hHook = Win32Api.SetWindowsHookEx(WH_MOUSE_LL, hProc, IntPtr.Zero, 0);
            return hHook;
        }
        public void UnHook()
        {
            Win32Api.UnhookWindowsHookEx(hHook);
        }
        private int MouseHookProc(int nCode, IntPtr wParam, IntPtr lParam)
        {
            Win32Api.MouseHookStruct MyMouseHookStruct = (Win32Api.MouseHookStruct)Marshal.PtrToStructure(lParam, typeof(Win32Api.MouseHookStruct));
            if (nCode < 0)
            {
                return Win32Api.CallNextHookEx(hHook, nCode, wParam, lParam);
            }
            else
            {
                if (MouseClickEvent != null)
                {
                    MouseButtons button = MouseButtons.None;
                    int clickCount = 0;
                    switch ((Int32)wParam)
                    {
                        case WM_LBUTTONDOWN:
                            button = MouseButtons.Left;
                            wasDown = true;
                            clickCount = 1;
                            MouseDownEvent?.Invoke(this, new MouseEventArgs(button, clickCount, point.X, point.Y));
                            break;
                        case WM_RBUTTONDOWN:
                            button = MouseButtons.Right;
                            wasDown = true;
                            clickCount = 1;
                            MouseDownEvent?.Invoke(this, new MouseEventArgs(button, clickCount, point.X, point.Y));
                            break;
                        case WM_MBUTTONDOWN:
                            button = MouseButtons.Middle;
                            wasDown = true;
                            clickCount = 1;
                            MouseDownEvent?.Invoke(this, new MouseEventArgs(button, clickCount, point.X, point.Y));
                            break;
                        case WM_LBUTTONUP:
                            button = MouseButtons.Left;
                            clickCount = 1;

                            if(wasDown)
                            {
                                wasDown = false;
                                MouseClickEvent?.Invoke(this, new MouseEventArgs(button, clickCount, point.X, point.Y));
                            }
                            
                            MouseUpEvent?.Invoke(this, new MouseEventArgs(button, clickCount, point.X, point.Y));
                            break;
                        case WM_RBUTTONUP:
                            button = MouseButtons.Right;
                            clickCount = 1;

                            if (wasDown)
                            {
                                wasDown = false;
                                MouseClickEvent?.Invoke(this, new MouseEventArgs(button, clickCount, point.X, point.Y));
                            }
                            
                            MouseUpEvent?.Invoke(this, new MouseEventArgs(button, clickCount, point.X, point.Y));
                            break;
                        case WM_MBUTTONUP:
                            button = MouseButtons.Middle;
                            clickCount = 1;

                            if (wasDown)
                            {
                                wasDown = false;
                                MouseClickEvent?.Invoke(this, new MouseEventArgs(button, clickCount, point.X, point.Y));
                            }
                            
                            MouseUpEvent?.Invoke(this, new MouseEventArgs(button, clickCount, point.X, point.Y));
                            break;
                    }
                        
                }
                this.Point = new PixelPoint(MyMouseHookStruct.pt.x, MyMouseHookStruct.pt.y);
                return Win32Api.CallNextHookEx(hHook, nCode, wParam, lParam);
            }
        }

        public delegate void MouseMoveHandler(object sender, MouseEventArgs e);
        public event MouseMoveHandler MouseMoveEvent;

        public delegate void MouseClickHandler(object sender, MouseEventArgs e);
        public event MouseClickHandler MouseClickEvent;

        public delegate void MouseDownHandler(object sender, MouseEventArgs e);
        public event MouseDownHandler MouseDownEvent;

        public delegate void MouseUpHandler(object sender, MouseEventArgs e);
        public event MouseUpHandler MouseUpEvent;
    }
}
