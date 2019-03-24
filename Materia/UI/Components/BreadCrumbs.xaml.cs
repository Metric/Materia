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
    /// Interaction logic for BreadCrumbs.xaml
    /// </summary>
    public partial class BreadCrumbs : UserControl
    {
        List<BreadCrumb> crumbs;

        public BreadCrumbs()
        {
            InitializeComponent();
            crumbs = new List<BreadCrumb>();
        }

        public void Add(BreadCrumb c)
        {
            crumbs.Add(c);
            CrumbStack.Children.Add(c);
        }

        public bool Contains(string id)
        {
            return crumbs.Find(m => !string.IsNullOrEmpty(m.Id) && m.Id.Equals(id)) != null;
        }

        public void Clear()
        {
            crumbs.Clear();
            CrumbStack.Children.Clear();
        }

        public void RemoveAfter(BreadCrumb c)
        {
            bool foundCrumb = false;
            for(int i = 0; i < crumbs.Count; i++)
            {
                if (!foundCrumb)
                {
                    if (crumbs[i] == c)
                    {
                        foundCrumb = true;
                    }
                }
                else
                {
                    CrumbStack.Children.Remove(crumbs[i]);
                    crumbs.RemoveAt(i);
                    i--;
                }
            }
        }
    }
}
