using InfinityUI.Core;
using Materia.Rendering.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;

namespace InfinityUI.Interfaces
{
    public class DrawEvent : UIEventArgs
    {
        public bool IsHandled { get; set; }

        public Matrix4 projection;
        public UIObject previous;

        public DrawEvent Copy()
        {
            return new DrawEvent
            {
                projection = projection
            };
        }
    }
    public interface IDrawable : ILayout
    {
        public bool Clip { get; }
        public UIObject Parent { get; }
        void Draw(DrawEvent e);
    }
}
