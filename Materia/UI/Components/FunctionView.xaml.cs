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

namespace Materia.UI.Components
{
    /// <summary>
    /// Interaction logic for CustomFunction.xaml
    /// </summary>
    public partial class FunctionView : UserControl
    {
        public delegate void FunctionRemove(FunctionView fn);
        public event FunctionRemove OnRemove;

        public FunctionGraph graph {get; protected set; }

        protected string nodeId;
        protected string param;

        protected bool isPromotedFunc;

        public FunctionView()
        {
            InitializeComponent();
        }

        /// <summary>
        /// This is used for Promoted Function Parameters
        /// And is used in GraphFunctionEditor
        /// </summary>
        /// <param name="node"></param>
        /// <param name="parameter"></param>
        /// <param name="g"></param>
        public FunctionView(string node, string parameter, FunctionGraph g)
        {
            InitializeComponent();
            isPromotedFunc = true;
            nodeId = node;
            param = parameter;
            graph = g;
            InitNameEdit(true);
        }

        /// <summary>
        /// This is used for Custom Functions
        /// And is used in CustomFunctionEditor
        /// </summary>
        /// <param name="g"></param>
        public FunctionView(FunctionGraph g)
        {
            InitializeComponent();
            isPromotedFunc = false;
            graph = g;
            InitNameEdit();
        }

        private void InitNameEdit(bool readOnly = false)
        {
            var prop = graph.GetType().GetProperty("Name");
            FuncName.Placeholder = "Function Name";
            FuncName.Set(prop, graph, readOnly);
        }

        private void EditFunc_Click(object sender, RoutedEventArgs e)
        {
            if(MateriaMainWindow.Instance != null)
            {
                if (isPromotedFunc)
                {
                    MateriaMainWindow.Instance.Push(graph.ParentNode, graph, param, GraphStackType.Parameter);
                }
                else
                {
                    MateriaMainWindow.Instance.Push(null, graph, null, GraphStackType.CustomFunction);
                }
            }
        }

        private void RemoveFunction_Click(object sender, RoutedEventArgs e)
        {
            var g = graph.ParentNode != null ? graph.ParentNode.ParentGraph : graph.ParentGraph;

            if (MessageBox.Show("Remove Function: " + graph.Name + "?", "", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
            {
                if (g != null)
                {
                    if (isPromotedFunc)
                    {
                        if(!string.IsNullOrEmpty(nodeId) && !string.IsNullOrEmpty(param))
                        {
                            g.RemoveParameterValue(nodeId, param);

                            if(OnRemove != null)
                            {
                                OnRemove.Invoke(this);
                            }
                        }
                    }
                    else
                    {
                        if (g.RemoveCustomFunction(graph))
                        {
                            if (OnRemove != null)
                            {
                                OnRemove.Invoke(this);
                            }
                        }
                    }
                }
            }
        }
    }
}
