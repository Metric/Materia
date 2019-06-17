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
using Materia.Nodes;

namespace Materia.UI.Components
{
    /// <summary>
    /// Interaction logic for CustomParameterEditor.xaml
    /// </summary>
    public partial class CustomParameterEditor : UserControl
    {
        Graph graph;

        public CustomParameterEditor()
        {
            InitializeComponent();
        }

        public CustomParameterEditor(Graph g)
        {
            InitializeComponent();
            graph = g;
            Populate();
        }

        void Populate()
        {
            foreach(var p in graph.CustomParameters)
            {
                CustomParameter cp = new CustomParameter(p);
                cp.OnRemove += Cp_OnRemove;
                Stack.Children.Add(cp);
            }
        }

        private void Cp_OnRemove(CustomParameter c)
        {
            if(graph != null)
            {
                graph.RemoveCustomParameter(c.Param);
            }

            Stack.Children.Remove(c);
        }

        private void AddParam_Click(object sender, RoutedEventArgs e)
        {
            if(graph != null)
            {
                int index = graph.CustomParameters.Count;
                string name = "Param" + index;
                GraphParameterValue gp = new GraphParameterValue(name, 0, "", NodeType.Float);
                graph.AddCustomParameter(gp);
                CustomParameter cp = new CustomParameter(gp);
                cp.OnRemove += Cp_OnRemove;
                Stack.Children.Add(cp);
            }
        }
    }
}
