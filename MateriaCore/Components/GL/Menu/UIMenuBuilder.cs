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
