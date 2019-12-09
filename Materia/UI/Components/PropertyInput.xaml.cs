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
using System.Threading;

namespace Materia.UI.Components
{
    /// <summary>
    /// Interaction logic for PropertyInput.xaml
    /// </summary>
    public partial class PropertyInput : UserControl
    {
        PropertyInfo property;
        object propertyOwner;
        bool initing;
        bool readOnly;

        CancellationTokenSource ctk;

        public static readonly DependencyProperty PlaceholderProperty = DependencyProperty.Register("Placeholder", typeof(string), typeof(PropertyInput));

        public string Placeholder
        {
            get
            {
                return (string)GetValue(PlaceholderProperty);
            }
            set
            {
                SetValue(PlaceholderProperty, value);
            }
        }

        public PropertyInput()
        {
            InitializeComponent();
        }

        public PropertyInput(PropertyInfo p, object owner, bool readOnly = false, bool multi = false)
        {
            InitializeComponent();
            Set(p, owner, readOnly, multi);
        }

        public void Set(string text, bool multi = false)
        {
            initing = true;
            this.readOnly = true;
            IField.IsReadOnly = true;
            property = null;
            propertyOwner = null;
            IField.Text = text;

            if (multi)
            {
                IField.TextWrapping = TextWrapping.Wrap;
                IField.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
                Height = 128;
            }
            else
            {
                IField.TextWrapping = TextWrapping.NoWrap;
                IField.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;
                Height = 32;
            }
        }

        public void Set(PropertyInfo p, object owner, bool readOnly = false, bool multi = false)
        {
            initing = true;

            this.readOnly = readOnly;

            IField.IsReadOnly = readOnly;

            property = p;
            propertyOwner = owner;

            var v = property.GetValue(owner);

            if (v != null)
            {
                try
                {
                    IField.Text = property.GetValue(owner).ToString();
                }
                catch { }
            }

            if (multi)
            {
                IField.TextWrapping = TextWrapping.Wrap;
                IField.AcceptsReturn = true;
                IField.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
                Height = 128;
            }
            else
            {
                IField.TextWrapping = TextWrapping.NoWrap;
                IField.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;
                IField.AcceptsReturn = false;
                Height = 32;
            }
        }

        private void TextInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            if(initing)
            {
                initing = false;
                return;
            }

            if(!string.IsNullOrEmpty(Placeholder) && IField.Text.Equals(Placeholder))
            {
                return;
            }

            if(ctk != null)
            {
                ctk.Cancel();
            }

            ctk = new CancellationTokenSource();

            Task.Delay(250, ctk.Token).ContinueWith(t =>
            {
                if (t.IsCanceled) return;

                Application.Current.Dispatcher.Invoke(() =>
                {
                    if (!readOnly && property != null && propertyOwner != null)
                    {
                        property.SetValue(propertyOwner, IField.Text);
                    }
                });
            });
        }

        private void IField_KeyDown(object sender, KeyEventArgs e)
        {
            if((e.Key == Key.Enter || e.Key == Key.Escape) && !IField.AcceptsReturn)
            {
                Keyboard.ClearFocus();
            }
        }

        private void IField_GotFocus(object sender, RoutedEventArgs e)
        {
            if(!string.IsNullOrEmpty(IField.Text) && !string.IsNullOrEmpty(Placeholder) 
                && IField.Text.Equals(Placeholder))
            {
                IField.Text = "";
            }
        }

        private void IField_LostFocus(object sender, RoutedEventArgs e)
        {
            if(string.IsNullOrEmpty(IField.Text))
            {
                IField.Text = Placeholder;
            }
        }
    }
}
