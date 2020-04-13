using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Materia.Graph;
using Materia.Nodes;
using Materia.Rendering.Attributes;
using System.Collections.Generic;
using System.Reflection;
using MLog;
using Materia.Nodes.Atomic;
using System;

namespace MateriaCore.Components
{
    public class PropertyLabel : UserControl
    {
        public class FuncMenuItem : MenuItem
        {
            public string Key { get; protected set; }
            public Function Graph { get; protected set; }
            public FuncMenuItem(Function g, string k) : base()
            {
                Graph = g;
                Key = k;
                Header = g.Name;
            }
        }

        MenuItem constantVar;
        MenuItem functionVar;
        MenuItem assignVar;
        MenuItem defaultVar;

        Avalonia.Controls.Image fIcon;

        TextBlock labelContent;
        SplitButton editVar;

        public string Title
        {
            get
            {
                return labelContent.Text;
            }
            set
            {
                labelContent.Text = value;
            }
        }

        public Node Node { get; protected set; }
        public string Parameter { get; protected set; }

        public PropertyLabel()
        {
            this.InitializeComponent();
            editVar.IsVisible = true;
            constantVar.Click += ConstantVar_Click;
            functionVar.Click += FunctionVar_Click;
            defaultVar.Click += DefaultVar_Click;
            editVar.Click += EditVar_Click;
        }

        private void EditVar_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (Node == null || string.IsNullOrEmpty(Parameter)) return;

            var p = Node.ParentGraph;

            p = p.ParentNode != null ? p.ParentNode.ParentGraph : p;

            if (p != null)
            {
                var realParam = Parameter.Replace("$Custom.", "");
                if (p.HasParameterValue(Node.Id, realParam))
                {
                    if (p.IsParameterValueFunction(Node.Id, realParam))
                    {
                        var v = p.GetParameterRaw(Node.Id, realParam);

                        /*if (MateriaMainWindow.Instance != null)
                        {
                            MateriaMainWindow.Instance.Push(Node, v.Value as Graph, Parameter);
                        }*/
                    }
                }
            }
        }

        private void DefaultVar_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (Node == null) return;

            var p = Node.ParentGraph;

            p = p.ParentNode != null ? p.ParentNode.ParentGraph : p;

            if (p != null)
            {
                p.RemoveParameterValue(Node.Id, Parameter.Replace("$Custom.", ""));

                fIcon.Opacity = 0.25;
                constantVar.IsEnabled = true;
                functionVar.IsEnabled = true;
                defaultVar.IsEnabled = false;
            }
        }

        private void FunctionVar_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (Node == null || string.IsNullOrEmpty(Parameter)) return;

            Function g = new Function(Node.Name + " - " + Parameter.Replace("$Custom.", "") + " Function", Node.Width, Node.Height);
            CreateFunctionParameter(g);
        }

        private void CreateFunctionParameter(Function g)
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

                                fIcon.Opacity = 1;
                                defaultVar.IsEnabled = true;
                                constantVar.IsEnabled = false;
                                functionVar.IsEnabled = false;
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

                        fIcon.Opacity = 1;
                        defaultVar.IsEnabled = true;
                        constantVar.IsEnabled = false;
                        functionVar.IsEnabled = false;
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

        private void ConstantVar_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
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

                                fIcon.Opacity = 0.25;
                                defaultVar.IsEnabled = true;
                                constantVar.IsEnabled = false;
                                functionVar.IsEnabled = false;
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
                else
                {
                    info = Node.GetType().GetProperty(Parameter);

                    if (info != null)
                    {
                        var pro = info.GetCustomAttribute<PromoteAttribute>();

                        NodeType t = NodeType.Float;
                        if (pro != null)
                        {
                            t = pro.ExpectedType;
                        }

                        var v = info.GetValue(Node);

                        var p = Node.ParentGraph;

                        p = p.ParentNode != null ? p.ParentNode.ParentGraph : p;

                        if (p != null)
                        {
                            p.SetParameterValue(Node.Id, Parameter, v, pro != null, t);

                            fIcon.Opacity = 0.25;
                            defaultVar.IsEnabled = true;
                            constantVar.IsEnabled = false;
                            functionVar.IsEnabled = false;
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

        public PropertyLabel(string title, Node n = null, string param = null) : this()
        {
            Title = title;

            Node = n;
            Parameter = param;

            if (Node != null && !string.IsNullOrEmpty(Parameter))
            {
                if (!Parameter.StartsWith("$Custom."))
                {
                    var prop = Node.GetType().GetProperty(Parameter);
                    if (prop != null && prop.GetCustomAttribute<PromoteAttribute>() == null)
                    {
                        editVar.IsVisible = false;
                        return;
                    }
                }

                editVar.IsVisible = true;

                var p = Node.ParentGraph;

                p = p.ParentNode != null ? p.ParentNode.ParentGraph : p;

                if (p != null)
                {
                    if (p.HasParameterValue(Node.Id, Parameter.Replace("$Custom.", "")))
                    {
                        if (p.IsParameterValueFunction(Node.Id, Parameter.Replace("$Custom.", "")))
                        {
                            fIcon.Opacity = 1;
                        }
                        else
                        {
                            fIcon.Opacity = 0.25;
                        }

                        constantVar.IsEnabled = false;
                        functionVar.IsEnabled = false;
                        assignVar.IsEnabled = false;
                        defaultVar.IsEnabled = true;
                    }
                    else
                    {
                        fIcon.Opacity = 0.25;
                        defaultVar.IsEnabled = false;
                        assignVar.IsEnabled = true;
                        constantVar.IsEnabled = true;
                        functionVar.IsEnabled = true;
                    }
                }

                var functionsAvailable = Node.ParentGraph.ParameterFunctions;

                List<MenuItem> items = new List<MenuItem>();

                foreach (string k in functionsAvailable.Keys)
                {
                    Function f = functionsAvailable[k];
                    MenuItem mitem = new FuncMenuItem(f, k);
                    mitem.Click += Mitem_Click;
                    items.Add(mitem);
                }

                assignVar.Items = items;
            }
            else
            {
                editVar.IsVisible = false;
            }
        }

        private void Mitem_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (Node == null || string.IsNullOrEmpty(Parameter)) return;

            FuncMenuItem fmenu = sender as FuncMenuItem;
            Function g = fmenu.Graph;
            Graph parent = g.ParentNode != null ? g.ParentNode.ParentGraph : g.ParentGraph;
            parent.RemoveParameterValueNoDispose(fmenu.Key);
            g.Name = Node.Name + " - " + Parameter.Replace("$Custom.", "") + " Function";

            CreateFunctionParameter(g);
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            labelContent = this.FindControl<TextBlock>("LabelContent");
            fIcon = this.FindControl<Avalonia.Controls.Image>("FIcon");
            constantVar = this.FindControl<MenuItem>("ConstantVar");
            assignVar = this.FindControl<MenuItem>("AssignVar");
            functionVar = this.FindControl<MenuItem>("FunctionVar");
            defaultVar = this.FindControl<MenuItem>("DefaultVar");
            editVar = this.FindControl<SplitButton>("EditVar");
        }
    }
}
