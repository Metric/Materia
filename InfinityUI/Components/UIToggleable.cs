using InfinityUI.Core;
using InfinityUI.Interfaces;
using Materia.Rendering.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;

namespace InfinityUI.Components
{
    public class UIToggleable : IComponent
    {
        public event Action<UIToggleable, bool> ValueChanged;
        public Vector4 ActiveToggleColor { get; set; } = new Vector4(0, 0.5f, 0.75f, 1f);
        public Vector4 InactiveToggleColor { get; set; } = new Vector4(0, 0, 0, 0.75f);

        protected UISelectable selectable;

        protected bool isToggled;
        public bool IsToggled
        {
            get
            {
                return isToggled;
            }
            set
            {
                bool prev = isToggled;
                isToggled = value;
                if (isToggled != prev)
                {
                    ValueChanged?.Invoke(this, value);
                }
                UpdateToggleState();
            }
        }

        public UIObject Parent { get; set; }

        /// <summary>
        /// Assigns the toggle state without raising the event
        /// </summary>
        /// <param name="v">if set to <c>true</c> [v].</param>
        public void Assign(bool v)
        {
            isToggled = v;
            UpdateToggleState();
        }

        protected void UpdateToggleState()
        {
            if (selectable == null)
            {
                selectable = Parent?.GetComponent<UISelectable>();
            }

            if (selectable == null) return;

            selectable.HoverColor = ActiveToggleColor;

            if (IsToggled)
            {
                selectable.NormalColor = ActiveToggleColor;
            }
            else
            {
                selectable.NormalColor = InactiveToggleColor;
            }
        }

        public virtual void Awake()
        {

        }

        public virtual void Dispose()
        {

        }
    }
}
