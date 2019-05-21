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
                InputValue.Text = SlideInput.Value > 0 ? String.Format("{0:0}", SlideInput.Value) : "0";
            }
            else
            {
                SlideInput.Value = Convert.ToSingle(p.GetValue(owner));
                InputValue.Text = SlideInput.Value >= 0.01 ? String.Format("{0:0.000}", SlideInput.Value) : "0";
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
            InputValue.Text = SlideInput.Value >= 0.01 ? String.Format("{0:0.000}", SlideInput.Value) : "0";
        }

        private void SlideInput_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {

            if (!initValue)
            {
                if (isInt)
                {
                    InputValue.Text = SlideInput.Value > 0 ? String.Format("{0:0}", SlideInput.Value) : "0";
                    int v = (int)SlideInput.Value;

                    if(ctk != null)
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
                    InputValue.Text = SlideInput.Value >= 0.01 ? String.Format("{0:0.000}", SlideInput.Value) : "0";
                    float v = (float)SlideInput.Value;

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
