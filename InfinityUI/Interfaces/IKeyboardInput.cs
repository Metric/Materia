using System;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace InfinityUI.Interfaces
{
    public class KeyboardEventArgs : UIEventArgs
    {
        public bool IsHandled { get; set; } = false;
        public string Char { get; set; }
        public Keys Key { get; set; } = Keys.A;
        public bool IsShift { get; set; } = false;
        public bool IsAlt { get; set; } = false;
        public bool IsCtrl { get; set; } = false;
        public bool IsLeftShift { get; set; } = false;
        public bool IsRightShift { get; set; } = false;
        public bool IsLeftAlt { get; set; } = false;
        public bool IsRightAlt { get; set; } = false;
        public bool IsLeftCtrl { get; set; } = false;
        public bool IsRightCtrl { get; set; } = false;

        public bool IsCapsLock { get; set; } = false;
        public Func<object> GetClipboardContent { get; set; } = null;
        public Action<object> SetClipboardContent { get; set; } = null;
    }

    public interface IKeyboardInput
    {
        void OnKeyDown(KeyboardEventArgs e);
        void OnKeyUp(KeyboardEventArgs e);
        void OnTextInput(KeyboardEventArgs e);
    }
}
