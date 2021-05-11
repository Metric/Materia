using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System.Collections.Generic;
using Materia.Graph;
using System;
using Materia.Nodes.Atomic;
using MateriaCore.Components.GL;
using MateriaCore.Utils;

namespace MateriaCore.Components.Dialogs
{
    public class NewGraph : Window
    {
        public Graph Result { get; protected set; }

        ComboBox defaultFormat;
        ComboBox defaultNodeSize;
        ComboBox templateType;

        Button create;
        Button cancel;

        public NewGraph()
        {
            InitializeComponent();
            InitializeFormats();
        }

        private void InitializeEvents()
        {
            create.Click += Create_Click;
            cancel.Click += Cancel_Click;
        }

        private void Cancel_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            Result = null;
            Close(Result);
        }

        private void Create_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            GraphPixelType px;

            string spx = (defaultFormat.SelectedItem as ComboBoxItem).Content.ToString();

            if (!Enum.TryParse(spx, out px))
            {
                px = GraphPixelType.RGBA;
            }

            int sizeIndex = defaultNodeSize.SelectedIndex;
            int size = Graph.DEFAULT_SIZE;

            if (sizeIndex > -1 && sizeIndex < Graph.GRAPH_SIZES.Length)
            {
                size = Graph.GRAPH_SIZES[sizeIndex];
            }

            Materia.Graph.Image g = new Materia.Graph.Image("Untitled", size, size);
            g.DefaultTextureType = px;

            switch (templateType.SelectedIndex)
            {
                case 0:
                    GraphTemplate.PBRFull(g);
                    break;
                case 1:
                    GraphTemplate.PBRNoHeight(g);
                    break;
                case 2:
                    GraphTemplate.PBRNoHeightAO(g);
                    break;
                case 3:
                    GraphTemplate.PBRNoHeightAONormal(g);
                    break;
                default:
                    break;
            }

            Result = g;
            Close(Result);
        }

        private void InitializeFormats()
        {
            string[] names = Enum.GetNames(typeof(GraphPixelType));

            List<ComboBoxItem> items = new List<ComboBoxItem>();

            foreach (string s in names)
            {
                ComboBoxItem cbi = new ComboBoxItem();
                cbi.Content = s;
                items.Add(cbi);
            }

            defaultFormat.Items = items;
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            create = this.FindControl<Button>("CreateButton");
            cancel = this.FindControl<Button>("CancelButton");
            defaultFormat = this.FindControl<ComboBox>("DefaultFormat");
            defaultNodeSize = this.FindControl<ComboBox>("DefaultNodeSize");
            templateType = this.FindControl<ComboBox>("TemplateType");
        }
    }
}
