using System;
using System.Collections.Generic;
using System.Text;

namespace InfinityUI.Core
{
    public enum SizeMode
    {
        Pixel,
        Percent
    }

    public enum SnapMode
    {
        None,
        Normal,
        Grid
    }

    public enum Axis
    {
        Both,
        Vertical,
        Horizontal,
        None
    }

    public enum Anchor
    {
        BottomLeft,
        BottomRight,
        TopLeft,
        TopRight,
        Bottom,
        Top,
        Fill,
        Center,
        Left,
        Right,
        TopHorizFill,
        CenterHorizFill,
        BottomHorizFill
    };

    public enum Navigation
    {
        None = 0,
        Up = 2,
        Down = 4,
        Left = 8,
        Right = 16
    }

    public enum Orientation
    {
        Horizontal,
        Vertical
    };
}
