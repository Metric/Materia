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

        public PropertyInput()
        {
            InitializeComponent();
        }

        public PropertyInput(PropertyInfo p, object owner)
        {
            InitializeComponent();
            initing = true;

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

            property.SetValue(propertyOwner, IField.Text);
        }

        private void IField_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter)
            {
                Keyboard.ClearFocus();
            }
        }
    }
}
