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
using System.Reflection;

namespace Materia.UI.Components
{
    /// <summary>
    /// Interaction logic for CustomFunction.xaml
    /// </summary>
    public partial class CustomFunction : UserControl
    {
        public delegate void FunctionRemove(CustomFunction fn);
        public event FunctionRemove OnRemove;

        public FunctionGraph graph {get; protected set; }

        public CustomFunction()
        {
            InitializeComponent();
        }

        public CustomFunction(FunctionGraph g)
        {
            InitializeComponent();
            graph = g;

            var prop = graph.GetType().GetProperty("Name");

            PropertyLabel lbl = new PropertyLabel("Function Name");
            Stack.Children.Add(lbl);

            PropertyInput pinput = new PropertyInput(prop, graph);
            Stack.Children.Add(pinput);
        }

        private void EditFunc_Click(object sender, RoutedEventArgs e)
        {
            if(MainWindow.Instance != null)
            {
                MainWindow.Instance.Push(null, graph, null, GraphStackType.CustomFunction);
            }
        }

        private void RemoveFunction_Click(object sender, RoutedEventArgs e)
        {
            var g = graph.TopGraph();
            if(g != null)
            {
                if(g.CustomFunctions.Remove(graph))
                {
                    if(OnRemove != null)
                    {
                        OnRemove.Invoke(this);
                    }
                }
            }
        }
    }
}
