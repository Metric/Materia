using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Materia.UI.Components
{
    public class StackPanelAuto : Panel
    {
        public static readonly DependencyProperty DirectionProperty = DependencyProperty.Register("Direction", typeof(Orientation), typeof(StackPanelAuto));
        public Orientation Direction
        {
            get { return (Orientation)GetValue(DirectionProperty);  }
            set { SetValue(DirectionProperty, value);  }
        }

        public static readonly DependencyProperty HalfAndHalfProperty = DependencyProperty.Register("HalfAndHalf", typeof(bool), typeof(StackPanelAuto));

        public bool HalfAndHalf
        {
            get { return (bool)GetValue(HalfAndHalfProperty);  }
            set { SetValue(HalfAndHalfProperty, value);  }
        }

        public StackPanelAuto() : base()
        {
            HalfAndHalf = false;
        }

        protected override Size MeasureOverride(Size constraint)
        {
            UIElementCollection children = InternalChildren;

            double parentWidth = 0;
            double parentHeight = 0;
            double accumulatedWidth = 0;
            double accumulatedHeight = 0;

            bool isHorizontal = Direction == Orientation.Horizontal;

            for(int i = 0; i < children.Count; ++i)
            {
                UIElement child = children[i];
                if (child == null) continue;

                var childConstraint = new Size(Math.Max(0, constraint.Width - accumulatedWidth), Math.Max(0, constraint.Height - accumulatedHeight));
                child.Measure(childConstraint);
                var desired = child.DesiredSize;

                if(isHorizontal)
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
            parentWidth =  Math.Max(parentWidth, accumulatedWidth);
            parentHeight = Math.Max(parentHeight, accumulatedHeight);

            var parent = new Size(parentWidth, parentHeight);

            parent.Width = double.IsPositiveInfinity(constraint.Width) ? 0 : Math.Min(parent.Width, constraint.Width);

            return parent;
        }

        protected override Size ArrangeOverride(Size arrangeSize)
        {
            UIElementCollection children = InternalChildren;
            int totalChildrenCount = children.Count;

            double accumulatedLeft = 0;
            double accumulatedTop = 0;
            double prevAccumLeft = 0;
            double prevAccumTop = 0;

            var isHorizontal = Direction == Orientation.Horizontal;

            Size halfAndHalf = new Size(arrangeSize.Width / (children.Count + 1) * 0.5, arrangeSize.Height / (children.Count + 1) * 0.5);

            for (int i = 0; i < children.Count; ++i)
            {
                UIElement child = children[i];
                if (child == null) { continue; }
                Size childDesiredSize = child.DesiredSize;
                var isCollapsed = child.Visibility == Visibility.Collapsed;

                Rect rcChild = new Rect(
                    accumulatedLeft,
                    accumulatedTop,
                    Math.Max(0.0, arrangeSize.Width - accumulatedLeft),
                    Math.Max(0.0, arrangeSize.Height - accumulatedTop));

                if (isHorizontal)
                {
                    rcChild.Width = isCollapsed ? 0 : (HalfAndHalf ? halfAndHalf.Width : childDesiredSize.Width);
                    rcChild.Height = arrangeSize.Height;
                    prevAccumLeft = accumulatedLeft;
                    accumulatedLeft += rcChild.Width;
                }
                else
                {
                    rcChild.Width = arrangeSize.Width;
                    rcChild.Height = isCollapsed ? 0 : (HalfAndHalf ? halfAndHalf.Height : childDesiredSize.Height);
                    prevAccumTop = accumulatedTop;
                    accumulatedTop += rcChild.Height;
                }

                if (i == children.Count - 1)
                {
                    if (isHorizontal && !isCollapsed)
                    {
                        rcChild.Width = Math.Max(arrangeSize.Width - prevAccumLeft, 0);
                    }
                }

                child.Arrange(rcChild);
            }

            return arrangeSize;
        }
    }
}
