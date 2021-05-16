﻿using InfinityUI.Components;
using InfinityUI.Core;
using InfinityUI.Interfaces;
using Materia.Rendering.Mathematics;
using System;
using System.Diagnostics;

namespace InfinityUI.Controls
{
    public enum MovablePaneSnapMode
    {
        Panes = 0,
        Grid = 1,
        None = 2
    }

    public class MovablePane : UIObject
    {
        public event Action<MovablePane> DoubleClick;
        public event Action<MovablePane, Vector2, MouseEventArgs> Moved;
        public event Action<MovablePane, Vector2> MovedTo;

        public Axis MoveAxis { get; set; } = Axis.Both;

        public float SnapTolerance { get; set; } = 4;
        public MovablePaneSnapMode SnapMode { get; set; } = MovablePaneSnapMode.Panes;

        protected bool isMouseDown = false;
        private long lastClick;
        private int clickCount = 0;

        public bool Collaspable { get; set; } = false;
        protected float CollapseToHeight { get; set; } = 32;
        private Vector2 originSize;

        protected UISelectable selectable;
        public UIImage Background { get; protected set; }

        public MovablePane(Vector2 size) : base()
        {
            Size = size;
            originSize = size;
            selectable = AddComponent<UISelectable>();
            Background = AddComponent<UIImage>();
            Background.Color = new Vector4(0, 0, 0, 0.75f);
            InitEvents();
        }

        protected virtual void InitEvents()
        {
            if (selectable == null) return;
            selectable.Click += OnMouseClick;
            selectable.PointerDown += OnMouseDown;
            selectable.PointerUp += OnMouseUp;
            selectable.PointerMove += OnMouseMove;
            selectable.PointerExit += OnMouseLeave;
        }

        public virtual void OnMouseClick(UISelectable selectable, MouseEventArgs e)
        {
            if (new TimeSpan(DateTime.Now.Ticks - lastClick).TotalMilliseconds > 250)
            {
                clickCount = 0;
            }

            ++clickCount;

            if (clickCount >= 2)
            {
                clickCount = 0;

                if (Collaspable)
                {
                    if (originSize.Equals(Size))
                    {
                        Background.Clip = true;
                        Size = new Vector2(Size.X, CollapseToHeight);
                    }
                    else
                    {
                        Background.Clip = false;
                        Size = originSize;
                    }
                }

                DoubleClick?.Invoke(this);
            }

            lastClick = DateTime.Now.Ticks;
        }

        protected virtual void OnMouseDown(UISelectable selectable,  MouseEventArgs e)
        {
            ZOrder = -1;
            isMouseDown = true;
        }

        protected virtual void OnMouseLeave(UISelectable selectable,  MouseEventArgs e)
        {
            isMouseDown = false;
        }

        public virtual void Move(Vector2 delta, bool invokeEvent = true, MouseEventArgs e = null)
        {
            float xSign = 1;
            float ySign = 1;

            Vector2 scaledDelta = delta;

            if (Canvas != null)
            {
                scaledDelta *= Canvas.Scale;
            }

            switch (RelativeTo)
            {
                case Anchor.BottomRight:
                case Anchor.BottomLeft:
                case Anchor.BottomHorizFill:
                case Anchor.Bottom:
                    ySign = -1;
                    break;
            }

            switch (RelativeTo)
            {
                case Anchor.CenterRight:
                case Anchor.BottomRight:
                case Anchor.TopRight:
                    xSign = -1;
                    break;
            }

            Vector2 movementDelta = Vector2.Zero;

            switch (MoveAxis)
            {
                case Axis.Both:
                    movementDelta = new Vector2(scaledDelta.X / Scale.X * xSign, scaledDelta.Y / Scale.Y * ySign);
                    break;
                case Axis.Horizontal:
                    movementDelta = new Vector2(scaledDelta.X / Scale.X * xSign, 0);
                    break;
                case Axis.Vertical:
                    movementDelta = new Vector2(0, scaledDelta.Y / Scale.Y * ySign);
                    break;
            }

            if (RelativeMode == SizeMode.Percent && Parent != null)
            {
                Vector2 psize = Parent.WorldSize;
                movementDelta = new Vector2(movementDelta.X / psize.X, movementDelta.Y / psize.Y);
            }

            Position += movementDelta;

            switch (SnapMode)
            {
                case MovablePaneSnapMode.Panes:
                    //todo: fix snap to element for Relative Mode = Percent
                    //break for the moment on RelativeMode == Percent
                    //as we have not taken that into account in snap to element yet
                    if (Parent == null || RelativeMode == SizeMode.Percent) break;
                    for (int i = 0; i < Parent.Children.Count; ++i)
                    {
                        UIObject el = Parent.Children[i];
                        if (el == this || !el.Visible) continue;
                        if (!el.Rect.Intersects(Rect) || el.Rect.Contains(Rect)) continue;
                        UI.SnapToElement(this, el, SnapTolerance, xSign, ySign);
                    }
                    break;
            }

            if (invokeEvent)
            {
                Moved?.Invoke(this, delta, e);
            }
        }

        public virtual void MoveTo(Vector2 pos, bool invokeEvent = true)
        {
            float xSign = 1;
            float ySign = 1;

            switch (RelativeTo)
            {
                case Anchor.BottomRight:
                case Anchor.BottomLeft:
                case Anchor.BottomHorizFill:
                case Anchor.Bottom:
                    ySign = -1;
                    break;
            }

            switch (RelativeTo)
            {
                case Anchor.CenterRight:
                case Anchor.BottomRight:
                case Anchor.TopRight:
                    xSign = -1;
                    break;
            }

            switch (MoveAxis)
            {
                case Axis.Both:
                    Position = pos;
                    break;
                case Axis.Horizontal:
                    Position = new Vector2(pos.X, 0);
                    break;
                case Axis.Vertical:
                    Position = new Vector2(0, pos.Y);
                    break;
            }

            switch (SnapMode)
            {
                case MovablePaneSnapMode.Panes:
                    if (Parent == null || RelativeMode == SizeMode.Percent) break;
                    for (int i = 0; i < Parent.Children.Count; ++i)
                    {
                        UIObject el = Parent.Children[i];
                        if (el == this || !el.Visible) continue;
                        if (!el.Rect.Intersects(Rect) || el.Rect.Contains(Rect)) continue;
                        UI.SnapToElement(this, el, SnapTolerance, xSign, ySign);
                    }

                    break;
                case MovablePaneSnapMode.Grid:
                    if (RelativeMode == SizeMode.Percent) break;
                    UI.SnapToGrid(this, (int)SnapTolerance);
                    break;
            }

            if (invokeEvent)
            {
                MovedTo?.Invoke(this, pos);
            }
        }

        protected virtual void OnMouseMove(UISelectable selectable,  MouseEventArgs e)
        {
            if (!isMouseDown) return;
            Move(e.Delta, true, e);
        }

        protected virtual void OnMouseUp(UISelectable selectable,  MouseEventArgs e)
        {
            ZOrder = 0;

            //todo: handle Relate Mode = Percent for Snap To Grid
            if (SnapMode == MovablePaneSnapMode.Grid && RelativeMode == SizeMode.Pixel)
            {
                UI.SnapToGrid(this, (int)SnapTolerance);
            }

            isMouseDown = false;
        }

        public override void Dispose(bool disposing = true)
        {
            base.Dispose(disposing);
            if (selectable == null) return;
            selectable.Click -= OnMouseClick;
            selectable.PointerDown -= OnMouseDown;
            selectable.PointerUp -= OnMouseUp;
            selectable.PointerMove -= OnMouseMove;
            selectable.PointerExit -= OnMouseLeave;
        }
    }
}