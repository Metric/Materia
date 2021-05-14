using InfinityUI.Core;
using InfinityUI.Interfaces;
using Materia.Rendering.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;

namespace InfinityUI.Components
{
    public class UISelectable : IComponent, IKeyboardInput, IFocusable, IMouseInput, IMouseWheel
    {
        public event Action<UISelectable> Submit;
        public event Action<UISelectable, MouseEventArgs> PointerUp;
        public event Action<UISelectable, MouseEventArgs> PointerDown;
        public event Action<UISelectable, MouseEventArgs> PointerMove;
        public event Action<UISelectable, MouseEventArgs> PointerEnter;
        public event Action<UISelectable, MouseEventArgs> PointerExit;
        public event Action<UISelectable, MouseEventArgs> Click;
        public event Action<UISelectable, KeyboardEventArgs> TextInput;
        public event Action<UISelectable, KeyboardEventArgs> KeyDown;
        public event Action<UISelectable, KeyboardEventArgs> KeyUp;
        public event Action<UISelectable, bool> FocusChanged;
        public event Action<UISelectable, MouseWheelArgs> Wheel;
        public event Action<UISelectable> BeforeUpdateTarget;

        public bool BubbleEvents { get; set; } = true;

        public UIObject Parent { get; set; }

        protected UIDrawable targetGraphic;
        public UIDrawable TargetGraphic
        {
            get => targetGraphic;
            set
            {
                targetGraphic = value;
                UpdateTargetGraphic();
            }
        }

        public Navigation TabDirection { get; set; } = Navigation.Right | Navigation.Down;
        
        public UISelectable Left { get; set; }
        public UISelectable Right { get; set; }
        public UISelectable Up { get; set; }
        public UISelectable Down { get; set; }

        public bool Focused { get; protected set; }

        protected bool enabled = true;
        public bool Enabled
        {
            get => enabled;
            set
            {
                enabled = value;
                UpdateTargetGraphic();
            }
        }

        protected Vector4 normalColor = new Vector4(0.3f, 0.3f, 0.3f, 1);
        public Vector4 NormalColor
        {
            get => normalColor;
            set
            {
                normalColor = value;
                UpdateTargetGraphic();
            }
        }

        protected Vector4 hoverColor = new Vector4(1.05f, 1.05f, 1.05f, 1);
        public Vector4 HoverColor
        {
            get => hoverColor;
            set
            {
                hoverColor = value;
                UpdateTargetGraphic();
            }
        }

        protected Vector4 pressedColor = new Vector4(0.9f, 0.9f, 0.9f, 1);
        public Vector4 PressedColor
        {
            get => pressedColor;
            set
            {
                pressedColor = value;
                UpdateTargetGraphic();
            }
        }

        protected Vector4 disabledColor = new Vector4(1, 1, 1, 0.25f);
        public Vector4 DisabledColor
        {
            get => disabledColor;
            set
            {
                disabledColor = value;
                UpdateTargetGraphic();
            }
        }

        protected Vector4 focusedColor = new Vector4(1, 1, 1, 1);
        public Vector4 FocusedColor
        {
            get => focusedColor;
            set
            {
                focusedColor = value;
                UpdateTargetGraphic();
            }
        }

        private bool isHovered = false;
        private bool isDown = false;

        public virtual void Awake()
        {
            if (Parent == null) return;
            Parent.RaycastTarget = true;
        }

        public virtual void Dispose()
        {
         
        }

        public virtual void OnFocus(FocusEvent fev)
        {
            if (fev.IsHandled) return;
            fev.IsHandled = !BubbleEvents;
            UI.Focus = this;
            Focused = true;
            FocusChanged?.Invoke(this, Focused);
            UpdateTargetGraphic();
        }

        public virtual void OnKeyDown(KeyboardEventArgs e)
        {
            if (e.IsHandled) return;
            e.IsHandled = !BubbleEvents;
            KeyDown?.Invoke(this, e);
        }

        public virtual void OnKeyUp(KeyboardEventArgs e)
        {
            if (!enabled) return;
            if (e.IsHandled) return;
            e.IsHandled = !BubbleEvents;

            //handle basic navigation tasks
            if (e.Key == OpenTK.Windowing.GraphicsLibraryFramework.Keys.Tab)
            {
                //handle tabbing based on navigation type
                if (TabDirection.HasFlag(Navigation.Right) && Right != null)
                {
                    Right.OnFocus(new FocusEvent());
                }
                else if (TabDirection.HasFlag(Navigation.Left) && Left != null)
                {
                    Left.OnFocus(new FocusEvent());
                }
                else if (TabDirection.HasFlag(Navigation.Down) && Down != null)
                {
                    Down.OnFocus(new FocusEvent());
                }
                else if (TabDirection.HasFlag(Navigation.Up) && Up != null)
                {
                    Up.OnFocus(new FocusEvent());
                }
            }
            else if (e.Key == OpenTK.Windowing.GraphicsLibraryFramework.Keys.Up)
            {
                //handle navigation based on flow
                Up?.OnFocus(new FocusEvent());
            }
            else if (e.Key == OpenTK.Windowing.GraphicsLibraryFramework.Keys.Down)
            {
                //handle navigation based on flow
                Down?.OnFocus(new FocusEvent());
            }
            else if (e.Key == OpenTK.Windowing.GraphicsLibraryFramework.Keys.Left)
            {
                //handle navigation based on flow
                Left?.OnFocus(new FocusEvent());
            }
            else if (e.Key == OpenTK.Windowing.GraphicsLibraryFramework.Keys.Right)
            {
                //handle navigation based on flow
                Right?.OnFocus(new FocusEvent());
            }
           
            if(e.Key == OpenTK.Windowing.GraphicsLibraryFramework.Keys.Enter && Focused)
            {
                Submit?.Invoke(this);
            }

            KeyUp?.Invoke(this, e);
        }

        public virtual void OnLostFocus(FocusEvent fev)
        {
            if (fev.IsHandled) return;
            fev.IsHandled = !BubbleEvents;
            Focused = false;
            FocusChanged?.Invoke(this, Focused);
            UpdateTargetGraphic();
        }

        public virtual void OnMouseClick(MouseEventArgs e)
        {
            if (!enabled) return;
            if (e.IsHandled) return;
            e.IsHandled = !BubbleEvents;
            Click?.Invoke(this, e);
        }

        public virtual void OnMouseDown(MouseEventArgs e)
        {
            if (!enabled) return;
            if (e.IsHandled) return;
            e.IsHandled = !BubbleEvents;
            isDown = true;
            UpdateTargetGraphic();
            PointerDown?.Invoke(this, e);
        }

        public virtual void OnMouseEnter(MouseEventArgs e)
        {
            if (!enabled) return;
            if (e.IsHandled) return;
            e.IsHandled = !BubbleEvents;
            isHovered = true;
            UpdateTargetGraphic();
            PointerEnter?.Invoke(this, e);
        }

        public virtual void OnMouseLeave(MouseEventArgs e)
        {
            if (!enabled) return;
            if (e.IsHandled) return;
            e.IsHandled = !BubbleEvents;
            isDown = false;
            isHovered = false;
            UpdateTargetGraphic();
            PointerExit?.Invoke(this, e);
        }

        public virtual void OnMouseMove(MouseEventArgs e)
        {
            if (!enabled) return;
            if (e.IsHandled) return;
            e.IsHandled = !BubbleEvents;
            PointerMove?.Invoke(this, e);
        }

        public virtual void OnMouseUp(MouseEventArgs e)
        {
            if (!enabled) return;
            if (e.IsHandled) return;
            e.IsHandled = !BubbleEvents;
            isDown = false;
            UpdateTargetGraphic();
            PointerUp?.Invoke(this, e);
        }

        public virtual void OnTextInput(KeyboardEventArgs e)
        {
            if (!enabled) return;
            if (e.IsHandled) return;
            e.IsHandled = !BubbleEvents;
            TextInput?.Invoke(this, e);
        }

        public virtual void OnMouseWheel(MouseWheelArgs e)
        {
            if (!enabled) return;
            if (e.IsHandled) return;
            e.IsHandled = !BubbleEvents;
            Wheel?.Invoke(this, e);
        }

        protected virtual void UpdateTargetGraphic()
        {
            if (TargetGraphic != null)
            {
                if (!enabled)
                {
                    TargetGraphic.Color = normalColor * disabledColor;
                }
                else if (isHovered && !isDown)
                {
                    TargetGraphic.Color = normalColor * hoverColor;
                }
                else if (isHovered && isDown)
                {
                    TargetGraphic.Color = normalColor * pressedColor;
                }
                else if(Focused)
                {
                    TargetGraphic.Color = normalColor * focusedColor;
                }
                else
                {
                    TargetGraphic.Color = normalColor;
                }
            }
        }
    }
}
