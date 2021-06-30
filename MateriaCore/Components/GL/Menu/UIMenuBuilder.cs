using InfinityUI.Components.Layout;
using InfinityUI.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MateriaCore.Components.GL.Menu
{
    public class UIMenuBuilder
    {
        protected UIMenu menu;

        protected Stack<UIMenu> submenus = new Stack<UIMenu>();

        public UIMenuBuilder()
        {
            menu = new UIMenu();
        }

        public UIMenuBuilder Add(string title, Action<Button> click = null)
        {
            UIMenuItem item = new UIMenuItem(title);
            if (click != null)
            {
                item.Submit += click;
            }
            item.ShowSubMenuArrow = submenus.Count > 0;
            menu.AddChild(item);
            return this;
        }

        public UIMenuBuilder DynamicSubMenu(Func<List<UIMenuItem>> dataSource)
        {
            var sub = new UIMenu();
            var menuItem = (menu.Children.Last() as UIMenuItem);
            sub.Visible = false;
            sub.Direction = InfinityUI.Core.Orientation.Vertical;
            sub.ChildAlignment = InfinityUI.Core.Anchor.TopHorizFill;
            if (submenus.Count == 1)
            {
                menuItem.SubMenuAnchor = InfinityUI.Core.Anchor.Bottom;
            }
            else if (submenus.Count > 1)
            {
                menuItem.SubMenuAnchor = InfinityUI.Core.Anchor.Right;
            }
            menuItem.SubMenu = sub;
            menuItem.Focused += (a,b) =>
            {
                //clear previous children
                for (int i = 0; i < sub.Children.Count; ++i)
                {
                    var child = sub.Children[i];
                    if (child == null) continue;
                    sub.RemoveChild(child);
                    child.Dispose();
                    --i;
                }

                var newItems = dataSource?.Invoke();
                for (int i = 0; i < newItems.Count; ++i)
                {
                    sub.AddChild(newItems[i]);
                }
            };
            return this;
        }

        public UIMenuBuilder Separator()
        {
            menu.AddChild(new UIMenuSeparator());
            return this;
        }

        public UIMenuBuilder StartSubMenu()
        {
            submenus.Push(menu);
            var sub = new UIMenu();
            var menuItem = (menu.Children.Last() as UIMenuItem);
            menu = sub;
            menu.Visible = false;
            menu.Direction = InfinityUI.Core.Orientation.Vertical;
            menu.ChildAlignment = InfinityUI.Core.Anchor.TopHorizFill;
            if (submenus.Count == 1)
            {
                menuItem.SubMenuAnchor = InfinityUI.Core.Anchor.Bottom;
            }
            else if (submenus.Count > 1)
            {
                menuItem.SubMenuAnchor = InfinityUI.Core.Anchor.Right;
            }
            menuItem.SubMenu = sub;
            return this;
        }

        public UIMenuBuilder FinishSubMenu()
        {
            if (submenus.Count == 0) return this;
            menu = submenus.Pop();
            return this;
        }

        public UIMenu Finilize()
        {
            while (submenus.Count > 0)
            {
                menu = submenus.Pop();
            }

            return menu;
        }
    }
}
