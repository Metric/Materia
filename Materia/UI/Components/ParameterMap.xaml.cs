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
using Materia.Nodes.Attributes;
using OpenTK;

namespace Materia.UI.Components
{
    /// <summary>
    /// Interaction logic for ParameterMap.xaml
    /// </summary>
    public partial class ParameterMap : UserControl
    {
        public ParameterMap()
        {
            InitializeComponent();
        }

        public ParameterMap(Graph g, Dictionary<string, GraphParameterValue> values)
        {
            InitializeComponent();

            foreach (var k in values.Keys)
            {
                string[] split = k.Split('.');
                var v = values[k];

                if (!v.IsFunction())
                {
                    Node n = null;
                    g.NodeLookup.TryGetValue(split[0], out n);

                    PropertyLabel lbl = new PropertyLabel();
                    lbl.Title = v.Name;
                    Stack.Children.Add(lbl);

                    if(n != null)
                    {
                        BuildParameter(split[1], v, n);
                    }
                }
            }
        }

        protected void BuildParameter(string parameter, GraphParameterValue v, Node n)
        {
            try
            {
                PropertyInfo info1 = n.GetType().GetProperty(parameter);
                PropertyInfo info2 = v.GetType().GetProperty("Value");

                if (info1 == null)
                {
                    if(v.Value is float)
                    {
                        NumberInput np = new NumberInput(NumberInputType.Float, v, info2);
                        Stack.Children.Add(np);
                    }
                    else if(v.Value is int)
                    {
                        NumberInput np = new NumberInput(NumberInputType.Int, v, info2);
                        Stack.Children.Add(np);
                    }
                    else if(v.Value is bool)
                    {
                        ToggleControl tc = new ToggleControl("True", info2, v);
                        Stack.Children.Add(tc);
                    }
                }
                else
                {
                    if (v.Value is float || v.Value is int)
                    {
                        SliderAttribute sl = info1.GetCustomAttribute<SliderAttribute>();

                        if (sl != null)
                        {
                            NumberSlider inp = new NumberSlider(sl, info2, v);
                            Stack.Children.Add(inp);
                        }
                        else if(v.Value is float)
                        {
                            NumberInput np = new NumberInput(NumberInputType.Float, v, info2);
                            Stack.Children.Add(np);
                        }
                        else if(v.Value is int)
                        {
                            NumberInput np = new NumberInput(NumberInputType.Int, v, info2);
                            Stack.Children.Add(np);
                        }
                    }
                    else if(v.Value is bool)
                    {
                        ToggleControl tc = new ToggleControl(v.Name, info2, v);
                        Stack.Children.Add(tc);
                    }
                    else if(v.Value is Vector4)
                    {
                        ColorSelect cs = new ColorSelect(info2, v);
                        Stack.Children.Add(cs);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
            }
        }
    }
}
