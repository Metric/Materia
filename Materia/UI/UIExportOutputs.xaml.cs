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
using System.IO;
using Materia.Exporters;

namespace Materia.UI
{
    /// <summary>
    /// Interaction logic for UIExportOutputs.xaml
    /// </summary>
    public partial class UIExportOutputs : Window
    {
        protected enum ExportTypes
        {
            SeparateFiles,
            UnrealEngine4,
            Unity5
        }

        UIGraph graph;

        public UIExportOutputs()
        {
            InitializeComponent();
        }

        public UIExportOutputs(UIGraph g)
        {
            InitializeComponent();
            graph = g;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void Confirm_Click(object sender, RoutedEventArgs e)
        {
            if (ExportType.SelectedItems.Count == 0) return;

            string path = null;
            var dialog = new Microsoft.Win32.SaveFileDialog();
            dialog.Title = "Select a Directory"; // instead of default "Save As"
            dialog.Filter = "Directory|*.Folder"; // Prevents displaying files
            dialog.FileName = "Select"; // Filename will then be "select.this.directory"
            if (dialog.ShowDialog() == true)
            {
                path = dialog.FileName;
                // Remove fake filename from resulting path
                path = path.Replace("\\Select.Folder", "");
                path = path.Replace(".Folder", "");
                // If user has changed the filename, create the new directory
                if (!System.IO.Directory.Exists(path))
                {
                    System.IO.Directory.CreateDirectory(path);
                }
                // Our final value is in path
                
            }
            else
            {
                return;
            }

            foreach (ListBoxItem lb in ExportType.SelectedItems)
            {
                var s = lb.Content.ToString().Replace(" ", "");
                ExportTypes et = (ExportTypes)Enum.Parse(typeof(ExportTypes), s);
                ExportTextures(path, et);
            }
        }

        protected void ExportTextures(string path, ExportTypes type)
        {
            if (graph.Graph.OutputNodes.Count == 0)
            {
                MessageBox.Show("No image output nodes to export");
                return;
            }

            ProgressView.Visibility = Visibility.Visible;

            Task.Delay(10).ContinueWith(t =>
            {
                App.Current.Dispatcher.Invoke(() =>
                {
                    Exporter exporter = null;
                    switch (type)
                    {
                        case ExportTypes.SeparateFiles:
                            exporter = new SeparateExporter(graph.Graph);
                            break;
                        case ExportTypes.Unity5:
                            exporter = new Unity5Exporter(graph.Graph);
                            break;
                        case ExportTypes.UnrealEngine4:
                            exporter = new Unreal4Exporter(graph.Graph);
                            break;
                    }

                    if (exporter == null)
                    {
                        DialogResult = false;
                        return;
                    }

                    exporter.OnProgress += Exporter_OnProgress;
                    Task extask = exporter.Export(path);
                    Task.Run(async () =>
                    {
                        await extask;

                        App.Current.Dispatcher.Invoke(() =>
                        {
                            DialogResult = true;
                        });
                    });
                });
            });
        }

        private void Exporter_OnProgress(int current, int total, float progress)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                ExportProgress.Minimum = 0;
                ExportProgress.Maximum = total;
                ExportProgress.Value = current;
                ExportStatus.Text = current + " / " + total;
            });
        }
    }
}
