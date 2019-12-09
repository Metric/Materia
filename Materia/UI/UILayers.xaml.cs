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
using Materia.Nodes.Atomic;
using Materia.Layering;
using Materia.UI.Components;
using System.Reflection;

namespace Materia.UI
{
    /// <summary>
    /// Interaction logic for UILayers.xaml
    /// </summary>
    public partial class UILayers : UserControl
    {
        public delegate void ViewEvent(Graph g);
        public delegate void LayerEvent(Layer layer);
        public static event LayerEvent OnSelected;
        public static event ViewEvent OnRestoreRootView;
        public static event LayerEvent OnRemoveLayer;

        protected static UILayers Instance { get; set; }
        public static Graph Current { get; protected set; }

        public static LayerItem Selected { get; protected set; }

        public UILayers()
        {
            InitializeComponent();
            Instance = this;
        }

        public static void Assign(Graph g)
        {
            Selected = null;
            Instance.LayerStack.Children.Clear();
            Current = g;
            Instance.Populate();
        }

        protected void Populate()
        {
            if (Current != null)
            {
                foreach (Layer layer in Current.Layers)
                {
                    //create layer stuff
                    LayerItem item = new LayerItem(layer);
                    item.Height = 32;
                    LayerStack.Children.Add(item);
                    item.OnSelect += Item_OnSelect;
                    item.OnView += Item_OnView;
                }

                AddLayerButton.IsEnabled = true;
            }
            else
            {
                AddLayerButton.IsEnabled = false;
            }

            DeleteLayerButton.IsEnabled = false;
            BlendModeDropDown.IsEnabled = false;
            OpacitySlider.IsEnabled = false;
        }

        private void Item_OnSelect(LayerItem layer)
        {
            if (Selected != null)
            {
                if (Selected.State == LayerItemState.SelectedViewing)
                {
                    Selected.State = LayerItemState.Viewing;
                }
                else if(Selected.State == LayerItemState.Selected)
                {
                    Selected.State = LayerItemState.None;
                }
            }

            Selected = layer;

            HookInputsForSelected();
        }

        private void HookInputsForSelected()
        {
            if(Selected == null)
            {
                DeleteLayerButton.IsEnabled = false;
                BlendModeDropDown.IsEnabled = false;
                OpacitySlider.IsEnabled = false;
            }
            else
            {
                DeleteLayerButton.IsEnabled = true;
                BlendModeDropDown.IsEnabled = true;
                OpacitySlider.IsEnabled = true;
            }

            if (Selected != null)
            {
                PropertyInfo prop = Selected.Item.GetType().GetProperty("Blending");
                BlendModeDropDown.Set(Enum.GetNames(typeof(BlendType)), Selected.Item, prop, null, false);

                prop = Selected.Item.GetType().GetProperty("Opacity");
                OpacitySlider.Set(0, 1, prop, Selected.Item);
            }
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            AddLayerButton.ContextMenu.IsOpen = true;
        }

        private void AddLayer_Click(object sender, RoutedEventArgs e)
        {
            MenuItem item = sender as MenuItem;
            if (item.Header.ToString().ToLower().Contains("empty") && Current != null)
            {
                Layer layer = new Layer("Layer " + LayerStack.Children.Count, Current.Width, Current.Height, Current);
                LayerItem layerItem = new LayerItem(layer);
                layerItem.Height = 32;
                layerItem.OnSelect += Item_OnSelect;
                layerItem.OnView += Item_OnView;

                if (Current.Layers.Count > 0)
                {
                    Current.Layers.Insert(0, layer);
                }
                else
                {
                    Current.Layers.Add(layer);
                }

                Current.LayerLookup[layer.Id] = layer;

                if (LayerStack.Children.Count > 0)
                {
                    LayerStack.Children.Insert(0, layerItem);
                }
                else
                {
                    LayerStack.Children.Add(layerItem);
                }

                Current.Modified = true;
            }
            //handle from file eventually
            else
            {

            }
        }

        private void DuplicateLayer(Layer l)
        {
            Layer copy = new Layer(l);
            LayerItem layerItem = new LayerItem(copy);
            layerItem.Height = 32;
            layerItem.OnSelect += Item_OnSelect;
            layerItem.OnView += Item_OnView;

            if (Current.Layers.Count > 0)
            {
                Current.Layers.Insert(0, copy);
            }
            else
            {
                Current.Layers.Add(copy);
            }

            Current.LayerLookup[copy.Id] = copy;

            if (LayerStack.Children.Count > 0)
            {
                LayerStack.Children.Insert(0, layerItem);
            }
            else
            {
                LayerStack.Children.Add(layerItem);
            }

            Current.Modified = true;

            copy.TryAndProcess();
        }

        private void Item_OnView(LayerItem layer)
        {
            OnSelected?.Invoke(layer.Item);
        }

        private void DeleteLayer_Click(object sender, RoutedEventArgs e)
        {
            if(Selected != null && Current != null)
            {
                //dispose etc
                //remove from parent graph etc
                if(MessageBox.Show("Delete Layer [" + Selected.Item.Name + "]", "", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    RemoveLayer(Selected);
                }
            }
        }

        private void RemoveLayer(LayerItem layer)
        {
            if (Current.Layers.Remove(layer.Item))
            {
                Current.LayerLookup.Remove(layer.Item.Id);
                Current.Layers.Remove(layer.Item);
                LayerStack.Children.Remove(layer);

                if (layer.State == LayerItemState.SelectedViewing || layer.State == LayerItemState.Viewing)
                {
                    OnRestoreRootView?.Invoke(Current);
                }

                Current.Modified = true;
                Current?.CombineLayers();

                OnRemoveLayer?.Invoke(layer.Item);

                layer.Item?.Dispose();
                layer.OnSelect -= Item_OnSelect;
                layer.OnView -= Item_OnView;

                if (Selected == layer)
                {
                    DeleteLayerButton.IsEnabled = false;
                    BlendModeDropDown.IsEnabled = false;
                    OpacitySlider.IsEnabled = false;
                    Selected = null;
                }
            }
        }

        private void ScrollViewer_Drop(object sender, DragEventArgs e)
        {
            string data = (string)e.Data.GetData(DataFormats.Text);
            if (data.Contains("Materia::Layer::"))
            {
                Point p = e.GetPosition(ScrollView);
                double offset = ScrollView.VerticalOffset;
                double realY = offset + p.Y;

                LayerItem orig = LayerStack.Children.OfType<LayerItem>().FirstOrDefault(m => data.Contains(m.Item.Id));

                if (orig == null) return;

                LayerStack.Children.Remove(orig);

                bool added = false;

                for (int i = 0; i < LayerStack.Children.Count; ++i)
                {
                    LayerItem item = LayerStack.Children[i] as LayerItem;
                    if (item == null) continue;
                    if (item == orig) continue;
                    Point relative = item.TransformToAncestor(LayerStack).Transform(new Point(0, 0));
                    if (realY >= relative.Y && realY <= relative.Y + item.ActualHeight)
                    {
                        LayerStack.Children.Insert(i, orig);

                        //update graph layer data as well
                        Current.Layers.Remove(orig.Item);
                        Current.Layers.Insert(i, orig.Item);

                        added = true;
                        break;
                    }
                }

                if(!added)
                {
                    LayerStack.Children.Add(orig);
                    Current.Layers.Remove(orig.Item);
                    Current.Layers.Add(orig.Item);
                }

                Current?.CombineLayers();
            }
        }

        private void RootViewButton_Click(object sender, RoutedEventArgs e)
        {
            OnRestoreRootView?.Invoke(Current);
        }

        private void DeleteLayerButton_Drop(object sender, DragEventArgs e)
        {
            string data = (string)e.Data.GetData(DataFormats.Text);
            if (data.Contains("Materia::Layer::"))
            {
                for (int i = 0; i < LayerStack.Children.Count; ++i)
                {
                    LayerItem item = LayerStack.Children[i] as LayerItem;
                    if (item == null) continue;
                    if (data.Contains(item.Item.Id))
                    {
                        RemoveLayer(item);
                        break;
                    }
                }
            }
        }

        private void AddLayerButton_Drop(object sender, DragEventArgs e)
        {
            string data = (string)e.Data.GetData(DataFormats.Text);
            if (data.Contains("Materia::Layer::"))
            {
                for (int i = 0; i < LayerStack.Children.Count; ++i)
                {
                    LayerItem item = LayerStack.Children[i] as LayerItem;
                    if (item == null) continue;
                    if (data.Contains(item.Item.Id))
                    {
                        DuplicateLayer(item.Item);
                        break;
                    }
                }
            }
        }
    }
}
