using InfinityUI.Core;
using InfinityUI.Interfaces;
using Materia.Rendering.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;

namespace InfinityUI.Components.Layout
{
    public class UIResizable : IComponent, IMouseInput
    {
        protected enum Direction
        {
            None,
            Left,
            Right,
            Top,
            Bottom
        }

        Direction horizontalResize = Direction.None;
        Direction verticalResize = Direction.None;

        bool isMouseDown = false;

        public float Area { get; set; } = 4;

        public UIObject Parent { get; set; }

        public virtual void Awake()
        {
          
        }

        public virtual void Dispose()
        {
           
        }

        public virtual void OnMouseClick(MouseEventArgs e)
        {
           
        }

        public virtual void OnMouseDown(MouseEventArgs e)
        {
            isMouseDown = e.Button.HasFlag(MouseButton.Left);

            if (isMouseDown && Parent != null)
            {
                //reset resize mode
                horizontalResize = Direction.None;
                verticalResize = Direction.None;

                var rect = Parent.Rect;
                var pos = e.Position;

                if (Parent.Canvas != null)
                {
                    pos = Parent.Canvas.ToCanvasSpace(pos);
                }

                if (pos.X >= rect.Left && pos.X <= rect.Left + Area)
                {
                    e.IsHandled = true;
                    horizontalResize = Direction.Left;
                }
                else if (pos.X <= rect.Right && pos.X >= rect.Right - Area)
                {
                    e.IsHandled = true;
                    horizontalResize = Direction.Right;
                }

                if (pos.Y >= rect.Top && pos.Y <= rect.Top + Area)
                {
                    e.IsHandled = true;
                    verticalResize = Direction.Top;
                }
                else if (pos.Y <= rect.Bottom && pos.Y >= rect.Bottom - Area)
                {
                    e.IsHandled = true;
                    verticalResize = Direction.Bottom;
                }
            }
        }

        public virtual void OnMouseEnter(MouseEventArgs e)
        {

        }

        public virtual void OnMouseLeave(MouseEventArgs e)
        {

        }

        public virtual void OnMouseMove(MouseEventArgs e)
        {
            if (!isMouseDown || Parent == null) return;

            float xSign = 1, ySign = 1;

            //todo: implement corner 
            var size = Vector2.Zero;
            var cpos = Vector2.Zero;

            var RelativeTo = Parent.RelativeTo;

            switch (RelativeTo)
            {
                case Anchor.BottomRight:
                case Anchor.BottomLeft:
                case Anchor.BottomHorizFill:
                case Anchor.Bottom:
                    ySign = 0;
                    break;
            }

            switch (RelativeTo)
            {
                case Anchor.RightVerticalFill:
                case Anchor.Right:
                case Anchor.BottomRight:
                case Anchor.TopRight:
                    xSign = 0;;
                    break;
            }

            switch (horizontalResize)
            {
                case Direction.Left:
                    e.IsHandled = true;
                    //left side resize
                    size.X = -e.Delta.X;
                    //this should only be applied if left orient
                    cpos.X = e.Delta.X * xSign;
                    break;
                case Direction.Right:
                    e.IsHandled = true;

                    //right side resize
                    size.X = e.Delta.X;
                    //this should only be applied if right orient
                    cpos.X = e.Delta.X * (xSign - 1);
                    break;
            }

            switch (verticalResize)
            {
                case Direction.Top:
                    e.IsHandled = true;

                    //top side resize
                    size.Y = -e.Delta.Y;
                    //should only apply if top orient
                    cpos.Y = e.Delta.Y * ySign;
                    break;
                case Direction.Bottom:
                    e.IsHandled = true;

                    //bottom size resize
                    size.Y = e.Delta.Y;
                    //should only apply if bottom orient
                    cpos.Y = e.Delta.Y * (ySign - 1);
                    break;
            }

            Parent.Size += size;
            Parent.Position += cpos;
        }

        public virtual void OnMouseUp(MouseEventArgs e)
        {
            if (e.Button.HasFlag(MouseButton.Left))
            {
                isMouseDown = false;

                horizontalResize = Direction.None;
                verticalResize = Direction.None;
            }
        }

        public virtual void Update()
        {
            
        }
    }
}
