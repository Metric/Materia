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
using Materia.Nodes.Atomic;
using Materia.Nodes;
using Materia.Imaging;
using System.IO;

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
                    switch (type)
                    {
                        case ExportTypes.SeparateFiles:
                            ExportAsSeparate(path);
                            break;
                        case ExportTypes.Unity5:
                            ExportAsUnity5(path);
                            break;
                        case ExportTypes.UnrealEngine4:
                            ExportAsUnrealEngine4(path);
                            break;
                    }
                });
            });
        }

        protected void ExportAsSeparate(string path)
        {
            int i = 0;

            string name = graph.Graph.Name;

            ExportProgress.Minimum = 0;
            ExportProgress.Maximum = graph.Graph.OutputNodes.Count;

            foreach (var s in graph.Graph.OutputNodes)
            {
                Node n = null;

                if (graph.Graph.NodeLookup.TryGetValue(s, out n))
                {
                    if (n is OutputNode)
                    {
                        OutputNode on = n as OutputNode;

                        ExportStatus.Text = "Exporting " + on.OutType.ToString();

                        if (on.OutType == OutputType.basecolor)
                        {
                            RawBitmap bmp = null;
                            byte[] bits = on.GetPreview(on.Width, on.Height);

                            if (bits != null)
                            {
                                Task.Run(() =>
                                {
                                    bmp = new RawBitmap(on.Width, on.Height, bits);
                                    var src = bmp.ToImageSource();
                                    PngBitmapEncoder encoder = new PngBitmapEncoder();
                                    BitmapFrame frame = BitmapFrame.Create(src);
                                    encoder.Frames.Add(frame);

                                    using (FileStream fs = new FileStream(System.IO.Path.Combine(path, name + "_basecolor.png"), FileMode.OpenOrCreate))
                                    {
                                        encoder.Save(fs);
                                    }
                                });
                            }
                        }
                        else if (on.OutType == OutputType.normal)
                        {
                            RawBitmap bmp = null;
                            byte[] bits = on.GetPreview(on.Width, on.Height);

                            if (bits != null)
                            {
                                Task.Run(() =>
                                {
                                    bmp = new RawBitmap(on.Width, on.Height, bits);
                                    var src = bmp.ToImageSource();
                                    PngBitmapEncoder encoder = new PngBitmapEncoder();
                                    BitmapFrame frame = BitmapFrame.Create(src);
                                    encoder.Frames.Add(frame);

                                    using (FileStream fs = new FileStream(System.IO.Path.Combine(path, name + "_normal.png"), FileMode.OpenOrCreate))
                                    {
                                        encoder.Save(fs);
                                    }
                                });
                            }
                        }
                        else if (on.OutType == OutputType.metallic)
                        {
                            RawBitmap bmp = null;
                            byte[] bits = on.GetPreview(on.Width, on.Height);

                            if (bits != null)
                            {
                                Task.Run(() =>
                                {
                                    bmp = new RawBitmap(on.Width, on.Height, bits);
                                    var src = bmp.ToImageSource();
                                    PngBitmapEncoder encoder = new PngBitmapEncoder();
                                    BitmapFrame frame = BitmapFrame.Create(src);
                                    encoder.Frames.Add(frame);

                                    using (FileStream fs = new FileStream(System.IO.Path.Combine(path, name + "_metallic.png"), FileMode.OpenOrCreate))
                                    {
                                        encoder.Save(fs);
                                    }
                                });
                            }
                        }
                        else if (on.OutType == OutputType.roughness)
                        {
                            RawBitmap bmp = null;
                            byte[] bits = on.GetPreview(on.Width, on.Height);

                            if (bits != null)
                            {
                                Task.Run(() =>
                                {
                                    bmp = new RawBitmap(on.Width, on.Height, bits);
                                    var src = bmp.ToImageSource();
                                    PngBitmapEncoder encoder = new PngBitmapEncoder();
                                    BitmapFrame frame = BitmapFrame.Create(src);
                                    encoder.Frames.Add(frame);

                                    using (FileStream fs = new FileStream(System.IO.Path.Combine(path, name + "_roughness.png"), FileMode.OpenOrCreate))
                                    {
                                        encoder.Save(fs);
                                    }
                                });
                            }
                        }
                        else if (on.OutType == OutputType.occlusion)
                        {
                            RawBitmap bmp = null;
                            byte[] bits = on.GetPreview(on.Width, on.Height);

                            if (bits != null)
                            {
                                Task.Run(() =>
                                {
                                    bmp = new RawBitmap(on.Width, on.Height, bits);
                                    var src = bmp.ToImageSource();
                                    PngBitmapEncoder encoder = new PngBitmapEncoder();
                                    BitmapFrame frame = BitmapFrame.Create(src);
                                    encoder.Frames.Add(frame);

                                    using (FileStream fs = new FileStream(System.IO.Path.Combine(path, name + "_occlusion.png"), FileMode.OpenOrCreate))
                                    {
                                        encoder.Save(fs);
                                    }
                                });
                            }
                        }
                        else if (on.OutType == OutputType.height)
                        {
                            RawBitmap bmp = null;
                            byte[] bits = on.GetPreview(on.Width, on.Height);

                            if (bits != null)
                            {
                                Task.Run(() =>
                                {
                                    bmp = new RawBitmap(on.Width, on.Height, bits);
                                    var src = bmp.ToImageSource();
                                    PngBitmapEncoder encoder = new PngBitmapEncoder();
                                    BitmapFrame frame = BitmapFrame.Create(src);
                                    encoder.Frames.Add(frame);

                                    using (FileStream fs = new FileStream(System.IO.Path.Combine(path, name + "_height.png"), FileMode.OpenOrCreate))
                                    {
                                        encoder.Save(fs);
                                    }
                                });
                            }
                        }
                    }
                }

                i++;
                ExportProgress.Value = i;
            }

            DialogResult = true;
        }

        protected void ExportAsUnity5(string path)
        {
            RawBitmap mr = null;

            int i = 0;

            string name = graph.Graph.Name;

            Queue<Task> runningTasks = new Queue<Task>();

            foreach (var s in graph.Graph.OutputNodes)
            {
                Node n = null;

                if (graph.Graph.NodeLookup.TryGetValue(s, out n))
                {
                    if (n is OutputNode)
                    {
                        OutputNode on = n as OutputNode;

                        if (on.OutType == OutputType.basecolor)
                        {
                            RawBitmap bmp = null;
                            byte[] bits = on.GetPreview(on.Width, on.Height);

                            if (bits != null)
                            {
                                var t = Task.Run(() =>
                                {
                                    bmp = new RawBitmap(on.Width, on.Height, bits);
                                    var src = bmp.ToImageSource();
                                    PngBitmapEncoder encoder = new PngBitmapEncoder();
                                    BitmapFrame frame = BitmapFrame.Create(src);
                                    encoder.Frames.Add(frame);

                                    using (FileStream fs = new FileStream(System.IO.Path.Combine(path, "unity_" + name + "_basecolor.png"), FileMode.OpenOrCreate))
                                    {
                                        encoder.Save(fs);
                                    }
                                });
                                runningTasks.Enqueue(t);
                            }
                        }
                        else if (on.OutType == OutputType.normal)
                        {
                            RawBitmap bmp = null;
                            byte[] bits = on.GetPreview(on.Width, on.Height);

                            if (bits != null)
                            {
                                var t = Task.Run(() =>
                                {
                                    bmp = new RawBitmap(on.Width, on.Height, bits);
                                    var src = bmp.ToImageSource();
                                    PngBitmapEncoder encoder = new PngBitmapEncoder();
                                    BitmapFrame frame = BitmapFrame.Create(src);
                                    encoder.Frames.Add(frame);

                                    using (FileStream fs = new FileStream(System.IO.Path.Combine(path, "unity_" + name + "_normal.png"), FileMode.OpenOrCreate))
                                    {
                                        encoder.Save(fs);
                                    }
                                });
                                runningTasks.Enqueue(t);
                            }
                        }
                        else if (on.OutType == OutputType.metallic)
                        {
                            if (mr == null)
                            {
                                mr = new RawBitmap(on.Width, on.Height);
                            }

                            byte[] bits = on.GetPreview(on.Width, on.Height);

                            if (bits != null)
                            {
                                var t = Task.Run(() =>
                                {
                                    RawBitmap tmp = new RawBitmap(on.Width, on.Height, bits);
                                    mr.CopyRedToRed(tmp);
                                });
                                runningTasks.Enqueue(t);
                            }
                        }
                        else if (on.OutType == OutputType.roughness)
                        {
                            if (mr == null)
                            {
                                mr = new RawBitmap(on.Width, on.Height);
                            }

                            byte[] bits = on.GetPreview(on.Width, on.Height);

                            if (bits != null)
                            {
                                var t = Task.Run(() =>
                                {
                                    RawBitmap tmp = new RawBitmap(on.Width, on.Height, bits);
                                    mr.CopyRedToAlpha(tmp);
                                });
                                runningTasks.Enqueue(t);
                            }
                        }
                        else if (on.OutType == OutputType.occlusion)
                        {
                            RawBitmap bmp = null;
                            byte[] bits = on.GetPreview(on.Width, on.Height);

                            if (bits != null)
                            {
                                var t = Task.Run(() =>
                                {
                                    bmp = new RawBitmap(on.Width, on.Height, bits);
                                    var src = bmp.ToImageSource();
                                    PngBitmapEncoder encoder = new PngBitmapEncoder();
                                    BitmapFrame frame = BitmapFrame.Create(src);
                                    encoder.Frames.Add(frame);

                                    using (FileStream fs = new FileStream(System.IO.Path.Combine(path, "unity_" + name + "_occlusion.png"), FileMode.OpenOrCreate))
                                    {
                                        encoder.Save(fs);
                                    }
                                });
                                runningTasks.Enqueue(t);
                            }
                        }
                        else if (on.OutType == OutputType.height)
                        {
                            RawBitmap bmp = null;
                            byte[] bits = on.GetPreview(on.Width, on.Height);

                            if (bits != null)
                            {
                                var t = Task.Run(() =>
                                {
                                    bmp = new RawBitmap(on.Width, on.Height, bits);
                                    var src = bmp.ToImageSource();
                                    PngBitmapEncoder encoder = new PngBitmapEncoder();
                                    BitmapFrame frame = BitmapFrame.Create(src);
                                    encoder.Frames.Add(frame);

                                    using (FileStream fs = new FileStream(System.IO.Path.Combine(path, "unity_" + name + "_height.png"), FileMode.OpenOrCreate))
                                    {
                                        encoder.Save(fs);
                                    }
                                });
                                runningTasks.Enqueue(t);
                            }
                        }
                    }
                }
            }

            int totalTasks = runningTasks.Count;

            ExportProgress.Minimum = 0;
            ExportProgress.Maximum = totalTasks;

            Task.Run(async () =>
            {
                while(runningTasks.Count > 0)
                {
                    i = totalTasks - runningTasks.Count + 1;

                    Task t = runningTasks.Dequeue();

                    App.Current.Dispatcher.Invoke(() =>
                    {
                        ExportStatus.Text = "Exporting " + i + " / " + totalTasks;

                        ExportProgress.Value = i;
                    });

                    if(!t.IsCompleted && !t.IsCanceled)
                    {
                        await t;
                    }
                }

                if (mr != null)
                {
                    App.Current.Dispatcher.Invoke(() =>
                    {
                        ExportStatus.Text = "Finalizing";
                    });

                    var src = mr.ToImageSource();
                    PngBitmapEncoder encoder = new PngBitmapEncoder();
                    BitmapFrame frame = BitmapFrame.Create(src);
                    encoder.Frames.Add(frame);

                    using (FileStream fs = new FileStream(System.IO.Path.Combine(path, "unity_" + name + "_ms.png"), FileMode.OpenOrCreate))
                    {
                        encoder.Save(fs);
                    }
                }

                App.Current.Dispatcher.Invoke(() =>
                {
                    DialogResult = true;
                });
            });
        }

        protected void ExportAsUnrealEngine4(string path)
        {
            RawBitmap mroh = null;

            int i = 0;

            string name = graph.Graph.Name;

            Queue<Task> runningTasks = new Queue<Task>();

            foreach (var s in graph.Graph.OutputNodes)
            {
                Node n = null;

                if(graph.Graph.NodeLookup.TryGetValue(s, out n))
                {
                    if(n is OutputNode)
                    {
                        OutputNode on = n as OutputNode;

                        ExportStatus.Text = "Exporting " + on.OutType.ToString();

                        if (on.OutType == OutputType.basecolor)
                        {
                            RawBitmap bmp = null;
                            byte[] bits = on.GetPreview(on.Width, on.Height);

                            if(bits != null)
                            {
                                var t = Task.Run(() =>
                                {
                                    bmp = new RawBitmap(on.Width, on.Height, bits);
                                    var src = bmp.ToImageSource();
                                    PngBitmapEncoder encoder = new PngBitmapEncoder();
                                    BitmapFrame frame = BitmapFrame.Create(src);
                                    encoder.Frames.Add(frame);

                                    using (FileStream fs = new FileStream(System.IO.Path.Combine(path, "ue_" + name + "_basecolor.png"), FileMode.OpenOrCreate))
                                    {
                                        encoder.Save(fs);
                                    }
                                });
                                runningTasks.Enqueue(t);
                            }
                        }
                        else if(on.OutType == OutputType.normal)
                        {
                            RawBitmap bmp = null;
                            byte[] bits = on.GetPreview(on.Width, on.Height);

                            if (bits != null)
                            {

                                var t = Task.Run(() =>
                                {
                                    bmp = new RawBitmap(on.Width, on.Height, bits);
                                    var src = bmp.ToImageSource();
                                    PngBitmapEncoder encoder = new PngBitmapEncoder();
                                    BitmapFrame frame = BitmapFrame.Create(src);
                                    encoder.Frames.Add(frame);

                                    using (FileStream fs = new FileStream(System.IO.Path.Combine(path, "ue_" + name + "_normal.png"), FileMode.OpenOrCreate))
                                    {
                                        encoder.Save(fs);
                                    }
                                });
                                runningTasks.Enqueue(t);
                            }
                        }
                        else if(on.OutType == OutputType.metallic)
                        {
                            if(mroh == null)
                            {
                                mroh = new RawBitmap(on.Width, on.Height);
                            }

                            byte[] bits = on.GetPreview(on.Width, on.Height);

                            if(bits != null)
                            {
                                var t = Task.Run(() =>
                                {
                                    RawBitmap tmp = new RawBitmap(on.Width, on.Height, bits);
                                    mroh.CopyRedToBlue(tmp);
                                });
                                runningTasks.Enqueue(t);
                            }
                        }
                        else if(on.OutType == OutputType.roughness)
                        {
                            if (mroh == null)
                            {
                                mroh = new RawBitmap(on.Width, on.Height);
                            }

                            byte[] bits = on.GetPreview(on.Width, on.Height);

                            if (bits != null)
                            {
                                var t = Task.Run(() =>
                                {
                                   RawBitmap tmp = new RawBitmap(on.Width, on.Height, bits);
                                   mroh.CopyRedToGreen(tmp);
                                });
                                runningTasks.Enqueue(t);
                            }
                        }
                        else if(on.OutType == OutputType.occlusion)
                        {
                            if (mroh == null)
                            {
                                mroh = new RawBitmap(on.Width, on.Height);
                            }

                            byte[] bits = on.GetPreview(on.Width, on.Height);

                            if (bits != null)
                            {
                                var t = Task.Run(() =>
                                {
                                    RawBitmap tmp = new RawBitmap(on.Width, on.Height, bits);
                                    mroh.CopyRedToRed(tmp);
                                });
                                runningTasks.Enqueue(t);
                            }
                        }
                        else if(on.OutType == OutputType.height)
                        {
                            if (mroh == null)
                            {
                                mroh = new RawBitmap(on.Width, on.Height);
                            }

                            byte[] bits = on.GetPreview(on.Width, on.Height);

                            if (bits != null)
                            {
                                var t  = Task.Run(() =>
                                {
                                    RawBitmap tmp = new RawBitmap(on.Width, on.Height, bits);
                                    mroh.CopyRedToAlpha(tmp);
                                });
                                runningTasks.Enqueue(t);
                            }
                        }
                    }
                }
            }

            int totalTasks = runningTasks.Count;

            ExportProgress.Minimum = 0;
            ExportProgress.Maximum = totalTasks;

            Task.Run(async () =>
            {
                while (runningTasks.Count > 0)
                {
                    i = totalTasks - runningTasks.Count + 1;

                    Task t = runningTasks.Dequeue();

                    App.Current.Dispatcher.Invoke(() =>
                    {
                        ExportStatus.Text = "Exporting " + i + " / " + totalTasks;

                        ExportProgress.Value = i;
                    });

                    if (!t.IsCompleted && !t.IsCanceled)
                    {
                        await t;
                    }
                }

                if (mroh != null)
                {
                    App.Current.Dispatcher.Invoke(() =>
                    {
                        ExportStatus.Text = "Finalizing";
                    });

                    var src = mroh.ToImageSource();
                    PngBitmapEncoder encoder = new PngBitmapEncoder();
                    BitmapFrame frame = BitmapFrame.Create(src);
                    encoder.Frames.Add(frame);

                    using (FileStream fs = new FileStream(System.IO.Path.Combine(path, "ue_" + name + "_ms.png"), FileMode.OpenOrCreate))
                    {
                        encoder.Save(fs);
                    }
                }

                App.Current.Dispatcher.Invoke(() =>
                {
                    DialogResult = true;
                });
            });
        }
    }
}
