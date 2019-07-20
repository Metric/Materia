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
using System.Reflection;
using Materia.Nodes.Attributes;
using System.Threading;

namespace Materia
{
    /// <summary>
    /// Interaction logic for NumberSlider.xaml
    /// </summary>
    public partial class NumberSlider : UserControl
    {
        CancellationTokenSource ctk;

        PropertyInfo property;
        object propertyOwner;
        SliderAttribute attributes;
        bool initValue;
        bool isInt;

        public NumberSlider()
        {
            InitializeComponent();
        }

        public NumberSlider(SliderAttribute attributes, PropertyInfo p, object owner)
        {
            InitializeComponent();
            this.attributes = attributes;
            property = p;
            propertyOwner = owner;

            initValue = true;

            SlideInput.Minimum = attributes.Min;

            initValue = true;

            SlideInput.Maximum = attributes.Max;

            initValue = true;

            if (attributes.Snap && attributes.Ticks != null && attributes.Ticks.Length > 0)
            {
                SlideInput.IsSnapToTickEnabled = true;
                foreach (float t in attributes.Ticks)
                {
                    SlideInput.Ticks.Add(t);
                }
            }

            isInt = attributes.IsInt;

            if (isInt)
            {
                SlideInput.Value = Convert.ToInt32(p.GetValue(owner));
                TextInput.Set(NumberInputType.Int, owner, p);
            }
            else
            {
                SlideInput.Value = Convert.ToSingle(p.GetValue(owner));
                TextInput.Set(NumberInputType.Float, owner, p);
            }
        }

        public void Set(float min, float max, PropertyInfo p, object owner)
        {
            property = p;
            propertyOwner = owner;

            initValue = true;

            SlideInput.Minimum = min;

            initValue = true;

            SlideInput.Maximum = max;

            initValue = true;

            SlideInput.Value = Convert.ToSingle(p.GetValue(owner));

            if (isInt)
            {
                TextInput.Set(NumberInputType.Int, owner, p);
            }
            else
            {
                TextInput.Set(NumberInputType.Float, owner, p);
            }
        }

        private void SlideInput_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {

            if (!initValue)
            {
                if (isInt)
                {
                    int v = (int)SlideInput.Value;

                    TextInput.UpdateValue(NumberInputType.Int, v);

                    if (ctk != null)
                    {
                        ctk.Cancel();
                    }

                    ctk = new CancellationTokenSource();

                    Task.Delay(250, ctk.Token).ContinueWith(t =>
                    {
                        if (t.IsCanceled) return;

                        App.Current.Dispatcher.Invoke(() =>
                        {
                            property.SetValue(propertyOwner, v);
                        });
                    });
                    
                }
                else
                {
                    float v = (float)SlideInput.Value;
                    TextInput.UpdateValue(NumberInputType.Float, v);

                    if (ctk != null)
                    {
                        ctk.Cancel();
                    }

                    ctk = new CancellationTokenSource();

                    Task.Delay(250, ctk.Token).ContinueWith(t =>
                    {
                        if (t.IsCanceled) return;

                        App.Current.Dispatcher.Invoke(() =>
                        {
                            property.SetValue(propertyOwner, v);
                        });
                    });
                }
            }
            initValue = false;
        }
    }
}
