using InfinityUI.Core;
using InfinityUI.Interfaces;
using System;
using System.Collections.Generic;
using Materia.Rendering.Mathematics;
using System.Text;
using InfinityUI.Components;

namespace InfinityUI.Controls
{
    public class Slider : UIObject, ILayout
    {
        public event Action<float> ValueChanged;

        protected bool mouseDown;
        protected bool isFocused;

        /// <summary>
        /// Gets or sets the size of the step.
        /// Only applies to keyboard interaction
        /// </summary>
        /// <value>
        /// The size of the step.
        /// </value>
        public float StepSize { get; set; } = 0.01f;

        protected float max = 1.0f;
        public float Max
        {
            get
            {
                return max;
            }
            set
            {
                max = value;
                Clamp();
                Invalidate();
            }
        }

        protected float min = 0;
        public float Min
        {
            get
            {
                return min;
            }
            set
            {
                min = value;
                Clamp();
                Invalidate();
            }
        }

        protected float val = 0.5f;
        public float Value
        {
            get
            {
                return val;
            }
            set
            {
                float prev = val;
                val = value;

                Clamp();
                if (prev != val)
                {
                    ValueChanged?.Invoke(val);
                    Invalidate();
                }
            }
        }

        protected Orientation direction = Orientation.Horizontal;
        public Orientation Direction
        {
            get
            {
                return direction;
            }
            set
            {
                direction = value;
                Invalidate();
            }
        }

        public UIObject FillBar { get; set; }

        protected UIImage background;
        protected UISelectable selectable;

        public Slider(Vector2 size) : base()
        {
            Size = size;

            background = AddComponent<UIImage>();
            background.Color = new Vector4(0, 0, 0, 0.75f);

            selectable = AddComponent<UISelectable>();

            FillBar = new UIObject
            {
                Size = size,
                Position = Vector2.Zero,
                RelativeTo = Anchor.BottomLeft
            };

            var fillImage = FillBar.AddComponent<UIImage>();
            fillImage.Color = new Vector4(0, 0.5f, 1, 1);
            AddChild(FillBar);
            Invalidate();
            InitEvents();
        }

        protected virtual void InitEvents()
        {
            if (selectable == null) return;
            selectable.FocusChanged += Selectable_FocusChanged;
            selectable.KeyDown += OnKeyDown;
            selectable.PointerExit += OnMouseLeave;
            selectable.PointerDown += OnMouseDown;
            selectable.PointerMove += OnMouseMove;
            selectable.PointerUp += OnMouseUp;
        }

        private void Selectable_FocusChanged(UISelectable arg1, bool arg2)
        {
            isFocused = arg2;
        }

        public virtual void Invalidate()
        {
            float fx = (val - min) / (max - min);

            if (Direction == Orientation.Horizontal) 
            {
                float rx = Size.X * fx;
                FillBar.Size = new Vector2(rx, Size.Y);
            }
            else
            {
                float ry = Size.Y * fx;
                FillBar.Size = new Vector2(Size.X, ry);
            }
        }

        public void Assign(float v)
        {
            val = v;
            Clamp();
            Invalidate();
        }

        protected void Clamp()
        {
            val = MathF.Min(max, MathF.Max(min, val));
        }

        public virtual void Focus()
        {
            selectable?.OnFocus(new FocusEvent());
        }

        protected virtual void OnKeyDown(UISelectable selectable, KeyboardEventArgs e)
        {
            if (!isFocused) return;
            if (e.Key == OpenTK.Windowing.GraphicsLibraryFramework.Keys.Left)
            {
                Value -= StepSize;
            }
            else if(e.Key == OpenTK.Windowing.GraphicsLibraryFramework.Keys.Right)
            {
                Value += StepSize;
            }
        }

        protected float GetValFromPos(Vector2 p)
        {
            float f;

            if (Direction == Orientation.Horizontal)
            {
                f = (p.X - Rect.Left) / (Rect.Right - Rect.Left) * (max - min) + min;
            }
            else
            {
                switch (FillBar.RelativeTo)
                {
                    case Anchor.Top:
                    case Anchor.TopLeft:
                    case Anchor.TopRight:
                    case Anchor.TopHorizFill:
                    case Anchor.CenterHorizFill:
                        f = (1.0f - (p.Y - Rect.Top) / (Rect.Bottom - Rect.Top)) * (max - min) + min;
                        break;
                    default:
                        f = (p.Y - Rect.Top) / (Rect.Bottom - Rect.Top) * (max - min) + min;
                        break;

                }
            }

            return f;
        }

        protected virtual void OnMouseDown(UISelectable selectable, MouseEventArgs e)
        {
            if (e.Button.HasFlag(MouseButton.Left))
            {
                mouseDown = true;
                Value = MathF.Round(GetValFromPos(e.Position) / StepSize) * StepSize;
            }
        }

        protected virtual void OnMouseLeave(UISelectable selectable, MouseEventArgs e)
        {
            mouseDown = false;
        }

        protected virtual void OnMouseMove(UISelectable selectable, MouseEventArgs e)
        {
            if (!mouseDown) return;
            Value = MathF.Round(GetValFromPos(e.Position) / StepSize) * StepSize;
        }

        protected virtual void OnMouseUp(UISelectable selectable, MouseEventArgs e)
        {
            if (e.Button.HasFlag(MouseButton.Left))
            {
                mouseDown = false;
            }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (selectable == null) return;
            selectable.FocusChanged -= Selectable_FocusChanged;
            selectable.KeyDown -= OnKeyDown;
            selectable.PointerExit -= OnMouseLeave;
            selectable.PointerDown -= OnMouseDown;
            selectable.PointerMove -= OnMouseMove;
            selectable.PointerUp -= OnMouseUp;
        }
    }
}
