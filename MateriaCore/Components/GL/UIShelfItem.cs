using InfinityUI.Components;
using InfinityUI.Components.Layout;
using InfinityUI.Controls;
using InfinityUI.Core;
using Materia.Rendering.Mathematics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace MateriaCore.Components.GL
{
    public class UIShelfItem : Button
    {
        public event Action<UIShelfItem> Selected;

        protected static List<UINodeSource> nodes = new List<UINodeSource>();
        protected static HashSet<string> nodesCache = new HashSet<string>();

        #region components
        protected Button more;
        protected UIStackPanel stack;

        protected UIObject innerChildren;
        protected UIStackPanel innerStack;

        protected UIShelfItem shelfParent;
        #endregion


        public UIShelfItem ShelfParent
        {
            get => shelfParent;
        }

        protected string path;
        public string Path
        {
            get => path;
        }

        protected bool expanded;
        public bool Expanded
        {
            get => expanded;
            set
            {
                expanded = value;
                if (more != null)
                {
                    more.Text = expanded ? "-" : "+";
                }
                if (innerChildren != null)
                {
                    innerChildren.Visible = expanded;
                }
            }
        }

        public UIShelfItem(string t) : base(t, new Vector2(128, 32))
        {
            InitializeComponent();
            Name = t;
            path = t;
        }

        private void InitializeComponent()
        {
            RelativeTo = Anchor.TopHorizFill;

            RaycastTarget = true;

            AddComponent<UIContentFitter>();

            selectable.PointerEnter += Selectable_PointerEnter;
            selectable.PointerExit += Selectable_PointerExit;
            selectable.NormalColor = new Vector4(0.15f, 0.15f, 0.15f, 1f);

            textContainer.RelativeTo = Anchor.TopLeft;
            textContainer.Margin = new Box2(0, 5, 0, 0);
            textView.Alignment = TextAlignment.Left;

            more = new Button("", new Vector2(32, 32));
            more.Visible = false;
            more.Submit += More_Submit;

            innerChildren = new UIObject
            {
                RelativeTo = Anchor.TopHorizFill,
                Margin = new Box2(0, Size.Y, 0, 0),
                Size = new Vector2(1, 1),
                Visible = false,
                RaycastTarget = true
            };
            innerStack = innerChildren.AddComponent<UIStackPanel>();
            innerStack.Direction = Orientation.Vertical;
            innerStack.ChildAlignment = Anchor.TopHorizFill;

            Submit += UIShelfItem_Submit;

            AddChild(more);
            AddChild(innerChildren);
        }

        private void UIShelfItem_Submit(Button obj)
        {
            Debug.WriteLine("Clicked: " + Name);
            Selected?.Invoke(this);
        }

        private void Selectable_PointerExit(UISelectable arg1, InfinityUI.Interfaces.MouseEventArgs arg2)
        {
            Debug.WriteLine($"Exited: {Name}");
        }

        private void Selectable_PointerEnter(UISelectable arg1, InfinityUI.Interfaces.MouseEventArgs arg2)
        {
            Debug.WriteLine($"Entered: {Name}");
        }

        private void More_Submit(Button obj)
        {
            if (!more.Visible) return;
            expanded = !expanded;
            innerChildren.Visible = expanded;
            more.Text = expanded ? "-" : "+";
        }

        public void Add(UIShelfItem item)
        {
            textContainer.Margin = new Box2(42, 5, 0, 0);
            textContainer.Padding = new Box2(0, 0, 42, 5);
            more.Visible = true;
            more.Text = "+";
            item.Margin = new Box2(Margin.Left + 8, 0, 0, 0);
            item.shelfParent = this;
            item.path = System.IO.Path.Combine(Path, item.path);
            innerChildren.AddChild(item);
        }

        public void Add(UINodeSource src)
        {
            src.Path = path;
            string key = $"{path}.{src.Title}.{src.Type}";
            if (!nodesCache.Contains(key))
            {
                nodesCache.Add(key);
                nodes.Add(src);
            }
        }

        public void Remove(UINodeSource src)
        {
            if (nodes.Remove(src))
            {
                src.Path = "";
            }
        }

        public void Remove(UIShelfItem item)
        {
            bool contains = innerChildren.Children.Contains(item);
            if (contains)
            {
                item.shelfParent = null;
                innerChildren.RemoveChild(item);

                if (innerChildren.Children.Count == 0)
                {
                    more.Text = "+";
                    more.Visible = false;
                    expanded = false;
                }
            }
        }

        public List<UIShelfItem> GetShelfItems()
        {
            List<UIShelfItem> items = new List<UIShelfItem>();
            var children = innerChildren.Children;
            for (int i = 0; i < children.Count; ++i)
            {
                var c = children[i];
                if (c is UIShelfItem)
                {
                    items.Add(c as UIShelfItem);
                }
            }
            return items;
        }

        public UIShelfItem FindChild(string path)
        {
            if (string.IsNullOrEmpty(path)) return this;

            UIShelfItem parent = this;
            string[] split = path.Split(new string[] { System.IO.Path.DirectorySeparatorChar + "" }, StringSplitOptions.RemoveEmptyEntries);

            int i = 0;
            List<UIShelfItem> children = parent.GetShelfItems();
            for (i = 0; i < split.Length; ++i)
            {
                var s = split[i];
                if (i == 0 && s.Equals(Name))
                {
                    continue;
                }

                var nf = children.Find(m => m.Name.Equals(s));

                if (nf != null)
                {
                    parent = nf;
                    children = parent.GetShelfItems();
                }
                else
                {
                    return parent;
                }
            }

            return parent;
        }

        public static List<UINodeSource> Find(string query)
        {
            if (string.IsNullOrEmpty(query))
            {
                return new List<UINodeSource>();
            }

            query = query.ToLower();
            var n = nodes.FindAll(m => m.Path.ToLower().Contains(query) || m.Name.ToLower().Contains(query));
            if (n != null)
            {
                n.Sort((m1, m2) => m1.Name.CompareTo(m2.Name));
                return n;
            }

            return new List<UINodeSource>();
        }
    }
}
