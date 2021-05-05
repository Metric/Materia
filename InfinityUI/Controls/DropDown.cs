using InfinityUI.Components;
using InfinityUI.Core;
using InfinityUI.Interfaces;
using Materia.Rendering.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;

namespace InfinityUI.Controls
{
    public class DropDown : Button
    {
        public event Action<int> SelectionChanged;
        protected ListView list;

        protected Vector2 originSize;
        protected bool isFocused = false;

        public List<string> Items { get; protected set; } = new List<string>();

        public bool IsActive
        {
            get
            {
                if (list == null) return false;
                return list.Visible;
            }
            set
            {
                if (list == null) return;
                list.Visible = value;
                if (value)
                {
                    Size = new Vector2(originSize.X, originSize.Y + 266);
                }
                else
                {
                    Size = originSize;
                }
            }
        }

        public int SelectedIndex
        {
            get
            {
                if (list == null) return -1;
                return list.SelectedIndex;
            }
            set
            {
                if (list == null) return;
                list.SelectedIndex = value;
            }
        }

        public DropDown(Vector2 size) : base()
        {
            Size = size;
            originSize = Size;

            textView.Alignment = TextAlignment.Left;
            textContainer.RelativeTo = Anchor.TopLeft;
            textContainer.Position = new Vector2(5, textView.FontSize * 0.25f);

            list = new ListView(new Vector2(Size.X, 256));
            list.SelectionChanged += List_SelectionChanged;
            list.RelativeTo = Anchor.TopHorizFill;
            list.Position = new Vector2(0, Size.Y);

            Submit += new Action<Button>((b) =>
            {
                IsActive = !IsActive;
            });

            AddChild(list);
        }

        private void List_SelectionChanged(ListView arg1, Button arg2, int arg3)
        {
            if (arg3 < 0 || arg3 >= Items.Count) return;
            SelectionChanged?.Invoke(arg3);
            IsActive = false;
            if (arg2 == null) return;
            Text = arg2.Text;
        }

        public void Clear()
        {
            Items?.Clear();
            list?.Clear();
        }

        public void Add(string s)
        {
            Items?.Add(s);
            list?.Add(s);
        }

        public void Remove(int id)
        {
            if (id < 0 || id >= Items.Count) return;
            Items?.RemoveAt(id);
            list?.RemoveAt(id);
        }
    }
}
