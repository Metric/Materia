using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using MateriaCore.Utils;
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

        private bool isMultiline;

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
            input.AddHandler(TextInputEvent, Input_TextInput, Avalonia.Interactivity.RoutingStrategies.Tunnel);
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

            isMultiline = multi;

            if (isMultiline)
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

            UpdateValuesFromProperty();
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

        private void UpdateValuesFromProperty()
        {
            if (property == null || propertyOwner == null) return;
            var v = property.GetValue(propertyOwner);

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

            if (isMultiline)
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
