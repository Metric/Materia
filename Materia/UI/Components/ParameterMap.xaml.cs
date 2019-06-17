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
using NLog;

namespace Materia.UI.Components
{
    /// <summary>
    /// Interaction logic for ParameterMap.xaml
    /// </summary>
    public partial class ParameterMap : UserControl
    {
        private static ILogger Log = LogManager.GetCurrentClassLogger();

        public ParameterMap()
        {
            InitializeComponent();
        }

        public ParameterMap(Node n, List<GraphParameterValue> values, bool useBasic = false)
        {
            InitializeComponent();

            foreach(var v in values)
            {
                if(!v.IsFunction())
                {
                    PropertyLabel lbl = new PropertyLabel(v.Name, n, "$Custom." + v.Name);
                    Stack.Children.Add(lbl);

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

        public ParameterMap(Graph g, Dictionary<string, GraphParameterValue> values)
        {
            InitializeComponent();

            foreach (var k in values.Keys)
            {
                string[] split = k.Split('.');
                var v = values[k];

                var n = g.FindSubNodeById(split[0]);

                PropertyInfo nodeInfo = null;

                if(n != null)
                {
                    nodeInfo = n.GetType().GetProperty(split[1]);
                }

                if (!v.IsFunction())
                {
                    PropertyLabel lbl = new PropertyLabel(v.Name, n, split[1]);
                    Stack.Children.Add(lbl);
                    BuildParameter(v, nodeInfo);
                }
            }
        }

        protected void BuildParameterCustom(GraphParameterValue v)
        {
            try
            {
                PropertyInfo info2 = v.GetType().GetProperty("Value");

                if (v.Value is double || v.Value is float || v.Value is int || v.Value is long || v.Type == NodeType.Float)
                {
                    if (v.InputType == ParameterInputType.Slider)
                    {
                        NumberSlider sp = new NumberSlider();
                        sp.Set(v.Min, v.Max, info2, v);
                        Stack.Children.Add(sp);
                    }
                    else if(v.InputType == ParameterInputType.Input)
                    {
                        NumberInput np = new NumberInput();
                        np.Set(NumberInputType.Float, v, info2);
                        Stack.Children.Add(np);
                    }
                }
                else if (v.Value is bool || v.Type == NodeType.Bool)
                {
                    ToggleControl tc = new ToggleControl(v.Name, info2, v);
                    Stack.Children.Add(tc);
                }
                else if (v.Type == NodeType.Float2 || v.Type == NodeType.Float3 || v.Type == NodeType.Float4)
                {
                    if (v.InputType == ParameterInputType.Slider)
                    {
                        VectorSlider vs = new VectorSlider(info2, v, v.Min, v.Max, v.Type);
                        Stack.Children.Add(vs);
                    }
                    else if(v.InputType == ParameterInputType.Input)
                    {
                        VectorInput vi = new VectorInput(info2, v, v.Type);
                        Stack.Children.Add(vi);
                    }
                }
                else if (v.Value is MVector || v.Type == NodeType.Color || v.Type == NodeType.Gray)
                {
                    ColorSelect cs = new ColorSelect(info2, v);
                    Stack.Children.Add(cs);
                }
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }

        protected void BuildParameter(GraphParameterValue v, PropertyInfo nprop = null)
        {
            try
            {
                PropertyInfo info2 = v.GetType().GetProperty("Value");

                SliderAttribute slide = null;
                if(nprop != null)
                {
                    slide = nprop.GetCustomAttribute<SliderAttribute>();
                }

                if (v.Value is double || v.Value is float || v.Value is int || v.Value is long)
                {
                    if (nprop != null && nprop.PropertyType.IsEnum)
                    {
                        DropDown dp = new DropDown(Enum.GetNames(nprop.PropertyType), v, info2);
                        Stack.Children.Add(dp);
                        return;
                    }

                    if(slide != null)
                    {
                        NumberSlider ns = new NumberSlider(slide, info2, v);
                        Stack.Children.Add(ns);
                        return;
                    }

                    NumberInputType nt = NumberInputType.Float;

                    if(v.Value is int)
                    {
                        nt = NumberInputType.Int;
                    }

                    if (v.InputType == ParameterInputType.Slider)
                    {
                        NumberSlider sp = new NumberSlider();
                        sp.Set(v.Min, v.Max, info2, v);
                        Stack.Children.Add(sp);
                    }
                    else if (v.InputType == ParameterInputType.Input)
                    {
                        NumberInput np = new NumberInput();
                        np.Set(nt, v, info2);
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
                    if (v.InputType == ParameterInputType.Slider)
                    {
                        VectorSlider vs = new VectorSlider(info2, v, v.Min, v.Max, v.Type);
                        Stack.Children.Add(vs);
                    }
                    else if(v.InputType == ParameterInputType.Input)
                    {
                        VectorInput vs = new VectorInput(info2, v, v.Type);
                        Stack.Children.Add(vs);
                    }
                }
                else if(v.Value is MVector || v.Type == NodeType.Color || v.Type == NodeType.Gray)
                {
                    ColorSelect cs = new ColorSelect(info2, v);
                    Stack.Children.Add(cs);
                }
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }
    }
}
