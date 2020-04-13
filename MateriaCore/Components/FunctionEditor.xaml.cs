using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Materia.Graph;
using System.Collections.Generic;

namespace MateriaCore.Components
{
    public class FunctionEditor : UserControl
    {
        Graph graph;
        StackPanel stack;
        Button addFunc;

        Dictionary<string, Function> promotedFunctions;
        List<Function> customFunctions;

        public FunctionEditor()
        {
            this.InitializeComponent();
            
            customFunctions = new List<Function>();
            promotedFunctions = new Dictionary<string, Function>();

            addFunc.Click += AddFunc_Click;
        }

        public FunctionEditor(Graph g, List<Function> funcs) : this()
        {
            graph = g;
            customFunctions = funcs;
            addFunc.IsVisible = true;
            PopulateCustom();
        }

        public FunctionEditor(Graph g, Dictionary<string, Function> funcs) : this()
        {
            graph = g;
            promotedFunctions = funcs;
            addFunc.IsVisible = false;
            PopulatePromoted();
        }

        void PopulatePromoted()
        {
            foreach(var k in promotedFunctions.Keys)
            {
                string[] split = k.Split('.');

                if (split.Length >= 2)
                {
                    CreateItem(promotedFunctions[k], split[0], split[1]);
                }
            }
        }

        void PopulateCustom()
        {
            for(int i = 0; i < customFunctions.Count; ++i)
            {
                Function gp = customFunctions[i];
                CreateItem(gp);
            }
        }

        private void AddFunc_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (graph == null)
            {
                return;
            }

            int index = graph.CustomFunctions.Count;
            string name = "Func" + index;
            Function gp = new Function(name);
            gp.AssignParentGraph(graph);
            graph.AddCustomFunction(gp);
            CreateItem(gp);
        }

        private void CreateItem(Function gp, string node, string parameter)
        {
            FunctionListItem item = new FunctionListItem(node, parameter, gp);
            item.OnRemove += Item_OnRemove;
            stack.Children.Add(item);
        }

        private void CreateItem(Function gp)
        {
            FunctionListItem item = new FunctionListItem(gp);
            item.OnRemove += Item_OnRemove;
            stack.Children.Add(item);
        }

        private void Item_OnRemove(FunctionListItem fn)
        {
            stack.Children.Remove(fn);
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            stack = this.FindControl<StackPanel>("Stack");
            addFunc = this.FindControl<Button>("AddFunc");
        }
    }
}
