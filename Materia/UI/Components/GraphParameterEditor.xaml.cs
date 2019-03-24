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
        public GraphParameterEditor()
        {
            InitializeComponent();
        }

        public GraphParameterEditor(Graph g, Dictionary<string, GraphParameterValue> values)
        {
            InitializeComponent();

            foreach(var k in values.Keys)
            {
                string[] split = k.Split('.');
                var v = values[k];

                if (!v.IsFunction())
                {
                    Node n = null;
                    g.NodeLookup.TryGetValue(split[0], out n);

                    PropertyLabel lbl = new PropertyLabel();

                    if (n == null)
                    {
                        lbl.Title = split[1] + " - Parameter Name";
                    }
                    else
                    {
                        lbl.Title = n.Name + " - " + split[1] + " - Parameter Name";
                    }

                    Stack.Children.Add(lbl);

                    var info = v.GetType().GetProperty("Name");

                    PropertyInput inp = new PropertyInput(info, v);

                    Stack.Children.Add(inp);
                }
            }
        }
    }
}
