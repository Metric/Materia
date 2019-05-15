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

namespace Materia.UI
{
    /// <summary>
    /// Interaction logic for UIShelf.xaml
    /// </summary>
    public partial class UIShelf : UserControl
    {
        protected class TItem
        {
            protected Dictionary<string, TItem> Items { get; set; }
            public TreeViewItem Item { get; protected set;  }

            public TItem(TreeViewItem r)
            {
                Item = r;
                Items = new Dictionary<string, TItem>();
            }

            public void Add(string k, TItem t)
            {
                Items[k] = t;
            }

            public TItem Get(string k)
            {
                TItem t = null;

                Items.TryGetValue(k, out t);

                return t;
            }
        }

        CancellationTokenSource ctk;

        TItem root;

        public UIShelf()
        {
            InitializeComponent();
            root = new TItem(null);
            LoadShelf();
        }

        void LoadShelf()
        {
            string dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Shelf");

            if (Directory.Exists(dir))
            {
                string[] path = Directory.GetFiles(dir, "*.mtg", SearchOption.AllDirectories);

                List<string> sorter = new List<string>();
                Dictionary<string, string> lookup = new Dictionary<string, string>();

                foreach (string p in path)
                {
                    string fname = Path.GetFileNameWithoutExtension(p);

                    if (Path.GetExtension(p).Equals(".mtg"))
                    {
                        sorter.Add(fname);
                        lookup[fname] = p;
                    }
                }

                sorter.Sort();

                foreach(string fname in sorter)
                {
                    string p = null;

                    if (lookup.TryGetValue(fname, out p))
                    {
                        NodeResource nsr = new NodeResource();
                        nsr.Title = fname;
                        nsr.Type = p;
                        string structure = p.Replace(dir, "");
                        if (structure.IndexOf(Path.DirectorySeparatorChar) > -1) 
                        {
                            TItem parent = root;

                            string[] split = structure.Split(new char[] { Path.DirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries);

                            for(int i = 0; i < split.Length - 1; i++)
                            {
                                var s = split[i];
                                var t = parent.Get(s);

                                if (t == null)
                                {
                                    TreeViewItem it = new TreeViewItem();
                                    it.Foreground = new SolidColorBrush(Colors.LightGray);
                                    it.Header = s;
                                    t = new TItem(it);

                                    if (parent == root || parent == null)
                                    {
                                        TreeList.Items.Add(it);
                                    }
                                    else
                                    {
                                        parent.Item.Items.Add(it);
                                    }

                                    parent.Add(s, t);
                                }

                                parent = t;
                            }


                            if(parent == root || parent == null)
                            {
                                TreeList.Items.Add(nsr);
                            }
                            else
                            {
                                parent.Item.Items.Add(nsr);
                            }
                        }
                        else
                        {
                            TreeList.Items.Add(nsr);
                        }
                    }
                }
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

                    if (Path.GetExtension(p).Equals(".mtg"))
                    {
                        NodeResource nsr = new NodeResource();
                        nsr.Title = fname;
                        nsr.Type = p;
                        TreeList.Items.Add(nsr);
                    }
                }
            }
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string s = SearchBox.Text;

            if (TreeList == null || TreeList.Items == null) return;

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

            Predicate<object> fn = (object b) =>
            {
                if (b is TreeViewItem)
                {
                    return true;
                }
                else if (b is NodeResource)
                {
                    NodeResource nsr = (NodeResource)b;

                    if (nsr.Title.ToLower().Contains(s))
                    {
                        return true;
                    }
                }

                return false;
            };

            TreeList.Items.Filter = fn;

            Stack<ItemCollection> stack = new Stack<ItemCollection>();
            stack.Push(TreeList.Items);

            while (stack.Count > 0)
            {
                ItemCollection c = stack.Pop();

                foreach (object o in c)
                {
                    if (o is TreeViewItem)
                    {
                        TreeViewItem t = (TreeViewItem)o;

                        t.Items.Filter = fn;
                        stack.Push(t.Items);
                    }
                }
            }
        }

        private void ClearFilters()
        {
            TreeList.Items.Filter = null;

            Stack<ItemCollection> stack = new Stack<ItemCollection>();
            stack.Push(TreeList.Items);

            while (stack.Count > 0)
            {
                ItemCollection c = stack.Pop();

                foreach (object o in c)
                {
                    if (o is TreeViewItem)
                    {
                        TreeViewItem t = (TreeViewItem)o;

                        t.Items.Filter = null;
                        stack.Push(t.Items);
                    }
                }
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
    }
}
