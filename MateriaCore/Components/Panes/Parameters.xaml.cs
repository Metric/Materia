using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Materia.Nodes;
using Materia.Graph;
using Materia.Rendering;
using System.Linq;
using MateriaCore.Utils;

namespace MateriaCore.Components.Panes
{
    public class Parameters : Window
    {
        object node;
        ParameterMap map;

        MainGLWindow parent;

        TextBlock title;

        const float MAX_HEIGHT_PERCENT = 0.5f; //todo: move this to an optional setting for users

        public Parameters()
        {
            InitializeComponent();
        }

        public Parameters(MainGLWindow window) : this()
        {
            parent = window;
            parent.Closing += Parent_Closing;
            parent.Minimized += Parent_Minimized;
            parent.Maximized += Parent_Maximized;
            parent.Resize += Parent_Resize;
            parent.Move += Parent_Move;
            parent.Restored += Parent_Restored;

            Closing += Parameters_Closing;

            GlobalEvents.On(GlobalEvent.ClearViewParameters, OnClearParameters);
            GlobalEvents.On(GlobalEvent.ViewParameters, OnViewParameters);

            AlignToParent();
        }

        private void Parent_Restored()
        {
            Show();
            AlignToParent();
        }

        private void AlignToParent()
        {
            if (parent == null) return;
            var bounds = parent.Bounds;
            Height = bounds.Size.Y * MAX_HEIGHT_PERCENT;
            Position = new PixelPoint(bounds.Max.X - (int)Width - 1, bounds.Min.Y + 5);
        }

        private void Parent_Closing(System.ComponentModel.CancelEventArgs obj)
        {
            Close();
        }

        private void Parent_Minimized(OpenTK.Windowing.Common.MinimizedEventArgs obj)
        {
            Hide();
        }

        private void Parent_Maximized(OpenTK.Windowing.Common.MaximizedEventArgs obj)
        {
            AlignToParent();
        }

        private void Parent_Move(OpenTK.Windowing.Common.WindowPositionEventArgs obj)
        {
            AlignToParent();
        }

        private void Parent_Resize(OpenTK.Windowing.Common.ResizeEventArgs obj)
        {
            AlignToParent();
        }

        private void OnClearParameters(object sender, object n)
        {
            title.Text = Title = "Properties";
            node = null;
            map?.Set(null);
            map?.Clear();
        }

        private void OnViewParameters(object sender, object n)
        {
            Set(n);
        }

        private void Parameters_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            parent.Closing -= Parent_Closing;
            parent.Minimized -= Parent_Minimized;
            parent.Maximized -= Parent_Maximized;
            parent.Resize -= Parent_Resize;
            parent.Move -= Parent_Move;
            parent.Restored -= Parent_Restored;

            GlobalEvents.Off(GlobalEvent.ClearViewParameters, OnClearParameters);
            GlobalEvents.Off(GlobalEvent.ViewParameters, OnViewParameters);
        }

        private void Set(object n)
        {
            if (n == null) return;
            if (n == node) return;

            node = n;

            map?.Clear();

            if (n is Node)
            {
                Node nd = (Node)n;
                title.Text = Title = "Properties - " + nd.Name;
            }
            else if(n is Graph)
            {
                title.Text = Title = "Properties - " + "Graph";
            }
            else if(n is Camera)
            {
                title.Text = Title = "Properties - " + "Camera";
            }
            //note add back in support for material settings
            //and lighting settings here
            else
            {
                title.Text = Title = "Properties - " + node.GetType().Name.ToString().Split(new char[] { '.' }).LastOrDefault();
            }

            map?.Set(n);
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            map = this.FindControl<ParameterMap>("Params");
            title = this.FindControl<TextBlock>("Title");
        }
    }
}
