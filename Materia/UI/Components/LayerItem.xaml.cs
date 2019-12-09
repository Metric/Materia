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
using Materia.Layering;
using System.Reflection;

namespace Materia.UI.Components
{
    public enum LayerItemState
    {
        None,
        Selected,
        Viewing,
        SelectedViewing
    }

    /// <summary>
    /// Interaction logic for LayerItem.xaml
    /// </summary>
    public partial class LayerItem : UserControl
    {
        const float MOVE_DEAD_TOTAL = 10;

        public delegate void LayerItemEvent(LayerItem layer);
        public event LayerItemEvent OnSelect;
        public event LayerItemEvent OnView;

        private Window dragWindow;
        private WinApi.MouseHook hook;

        float moveX = 0;
        float moveY = 0;

        Point moveStart;

        protected LayerItemState state;
        public LayerItemState State
        {
            get
            {
                return state;
            }
            set
            {
                state = value;

                if (state == LayerItemState.None)
                {
                    Container.Background = (SolidColorBrush)Application.Current.Resources["Overlay5"];
                    BorderThickness = new Thickness(0);
                }
                else if (state == LayerItemState.Selected) 
                {
                    Container.Background = (SolidColorBrush)Application.Current.Resources["Overlay8"];
                    BorderThickness = new Thickness(0);
                }
                else if(state == LayerItemState.Viewing)
                {
                    Container.Background = (SolidColorBrush)Application.Current.Resources["Overlay5"];
                    BorderThickness = new Thickness(5,0,0,0);
                }
                else if(state == LayerItemState.SelectedViewing)
                {
                    Container.Background = (SolidColorBrush)Application.Current.Resources["Overlay8"];
                    BorderThickness = new Thickness(5, 0, 0, 0);
                }
            }
        }
       
        protected bool moveDown;

        double screenScale;

        public Layer Item { get; protected set; }
        public LayerItem()
        {
            InitializeComponent();
            Height = 32;
            VerticalAlignment = VerticalAlignment.Top;
            HorizontalAlignment = HorizontalAlignment.Stretch;
        }

        public LayerItem(Layer item)
        {
            InitializeComponent();
            Item = item;
            Height = 100;
            VerticalAlignment = VerticalAlignment.Top;
            HorizontalAlignment = HorizontalAlignment.Stretch;
            PropertyInfo prop = item.GetType().GetProperty("Name");
            LayerName.Set(prop, item, false, false);
            ToggleVisibilityIcon.Color = Item.Visible ? (Color)Application.Current.Resources["ColorTextLight"] : (Color)Application.Current.Resources["ColorTextDisabled"];
            if (Item.Mask != null)
            {
                MaskRemoveButton.Visibility = Visibility.Visible;
            }
            else
            {
                MaskRemoveButton.Visibility = Visibility.Collapsed;
            }
        }

        private void ToggleVisibility_Click(object sender, RoutedEventArgs e)
        {
            if (Item != null)
            {
                Item.Visible = !Item.Visible;
                ToggleVisibilityIcon.Color = Item.Visible ? (Color)Application.Current.Resources["ColorTextLight"] : (Color)Application.Current.Resources["ColorTextDisabled"];
            }
        }

        private void MaskRemoveButton_Click(object sender, RoutedEventArgs e)
        {
            if (Item != null)
            {
                MaskRemoveButton.ContextMenu.IsOpen = true;
            }
        }

        private void RemoveMask_Click(object sender, RoutedEventArgs e)
        {
            if (Item != null)
            {
                Item.Mask = null;
                MaskRemoveButton.Visibility = Visibility.Collapsed;
                Item.ParentGraph?.CombineLayers();
            }
        }

        private void MoveButton_MouseUp(object sender, MouseButtonEventArgs e)
        {
            moveDown = false;
        }

        private void Container_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //assign the node as a mask if possible
            if (UINodePoint.SelectOrigin != null)
            {
                var point = UINodePoint.SelectOrigin;
                if (point.Output != null && Item != null)
                {
                    Item.Mask = point.Output.Node;
                    MaskRemoveButton.Visibility = Visibility.Visible;
                    Item.ParentGraph?.CombineLayers();
                }
            }
            else
            {
                moveX = 0;
                moveY = 0;
                moveStart = e.GetPosition(this);
                moveDown = true;

                OnSelect?.Invoke(this);

                if (state == LayerItemState.None)
                {
                    State = LayerItemState.Selected;
                }
                else if (state == LayerItemState.Viewing)
                {
                    State = LayerItemState.SelectedViewing;
                }
            }
        }

        private void MoveButton_MouseMove(object sender, MouseEventArgs e)
        {
            if (moveDown)
            {
                Point p = e.GetPosition(this);
                moveX += (float)Math.Abs(moveStart.X - p.X);
                moveY += (float)Math.Abs(moveStart.Y - p.Y);

                if (moveX * moveX + moveY * moveY >= MOVE_DEAD_TOTAL * MOVE_DEAD_TOTAL)
                {
                    moveDown = false;
                    CreateDragWindow(this, ref p);
                    if (hook == null)
                    {
                        hook = new WinApi.MouseHook();
                        hook.MouseMoveEvent += Hook_MouseMoveEvent;
                    }
                    hook.SetHook();
                    DragDrop.DoDragDrop(this, $"Materia::Layer::{Item.Id}", DragDropEffects.Move);
                    hook.UnHook();
                    if (dragWindow != null)
                    {
                        dragWindow.Close();
                        dragWindow = null;
                    }
                }
            }
        }

        private void Hook_MouseMoveEvent(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if(dragWindow != null)
            {
                dragWindow.Left = (e.X / screenScale) + 5;
                dragWindow.Top = (e.Y / screenScale) + 5;
            }
        }

        private void UserControl_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (state == LayerItemState.None)
            {
                State = LayerItemState.Viewing;
            }
            else if (state == LayerItemState.Selected)
            {
                State = LayerItemState.SelectedViewing;
            }
            OnView?.Invoke(this);
        }

        private void CreateDragWindow(Visual dragElement, ref Point p)
        {
            p = PointToScreen(p);
            dragWindow = new Window();
            dragWindow.WindowStyle = WindowStyle.None;
            dragWindow.ResizeMode = ResizeMode.NoResize;
            dragWindow.AllowsTransparency = true;
            dragWindow.AllowDrop = false;
            dragWindow.Background = null;
            dragWindow.IsHitTestVisible = false;
            dragWindow.SizeToContent = SizeToContent.WidthAndHeight;
            dragWindow.Topmost = true;
            dragWindow.ShowInTaskbar = false;

            Rectangle r = new Rectangle();
            r.Width = ((FrameworkElement)dragElement).ActualWidth;
            r.Height = ((FrameworkElement)dragElement).ActualHeight;
            r.Fill = new VisualBrush(dragElement);
            dragWindow.Content = r;

            dragWindow.Left = p.X;
            dragWindow.Top = p.Y;

            dragWindow.Show();
        }

        public void OnDropped()
        {
            if (dragWindow != null)
            {
                dragWindow.Close();
                dragWindow = null;
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            screenScale = PresentationSource.FromVisual(this).CompositionTarget.TransformToDevice.M11;
        }
    }
}
