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
using System.Reflection;

namespace Materia.UI.Components
{
    /// <summary>
    /// Interaction logic for FileSelector.xaml
    /// </summary>
    public partial class FileSelector : UserControl
    {
        PropertyInfo property;
        object propertyOwner;
        string filter;

        public FileSelector()
        {
            InitializeComponent();
        }

        public FileSelector(PropertyInfo p, object owner, string filter)
        {
            InitializeComponent();

            property = p;
            propertyOwner = owner;

            string cpath = (string)p.GetValue(owner);

            if (!string.IsNullOrEmpty(cpath))
            {
                PathLabel.Text = System.IO.Path.GetFileName(cpath);
            }
            else
            {
                PathLabel.Text = "";
            }

            this.filter = filter;
        }

        private void SelectFile_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog opf = new System.Windows.Forms.OpenFileDialog();
            opf.CheckFileExists = true;
            opf.CheckPathExists = true;
            opf.Filter = filter;
            opf.Multiselect = false;
            if (opf.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string path = opf.FileName;

                PathLabel.Text = System.IO.Path.GetFileName(path);

                property.SetValue(propertyOwner, path);
            }
        }

        private void ClearFile_Click(object sender, RoutedEventArgs e)
        {
            PathLabel.Text = "";
            property.SetValue(propertyOwner, "");
        }
    }
}
