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
        Action click;

        public BreadCrumb()
        {
            InitializeComponent();
        }

        public BreadCrumb(BreadCrumbs view, string name, Action clickAction)
        {
            InitializeComponent();
            CrumbName.Text = name;
            CrumbsView = view;
            click = clickAction;
            view.Add(this);
        }

        private void UserControl_MouseDown(object sender, MouseButtonEventArgs e)
        {
            CrumbsView.RemoveAfter(this);

            if(click != null)
            {
                click.Invoke();
            }
        }
    }
}
