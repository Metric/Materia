using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace MateriaCore.Components
{
    public class TextInput : UserControl
    {
        PropertyInfo property;
        object propertyOwner;
        bool readOnly;

        CancellationTokenSource ctk;

        private TextBox input;

        string placeholder;
        public string Placeholder
        {
            get
            {
                return placeholder;
            }
            set
            {
                placeholder = value;
            }
        }

        public TextInput()
        {
            this.InitializeComponent();
            input.TextInput += Input_TextInput;
            input.KeyDown += Input_KeyDown;
            input.GotFocus += Input_GotFocus;
            input.LostFocus += Input_LostFocus;
        }

        public TextInput(PropertyInfo p, object owner, bool readOnly = false, bool multi = false) : this()
        {
            Set(p, owner, readOnly, multi);
        }

        public void Set(string text, bool multi = false)
        {
            this.readOnly = true;
            input.IsReadOnly = true;
            property = null;
            propertyOwner = null;
            input.Text = text;

            if (multi)
            {
                input.TextWrapping = Avalonia.Media.TextWrapping.Wrap;
                input.AcceptsReturn = true;
                Height = 128;
            }
            else
            {
                input.TextWrapping = Avalonia.Media.TextWrapping.NoWrap;
                input.AcceptsReturn = false;
                Height = 32;
            }
        }

        public void Set(PropertyInfo p, object owner, bool readOnly = false, bool multi = false)
        {
            this.readOnly = readOnly;
            input.IsReadOnly = readOnly;

            property = p;
            propertyOwner = owner;

            var v = property.GetValue(owner);

            if (v != null)
            {
                try
                {
                    input.Text = v.ToString();
                }
                catch (Exception e) 
                { 
                }
            }

            if (multi)
            {
                input.TextWrapping = Avalonia.Media.TextWrapping.Wrap;
                input.AcceptsReturn = true;
                Height = 128;
            }
            else
            {
                input.TextWrapping = Avalonia.Media.TextWrapping.NoWrap;
                input.AcceptsReturn = false;
                Height = 32;
            }
        }

        private void Input_LostFocus(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(input.Text))
            {
                input.Text = placeholder;
            }
        }

        private void Input_GotFocus(object sender, Avalonia.Input.GotFocusEventArgs e)
        {
            if (!string.IsNullOrEmpty(input.Text) && !string.IsNullOrEmpty(placeholder)
               && input.Text.Equals(placeholder))
            {
                input.Text = "";
            }
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
            if (!string.IsNullOrEmpty(placeholder) && input.Text.Equals(placeholder))
            {
                return;
            }

            if (ctk != null)
            {
                ctk.Cancel();
            }

            ctk = new CancellationTokenSource();

            Task.Delay(250, ctk.Token).ContinueWith(t =>
            {
                if (t.IsCanceled)
                {
                    return;
                }

                if(!readOnly && property != null && propertyOwner != null)
                {
                    property.SetValue(propertyOwner, input.Text);
                }
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            input = this.FindControl<TextBox>("Input");
        }
    }
}
