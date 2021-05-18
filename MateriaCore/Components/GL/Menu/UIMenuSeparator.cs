﻿using InfinityUI.Components.Layout;
using Materia.Rendering.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;

namespace MateriaCore.Components.GL.Menu
{
    public class UIMenuSeparator : UIMenuItem
    {
        public UIMenuSeparator() : base("")
        {
            RemoveComponent<UIContentFitter>();
            Size = new Vector2(1, 2);
            Padding = new Box2(0,0,0,0);
            background.Color = new Vector4(0.2f, 0.2f, 0.2f, 0);
        }
    }
}