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
using Materia.Nodes;
using Materia.Nodes.Atomic;

namespace Materia.UI
{
    /// <summary>
    /// Interaction logic for UINewGraph.xaml
    /// </summary>
    public partial class UINewGraph : Window
    {
        public ImageGraph Result { get; protected set; }

        public UINewGraph()
        {
            InitializeComponent();
            InitializeFormats();
        }

        private void InitializeFormats()
        {
            string[] names = Enum.GetNames(typeof(GraphPixelType));

            foreach(string s in names)
            {
                ComboBoxItem cbi = new ComboBoxItem();
                cbi.Content = s;
                DefaultFormat.Items.Add(cbi);
            }
        }

        private void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            GraphPixelType px;

            string spx = (DefaultFormat.SelectedItem as ComboBoxItem).Content.ToString();

            if(!Enum.TryParse(spx, out px))
            {
                px = GraphPixelType.RGBA;
            }

            int sizeIndex = DefaultNodeSize.SelectedIndex;
            int size = Graph.DEFAULT_SIZE;

            if(sizeIndex > -1 && sizeIndex < Graph.GRAPH_SIZES.Length)
            {
                size = Graph.GRAPH_SIZES[sizeIndex];
            }

            ImageGraph g = new ImageGraph("Untitled", size, size);
            g.DefaultTextureType = px;
            Result = g;

            switch(TemplateType.SelectedIndex)
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

            DialogResult = true;
        }

        private void PBRFullTemplate()
        {
            OutputNode baseColor = new OutputNode(Result.DefaultTextureType);
            baseColor.Name = "Base Color";
            baseColor.ViewOriginY = -(UINode.defaultHeight + 5) * 3;

            Result.Add(baseColor);

            OutputNode metallic = new OutputNode(Result.DefaultTextureType);
            metallic.Name = "Metallic";
            metallic.OutType = OutputType.metallic;
            metallic.ViewOriginY = -(UINode.defaultHeight + 5) * 2;

            Result.Add(metallic);

            OutputNode roughness = new OutputNode(Result.DefaultTextureType);
            roughness.Name = "Roughness";
            roughness.OutType = OutputType.roughness;
            roughness.ViewOriginY = -(UINode.defaultHeight + 5);

            Result.Add(roughness);

            OutputNode normal = new OutputNode(Result.DefaultTextureType);
            normal.Name = "Normal";
            normal.OutType = OutputType.normal;
            normal.ViewOriginY = 5;

            Result.Add(normal);

            OutputNode ao = new OutputNode(Result.DefaultTextureType);
            ao.Name = "Occlusion";
            ao.OutType = OutputType.occlusion;
            ao.ViewOriginY = (UINode.defaultHeight + 5);

            Result.Add(ao);

            OutputNode height = new OutputNode(Result.DefaultTextureType);
            height.Name = "Height";
            height.OutType = OutputType.height;
            height.ViewOriginY = (UINode.defaultHeight + 5) * 2;

            Result.Add(height);

            OutputNode emission = new OutputNode(Result.DefaultTextureType);
            emission.Name = "Emission";
            emission.OutType = OutputType.emission;
            emission.ViewOriginY = (UINode.defaultHeight + 5) * 3;

            Result.Add(emission);

            OutputNode thickness = new OutputNode(Result.DefaultTextureType);
            thickness.Name = "Thickness";
            thickness.OutType = OutputType.thickness;
            thickness.ViewOriginY = (UINode.defaultHeight + 5) * 4;

            Result.Add(thickness);
        }

        private void PBRNoHeightTemplate()
        {
            OutputNode baseColor = new OutputNode(Result.DefaultTextureType);
            baseColor.Name = "Base Color";
            baseColor.ViewOriginY = -(UINode.defaultHeight + 5) * 3;

            Result.Add(baseColor);

            OutputNode metallic = new OutputNode(Result.DefaultTextureType);
            metallic.Name = "Metallic";
            metallic.OutType = OutputType.metallic;
            metallic.ViewOriginY = -(UINode.defaultHeight + 5) * 2;

            Result.Add(metallic);

            OutputNode roughness = new OutputNode(Result.DefaultTextureType);
            roughness.Name = "Roughness";
            roughness.OutType = OutputType.roughness;
            roughness.ViewOriginY = -(UINode.defaultHeight + 5);

            Result.Add(roughness);

            OutputNode normal = new OutputNode(Result.DefaultTextureType);
            normal.Name = "Normal";
            normal.OutType = OutputType.normal;
            normal.ViewOriginY = 5;

            Result.Add(normal);

            OutputNode ao = new OutputNode(Result.DefaultTextureType);
            ao.Name = "Occlusion";
            ao.OutType = OutputType.occlusion;
            ao.ViewOriginY = (UINode.defaultHeight + 5);

            Result.Add(ao);

            OutputNode emission = new OutputNode(Result.DefaultTextureType);
            emission.Name = "Emission";
            emission.OutType = OutputType.emission;
            emission.ViewOriginY = (UINode.defaultHeight + 5) * 2;

            Result.Add(emission);

            OutputNode thickness = new OutputNode(Result.DefaultTextureType);
            thickness.Name = "Thickness";
            thickness.OutType = OutputType.thickness;
            thickness.ViewOriginY = (UINode.defaultHeight + 5) * 3;

            Result.Add(thickness);
        }

        private void PBRNoHeightAOTemplate()
        {
            OutputNode baseColor = new OutputNode(Result.DefaultTextureType);
            baseColor.Name = "Base Color";
            baseColor.ViewOriginY = -(UINode.defaultHeight + 5) * 2;

            Result.Add(baseColor);

            OutputNode metallic = new OutputNode(Result.DefaultTextureType);
            metallic.Name = "Metallic";
            metallic.OutType = OutputType.metallic;
            metallic.ViewOriginY = -(UINode.defaultHeight + 5);

            Result.Add(metallic);

            OutputNode roughness = new OutputNode(Result.DefaultTextureType);
            roughness.Name = "Roughness";
            roughness.OutType = OutputType.roughness;
            roughness.ViewOriginY = 5;

            Result.Add(roughness);

            OutputNode normal = new OutputNode(Result.DefaultTextureType);
            normal.Name = "Normal";
            normal.OutType = OutputType.normal;
            normal.ViewOriginY = (UINode.defaultHeight + 5) * 2;

            Result.Add(normal);
        }


        private void PBRNoHeightAONormalTemplate()
        {
            OutputNode baseColor = new OutputNode(Result.DefaultTextureType);
            baseColor.Name = "Base Color";
            baseColor.ViewOriginY = -(UINode.defaultHeight + 5);

            Result.Add(baseColor);

            OutputNode metallic = new OutputNode(Result.DefaultTextureType);
            metallic.Name = "Metallic";
            metallic.OutType = OutputType.metallic;
            metallic.ViewOriginY = 5;

            Result.Add(metallic);

            OutputNode roughness = new OutputNode(Result.DefaultTextureType);
            roughness.Name = "Roughness";
            roughness.OutType = OutputType.roughness;
            roughness.ViewOriginY = (UINode.defaultHeight + 5);

            Result.Add(roughness);
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
