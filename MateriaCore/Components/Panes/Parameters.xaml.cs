using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Materia.Nodes;
using Materia.Graph;
using Materia.Rendering;
using System.Linq;
using MateriaCore.Utils;
using MateriaCore.Components.GL;
using Materia.Rendering.Mathematics;
using System.Threading.Tasks;

namespace MateriaCore.Components.Panes
{
    public class Parameters : Window
    {
        IGraphNode trackedNode;
        object node;
        ParameterMap map;

        MainGLWindow parent;

        TextBlock title;

        ScrollViewer scrollView;

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
            GlobalEvents.On(GlobalEvent.UpdateTrackedNode, OnUpdateTrackedNode);
            GlobalEvents.On(GlobalEvent.ShowParameters, OnShowTrackedNode);
            GlobalEvents.On(GlobalEvent.HideParameters, OnHideTrackedNode);

            Hide();
        }

        private bool AlignToNode()
        {
            if (parent == null || trackedNode == null) return false;

            bool delayNeeded = Height != 256;

            Height = 256;

            if (delayNeeded)
            {
                //avalonia is fucking retarded!
                //why window bounds are not recalculated the moement
                //Height or Width is changed, is beyond me.
                //They even break their own ScrollViewer because of this!
                Task.Delay(100).ContinueWith(t =>
                {
                    FinalizeAlignToNode();
                }, TaskScheduler.FromCurrentSynchronizationContext());
                return true;
            }
            else
            {
                FinalizeAlignToNode();
            }
            return false;
        }

        private void FinalizeAlignToNode()
        {
            var parentBounds = parent.Bounds;
            Box2 rect = trackedNode.GetViewSpaceRect();

            var bounds = Bounds;

            float x = parentBounds.Min.X + rect.Left + rect.Width;
            float y = parentBounds.Min.Y + rect.Top;

            float maxX = parentBounds.Max.X;
            float maxY = parentBounds.Max.Y;

            if (x > maxX)
            {
                x = maxX - 5 - (float)Width;
                if (rect.Left < maxX)
                {
                    x = parentBounds.Min.X + rect.Left - (float)bounds.Width;
                }
            }
            else if (x < parentBounds.Min.X)
            {
                x = parentBounds.Min.X + 5;
            }

            if (y + bounds.Height > maxY)
            {
                y = maxY - (float)bounds.Height;
            }
            else if (y < parentBounds.Min.Y)
            {
                y = parentBounds.Min.Y + 5;
            }

            Position = new PixelPoint((int)x, (int)y);

            //why this is needed is beyond common sense
            //apparently setting it to 256 the first time
            //was not an actual real thing when it moved
            //oh no, after it moves it sets it to 450
            //after setting it to 256 the first time,
            //even though it really did 
            //resize the window to 256...
            //avalonia is officially really really retarded in
            //how they handle calculations
            Height = 256; 
            
        }

        private bool AlignToParent()
        {
            if (parent == null) return false;
            var parentBounds = parent.Bounds;
            bool delayed = Height != parentBounds.Size.Y * MAX_HEIGHT_PERCENT;
            Height = parentBounds.Size.Y * MAX_HEIGHT_PERCENT;
            var bounds = Bounds;
            Position = new PixelPoint(parentBounds.Max.X - (int)bounds.Width - 1, parentBounds.Min.Y + 5);
            return delayed;
        }


        private void Parent_Restored()
        {
            if (node != null)
            {
                Show();
            }

            if (trackedNode != null)
            {
                AlignToNode();
            }
            else if(node != null)
            {
                AlignToParent();
            }
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
            if (trackedNode != null)
            {
                AlignToNode();
            }
            else if (node != null)
            {
                AlignToParent();
            }
        }

        private void Parent_Move(OpenTK.Windowing.Common.WindowPositionEventArgs obj)
        {
            if (trackedNode != null)
            {
                AlignToNode();
            }
            else if(node != null)
            {
                AlignToParent();
            }
        }

        private void Parent_Resize(OpenTK.Windowing.Common.ResizeEventArgs obj)
        {
            if (trackedNode != null)
            {
                AlignToNode();
            }
            else if (node != null)
            {
                AlignToParent();
            }
        }

        private void OnClearParameters(object sender, object n)
        {
            title.Text = Title = "Properties";
            node = null;
            map?.Set(null);
            map?.Clear();
            Hide();
        }

        private void OnHideTrackedNode(object sender, object n)
        {
            if (trackedNode == n)
            {
                Hide();
            }
        }

        private void OnShowTrackedNode(object sender, object n)
        {
            if (trackedNode == n)
            {
                AlignToNode();
                Show();
            }
        }

        private void OnUpdateTrackedNode(object sender, object n)
        {
            if (trackedNode != null)
            {
                AlignToNode();
            }
        }

        private void OnViewParameters(object sender, object n)
        {
            //allow toggling of view
            //since we now only trigger this event
            //on double click of a node
            if (sender == trackedNode 
                && trackedNode != null && sender != null)
            {
                if (IsVisible)
                {
                    Hide();
                }
                else
                {
                    AlignToNode();
                    Show();
                    Set(n);
                }

                return;
            }

            //otherwise set new tracked node
            if (sender is IGraphNode)
            {
                trackedNode = sender as IGraphNode;
            }
            else
            {
                trackedNode = null;
            }

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
            if (n == null)
            {
                node = null;
                map?.Clear();
                Hide();
                return;
            }

            if (n == node)
            {
                if (trackedNode != null)
                {
                    AlignToNode();
                }
                else
                {
                    AlignToParent();
                }
                return;
            }

            node = n;

            map?.Clear();

            if (n is Node)
            {
                Node nd = (Node)n;
                title.Text = Title = "Properties - " + nd.Name;
            }
            else if(n is Graph)
            {
                title.Text = Title = "Properties - Graph";
            }
            else if(n is Camera)
            {
                title.Text = Title = "Properties - Camera";
            }
            else if(n is Settings.Lighting)
            {
                title.Text = Title = "Properties - Light";
            }
            else if(n is Settings.Material)
            {
                title.Text = Title = "Properties - Material";
            }
            else
            {
                title.Text = Title = "Properties - " + node.GetType().Name.ToString().Split(new char[] { '.' }).LastOrDefault();
            }

            bool delayed = false;

            if (trackedNode != null)
            {
                delayed = AlignToNode();
            }
            else
            {
                delayed = AlignToParent();
            }

            //we have to do this otherwise avalonia is so retarded
            //that their ScrollViwer breaks if Height of window is changed via Height property
            //and the only way to fix it is to populate after window is fully resized
            if (delayed)
            {
                Task.Delay(101).ContinueWith(t =>
                {
                    map?.Set(n);
                    if (parent != null && parent.WindowState != OpenTK.Windowing.Common.WindowState.Minimized)
                    {
                        Show();
                    }
                }, TaskScheduler.FromCurrentSynchronizationContext());
            }
            else
            {
                map?.Set(n);
                if (parent != null && parent.WindowState != OpenTK.Windowing.Common.WindowState.Minimized)
                {
                    Show();
                }
            }
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            map = this.FindControl<ParameterMap>("Params");
            title = this.FindControl<TextBlock>("Title");
            scrollView = this.FindControl<ScrollViewer>("ScrollView");
        }
    }
}
