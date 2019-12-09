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
using NLog;

namespace Materia.UI.Components
{
    /// <summary>
    /// Interaction logic for PropertyLabel.xaml
    /// </summary>
    public partial class PropertyLabel : UserControl
    {
        public class FuncMenuItem : MenuItem
        {
            public string Key { get; protected set; }
            public FunctionGraph Graph { get; protected set; }
            public FuncMenuItem(FunctionGraph g, string key) : base()
            {
                Graph = g;
                Header = g.Name;
                Key = key;
            }
        }

        private static ILogger Log = LogManager.GetCurrentClassLogger();

        public string Title
        {
            get
            {
                return LabelContent.Text;
            }
            set
            {
                LabelContent.Text = value;
            }
        }

        public Node Node { get; protected set; }
        public string Parameter { get; protected set; }

        public PropertyLabel()
        {
            InitializeComponent();
            EditVar.Visibility = Visibility.Collapsed;
        }

        public PropertyLabel(string title, Node n = null, string param = null)
        {
            InitializeComponent();
            Title = title;

            Node = n;
            Parameter = param;

            if(Node != null && !string.IsNullOrEmpty(Parameter))
            {
                if(!Parameter.StartsWith("$Custom."))
                {
                    var prop = Node.GetType().GetProperty(Parameter);
                    if (prop != null && prop.GetCustomAttribute<PromoteAttribute>() == null)
                    {
                        EditVar.Visibility = Visibility.Collapsed;
                        return;
                    }
                }

                EditVar.Visibility = Visibility.Visible;

                var p = Node.ParentGraph;

                p = p.ParentNode != null ? p.ParentNode.ParentGraph : p;

                if(p != null)
                {
                    if(p.HasParameterValue(Node.Id, Parameter.Replace("$Custom.", "")))
                    {
                        if(p.IsParameterValueFunction(Node.Id, Parameter.Replace("$Custom.", "")))
                        {
                            FIcon.Opacity = 1;
                        }
                        else
                        {
                            FIcon.Opacity = 0.25;
                        }

                        ConstantVar.IsEnabled = false;
                        FunctionVar.IsEnabled = false;
                        AssignVar.IsEnabled = false;
                        DefaultVar.IsEnabled = true;
                    }
                    else
                    {
                        FIcon.Opacity = 0.25;
                        DefaultVar.IsEnabled = false;
                        AssignVar.IsEnabled = true;
                        ConstantVar.IsEnabled = true;
                        FunctionVar.IsEnabled = true;
                    }
                }

                var functionsAvailable = Node.ParentGraph.ParameterFunctions;

                AssignVar.Items.Clear();

                foreach(string k in functionsAvailable.Keys)
                {
                    FunctionGraph f = functionsAvailable[k];
                    MenuItem mitem = new FuncMenuItem(f,k);
                    mitem.Click += AssignVar_Click;
                    AssignVar.Items.Add(mitem);
                }
            }
            else
            {
                EditVar.Visibility = Visibility.Collapsed;
            }
        }

        private void ConstantVar_Click(object sender, RoutedEventArgs e)
        {
            if (Node == null || string.IsNullOrEmpty(Parameter)) return;

            try
            {
                PropertyInfo info = null;

                if (Parameter.StartsWith("$Custom."))
                {
                    if (Node is GraphInstanceNode)
                    {
                        GraphInstanceNode gn = Node as GraphInstanceNode;

                        string cparam = Parameter.Replace("$Custom.", "");

                        var param = gn.GetCustomParameter(cparam);

                        if (param != null)
                        {
                            var cparent = Node.ParentGraph;

                            if (cparent != null)
                            {
                                cparent.SetParameterValue(Node.Id, cparam, param.Value, true, param.Type);
                                var nparam = cparent.GetParameterRaw(Node.Id, cparam);

                                //copy settings over
                                if (nparam != null)
                                {
                                    nparam.Description = param.Description;
                                    nparam.InputType = param.InputType;
                                    nparam.Min = param.Min;
                                    nparam.Max = param.Max;
                                    nparam.Name = param.Name;
                                }

                                FIcon.Opacity = 0.25;
                                DefaultVar.IsEnabled = true;
                                ConstantVar.IsEnabled = false;
                                FunctionVar.IsEnabled = false;
                            }
                            else
                            {
                                Log.Warn("Failed to promote to constant");
                            }
                        }
                        else
                        {
                            //log error
                            Log.Error("Could not find custom parameter: " + cparam);
                        }
                    }
                    else
                    {
                        Log.Warn("Failed to promote to constant");
                    }
                }
                else {
                    info = Node.GetType().GetProperty(Parameter);

                    if (info != null)
                    {
                        var pro = info.GetCustomAttribute<PromoteAttribute>();

                        NodeType t = NodeType.Float;
                        if(pro != null)
                        {
                            t = pro.ExpectedType;
                        }

                        var v = info.GetValue(Node);

                        var p = Node.ParentGraph;

                        p = p.ParentNode != null ? p.ParentNode.ParentGraph : p;

                        if (p != null)
                        {
                            p.SetParameterValue(Node.Id, Parameter, v, pro != null, t);

                            FIcon.Opacity = 0.25;
                            DefaultVar.IsEnabled = true;
                            ConstantVar.IsEnabled = false;
                            FunctionVar.IsEnabled = false;
                        }
                        else
                        {
                            Log.Warn("Failed to promote to constant");
                        }
                    }
                    else
                    {
                        Log.Warn("Failed to promote to constant");
                    }
                }

            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
        }

        private void FunctionVar_Click(object sender, RoutedEventArgs e)
        {
            if (Node == null || string.IsNullOrEmpty(Parameter)) return;

            FunctionGraph g = new FunctionGraph(Node.Name + " - " + Parameter.Replace("$Custom.", "") + " Function", Node.Width, Node.Height);
            CreateFunctionParameter(g);
        }

        private void CreateFunctionParameter(FunctionGraph g)
        {
            g.AssignParentNode(Node);

            try
            {
                PropertyInfo info = null;
                if (Parameter.StartsWith("$Custom."))
                {
                    if (Node is GraphInstanceNode)
                    {
                        GraphInstanceNode gn = Node as GraphInstanceNode;

                        string cparam = Parameter.Replace("$Custom.", "");

                        var param = gn.GetCustomParameter(cparam);

                        if (param != null)
                        {
                            var cparent = Node.ParentGraph;

                            g.ExpectedOutput = param.Type;

                            if (cparent != null)
                            {
                                cparent.SetParameterValue(Node.Id, cparam, g, true, param.Type);

                                FIcon.Opacity = 1;
                                DefaultVar.IsEnabled = true;
                                ConstantVar.IsEnabled = false;
                                FunctionVar.IsEnabled = false;
                            }
                            else
                            {
                                Log.Warn("Failed to promote to function");
                            }
                        }
                        else
                        {
                            //log error
                            Log.Error("Could not find custom parameter: " + cparam);
                        }
                    }
                    else
                    {
                        Log.Warn("Failed to promoto to function");
                    }
                }
                else
                {
                    info = Node.GetType().GetProperty(Parameter);
                    if (info == null)
                    {
                        Log.Warn("Failed to promote to function");
                        return;
                    }

                    var pro = info.GetCustomAttribute<PromoteAttribute>();

                    if (pro != null)
                    {
                        g.ExpectedOutput = pro.ExpectedType;
                    }

                    var p = Node.ParentGraph;

                    p = p.ParentNode != null ? p.ParentNode.ParentGraph : p;

                    if (p != null)
                    {
                        p.SetParameterValue(Node.Id, Parameter, g, true, g.ExpectedOutput);

                        FIcon.Opacity = 1;
                        DefaultVar.IsEnabled = true;
                        ConstantVar.IsEnabled = false;
                        FunctionVar.IsEnabled = false;
                    }
                    else
                    {
                        Log.Warn("Failed to promote to function");
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
        }

        private void DefaultVar_Click(object sender, RoutedEventArgs e)
        {
            if (Node == null) return;

            var p = Node.ParentGraph;

            p = p.ParentNode != null ? p.ParentNode.ParentGraph : p;

            if (p != null)
            {
                p.RemoveParameterValue(Node.Id, Parameter.Replace("$Custom.", ""));

                FIcon.Opacity = 0.25;
                ConstantVar.IsEnabled = true;
                FunctionVar.IsEnabled = true;
                DefaultVar.IsEnabled = false;
            }
        }

        private void EditVar_Click(object sender, RoutedEventArgs e)
        {
            if (Node == null || string.IsNullOrEmpty(Parameter)) return;

            var p = Node.ParentGraph;

            p = p.ParentNode != null ? p.ParentNode.ParentGraph : p;

            if (p != null)
            {
                var realParam = Parameter.Replace("$Custom.", "");
                if (p.HasParameterValue(Node.Id, realParam))
                {
                    if(p.IsParameterValueFunction(Node.Id, realParam))
                    {
                        var v = p.GetParameterRaw(Node.Id, realParam);

                        if(MateriaMainWindow.Instance != null)
                        {
                            MateriaMainWindow.Instance.Push(Node, v.Value as Graph, Parameter);
                        }
                    }
                }
            }
        }

        private void AssignVar_Click(object sender, RoutedEventArgs e)
        {
            if (Node == null || string.IsNullOrEmpty(Parameter)) return;

            FuncMenuItem fmenu = sender as FuncMenuItem;
            FunctionGraph g = fmenu.Graph;
            Graph parent = g.ParentNode != null ? g.ParentNode.ParentGraph : g.ParentGraph;
            parent.RemoveParameterValueNoDispose(fmenu.Key);
            g.Name = Node.Name + " - " + Parameter.Replace("$Custom.", "") + " Function";

            CreateFunctionParameter(g);
        }
    }
}
