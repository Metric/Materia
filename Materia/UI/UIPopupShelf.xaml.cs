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
using System.Windows.Shapes;
using System.Threading;
using Materia.UI.Components;

namespace Materia.UI
{
    /// <summary>
    /// Interaction logic for UIPopupShelf.xaml
    /// </summary>
    public partial class UIPopupShelf : Window
    {
        CancellationTokenSource ctk;
        public UIGraph Graph { get; set; }

        List<NodeResource> clones;

        bool scheduledToClose = false;

        public UIPopupShelf()
        {
            InitializeComponent();
            clones = new List<NodeResource>();
            HorizontalAlignment = HorizontalAlignment.Left;
            VerticalAlignment = VerticalAlignment.Top;
            Visibility = Visibility.Collapsed;
            Margin = new Thickness(0);
        }

        public void Open(double x, double y)
        {
            Top = y;
            Left = x - ActualWidth * 0.5;
            ClearFilters();
            Show();
        }

        private void PopulateView(string path)
        {
            var items = ShelfItem.Find(path);
            clones = new List<NodeResource>();
            ResourcesList.Items.Clear();

            foreach (var item in items)
            {
                clones.Add(item.Clone());
            }
            
            foreach(var item in clones)
            {
                item.MouseDown += Item_MouseDown;
                ResourcesList.Items.Add(item);
            }
        }

        private void Item_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                NodeResource src = sender as NodeResource;
                if (Graph != null)
                {
                    Graph.Insert(src.Type);
                }

                CloseView();
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

            if (ctk != null)
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
            PopulateView("Categories");
        }

        private void SearchBox_GotFocus(object sender, RoutedEventArgs e)
        {
            string s = SearchBox.Text;

            if (s.Equals(Properties.Resources.TITLE_SEARCH))
            {
                SearchBox.Text = "";
            }
        }

        private void SearchBox_LostFocus(object sender, RoutedEventArgs e)
        {
            string s = SearchBox.Text;

            if (string.IsNullOrWhiteSpace(s) || string.IsNullOrWhiteSpace(s))
            {
                SearchBox.Text = Properties.Resources.TITLE_SEARCH;
            }
        }

        private void Window_Deactivated(object sender, EventArgs e)
        {
            CloseView();
        }

        private void CloseView()
        {
            if (!scheduledToClose)
            {
                scheduledToClose = true;
                Close();
            }
        }

        private void Window_Activated(object sender, EventArgs e)
        {
            Focus();
        }
    }
}
