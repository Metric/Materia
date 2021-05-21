using Materia.Rendering.Mathematics;
using InfinityUI.Core;
using InfinityUI.Components;
using InfinityUI.Controls;
using InfinityUI.Components.Layout;
using System;
using System.Collections.Generic;
using System.Text;
using MateriaCore.Utils;

namespace MateriaCore.Components.GL
{
    public class UIShelf : UIWindow
    {
        #region components
        protected InfinityUI.Controls.TextInput searchInput;

        protected UIObject shelfCategories;
        protected UIShelfSourceView shelfSourceView;
        protected UIObject shelfCategoriesView;
        protected UIScrollPanel shelfCategoriesScrollView;
        protected InfinityUI.Controls.Slider shelfCatergoiesScrollBar;

        protected UIShelfItem root;
        #endregion

        public UIShelf() : base(new Vector2(768, 512), "Shelf")
        {
            RelativeTo = Anchor.BottomLeft;
            InitializeComponents();
            BuildShelf();
        }

        private void BuildShelf()
        {
            ShelfBuilder builder = new ShelfBuilder();
            builder.OnBuildComplete += Builder_OnBuildComplete;
            builder.Build();
        }

        private void Builder_OnBuildComplete(ShelfBuilder builder)
        {
            var r = builder.Root;
            Stack<ShelfBuilder.ShelfBuilderItem> stack = new Stack<ShelfBuilder.ShelfBuilderItem>();
            stack.Push(r);

            while (stack.Count > 0)
            {
                var n = stack.Pop();
                var c = root.FindChild(n.Path);

                foreach (var resource in n.Nodes)
                {
                    UINodeSource nr = new UINodeSource(resource.Title) 
                    {
                        Path = resource.Path,
                        Type = resource.Type,
                    };
                    nr.Tooltip = ShelfDescriptions.Get(nr);

                    c.Add(nr);
                }

                List<ShelfBuilder.ShelfBuilderItem> children = n.Children;

                foreach (var child in children)
                {
                    UIShelfItem sh = new UIShelfItem(child.Name);
                    sh.Selected += Sh_Selected;
                    c.Add(sh);
                    stack.Push(child);
                }
            }

            root.Expanded = true;
            root.Selected += Sh_Selected;
            shelfSourceView.Filter = "Categories";
        }

        private void Sh_Selected(UIShelfItem obj)
        {
            shelfSourceView.Filter = obj.Name;
        }

        private void InitializeComponents()
        {
            searchInput = new InfinityUI.Controls.TextInput(16, new Vector2(512, 32), 0, UI.GetEmbeddedImage(Icons.CLOSE, typeof(UIShelf)))
            {
                RelativeTo = Anchor.TopHorizFill,
                Placeholder = "Search..."
            };
            searchInput.OnSubmit += SearchInput_OnSubmit;
            searchInput.OnClear += SearchInput_OnClear;
            searchInput.Background.Color = new Vector4(0.025f, 0.025f, 0.025f, 1);

            #region shelf categories area
            shelfCategories = new UIObject
            {
                Size = new Vector2(384, 1),
                RelativeTo = Anchor.LeftVerticalFill,
                Margin = new Box2(0,32,0,0),
                RaycastTarget = true,
            };
            var cBackground = shelfCategories.AddComponent<UIImage>();
            cBackground.Color = new Vector4(0.12f, 0.12f, 0.12f, 1);
            cBackground.Clip = true;

            shelfCategoriesScrollView = shelfCategories.AddComponent<UIScrollPanel>();
            shelfCategoriesScrollView.ScrollStep = 32;
            shelfCategoriesView = shelfCategoriesScrollView.View;
            shelfCategoriesView.GetComponent<UIContentFitter>().Axis = Axis.Vertical;
            shelfCategoriesView.RelativeTo = Anchor.TopHorizFill;

            shelfCategoriesScrollView.MaximumOffsetChanged += ShelfCategoriesScrollView_MaximumOffsetChanged;
            shelfCategoriesScrollView.Scrolled += ShelfCategoriesScrollView_Scrolled;

            shelfCatergoiesScrollBar = new InfinityUI.Controls.Slider(new Vector2(8, 1))
            {
                RelativeTo = Anchor.RightVerticalFill,
                Direction = Orientation.Vertical,
                StepSize = 32,
                Max = 0,
                Min = 0,
                Visible = false,
            };
            shelfCatergoiesScrollBar.ValueChanged += ShelfCatergoiesScrollBar_ValueChanged;

            shelfCategories.AddChild(shelfCatergoiesScrollBar);

            root = new UIShelfItem("Categories");
            shelfCategoriesView.AddChild(root);
            #endregion

            shelfSourceView = new UIShelfSourceView()
            {
                Size = new Vector2(384, 1),
                RelativeTo = Anchor.RightVerticalFill,
                Margin = new Box2(0,32,0,0),
            };
          
            content.AddChild(searchInput);
            content.AddChild(shelfSourceView);
            content.AddChild(shelfCategories);
        }

        private void SearchInput_OnClear(InfinityUI.Controls.TextInput obj)
        {
            shelfSourceView.Filter = "Categories";
        }

        private void ShelfCatergoiesScrollBar_ValueChanged(float v)
        {
            if (shelfCatergoiesScrollBar.Max > 0)
            {
                shelfCategoriesScrollView.NormalizedOffset = new Vector2(0, v / shelfCatergoiesScrollBar.Max);
            }
        }

        private void ShelfCategoriesScrollView_Scrolled(UIScrollPanel obj)
        {
            float y = obj.NormalizedOffset.Y;
            shelfCatergoiesScrollBar.Assign(y * shelfCatergoiesScrollBar.Max);  
        }

        private void ShelfCategoriesScrollView_MaximumOffsetChanged(UIScrollPanel obj)
        {
            shelfCatergoiesScrollBar.Max = obj.MaximumOffset.Y;
            shelfCatergoiesScrollBar.Visible = shelfCatergoiesScrollBar.Max != 0;
        }

        private void SearchInput_OnSubmit(InfinityUI.Controls.TextInput obj)
        {
            if (shelfSourceView == null) return;
            shelfSourceView.Filter = string.IsNullOrEmpty(searchInput.Text) ? "Categories" : searchInput.Text; 
        }
    }
}
