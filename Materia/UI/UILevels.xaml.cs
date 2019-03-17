using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Materia.Imaging;
using Materia.Nodes.Helpers;
using Materia.Nodes.Containers;
using System.Reflection;

namespace Materia
{
    /// <summary>
    /// Interaction logic for UILevels.xaml
    /// </summary>
    public partial class UILevels : UserControl, IParameter
    {
        object propertyOwner;
        PropertyInfo property;

        bool inputFromUser;

        public enum LevelMode
        {
            RGB = -1,
            Red = 0,
            Green = 1,
            Blue = 2
        }

        MultiRange range;

        CancellationTokenSource ctk;

        RawBitmap fromBit;

        LevelMode mode;

        public UILevels()
        {
            InitializeComponent();
            mode = LevelMode.RGB;
            range = new MultiRange();
        }

        public UILevels(RawBitmap fromBitmap, object owner, PropertyInfo p)
        {
            InitializeComponent();
            mode = LevelMode.RGB;
            property = p;
            propertyOwner = owner;
            range = (MultiRange)p.GetValue(owner);

            fromBit = fromBitmap;
            MultiSlider.Set(range.min[0], range.mid[0], range.max[0]);

            inputFromUser = false;
        }

        public void OnUpdate(object obj)
        {
            if (!inputFromUser)
            {
                Nodes.Node n = obj as Nodes.Node;

                if (n != null)
                {
                    byte[] result = n.GetPreview(n.Width, n.Height);

                    if(result != null)
                    {
                        fromBit = new RawBitmap(n.Width, n.Height, result);
                        
                        Histogram.GenerateHistograph(fromBit);
                    }
                }
            }

            inputFromUser = false;
        }

        private void MultiSlider_OnValueChanged(object sender, float min, float mid, float max)
        {
            inputFromUser = true;

            if(mode == LevelMode.RGB)
            {
                range.mid[0] = mid;
                range.max[0] = max;
                range.min[0] = min;

                range.mid[1] = mid;
                range.max[1] = max;
                range.min[1] = min;

                range.mid[2] = mid;
                range.max[2] = max;
                range.min[2] = min;
            }
            else
            {
                int g = (int)mode;
                range.min[g] = min;
                range.max[g] = max;
                range.mid[g] = mid;
            }

            if(ctk != null)
            {
                ctk.Cancel();
                ctk = null;
            }

            ctk = new CancellationTokenSource();

            Task.Delay(250, ctk.Token)
                .ContinueWith(t =>
                {
                    if (t.IsCanceled) return;
                    property.SetValue(propertyOwner, range);
                });
        }

        private void Channels_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBoxItem item = (ComboBoxItem)Channels.SelectedItem;
            string c = (string)item.Content;
            mode = (LevelMode)Enum.Parse(typeof(LevelMode), c);

            if(mode != LevelMode.RGB)
            {
                int g = (int)mode;
                MultiSlider.Set(range.min[g], range.mid[g], range.max[g]);
            }
            else
            {
                MultiSlider.Set(range.min[0], range.mid[0], range.max[0]);
            }

            //build histogram
            Histogram.Mode = mode;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (fromBit != null)
            {
                Histogram.GenerateHistograph(fromBit);
                fromBit = null;
            }

            Channels.SelectedIndex = 0;
            inputFromUser = false;

            Task.Delay(10).ContinueWith(t =>
            {
                App.Current.Dispatcher.Invoke(() =>
                {
                    Histogram.BuildHistogramImage();
                });
            });
        }

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            MultiSlider.SetButtonPositions();
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            if (mode == LevelMode.RGB)
            {
                range.mid[0] = 0.5f;
                range.max[0] = 1;
                range.min[0] = 0;

                range.mid[1] = 0.5f;
                range.max[1] = 1;
                range.min[1] = 0;

                range.mid[2] = 0.5f;
                range.max[2] = 1;
                range.min[2] = 0;
            }
            else
            {
                int g = (int)mode;
                range.min[g] = 0;
                range.max[g] = 1;
                range.mid[g] = 0.5f;
            }

            MultiSlider.Set(0, 0.5f, 1);
            property.SetValue(propertyOwner, range);
        }
    }
}
