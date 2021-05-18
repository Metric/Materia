using InfinityUI.Components;
using InfinityUI.Interfaces;
using Materia.Rendering.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;

namespace InfinityUI.Controls
{
    public class ToggleButton : Button
    {

        public UIToggleable toggleState { get; protected set; }

        public ToggleButton(string text, Vector2 size) : base(text, size)
        {
            RaycastTarget = true;
            toggleState = AddComponent<UIToggleable>();
            Submit += OnSubmit;
        }

        protected virtual void OnSubmit(Button b)
        {
            toggleState.IsToggled = !toggleState.IsToggled;
        }
    }
}
