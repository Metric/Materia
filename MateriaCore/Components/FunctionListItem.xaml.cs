using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Materia.Graph;
using MateriaCore.Components.Dialogs;
using System.Threading.Tasks;

namespace MateriaCore.Components
{
    public class FunctionListItem : UserControl
    {
        public delegate void FunctionRemove(FunctionListItem fn);
        public event FunctionRemove OnRemove;

        TextInput funcName;
        Button editFunc;
        Button removeFunc;

        public Function Graph { get; protected set; }

        string nodeId;
        string param;

        bool isPromotedFunc;

        public FunctionListItem()
        {
            this.InitializeComponent();
            editFunc.Click += EditFunc_Click;
            removeFunc.Click += RemoveFunc_Click;
        }

        private void RemoveFunc_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            var g = Graph.ParentNode != null ? Graph.ParentNode.ParentGraph : Graph.ParentGraph;
            Task<MessageBox.MessageBoxResult> resulter = MessageBox.Show(MainWindow.Instance, "Remove Function: " + Graph.Name + "?", "", MessageBox.MessageBoxButtons.OkCancel);
            MessageBox.MessageBoxResult result = MessageBox.MessageBoxResult.No;
            Task.Run(async () =>
            {
                result = await resulter;
            }).ContinueWith(t =>
            {
                if (result == MessageBox.MessageBoxResult.Ok)
                {
                    if (g != null)
                    {
                        if (isPromotedFunc)
                        {
                            if (!string.IsNullOrEmpty(nodeId) && !string.IsNullOrEmpty(param))
                            {
                                g.RemoveParameterValue(nodeId, param);
                                OnRemove?.Invoke(this);
                            }
                        }
                        else
                        {
                            g.RemoveCustomFunction(Graph);
                            OnRemove?.Invoke(this);
                        }
                    }
                }
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        private void EditFunc_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (isPromotedFunc)
            {
                //TODO: implement main window handling here  
            }
        }

        public FunctionListItem(string node, string parameter, Function g) : this()
        {
            isPromotedFunc = true;
            nodeId = node;
            param = parameter;
            Graph = g;
            InitNameEdit();
        }

        public FunctionListItem(Function g) : this()
        {
            isPromotedFunc = false;
            Graph = g;
            InitNameEdit();
        }

        void InitNameEdit(bool readOnly = false)
        {
            var prop = Graph.GetType().GetProperty("Name");
            funcName.Placeholder = "Function Name";
            funcName.Set(prop, Graph, readOnly);
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            funcName = this.FindControl<TextInput>("FuncName");
            editFunc = this.FindControl<Button>("EditFunction");
            removeFunc = this.FindControl<Button>("RemoveFunction");
        }
    }
}
