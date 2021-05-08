using InfinityUI.Components;
using InfinityUI.Core;
using InfinityUI.Interfaces;
using Materia.Rendering.Mathematics;
using System;

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
        public event Action<MovablePane, Vector2> Moved;

        public Axis MoveAxis { get; set; } = Axis.Both;

        public float SnapTolerance { get; set; } = 4;
        public MovablePaneSnapMode SnapMode { get; set; } = MovablePaneSnapMode.Panes;

        protected bool isMouseDown = false;
        private long lastClick;
        private int clickCount = 0;

        public bool Collaspable { get; set; } = false;
        protected float CollapseToHeight { get; set; } = 48;
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
            if (e.Button.HasFlag(MouseButton.Left))
            {
                isMouseDown = true;
            }
        }

        protected virtual void OnMouseLeave(UISelectable selectable,  MouseEventArgs e)
        {
            isMouseDown = false;
        }

        protected virtual void InvokeMoved()
        {
            Moved?.Invoke(this, Vector2.Zero);
        }

        public virtual void Move(Vector2 delta, bool invokeEvent = true)
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
                    Position += new Vector2(delta.X / Scale.X * xSign, delta.Y / Scale.Y * ySign);
                    break;
                case Axis.Horizontal:
                    Position += new Vector2(delta.X / Scale.X * xSign, 0);
                    break;
                case Axis.Vertical:
                    Position += new Vector2(0, delta.Y / Scale.Y * ySign);
                    break;
            }

            switch (SnapMode)
            {
                case MovablePaneSnapMode.Panes:
                    if (Parent == null) break;
                    for (int i = 0; i < Parent.Children.Count; ++i)
                    {
                        UIObject el = Parent.Children[i];
                        if (el == this || !el.Visible) continue;
                        if (!el.Rect.Intersects(Rect) || el.Rect.Contains(Rect)) continue;
                        UI.SnapToElement(this, el, SnapTolerance, xSign, ySign);
                    }

                    break;
                case MovablePaneSnapMode.Grid:
                    UI.SnapToGrid(this, (int)SnapTolerance);
                    break;
            }

            if (invokeEvent)
            {
                Moved?.Invoke(this, delta);
            }
        }

        protected virtual void OnMouseMove(UISelectable selectable,  MouseEventArgs e)
        {
            if (!isMouseDown) return;
            Move(e.Delta);
        }

        protected virtual void OnMouseUp(UISelectable selectable,  MouseEventArgs e)
        {
            if (e.Button.HasFlag(MouseButton.Left))
            {
                isMouseDown = false;
            }
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
