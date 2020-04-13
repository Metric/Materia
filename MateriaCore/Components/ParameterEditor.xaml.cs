using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Materia.Graph;
using Materia.Rendering.Attributes;
using System.Collections.Generic;
using Materia.Nodes;

namespace MateriaCore.Components
{
    public class ParameterEditor : UserControl
    {
        StackPanel stack;
        Graph graph;
        Button addParam;

        List<ParameterValue> customParameters;
        Dictionary<string, ParameterValue> promotedParameters;

        public ParameterEditor()
        {
            this.InitializeComponent();

            customParameters = new List<ParameterValue>();
            promotedParameters = new Dictionary<string, ParameterValue>();

            addParam.Click += AddParam_Click;
        }

        public ParameterEditor(Graph g, List<ParameterValue> parameters) : this()
        {
            graph = g;
            customParameters = parameters;
            addParam.IsVisible = true;
            PopulateCustom();
        }

        public ParameterEditor(Graph g, Dictionary<string, ParameterValue> parameters) : this()
        {
            graph = g;
            promotedParameters = parameters;
            addParam.IsVisible = false;
            PopulatePromoted();
        }

        void PopulateCustom()
        {
            for(int i = 0; i < customParameters.Count; ++i)
            {
                ParameterValue p = customParameters[i];
                CreateItem(p);
            }
        }

        void PopulatePromoted()
        {
            foreach(string k in promotedParameters.Keys)
            {
                string[] split = k.Split('.');
                var v = promotedParameters[k];

                if (!v.IsFunction())
                {
                    Node n = null;
                    graph.NodeLookup.TryGetValue(split[0], out n);

                    string title = split[1];

                    if (n == null)
                    {
                        n = graph.FindSubNodeById(split[0]);
                    }

                    if (n != null)
                    {
                        title = n.Name + "." + split[1];
                    }

                    CreateItem(v, title, k);
                }
            }
        }

        void CreateItem(ParameterValue p, string title, string key)
        {
            ParameterListItem item = new ParameterListItem(title, key, p, "Name", "Type", "InputType");
            item.OnRemove += Item_Promoted_OnRemove;
            stack.Children.Add(item);
        }

        private void Item_Promoted_OnRemove(ParameterListItem c)
        {
            string[] split = c.Id.Split('.');
            graph.RemoveParameterValue(split[0], split[1]);
            stack.Children.Remove(c);
        }

        void CreateItem(ParameterValue p)
        {
            ParameterListItem item = new ParameterListItem(p, p.Id, false, "Name");
            item.OnRemove += Item_OnRemove;
            stack.Children.Add(item);
        }

        private void Item_OnRemove(ParameterListItem c)
        {
            graph?.RemoveCustomParameter(c.Param);
            stack.Children.Remove(c);
        }

        private void AddParam_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (graph == null)
            {
                return;
            }

            int index = graph.CustomParameters.Count;
            string name = "Param" + index;
            ParameterValue gp = new ParameterValue(name, 0, "", NodeType.Float);
            graph.AddCustomParameter(gp);
            CreateItem(gp);
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            addParam = this.FindControl<Button>("AddParam");
            stack = this.FindControl<StackPanel>("Stack");
        }
    }
}
