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
    /// Interaction logic for GraphFunctionEditor.xaml
    /// </summary>
    public partial class GraphFunctionEditor : UserControl
    {
        protected Graph graph;

        public GraphFunctionEditor()
        {
            InitializeComponent();
        }

        public GraphFunctionEditor(Graph g)
        {
            InitializeComponent();
            graph = g;
            Populate();
        }

        void Populate()
        {
            foreach (var k in graph.ParameterFunctions.Keys)
            {
                string[] split = k.Split('.');

                if (split.Length >= 2)
                {
                    GraphFunction fp = new GraphFunction(split[0], split[1], graph.ParameterFunctions[k]);
                    fp.OnRemove += Cp_OnRemove;
                    Stack.Children.Add(fp);
                }
            }
        }

        private void Cp_OnRemove(GraphFunction c)
        {
            Stack.Children.Remove(c);
        }
    }
}
