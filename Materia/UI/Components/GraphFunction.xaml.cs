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
using Materia.Nodes.Atomic;

namespace Materia.UI.Components
{
    /// <summary>
    /// Interaction logic for GraphFunction.xaml
    /// </summary>
    public partial class GraphFunction : UserControl
    {
        public delegate void FunctionRemove(GraphFunction fn);
        public event FunctionRemove OnRemove;

        public FunctionGraph graph { get; protected set; }

        protected string nodeId;
        protected string param;

        public GraphFunction()
        {
            InitializeComponent();
        }

        public GraphFunction(string node, string parameter, FunctionGraph g)
        {
            InitializeComponent();
            graph = g;
            param = parameter;
            nodeId = node;

            FuncName.Text = graph.Name;
        }

        private void EditFunc_Click(object sender, RoutedEventArgs e)
        {
            if (MateriaMainWindow.Instance != null)
            {
                MateriaMainWindow.Instance.Push(graph.ParentNode, graph, param, GraphStackType.Parameter);
            }
        }

        private void RemoveFunction_Click(object sender, RoutedEventArgs e)
        {
            var g = graph.TopGraph();

            if (MessageBox.Show("Remove Function: " + graph.Name + "?", "", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
            {
                if (g != null)
                {
                    g.RemoveParameterValue(nodeId, param);

                    if (OnRemove != null)
                    {
                        OnRemove.Invoke(this);
                    }
                }
            }
        }
    }
}
