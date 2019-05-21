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
        Dictionary<string, PropertyLabel> labelLookup;
        Graph graph;

        public GraphParameterEditor()
        {
            InitializeComponent();
            labelLookup = new Dictionary<string, PropertyLabel>();
        }

        public GraphParameterEditor(Graph g, Dictionary<string, GraphParameterValue> values)
        {
            InitializeComponent();
            graph = g;
            labelLookup = new Dictionary<string, PropertyLabel>();

            foreach(var k in values.Keys)
            {
                string[] split = k.Split('.');
                var v = values[k];

                if (!v.IsFunction())
                {
                    Node n = null;
                    g.NodeLookup.TryGetValue(split[0], out n);

                    PropertyLabel lbl = new PropertyLabel();

                    if(n == null)
                    {
                        n = g.FindSubNodeById(split[0]);
                    }

                    if (n == null)
                    {
                        lbl.Title = split[1];
                    }
                    else
                    {
                        lbl.Title = n.Name + " - " + split[1];
                    }

                    Stack.Children.Add(lbl);

                    GraphParameter cp = new GraphParameter(v, k);
                    cp.OnRemove += Cp_OnRemove;

                    labelLookup[k] = lbl;

                    Stack.Children.Add(cp);
                }
            }
        }

        private void Cp_OnRemove(GraphParameter c)
        {
            PropertyLabel lb = null;

            if(labelLookup.TryGetValue(c.Id, out lb))
            {
                Stack.Children.Remove(lb);
                labelLookup.Remove(c.Id);
            }

            Stack.Children.Remove(c);

            string[] split = c.Id.Split('.');
            graph.RemoveParameterValue(split[0], split[1]);
        }
    }
}
