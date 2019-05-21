using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Materia.UI.Components
{
    /// <summary>
    /// Interaction logic for ShelfItem.xaml
    /// </summary>
    public partial class ShelfItem : UserControl
    {
        public delegate void Selected(ShelfItem shelf, string path);
        public static event Selected OnSelected;

        public string Path { get; protected set; }
        public string BaseName { get; protected set; }

        public bool Expanded { get; protected set; }

        protected List<ShelfItem> children;
        public List<ShelfItem> Children
        {
            get
            {
                return children.ToList();
            }
        }
        public ShelfItem ShelfParent { get; protected set; }

        protected static List<NodeResource> nodes = new List<NodeResource>();

        public ShelfItem()
        {
            InitializeComponent();
            children = new List<ShelfItem>();
        }

        public ShelfItem(string name)
        {
            InitializeComponent();
            BaseName = name;
            Path = BaseName;
            Title.Text = BaseName;
            children = new List<ShelfItem>();
        }

        public void Add(ShelfItem item)
        {
            More.Visibility = Visibility.Visible;
            item.Path = System.IO.Path.Combine(Path, item.Path);
            item.ShelfParent = this;
            children.Add(item);
            Items.Children.Add(item);
            UpdateExpandIcon();
        }

        public void Add(NodeResource nr)
        {
            nr.Path = Path;
            nodes.Add(nr);
        }

        public void Remove(NodeResource nr)
        {
            if(nodes.Remove(nr))
            {
                nr.Path = "";
            }
        }

        public void Remove(ShelfItem item)
        {
            bool contained = children.Remove(item);
            if (contained)
            {
                item.ShelfParent = null;
                Items.Children.Remove(item);
                item.Path = item.BaseName;

                if (Items.Children.Count == 0)
                {
                    More.Visibility = Visibility.Collapsed;
                    Expanded = false;
                    UpdateExpandIcon();
                }
            }
        }

        public static List<NodeResource> Find(string query)
        {
            if(string.IsNullOrEmpty(query))
            {
                return new List<NodeResource>();
            }

            query = query.ToLower();
            var n = nodes.FindAll(m => m.Path.ToLower().Contains(query) || m.Title.ToLower().Contains(query));
            if(n != null)
            {
                n.Sort((m1, m2) =>
                {
                    return m1.Title.CompareTo(m2.Title);
                });

                return n;
            }

            return new List<NodeResource>();
        }

        public ShelfItem FindChild(string path)
        {
            if (string.IsNullOrEmpty(path)) return this;

            ShelfItem parent = this;
            string[] split = path.Split(new string[] { System.IO.Path.DirectorySeparatorChar + "" }, StringSplitOptions.RemoveEmptyEntries);

            int i = 0;
            for (i = 0; i < split.Length; i++)
            {
                var s = split[i];
                if (i == 0 && s.Equals(BaseName))
                {
                    continue;
                }

                var nf = parent.children.Find(m => m.BaseName.Equals(s));

                if (nf != null)
                {
                    parent = nf;
                }
                else
                {
                    return parent;
                }
            }

            return parent;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            OnSelected += ShelfItem_OnSelected;
        }

        private void ShelfItem_OnSelected(ShelfItem shelf, string path)
        {
            if(shelf == this)
            {
                Title.Background = new SolidColorBrush(Color.FromArgb(255, 10, 10, 10));
            }
            else
            {
                Title.Background = new SolidColorBrush(Colors.Transparent);
            }
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            OnSelected -= ShelfItem_OnSelected;
        }

        private void Expander_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if(e.LeftButton == MouseButtonState.Pressed)
            {
                Expanded = !Expanded;
                UpdateExpandIcon();
            }
        }

        public void Toggle()
        {
            Expanded = !Expanded;
            UpdateExpandIcon();
        }

        void UpdateExpandIcon()
        {
            if (Expanded)
            {
                More.Text = "-";
                Items.Visibility = Visibility.Visible;
            }
            else
            {
                More.Text = "+";
                Items.Visibility = Visibility.Collapsed;
            }
        }

        private void Title_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if(OnSelected != null)
            {
                OnSelected.Invoke(this, this.Path);
            }
        }
    }
}
