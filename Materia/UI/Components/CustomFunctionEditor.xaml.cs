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
    /// Interaction logic for CustomFunctionEditor.xaml
    /// </summary>
    public partial class CustomFunctionEditor : UserControl
    {
        Graph graph;

        public CustomFunctionEditor()
        {
            InitializeComponent();
        }

        public CustomFunctionEditor(Graph g)
        {
            InitializeComponent();
            graph = g;
            Populate();
        }

        void Populate()
        {
            foreach (var p in graph.CustomFunctions)
            {
                CustomFunction cp = new CustomFunction(p);
                cp.OnRemove += Cp_OnRemove;
                Stack.Children.Add(cp);
            }
        }

        private void Cp_OnRemove(CustomFunction c)
        {
            Stack.Children.Remove(c);
        }

        private void AddFunc_Click(object sender, RoutedEventArgs e)
        {
            if(graph != null)
            {
                int index = graph.CustomFunctions.Count;
                string name = "Func" + index;
                FunctionGraph gp = new FunctionGraph(name);
                gp.ParentGraph = graph;
                graph.CustomFunctions.Add(gp);
                CustomFunction cp = new CustomFunction(gp);
                cp.OnRemove += Cp_OnRemove;
                Stack.Children.Add(cp);
            }
        }
    }
}
