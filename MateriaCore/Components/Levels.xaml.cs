using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Materia.Nodes.Containers;
using Materia.Rendering.Imaging;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace MateriaCore.Components
{
    public enum LevelMode
    {
        RGB = -1,
        Red = 0,
        Green = 1,
        Blue = 2
    }

    public class Levels : UserControl
    {
        MultiRangeSlider multiSlider;
        RangeSlider valueRange;
        Histogram histogram;
        ComboBox channels;
        Button reset;

        PropertyInfo property;
        object propertyOwner;

        CancellationTokenSource ctk;
        RawBitmap bmp;
        LevelMode mode;

        MultiRange range;

        public Levels()
        {
            this.InitializeComponent();
            mode = LevelMode.RGB;
            range = new MultiRange();

            multiSlider.OnValueChanged += MultiSlider_OnValueChanged;
            valueRange.OnValueChanged += ValueRange_OnValueChanged;
            channels.SelectionChanged += Channels_SelectionChanged;
            reset.Click += Reset_Click;
            channels.SelectedIndex = 0;
        }

        private void Reset_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
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

            range.min[3] = 0;
            range.max[3] = 1;

            multiSlider.Set(0, 0.5f, 1);
            valueRange.Set(0, 1);

            if (ctk != null)
            {
                ctk.Cancel();
            }

            ctk = new CancellationTokenSource();

            Task.Delay(250, ctk.Token).ContinueWith(t =>
            {
                if (t.IsCanceled) return;
                property?.SetValue(propertyOwner, range);
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        private void Channels_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            mode = (LevelMode)(channels.SelectedIndex - 1);
            if (mode == LevelMode.RGB)
            {
                multiSlider.Set(range.min[0], range.mid[0], range.max[0]);
            }
            else
            {
                int g = (int)mode;
                multiSlider.Set(range.min[g], range.mid[g], range.max[g]);
            }

            histogram.Mode = mode;
        }

        private void ValueRange_OnValueChanged(RangeSlider slider, float min, float max)
        {
            range.min[3] = min;
            range.max[3] = max;

            if (ctk != null)
            {
                ctk.Cancel();
            }

            ctk = new CancellationTokenSource();

            Task.Delay(250, ctk.Token).ContinueWith(t =>
            {
                if (t.IsCanceled) return;
                property?.SetValue(propertyOwner, range);
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        private void MultiSlider_OnValueChanged(MultiRangeSlider slider, float min, float mid, float max)
        {
            if (mode == LevelMode.RGB)
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

            if (ctk != null)
            {
                ctk.Cancel();
            }

            ctk = new CancellationTokenSource();

            Task.Delay(250, ctk.Token).ContinueWith(t =>
            {
                if (t.IsCanceled) return;
                property?.SetValue(propertyOwner, range);
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        public Levels(RawBitmap bitmap, object owner, PropertyInfo p) : this()
        {
            property = p;
            propertyOwner = owner;
            
            range = (MultiRange)p.GetValue(owner);
            bmp = bitmap;

            multiSlider.Set(range.min[0], range.mid[0], range.max[0]);
            valueRange.Set(range.min[3], range.max[3]);
            
            if (bmp != null)
            {
                histogram.Create(bmp);
                bmp = null;
            }

            channels.SelectedIndex = 0;
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            multiSlider = this.FindControl<MultiRangeSlider>("MultiSlider");
            valueRange = this.FindControl<RangeSlider>("ValueRange");
            histogram = this.FindControl<Histogram>("Histogram");
            channels = this.FindControl<ComboBox>("Channels");
            reset = this.FindControl<Button>("ResetButton");
        }
    }
}
