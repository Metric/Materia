using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Materia.Nodes;
using Materia.Graph;
using Materia.Rendering;
using System.Linq;

namespace MateriaCore.Components.Panes
{
    public class Parameters : Window
    {
        object node;
        ParameterMap map;

        public static Parameters Current { get; protected set; }

        public Parameters()
        {
            InitializeComponent();
            Current = this;
        }

        public void Set(object n)
        {
            if (n == null) return;
            if (n == node) return;

            node = n;

            map?.Clear();

            if (n is Node)
            {
                Node nd = (Node)n;
                Title = nd.Name;
            }
            else if(n is Graph)
            {
                Title = "Graph";
            }
            else if(n is Camera)
            {
                Title = "Camera";
            }
            //note add back in support for material settings
            //and lighting settings here
            else
            {
                Title = node.GetType().Name.ToString().Split(new char[] { '.' }).LastOrDefault();
            }

            map?.Set(n);
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            map = this.FindControl<ParameterMap>("Params");
        }
    }
}
