using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace MateriaCore.Components
{
    public class FileSelector : UserControl
    {
        Button selectFile;
        Button clearFile;

        TextBlock pathLabel;

        PropertyInfo property;
        object propertyOwner;

        string filter;
        List<FileDialogFilter> filters;

        public FileSelector()
        {
            this.InitializeComponent();
            selectFile.Click += SelectFile_Click;
            clearFile.Click += ClearFile_Click;
        }

        public FileSelector(PropertyInfo p, object owner, string filter) : this()
        {
            property = p;
            propertyOwner = owner;

            string cpath = (string)p.GetValue(owner);

            if(!string.IsNullOrEmpty(cpath))
            {
                pathLabel.Text = System.IO.Path.GetFileName(cpath);
            }
            else
            {
                pathLabel.Text = "";
            }

            this.filter = filter;
            ParseFilter();
        }

        private void ClearFile_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            property?.SetValue(propertyOwner, "");
            pathLabel.Text = "";
        }

        private void SelectFile_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            OpenFileDialog opf = new OpenFileDialog();
            opf.AllowMultiple = false;
            opf.Filters = filters;
            opf.Title = "Select File";

            Task<string[]> resultor = opf.ShowAsync(MainWindow.Instance);
            string[] results = null;
            Task.Run(async () =>
            {
                results = await resultor;
            })
            .ContinueWith(t =>
            {
                if (results == null || results.Length == 0)
                {
                    return;
                }

                string path = results[0];
                pathLabel.Text = System.IO.Path.GetFileName(path);
                property?.SetValue(propertyOwner, path);
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        void ParseFilter()
        {
            filters = new List<FileDialogFilter>();
          
            if (string.IsNullOrEmpty(filter))
            {
                return;
            }

            FileDialogFilter f = new FileDialogFilter();

            string[] split = filter.Split('|');
            f.Name = split[0];

            if (split.Length >= 2)
            {
                string[] ext = split[1].Split(';');
                f.Extensions = new List<string>(ext);
            }

            filters.Add(f);
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            selectFile = this.FindControl<Button>("SelectFile");
            clearFile = this.FindControl<Button>("ClearFile");
            pathLabel = this.FindControl<TextBlock>("PathLabel");
        }
    }
}
