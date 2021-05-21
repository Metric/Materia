using InfinityUI.Core;
using InfinityUI.Controls;
using InfinityUI.Components;
using System;
using System.Collections.Generic;
using System.Text;
using InfinityUI.Interfaces;
using InfinityUI.Components.Layout;
using Materia.Rendering.Mathematics;
using System.Diagnostics;

namespace MateriaCore.Components.GL
{
    public class UIShelfSourceView : UIObject, ILayout
    {
        public bool NeedsUpdate { get; set; }

        protected string filter;
        public string Filter
        {
            get => filter;
            set
            {
                if(filter != value)
                {
                    filter = value;
                    NeedsUpdate = true;
                }
            }
        }

        #region components
        protected UIImage background;
        protected UIScrollPanel scrollView;
        protected UIObject scrollViewContainer;

        protected InfinityUI.Controls.Slider verticalScrollBar;
        #endregion

        public UIImage Background { get => background; }

        public UIShelfSourceView() : base()
        {
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            background = AddComponent<UIImage>();
            background.Color = new Vector4(0, 0, 0, 1);
            background.Clip = true;

            scrollView = AddComponent<UIScrollPanel>();
            scrollView.ScrollStep = 32;
            scrollViewContainer = scrollView.View;
            scrollViewContainer.RelativeTo = Anchor.TopHorizFill;
            scrollViewContainer.RemoveComponent<UIContentFitter>();
            var stack = scrollViewContainer.AddComponent<UIStackPanel>();
            stack.Direction = Orientation.Vertical;
            stack.ChildAlignment = Anchor.TopHorizFill;

            scrollView.Scrolled += ScrollView_Scrolled;
            scrollView.MaximumOffsetChanged += ScrollView_MaximumOffsetChanged;

            verticalScrollBar = new InfinityUI.Controls.Slider(new Vector2(8, 1))
            {
                RelativeTo = Anchor.RightVerticalFill,
                Direction = Orientation.Vertical,
                StepSize = 32,
                Max = 0,
                Min = 0,
                Visible = false,
            };

            verticalScrollBar.ValueChanged += VerticalScrollBar_ValueChanged;

            AddChild(verticalScrollBar);

            NeedsUpdate = true;
        }

        private void VerticalScrollBar_ValueChanged(float v)
        {
            if (verticalScrollBar.Max > 0)
            {
                float normalized = v / verticalScrollBar.Max;
                scrollView.NormalizedOffset = new Vector2(0, normalized);
            }
        }

        private void ScrollView_MaximumOffsetChanged(UIScrollPanel obj)
        {
            verticalScrollBar.Max = scrollView.MaximumOffset.Y;
            verticalScrollBar.Visible = verticalScrollBar.Max != 0;
        }

        private void ScrollView_Scrolled(UIScrollPanel obj)
        {
            float y = scrollView.NormalizedOffset.Y;
            verticalScrollBar.Assign(y * verticalScrollBar.Max);
        }

        public virtual void Invalidate()
        {
            if (!NeedsUpdate) return;
            var children = scrollViewContainer.Children;
            //clear view without disposing
            for (int i = 0; i < children.Count; ++i)
            {
                if(scrollViewContainer.RemoveChild(children[i]))
                {
                    --i;
                }
            }

            var items = UIShelfItem.Find(filter);
            for (int i = 0; i < items.Count; ++i)
            {
                items[i].ZOrder = items.Count - i;
                scrollViewContainer.AddChild(items[i]);
                items[i].Margin = new Box2(0, 0, 8, 0);
            }
            NeedsUpdate = false;
        }
    }
}
