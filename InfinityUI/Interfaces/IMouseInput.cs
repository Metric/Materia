using Materia.Rendering.Mathematics;
using System;

namespace InfinityUI.Interfaces
{
    [Flags]
    public enum MouseButton
    {
        None = 0,
        Left = 2,
        Right = 4,
        Middle = 8,
        Down = 16,
        Up = 32
    } 

    public interface UIEventArgs
    {
        bool IsHandled { get; set; }
    }

    public class MouseEventArgs : UIEventArgs
    {
        public bool IsHandled { get; set; } = false;
        public MouseButton Button { get; set; } = MouseButton.None;
        public Vector2 Position { get; set; } = Vector2.Zero;
        public Vector2 Delta { get; set; } = Vector2.Zero;
    }

    public class MouseWheelArgs : UIEventArgs
    {
        public Vector2 Delta { get; set; } = Vector2.Zero;
        public bool IsHandled { get; set; } = false;
    }

    public interface IMouseWheel
    {
        void OnMouseWheel(MouseWheelArgs e);
    }

    public interface IMouseInput
    {
        void OnMouseClick(MouseEventArgs e);
        void OnMouseDown(MouseEventArgs e);
        void OnMouseUp(MouseEventArgs e);
        void OnMouseMove(MouseEventArgs e);
        void OnMouseLeave(MouseEventArgs e);
        void OnMouseEnter(MouseEventArgs e);
    }
}
