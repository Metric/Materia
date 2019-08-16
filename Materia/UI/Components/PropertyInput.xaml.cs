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

        private static SolidColorBrush GrayColor = new SolidColorBrush(Colors.Gray);
        private static SolidColorBrush LightGrayColor = new SolidColorBrush(Colors.LightGray);

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
            IField.Foreground = GrayColor;
        }

        public PropertyInput(PropertyInfo p, object owner, bool readOnly = false)
        {
            InitializeComponent();
            initing = true;

            this.readOnly = readOnly;

            IField.IsReadOnly = readOnly;

            property = p;
            propertyOwner = owner;

            var v = property.GetValue(owner);

            if (v != null)
            {
                IField.Text = property.GetValue(owner).ToString();
            }

            IField.Foreground = GrayColor;
        }

        public void Set(string text)
        {
            initing = true;
            this.readOnly = true;
            IField.IsReadOnly = true;
            property = null;
            propertyOwner = null;
            IField.Text = text;
        }

        public void Set(PropertyInfo p, object owner, bool readOnly = false)
        {
            initing = true;

            this.readOnly = readOnly;

            IField.IsReadOnly = readOnly;

            property = p;
            propertyOwner = owner;

            var v = property.GetValue(owner);

            if (v != null)
            {
                IField.Text = property.GetValue(owner).ToString();
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

            if (!readOnly && property != null && propertyOwner != null)
            {
                property.SetValue(propertyOwner, IField.Text);
            }
        }

        private void IField_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter || e.Key == Key.Escape)
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

            IField.Foreground = LightGrayColor;
        }

        private void IField_LostFocus(object sender, RoutedEventArgs e)
        {
            if(string.IsNullOrEmpty(IField.Text))
            {
                IField.Text = Placeholder;
            }

            IField.Foreground = GrayColor;
        }
    }
}
