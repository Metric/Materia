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
        bool initValue;
        public bool IsInt { get; set; }

        float[] ticks;
        public float[] Ticks
        {
            get
            {
                return ticks;
            }
            set
            {
                ticks = value;
                if(ticks != null)
                {
                    SlideInputM.Ticks = ticks;
                    SlideInputM.SnapToTicks = true;
                }
                else
                {
                    SlideInputM.Ticks = null;
                    SlideInputM.SnapToTicks = false;
                }
            }
        }

        public NumberSlider()
        {
            InitializeComponent();
        }

        public void Set(float min, float max, PropertyInfo p, object owner)
        {
            property = p;
            propertyOwner = owner;

            initValue = true;

            initValue = true;

            SlideInputM.MaxValue = max;
            SlideInputM.MinValue = min;

            initValue = true;

            float f = Convert.ToSingle(p.GetValue(owner));

            if(float.IsInfinity(f) || float.IsNaN(f))
            {
                f = 0;
            }

            SlideInputM.Value = f;

            if (IsInt)
            {
                Input.Set(NumberInputType.Int, owner, p);
            }
            else
            {
                Input.Set(NumberInputType.Float, owner, p);
            }
        }

        private void SlideInputM_OnValueChanged(UI.Components.MSlider slider)
        {
            if (!IsEnabled) return;

            if (!initValue)
            {
                if (IsInt)
                {
                    int v = (int)SlideInputM.Value;

                    Input.UpdateValue(NumberInputType.Int, v);

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
                    float v = (float)SlideInputM.Value;
                    Input.UpdateValue(NumberInputType.Float, v);

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

        private void Input_OnValueChanged(NumberInput input, float value)
        {
            if (value > SlideInputM.MaxValue)
            {
                SlideInputM.MaxValue = value * 2;
            }
            else if(value < SlideInputM.MinValue)
            {
                SlideInputM.MinValue = value * 2;
            }

            SlideInputM.Value = value;
        }
    }
}
