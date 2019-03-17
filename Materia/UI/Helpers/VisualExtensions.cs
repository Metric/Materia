using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Materia.UI.Helpers
{
    public static class VisualExtensions
    {
        public static Visual FindAncestor(this Visual child, Type typeAncestor)
        {
            DependencyObject parent = VisualTreeHelper.GetParent(child);

            while (parent != null && !typeAncestor.IsInstanceOfType(parent))
            {

                parent = VisualTreeHelper.GetParent(parent);

            }

            return (parent as Visual);
        }

        public static Visual FindAncestor(this Button child, Type typeAncestor)
        {
            DependencyObject parent = VisualTreeHelper.GetParent(child);

            while (parent != null && !typeAncestor.IsInstanceOfType(parent))
            {

                parent = VisualTreeHelper.GetParent(parent);

            }

            return (parent as Visual);
        }

    }
}
