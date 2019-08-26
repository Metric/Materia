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
using System.Text.RegularExpressions;
using System.Reflection;
using System.Threading;

namespace Materia
{
    public enum NumberInputType
    {
        Int,
        Float
    }

    /// <summary>
    /// Interaction logic for NumberInput.xaml
    /// </summary>
    public partial class NumberInput : UserControl
    {
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
        }

        public NumberInput(NumberInputType t, object owner, PropertyInfo p)
        {
            InitializeComponent();
            initing = true;
            NumberType = t;
            property = p;
            propertyOwner = owner;

            if (t == NumberInputType.Int)
            {
                Input.Text = string.Format("{0:0}", Convert.ToInt32(p.GetValue(owner)));
            }
            else
            {
                Input.Text = string.Format("{0:0.000}", Convert.ToSingle(p.GetValue(owner)));
            }
        }

        public void Set(NumberInputType t, object owner, PropertyInfo p)
        {
            initing = true;
            NumberType = t;
            property = p;
            propertyOwner = owner;

            if (t == NumberInputType.Int)
            {
                Input.Text = string.Format("{0:0}", Convert.ToInt32(p.GetValue(owner)));
            }
            else
            {
                Input.Text = string.Format("{0:0.000}", Convert.ToSingle(p.GetValue(owner)));
            }
        }

        public void UpdateValue(NumberInputType t, object o)
        {
            initing = true;
            if (t == NumberInputType.Int)
            {
                Input.Text = string.Format("{0:0}", Convert.ToInt32(o));
            }
            else
            {
                Input.Text = string.Format("{0:0.000}", Convert.ToSingle(o));
            }
        }

        private void Input_PreviewTextInput(object sender, TextCompositionEventArgs e)
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
        }

        private void Input_TextChanged(object sender, TextChangedEventArgs e)
        {
            if(initing)
            {
                initing = false;
                return;
            }

            if(ctk != null)
            {
                ctk.Cancel();
            }

            ctk = new CancellationTokenSource();

            if(property != null)
            {
                switch(NumberType)
                {
                    case NumberInputType.Float:
                        Task.Delay(250, ctk.Token)
                            .ContinueWith(t =>
                            {
                                if (t.IsCanceled) return;

                                App.Current.Dispatcher.Invoke(() =>
                                {
                                    ctk = null;
                                    float fv = 0;
                                    float.TryParse(Input.Text, out fv);

                                    property.SetValue(propertyOwner, fv);
                                });
                            });
                        break;
                    case NumberInputType.Int:
                        Task.Delay(250, ctk.Token)
                            .ContinueWith(t =>
                            {
                                if (t.IsCanceled) return;

                                App.Current.Dispatcher.Invoke(() =>
                                {
                                    ctk = null;
                                    int iv = 0;
                                    int.TryParse(Input.Text, out iv);

                                    property.SetValue(propertyOwner, iv);
                                });
                            });
                        break;
                }
            }
        }

        private void Input_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter || e.Key == Key.Escape)
            {
                Keyboard.ClearFocus();
            }
        }
    }
}
