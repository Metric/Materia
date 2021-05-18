using InfinityUI.Components;
using InfinityUI.Components.Layout;
using InfinityUI.Core;
using InfinityUI.Interfaces;
using Materia.Rendering.Mathematics;
using OpenTK.Windowing.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace InfinityUI.Controls
{
    /// <summary>
    /// This is not a virtualized list so be careful
    /// how much data you feed it!
    /// </summary>
    /// <seealso cref="InfinityUI.Core.UIContainer" />
    public class ListView : UIObject
    {
        public event Action<ListView,Button,int> SelectionChanged;

        public UIImage Background { get; protected set; }

        protected int selectedIndex = -1;
        public int SelectedIndex
        {
            get
            {
                return selectedIndex;
            }
            set
            {
                selectedIndex = Math.Clamp(value, 0, Items.Count - 1);
                if (toggleGroup == null) return;
                toggleGroup.SetActive(selectedIndex);
            }
        }

        public List<Button> Items { get; protected set; } = new List<Button>();

        public Button SelectedItem { get; protected set; }

        protected UIObject viewContainer;
        protected UIScrollPanel scrollView;
        protected ToggleGroup toggleGroup;

        protected Slider scrollBar;
        
        public UIScrollPanel ScrollView
        {
            get => scrollView;
        }

        public ListView(Vector2 size) : base()
        {
            Size = size - new Vector2(6,0);

            Background = AddComponent<UIImage>();
            Background.Color = new Vector4(0, 0, 0, 0.85f);
            Background.Clip = true;

            viewContainer = new UIObject();

            var stack = viewContainer.AddComponent<UIStackPanel>();
            stack.Direction = Orientation.Vertical;
            toggleGroup = viewContainer.AddComponent<ToggleGroup>();

            toggleGroup.ToggleChanged += ToggleGroup_ToggleChanged;

            AddChild(viewContainer);
            scrollView = AddComponent<UIScrollPanel>();
            scrollView.Scrolled += ScrollView_Scrolled;

            scrollBar = new Slider(new Vector2(6, Size.Y));
            scrollBar.Position = new Vector2(Size.X, 0);
            scrollBar.Max = 1f;
            scrollBar.Min = 0f;
            scrollBar.Value = 0f;

            scrollBar.ValueChanged += ScrollBar_ValueChanged;

            AddChild(scrollBar);
        }

        private void ScrollBar_ValueChanged(float obj)
        {
            if (scrollView == null) return;
            scrollView.NormalizedOffset = new Vector2(0, obj);
        }

        private void ScrollView_Scrolled(UIScrollPanel obj)
        {
            scrollBar?.Assign(obj.NormalizedOffset.Y);
        }

        private void ToggleGroup_ToggleChanged(ToggleGroup arg1, UIToggleable arg2, int arg3)
        {
            selectedIndex = arg3;
            SelectedItem = arg2.Parent as Button;
            SelectionChanged?.Invoke(this, arg2.Parent as Button, arg3);
        }

        public virtual void Remove(ToggleButton b, bool dispose = false)
        {
            if (b == null) return;
            b.Focused -= B_Focused;

            // handle flow reassignment for tab
            UISelectable selectable = b.GetComponent<UISelectable>();
            if (selectable != null)
            {
                if (selectable.Up != null && selectable.Down != null)
                {
                    selectable.Up.Down = selectable.Up.Right = selectable.Down;
                    selectable.Down.Up = selectable.Down.Left = selectable.Up;
                }
                else if(selectable.Up != null && selectable.Down == null)
                {
                    selectable.Up.Down = selectable.Up.Right = null;
                }
                else if(selectable.Down != null && selectable.Up == null)
                {
                    selectable.Down.Up = selectable.Down.Left = null;
                }

                selectable.Down = selectable.Left = null;
                selectable.Up = selectable.Right = null;
            }

            Items?.Remove(b);
            viewContainer?.RemoveChild(b);
            if (b == SelectedItem)
            {
                selectedIndex = -1;
                SelectedItem = null;
            }
            if (dispose)
            {
                b.Dispose();
            }
        }

        public virtual void RemoveAt(int idx)
        {
            if (idx < 0 || idx >= Items.Count) return;
            var item = Items[idx];
            Remove(item as ToggleButton, true);
        }

        public virtual void Add(string s)
        {
            ToggleButton b = new ToggleButton(s, new Vector2(Size.X, 20));
            Add(b);
        }

        public virtual void Add(ToggleButton b)
        {
            if (b == null) return;

            int previousIdx = Items.Count - 1;

            b.TextContainer.Position = new Vector2(5, 0);
            b.TextAlignment = TextAlignment.Left;

            b.Focused += B_Focused;

            // assign tab movement flow
            UISelectable selectable = b.GetComponent<UISelectable>();
            if (selectable != null && previousIdx >= 0)
            {
                var previousSelectable = Items[previousIdx].GetComponent<UISelectable>();
                selectable.Up = selectable.Left = previousSelectable;

                if (previousSelectable != null)
                {
                    previousSelectable.Down = previousSelectable.Right = selectable;
                }
            }

            Items?.Add(b);
            viewContainer?.AddChild(b);
            if (Items.Count == 1)
            {
                toggleGroup.SetActive(0);
            }
        }

        private void B_Focused(Button obj, FocusEvent fv)
        {
            scrollView?.ScrollTo(obj);
        }

        public void Clear()
        {
            if (Items == null) return;

            for (int i = 0; i < Items.Count; ++i)
            {
                var item = Items[i];
                viewContainer?.RemoveChild(item);
                item?.Dispose();
            }

            selectedIndex = -1;
            SelectedItem = null;

            Items.Clear();
        }
    }
}
