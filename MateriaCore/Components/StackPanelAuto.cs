using Avalonia.Layout;
using Avalonia.Controls;
using Avalonia;
using System;

namespace MateriaCore.Components
{
    public class StackPanelAuto : Panel
    {
        public Orientation Direction
        {
            get; set;
        }

        public bool HalfAndHalf
        {
            get; set;
        }

        public StackPanelAuto() : base()
        {
            HalfAndHalf = false;
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            Controls children = Children;

            double parentWidth = 0;
            double parentHeight = 0;
            double accumulatedWidth = 0;
            double accumulatedHeight = 0;

            bool isHorizontal = Direction == Orientation.Horizontal;

            for (int i = 0; i < children.Count; ++i)
            {
                Control child = (Control)children[i];
                if (child == null) continue;

                var childConstraint = new Size(Math.Max(0, availableSize.Width - accumulatedWidth), Math.Max(0, availableSize.Height - accumulatedHeight));
                child.Measure(childConstraint);
                var desired = child.DesiredSize;

                if (isHorizontal)
                {
                    accumulatedWidth += desired.Width;
                    parentHeight = Math.Max(parentHeight, accumulatedHeight + desired.Height);
                }
                else
                {
                    parentWidth = Math.Max(parentWidth, accumulatedWidth + desired.Width);
                    accumulatedHeight += desired.Height;
                }
            }

            // Make sure the final accumulated size is reflected in parentSize. 
            parentWidth = Math.Max(parentWidth, accumulatedWidth);
            parentHeight = Math.Max(parentHeight, accumulatedHeight);

            double w = double.IsPositiveInfinity(availableSize.Width) ? 0 : Math.Min(parentWidth, availableSize.Width);
            double h = double.IsPositiveInfinity(availableSize.Height) ? 0 : Math.Min(parentHeight, availableSize.Height);
            return new Size(w, h);
        }

        protected override Size ArrangeOverride(Size arrangeSize)
        {
            Controls children = Children;

            double accumulatedLeft = 0;
            double accumulatedTop = 0;
            double prevAccumLeft = 0;
            double prevAccumTop = 0;

            var isHorizontal = Direction == Orientation.Horizontal;

            Size halfAndHalf = new Size(arrangeSize.Width / (children.Count + 1) * 0.5, arrangeSize.Height / (children.Count + 1) * 0.5);

            for (int i = 0; i < children.Count; ++i)
            {
                Control child = (Control)children[i];
                if (child == null) { continue; }
                Size childDesiredSize = child.DesiredSize;
                var isCollapsed = !child.IsVisible;

                Rect rcChild = new Rect(
                    accumulatedLeft,
                    accumulatedTop,
                    Math.Max(0.0, arrangeSize.Width - accumulatedLeft),
                    Math.Max(0.0, arrangeSize.Height - accumulatedTop));

                if (isHorizontal)
                {
                    rcChild = rcChild.WithWidth(isCollapsed ? 0 : (HalfAndHalf ? halfAndHalf.Width : childDesiredSize.Width));
                    rcChild = rcChild.WithHeight(arrangeSize.Height);
                    prevAccumLeft = accumulatedLeft;
                    accumulatedLeft += rcChild.Width;
                }
                else
                {
                    rcChild = rcChild.WithWidth(arrangeSize.Width);
                    rcChild = rcChild.WithHeight(isCollapsed ? 0 : (HalfAndHalf ? halfAndHalf.Height : childDesiredSize.Height));
                    prevAccumTop = accumulatedTop;
                    accumulatedTop += rcChild.Height;
                }

                if (i == children.Count - 1)
                {
                    if (isHorizontal && !isCollapsed)
                    {
                        rcChild = rcChild.WithWidth(Math.Max(arrangeSize.Width - prevAccumLeft, 0));
                    }
                    else if(!isHorizontal && !isCollapsed)
                    {
                        rcChild = rcChild.WithHeight(Math.Max(arrangeSize.Height - prevAccumTop, 0));
                    }
                }

                child.Arrange(rcChild);
            }

            return arrangeSize;
        }
    }
}
