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
    /// Interaction logic for RecentShortcutItem.xaml
    /// </summary>
    public partial class RecentShortcutItem : UserControl
    {
        public delegate void Open(string path);
        public event Open OnOpen;

        public RecentShortcutItem()
        {
            InitializeComponent();
        }

        public RecentShortcutItem(string path)
        {
            InitializeComponent();
            Title.Text = path;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            OnOpen?.Invoke(Title.Text);
        }
    }
}
