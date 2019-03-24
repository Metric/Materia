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
    /// Interaction logic for BreadCrumb.xaml
    /// </summary>
    public partial class BreadCrumb : UserControl
    {
        protected BreadCrumbs CrumbsView;
        UIGraph graph;
        public string Id { get; protected set; }

        public BreadCrumb()
        {
            InitializeComponent();
        }

        public BreadCrumb(BreadCrumbs view, string name, UIGraph g, string node)
        {
            InitializeComponent();
            CrumbName.Text = name;
            CrumbsView = view;
            graph = g;
            Id = node;
            view.Add(this);
        }

        private void UserControl_MouseDown(object sender, MouseButtonEventArgs e)
        {
            CrumbsView.RemoveAfter(this);

            graph.PopTo(Id);
        }
    }
}
