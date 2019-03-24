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

namespace Materia.UI.Components
{
    /// <summary>
    /// Interaction logic for PropertyLabel.xaml
    /// </summary>
    public partial class PropertyLabel : UserControl
    {
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

                if(Node.ParentGraph != null)
                {
                    var g = Node.ParentGraph;

                    if(g.HasParameterValue(Node.Id, Parameter))
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
                PropertyInfo info = Node.GetType().GetProperty(Parameter);

                if(info != null)
                {
                    var v = info.GetValue(Node);

                    if(Node.ParentGraph != null)
                    {
                        var pg = Node.ParentGraph;

                        pg.SetParameterValue(Node.Id, Parameter, v);

                        DefaultVar.IsEnabled = true;
                        ConstantVar.IsEnabled = false;
                        FunctionVar.IsEnabled = false;
                    }
                }
            }
            catch { }
        }

        private void FunctionVar_Click(object sender, RoutedEventArgs e)
        {
            if (Node == null || string.IsNullOrEmpty(Parameter)) return;

            FunctionGraph g = new FunctionGraph(Node.Name + " - " + Parameter + " Function");
            g.ParentNode = Node;

            try
            {
                PropertyInfo info = Node.GetType().GetProperty(Parameter);
                if (info == null) return;

                var pro = info.GetCustomAttribute<PromoteAttribute>();

                g.ExpectedOutput = pro.ExpectedType;

                if (Node.ParentGraph != null)
                {
                    var pg = Node.ParentGraph;

                    pg.SetParameterValue(Node.Id, Parameter, g);

                    DefaultVar.IsEnabled = true;
                    ConstantVar.IsEnabled = false;
                    FunctionVar.IsEnabled = false;
                }
            }
            catch { }
        }

        private void DefaultVar_Click(object sender, RoutedEventArgs e)
        {
            if (Node == null) return;

            if(Node.ParentGraph != null)
            {
                var g = Node.ParentGraph;

                g.RemoveParameterValue(Node.Id, Parameter);

                ConstantVar.IsEnabled = true;
                FunctionVar.IsEnabled = true;
                DefaultVar.IsEnabled = false;
            }
        }

        private void EditVar_Click(object sender, RoutedEventArgs e)
        {
            if (Node == null || string.IsNullOrEmpty(Parameter)) return;

            if(Node.ParentGraph != null)
            {
                var g = Node.ParentGraph;
                if(g.HasParameterValue(Node.Id, Parameter))
                {
                    if(g.IsParameterValueFunction(Node.Id, Parameter))
                    {
                        var v = g.GetParameterRaw(Node.Id, Parameter);

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
