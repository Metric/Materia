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
    /// Interaction logic for GraphParameterEditor.xaml
    /// </summary>
    public partial class GraphParameterEditor : UserControl
    {
        Graph graph;

        public GraphParameterEditor()
        {
            InitializeComponent();
        }

        public GraphParameterEditor(Graph g, Dictionary<string, GraphParameterValue> values)
        {
            InitializeComponent();
            graph = g;

            foreach(var k in values.Keys)
            {
                string[] split = k.Split('.');
                var v = values[k];

                if (!v.IsFunction())
                {
                    Node n = null;
                    g.NodeLookup.TryGetValue(split[0], out n);

                    string title = split[1];

                    if(n == null)
                    {
                        n = g.FindSubNodeById(split[0]);
                    }

                    if(n != null)
                    { 
                        title = n.Name + "." + split[1];
                    }


                    ParameterView cp = new ParameterView(title, k, v, "Name", "Type", "InputType");
                    cp.OnRemove += Cp_OnRemove;
                    Stack.Children.Add(cp);
                }
            }
        }

        private void Cp_OnRemove(ParameterView c)
        {
            string[] split = c.Id.Split('.');
            graph.RemoveParameterValue(split[0], split[1]);
            Stack.Children.Remove(c);
        }
    }
}
