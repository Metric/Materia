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
using Materia.Nodes.Atomic;
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

            //create a copy
            var temp = values.ToList();

            temp.Sort((a, b) =>
            {
                return a.Section.CompareTo(b.Section);
            });

            string lastSection = "Default";
            foreach(var v in temp)
            {
                if (v.IsFunction()) continue;

                if(!string.IsNullOrEmpty(v.Section) && !v.Section.Equals(lastSection))
                {
                    lastSection = v.Section;

                    PropertySection sect = new PropertySection();
                    sect.Title = v.Section;
                    Stack.Children.Add(sect);
                }

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

        public ParameterMap(Graph g, Dictionary<string, GraphParameterValue> values)
        {
            InitializeComponent();

            List<Tuple<PropertyLabel, GraphParameterValue, PropertyInfo>> sorter = new List<Tuple<PropertyLabel, GraphParameterValue, PropertyInfo>>();

            foreach (var k in values.Keys)
            {
                var v = values[k];

                if (v.IsFunction()) continue;

                string[] split = k.Split('.');

                var n = g.FindSubNodeById(split[0]);

                PropertyInfo nodeInfo = null;

                string customHeader = "";

                if(n != null)
                {
                    nodeInfo = n.GetType().GetProperty(split[1]);

                    if(nodeInfo == null && n is GraphInstanceNode)
                    {
                        //then it might be an underling custom parameter on the node
                        GraphInstanceNode inst = n as GraphInstanceNode;

                        var realParam = inst.GetCustomParameter(split[1]);

                        if(realParam != null)
                        {
                            //initiate custom header
                            //for proper underlying processing
                            //on the label
                            customHeader = "$Custom.";
                            //just set the parameter inputtype the same
                            //also ensure min and max are the same
                            v.InputType = realParam.InputType;
                            v.Max = realParam.Max;
                            v.Min = realParam.Min;
                            v.Section = realParam.Section;
                        }
                    }
                }

                PropertyLabel lbl = new PropertyLabel(v.Name, n, customHeader + split[1]);
                Tuple<PropertyLabel, GraphParameterValue, PropertyInfo> prop = new Tuple<PropertyLabel, GraphParameterValue, PropertyInfo>(lbl, v, nodeInfo);
                sorter.Add(prop);
            }

            sorter.Sort((a, b) =>
            {
                return a.Item2.Section.CompareTo(b.Item2.Section);
            });

            string lastSection = "Default";
            foreach(var prop in sorter)
            {
                GraphParameterValue v = prop.Item2;
                if(!string.IsNullOrEmpty(v.Section) && !v.Section.Equals(lastSection))
                {
                    lastSection = v.Section;
                    PropertySection sect = new PropertySection();
                    sect.Title = v.Section;
                    Stack.Children.Add(sect);
                }

                Stack.Children.Add(prop.Item1);
                BuildParameter(v, prop.Item3);
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
