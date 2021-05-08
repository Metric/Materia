using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System.Collections.Generic;
using Materia.Graph;
using System;
using Materia.Nodes.Atomic;
using MateriaCore.Components.GL;

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
            Result = g;

            switch (templateType.SelectedIndex)
            {
                case 0:
                    PBRFullTemplate();
                    break;
                case 1:
                    PBRNoHeightTemplate();
                    break;
                case 2:
                    PBRNoHeightAOTemplate();
                    break;
                case 3:
                    PBRNoHeightAONormalTemplate();
                    break;
                default:
                    break;
            }

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

        private void PBRFullTemplate()
        {
            OutputNode baseColor = new OutputNode(Result.DefaultTextureType);
            baseColor.Name = "Base Color";
            baseColor.ViewOriginY = -(UINode.DEFAULT_HEIGHT + 5) * 3;

            Result.Add(baseColor);

            OutputNode metallic = new OutputNode(Result.DefaultTextureType);
            metallic.Name = "Metallic";
            metallic.OutType = OutputType.metallic;
            metallic.ViewOriginY = -(UINode.DEFAULT_HEIGHT + 5) * 2;

            Result.Add(metallic);

            OutputNode roughness = new OutputNode(Result.DefaultTextureType);
            roughness.Name = "Roughness";
            roughness.OutType = OutputType.roughness;
            roughness.ViewOriginY = -(UINode.DEFAULT_HEIGHT + 5);

            Result.Add(roughness);

            OutputNode normal = new OutputNode(Result.DefaultTextureType);
            normal.Name = "Normal";
            normal.OutType = OutputType.normal;
            normal.ViewOriginY = 5;

            Result.Add(normal);

            OutputNode ao = new OutputNode(Result.DefaultTextureType);
            ao.Name = "Occlusion";
            ao.OutType = OutputType.occlusion;
            ao.ViewOriginY = (UINode.DEFAULT_HEIGHT + 5);

            Result.Add(ao);

            OutputNode height = new OutputNode(Result.DefaultTextureType);
            height.Name = "Height";
            height.OutType = OutputType.height;
            height.ViewOriginY = (UINode.DEFAULT_HEIGHT + 5) * 2;

            Result.Add(height);

            OutputNode emission = new OutputNode(Result.DefaultTextureType);
            emission.Name = "Emission";
            emission.OutType = OutputType.emission;
            emission.ViewOriginY = (UINode.DEFAULT_HEIGHT + 5) * 3;

            Result.Add(emission);

            OutputNode thickness = new OutputNode(Result.DefaultTextureType);
            thickness.Name = "Thickness";
            thickness.OutType = OutputType.thickness;
            thickness.ViewOriginY = (UINode.DEFAULT_HEIGHT + 5) * 4;

            Result.Add(thickness);
        }

        private void PBRNoHeightTemplate()
        {
            OutputNode baseColor = new OutputNode(Result.DefaultTextureType);
            baseColor.Name = "Base Color";
            baseColor.ViewOriginY = -(UINode.DEFAULT_HEIGHT + 5) * 3;

            Result.Add(baseColor);

            OutputNode metallic = new OutputNode(Result.DefaultTextureType);
            metallic.Name = "Metallic";
            metallic.OutType = OutputType.metallic;
            metallic.ViewOriginY = -(UINode.DEFAULT_HEIGHT + 5) * 2;

            Result.Add(metallic);

            OutputNode roughness = new OutputNode(Result.DefaultTextureType);
            roughness.Name = "Roughness";
            roughness.OutType = OutputType.roughness;
            roughness.ViewOriginY = -(UINode.DEFAULT_HEIGHT + 5);

            Result.Add(roughness);

            OutputNode normal = new OutputNode(Result.DefaultTextureType);
            normal.Name = "Normal";
            normal.OutType = OutputType.normal;
            normal.ViewOriginY = 5;

            Result.Add(normal);

            OutputNode ao = new OutputNode(Result.DefaultTextureType);
            ao.Name = "Occlusion";
            ao.OutType = OutputType.occlusion;
            ao.ViewOriginY = (UINode.DEFAULT_HEIGHT + 5);

            Result.Add(ao);

            OutputNode emission = new OutputNode(Result.DefaultTextureType);
            emission.Name = "Emission";
            emission.OutType = OutputType.emission;
            emission.ViewOriginY = (UINode.DEFAULT_HEIGHT + 5) * 2;

            Result.Add(emission);

            OutputNode thickness = new OutputNode(Result.DefaultTextureType);
            thickness.Name = "Thickness";
            thickness.OutType = OutputType.thickness;
            thickness.ViewOriginY = (UINode.DEFAULT_HEIGHT + 5) * 3;

            Result.Add(thickness);
        }

        private void PBRNoHeightAOTemplate()
        {
            OutputNode baseColor = new OutputNode(Result.DefaultTextureType);
            baseColor.Name = "Base Color";
            baseColor.ViewOriginY = -(UINode.DEFAULT_HEIGHT + 5) * 2;

            Result.Add(baseColor);

            OutputNode metallic = new OutputNode(Result.DefaultTextureType);
            metallic.Name = "Metallic";
            metallic.OutType = OutputType.metallic;
            metallic.ViewOriginY = -(UINode.DEFAULT_HEIGHT + 5);

            Result.Add(metallic);

            OutputNode roughness = new OutputNode(Result.DefaultTextureType);
            roughness.Name = "Roughness";
            roughness.OutType = OutputType.roughness;
            roughness.ViewOriginY = 5;

            Result.Add(roughness);

            OutputNode normal = new OutputNode(Result.DefaultTextureType);
            normal.Name = "Normal";
            normal.OutType = OutputType.normal;
            normal.ViewOriginY = (UINode.DEFAULT_HEIGHT + 5) * 2;

            Result.Add(normal);
        }


        private void PBRNoHeightAONormalTemplate()
        {
            OutputNode baseColor = new OutputNode(Result.DefaultTextureType);
            baseColor.Name = "Base Color";
            baseColor.ViewOriginY = -(UINode.DEFAULT_HEIGHT + 5);

            Result.Add(baseColor);

            OutputNode metallic = new OutputNode(Result.DefaultTextureType);
            metallic.Name = "Metallic";
            metallic.OutType = OutputType.metallic;
            metallic.ViewOriginY = 5;

            Result.Add(metallic);

            OutputNode roughness = new OutputNode(Result.DefaultTextureType);
            roughness.Name = "Roughness";
            roughness.OutType = OutputType.roughness;
            roughness.ViewOriginY = (UINode.DEFAULT_HEIGHT + 5);

            Result.Add(roughness);
        }
    }
}
