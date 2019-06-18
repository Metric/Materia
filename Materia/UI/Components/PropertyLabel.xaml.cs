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
                EditVar.Visibility = Visibility.Visible;

                var p = Node.ParentGraph;

                if(p != null)
                {
                    p = p.TopGraph();
                }

                if(p != null)
                {
                    var g = p;

                    if(g.HasParameterValue(Node.Id, Parameter.Replace("$Custom.", "")))
                    {
                        ConstantVar.IsEnabled = false;
                        FunctionVar.IsEnabled = false;
                        DefaultVar.IsEnabled = true;
                    }
                    else
                    {
                        DefaultVar.IsEnabled = false;
                        ConstantVar.IsEnabled = true;
                        FunctionVar.IsEnabled = true;
                    }
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

                            if(cparent != null)
                            {
                                cparent = cparent.TopGraph();
                            }

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

                        if (p != null)
                        {
                            p = p.TopGraph();
                        }

                        if (p != null)
                        {
                            var pg = p;

                            pg.SetParameterValue(Node.Id, Parameter, v, pro != null, t);

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

            FunctionGraph g = new FunctionGraph(Node.Name + " - " + Parameter.Replace("$Custom.", "") + " Function");
            g.ParentNode = Node;

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

                            if(cparent != null)
                            {
                                cparent = cparent.TopGraph();
                            }

                            g.ExpectedOutput = param.Type;

                            if (cparent != null)
                            {
                                cparent.SetParameterValue(Node.Id, cparam, g, true, param.Type);

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

                    if (p != null)
                    {
                        p = p.TopGraph();
                    }

                    if (p != null)
                    {
                        var pg = p;

                        pg.SetParameterValue(Node.Id, Parameter, g, true, g.ExpectedOutput);

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

            if(p != null)
            {
                p = p.TopGraph();
            }

            if (p != null)
            {
                var g = p;

                g.RemoveParameterValue(Node.Id, Parameter.Replace("$Custom.", ""));

                ConstantVar.IsEnabled = true;
                FunctionVar.IsEnabled = true;
                DefaultVar.IsEnabled = false;
            }
        }

        private void EditVar_Click(object sender, RoutedEventArgs e)
        {
            if (Node == null || string.IsNullOrEmpty(Parameter)) return;

            var p = Node.ParentGraph;

            if(p != null)
            {
                p = p.TopGraph();
            }

            if (p != null)
            {
                var g = p;
                var realParam = Parameter.Replace("$Custom.", "");
                if (g.HasParameterValue(Node.Id, realParam))
                {
                    if(g.IsParameterValueFunction(Node.Id, realParam))
                    {
                        var v = g.GetParameterRaw(Node.Id, realParam);

                        if(MainWindow.Instance != null)
                        {
                            MainWindow.Instance.Push(Node, v.Value as Graph, Parameter);
                        }
                    }
                }
            }
        }
    }
}
