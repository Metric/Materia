using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.IO;
using Materia.UI.Components;
using Materia.Shelf;

namespace Materia.UI
{
    /// <summary>
    /// Interaction logic for UIShelf.xaml
    /// </summary>
    public partial class UIShelf : UserControl
    {
        CancellationTokenSource ctk;

        ShelfItem root;

        ShelfItem selected;
        string selectedPath;
        ShelfBuilder builder;
        

        public UIShelf()
        {
            InitializeComponent();
            builder = new ShelfBuilder();
            builder.OnBuildComplete += Builder_OnBuildComplete;
            ShelfItem.OnSelected += ShelfItem_OnSelected;
            root = new ShelfItem("Categories");
            root.Toggle();
            ShelfPaths.Content = root;
            builder.Build();
        }

        private void Builder_OnBuildComplete(ShelfBuilder builder)
        {
            var r = builder.Root;
            Stack<ShelfBuilder.ShelfBuilderItem> stack = new Stack<ShelfBuilder.ShelfBuilderItem>();
            stack.Push(r);

            while(stack.Count > 0)
            {
                var n = stack.Pop();
                var c = root.FindChild(n.Path);

                foreach(var resource in n.Nodes)
                {
                    NodeResource nr = new NodeResource();
                    nr.Title = resource.Title;
                    nr.Path = resource.Path;
                    nr.Type = resource.Type;
                    nr.ToolTip = ShelfDescriptions.Get(nr);
                    c.Add(nr);
                }

                List<ShelfBuilder.ShelfBuilderItem> children = n.Children;

                foreach(var child in children)
                {
                    ShelfItem sh = new ShelfItem(child.BaseName);
                    c.Add(sh);
                    stack.Push(child);
                }
            }

            if(IsLoaded)
            {
                if (string.IsNullOrEmpty(selectedPath))
                {
                    PopulateView("Categories");
                }
                else
                {
                    PopulateView(selectedPath);
                }
            }
        }

        private void ShelfItem_OnSelected(ShelfItem shelf, string path)
        {
            selected = shelf;
            selectedPath = path;
            PopulateView(path);
        }

        void PopulateView(string path)
        {
            ShelfContent.Children.Clear();
            var items = ShelfItem.Find(path);
            foreach(var i in items)
            {
                ShelfContent.Children.Add(i);
            }
        }

        //we eventually will need to save these settings and allow removal
        //from the shelf
        private void UserControl_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] path = (string[])e.Data.GetData(DataFormats.FileDrop);

                foreach (string p in path)
                {
                    string fname = Path.GetFileNameWithoutExtension(p);
                    string ext = Path.GetExtension(p);

                    if (ext.Equals(".mtg") || ext.Equals(".mtga"))
                    {
                        NodeResource nsr = new NodeResource();
                        nsr.Title = fname;
                        nsr.Type = p;
                        nsr.ToolTip = ShelfDescriptions.Get(nsr);
                        root.Add(nsr);
                    }
                }
            }
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string s = SearchBox.Text;

            if (!IsLoaded) return;

            if (string.IsNullOrEmpty(s) || string.IsNullOrWhiteSpace(s) || s.Equals("Search..."))
            {
                ClearFilters();
                return;
            }

            if(ctk != null)
            {
                ctk.Cancel();
            }

            ctk = new CancellationTokenSource();

            Task.Delay(250, ctk.Token).ContinueWith(t =>
            {
                if (t.IsCanceled) return;

                App.Current.Dispatcher.Invoke(() =>
                {
                    SetFilters(s);
                });
            });
        }

        private void SetFilters(string s)
        {
            s = s.ToLower();
            PopulateView(s);
        }

        private void ClearFilters()
        {
            if (string.IsNullOrEmpty(selectedPath))
            {
                PopulateView("Categories");
            }
            else
            {
                PopulateView(selectedPath);
            }
        }

        private void SearchBox_GotFocus(object sender, RoutedEventArgs e)
        {
            string s = SearchBox.Text;

            if(s.Equals("Search..."))
            {
                SearchBox.Text = "";
            }
        }

        private void SearchBox_LostFocus(object sender, RoutedEventArgs e)
        {
            string s = SearchBox.Text;

            if(string.IsNullOrWhiteSpace(s) || string.IsNullOrWhiteSpace(s))
            {
                SearchBox.Text = "Search...";
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(selectedPath))
            {
                PopulateView("Categories");
            }
            else
            {
                PopulateView(selectedPath);
            }
        }
    }
}
