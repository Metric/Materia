using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
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
            input.TextInput += Input_TextInput;
            input.KeyDown += Input_KeyDown;
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
                switch (NumberType)
                {
                    case NumberInputType.Float:
                        Task.Delay(250, ctk.Token)
                            .ContinueWith(t =>
                            {
                                if (t.IsCanceled) return;

                                ctk = null;
                                float fv = 0;
                                float.TryParse(input.Text, out fv);

                                property?.SetValue(propertyOwner, fv);
                                OnValueChanged?.Invoke(this, fv);
                            }, TaskScheduler.FromCurrentSynchronizationContext());
                        break;
                    case NumberInputType.Int:
                        Task.Delay(250, ctk.Token)
                            .ContinueWith(t =>
                            {
                                if (t.IsCanceled) return;

                                ctk = null;
                                int iv = 0;
                                int.TryParse(input.Text, out iv);

                                property?.SetValue(propertyOwner, iv);
                                OnValueChanged?.Invoke(this, iv);
                            }, TaskScheduler.FromCurrentSynchronizationContext());
                        break;
                }
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

            UpdateValue(t, p.GetValue(owner));
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

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            input = this.FindControl<TextBox>("Input");
        }
    }
}
