﻿using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using MateriaCore.Utils;
using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace MateriaCore.Components
{
    public class NumberSlider : UserControl
    {
        CancellationTokenSource ctk;

        private Slider sliderInput;
        private NumberInput input;

        PropertyInfo property;
        object propertyOwner;
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
                if (ticks != null)
                {
                    sliderInput.Ticks = ticks;
                    sliderInput.SnapToTicks = true;
                }
                else
                {
                    sliderInput.Ticks = null;
                    sliderInput.SnapToTicks = false;
                }
            }
        }

        public bool ClampToMinMax
        {
            get
            {
                if (sliderInput == null) return false;
                return sliderInput.ClampToMinMax;
            }
            set
            {
                if (sliderInput == null) return;
                sliderInput.ClampToMinMax = value;
            }
        }

        public NumberSlider()
        {
            this.InitializeComponent();
            sliderInput.OnValueChanged += SliderInput_OnValueChanged;
            input.OnValueChanged += Input_OnValueChanged;
        }

        public NumberSlider(float min, float max, PropertyInfo p, object owner) : this()
        {
            Set(min, max, p, owner);
        }

        private void Input_OnValueChanged(NumberInput input, float value)
        {
            if (!IsEnabled)
            {
                return;
            }

            if (value > sliderInput.MaxValue)
            {
                sliderInput.MaxValue = value * 2;
            }
            else if (value < sliderInput.MinValue)
            {
                sliderInput.MinValue = value * 2;
            }

            sliderInput.Value = value;
        }

        private void SliderInput_OnValueChanged(Slider slider)
        {
            if (!IsEnabled)
            {
                return;
            }

            if (IsInt)
            {
                int v = (int)sliderInput.Value;
                input.UpdateValue(NumberInputType.Int, v);

                if (ctk != null)
                {
                    ctk.Cancel();
                }

                ctk = new CancellationTokenSource();

                Task.Delay(25, ctk.Token).ContinueWith(t =>
                {
                    if (t.IsCanceled) return;
                    property?.SetValue(propertyOwner, v);
                }, TaskScheduler.FromCurrentSynchronizationContext());
            }
            else
            {
                float v = sliderInput.Value;
                input.UpdateValue(NumberInputType.Float, v);

                if (ctk != null)
                {
                    ctk.Cancel();
                }

                ctk = new CancellationTokenSource();

                Task.Delay(25, ctk.Token).ContinueWith(t =>
                {
                    if (t.IsCanceled) return;
                    property?.SetValue(propertyOwner, v);
                }, TaskScheduler.FromCurrentSynchronizationContext());
            }
        }

        public void UpdateValue(float v)
        {
            sliderInput.Value = v;
            input.UpdateValue(IsInt ? NumberInputType.Int : NumberInputType.Float, v);
        }

        public void Set(float min, float max, PropertyInfo p, object owner)
        {
            property = p;
            propertyOwner = owner;

            sliderInput.MaxValue = max;
            sliderInput.MinValue = min;

            UpdateValuesFromProperty();
            input.Set(IsInt ? NumberInputType.Int : NumberInputType.Float, propertyOwner, property);
        }

        private void UpdateValuesFromProperty()
        {
            float f = Convert.ToSingle(property?.GetValue(propertyOwner));

            if (float.IsInfinity(f) || float.IsNaN(f))
            {
                f = 0;
            }

            sliderInput.Value = f;
        }

        private void OnUpdateParameter(object sender, object v)
        {
            if (v == propertyOwner)
            {
                UpdateValuesFromProperty();
            }
        }

        protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnAttachedToVisualTree(e);
            GlobalEvents.On(GlobalEvent.UpdateParameters, OnUpdateParameter);
            UpdateValuesFromProperty();
        }

        protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnDetachedFromVisualTree(e);
            GlobalEvents.Off(GlobalEvent.UpdateParameters, OnUpdateParameter);
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            sliderInput = this.FindControl<Slider>("SlideInput");
            input = this.FindControl<NumberInput>("Input");
        }
    }
}
