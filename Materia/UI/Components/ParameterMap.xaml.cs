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
using Materia.MathHelpers;

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

        public ParameterMap(List<GraphParameterValue> values, bool useBasic = false)
        {
            InitializeComponent();

            foreach(var v in values)
            {
                if(!v.IsFunction())
                {
                    if (v.Type != NodeType.Bool)
                    {
                        PropertyLabel lbl = new PropertyLabel();
                        lbl.Title = v.Name;
                        Stack.Children.Add(lbl);
                    }

                    if (!useBasic)
                    {
                        BuildParameterCustom(v);
                    }
                    else
                    {
                        BuildParameter(v);
                    }
                }
            }
        }

        public ParameterMap(Dictionary<string, GraphParameterValue> values)
        {
            InitializeComponent();

            foreach (var k in values.Keys)
            {
                string[] split = k.Split('.');
                var v = values[k];

                if (!v.IsFunction())
                {
                    if (v.Type != NodeType.Bool)
                    {
                        PropertyLabel lbl = new PropertyLabel();
                        lbl.Title = v.Name;
                        Stack.Children.Add(lbl);
                    }
                    BuildParameter(v);
                }
            }
        }

        protected void BuildParameterCustom(GraphParameterValue v)
        {
            try
            {
                PropertyInfo info2 = v.GetType().GetProperty("Value");

                if (v.Value is double || v.Value is float || v.Value is int || v.Type == NodeType.Float)
                {
                    NumberSlider sp = new NumberSlider();
                    sp.Set(v.Min, v.Max, info2, v);
                    Stack.Children.Add(sp);
                }
                else if (v.Value is bool || v.Type == NodeType.Bool)
                {
                    ToggleControl tc = new ToggleControl(v.Name, info2, v);
                    Stack.Children.Add(tc);
                }
                else if (v.Type == NodeType.Float2 || v.Type == NodeType.Float3 || v.Type == NodeType.Float4)
                {
                    VectorSlider vs = new VectorSlider(info2, v, v.Min, v.Max, v.Type);
                    Stack.Children.Add(vs);
                }
                else if (v.Value is MVector || v.Type == NodeType.Color || v.Type == NodeType.Gray)
                {
                    ColorSelect cs = new ColorSelect(info2, v);
                    Stack.Children.Add(cs);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
            }
        }

        protected void BuildParameter(GraphParameterValue v)
        {
            try
            {
                PropertyInfo info2 = v.GetType().GetProperty("Value");

                if (v.Value is double || v.Value is float || v.Value is int)
                {
                    if(v.Value is double)
                    {
                        NumberInput np = new NumberInput(NumberInputType.Float, v, info2);
                        Stack.Children.Add(np);
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
                else if(v.Value is bool || v.Type == NodeType.Bool)
                {
                    ToggleControl tc = new ToggleControl(v.Name, info2, v);
                    Stack.Children.Add(tc);
                }
                else if(v.Type == NodeType.Float2 || v.Type == NodeType.Float3 || v.Type == NodeType.Float4)
                {
                    VectorSlider vs = new VectorSlider(info2, v, v.Min, v.Max, v.Type);
                    Stack.Children.Add(vs);
                }
                else if(v.Value is MVector || v.Type == NodeType.Color || v.Type == NodeType.Gray)
                {
                    ColorSelect cs = new ColorSelect(info2, v);
                    Stack.Children.Add(cs);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
            }
        }
    }
}
