using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using MateriaCore.Utils;
using System;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace MateriaCore.Components
{
    public enum NumberInputType
    {
        Int,
        Float
    }

    public class NumberInput : UserControl
    {
        private TextBox input;

        public delegate void ValueChange(NumberInput input, float value);
        public event ValueChange OnValueChanged;

        bool initing;

        public NumberInputType NumberType { get; protected set; }

        CancellationTokenSource ctk;

        static Regex isFloatNumber = new Regex("\\-?[0-9]*\\.?[0-9]?");
        static Regex isIntNumber = new Regex("\\-?[0-9]*");

        PropertyInfo property;
        object propertyOwner;

        public NumberInput()
        {
            InitializeComponent();
            input.LostFocus += Input_LostFocus;
            input.AddHandler(TextInputEvent, Input_TextInput, Avalonia.Interactivity.RoutingStrategies.Tunnel);
            input.KeyDown += Input_KeyDown;
        }

        private void Input_LostFocus(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            UpdateProperty();
        }

        private void Input_KeyDown(object sender, Avalonia.Input.KeyEventArgs e)
        {
            if (e.Key == Avalonia.Input.Key.Enter || e.Key == Avalonia.Input.Key.Escape)
            {
                App.Current.FocusManager.Focus(null);
            }
        }

        private void Input_TextInput(object sender, Avalonia.Input.TextInputEventArgs e)
        {
            switch(NumberType)
            {
                case NumberInputType.Float:
                    e.Handled = !isFloatNumber.IsMatch(e.Text);
                    break;
                case NumberInputType.Int:
                    e.Handled = !isIntNumber.IsMatch(e.Text);
                    break;
            }

            if (e.Handled)
            {
                return;
            }

            if (initing)
            {
                initing = false;
                return;
            }

            if (ctk != null)
            {
                ctk.Cancel();
            }

            ctk = new CancellationTokenSource();

            if (property != null)
            {
                Task.Delay(250, ctk.Token)
                    .ContinueWith(t =>
                    {
                        if (t.IsCanceled) return;

                        ctk = null;

                        UpdateProperty();
                    }, TaskScheduler.FromCurrentSynchronizationContext());
            }
        }

        private void UpdateProperty()
        {
            switch (NumberType)
            {
                case NumberInputType.Float:
                    float fv;
                    float.TryParse(input.Text, out fv);

                    property?.SetValue(propertyOwner, fv);
                    OnValueChanged?.Invoke(this, fv);
                    break;
                case NumberInputType.Int:
                    int iv;
                    int.TryParse(input.Text, out iv);

                    property?.SetValue(propertyOwner, iv);
                    OnValueChanged?.Invoke(this, iv);
                    break;
            }
        }

        public NumberInput(NumberInputType t, object owner, PropertyInfo p) : this()
        {
            Set(t, owner, p);
        }

        public void Set(NumberInputType t, object owner, PropertyInfo p)
        {
            NumberType = t;
            property = p;
            propertyOwner = owner;

            UpdateValuesFromProperty();
        }

        public void UpdateValue(NumberInputType t, object o)
        {
            if (t == NumberInputType.Int)
            {
                input.Text = string.Format("{0:0}", Convert.ToInt32(o));
            }
            else
            {
                input.Text = string.Format("{0:0.000}", Convert.ToSingle(o));
            }
        }

        private void UpdateValuesFromProperty()
        {
            if (property == null || propertyOwner == null) return;
            UpdateValue(NumberType, property.GetValue(propertyOwner));
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
        }

        protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnDetachedFromVisualTree(e);
            GlobalEvents.Off(GlobalEvent.UpdateParameters, OnUpdateParameter);
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            input = this.FindControl<TextBox>("Input");
        }
    }
}
