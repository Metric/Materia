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
using Materia.MathHelpers;
using Materia.Nodes;

namespace Materia.UI.Components
{
    /// <summary>
    /// Interaction logic for VectorInput.xaml
    /// </summary>
    public partial class VectorInput : UserControl
    {
        object propertyOwner;
        PropertyInfo property;
        VectorPropertyContainer pc;

        public VectorInput()
        {
            InitializeComponent();
            pc = new VectorPropertyContainer(new MVector());
        }

        public VectorInput(PropertyInfo prop, object owner, NodeType type = NodeType.Float4, NumberInputType ntype = NumberInputType.Float)
        {
            InitializeComponent();
            property = prop;
            propertyOwner = owner;

            switch (type)
            {
                case NodeType.Float2:
                    ZPos.Visibility = Visibility.Collapsed;
                    WPos.Visibility = Visibility.Collapsed;
                    break;
                case NodeType.Float3:
                    ZPos.Visibility = Visibility.Visible;
                    WPos.Visibility = Visibility.Collapsed;
                    break;
                case NodeType.Float4:
                    ZPos.Visibility = Visibility.Visible;
                    WPos.Visibility = Visibility.Visible;
                    break;
            }

            object b = prop.GetValue(owner);

            if (b == null)
            {
                pc = new VectorPropertyContainer(new MVector());
            }
            else
            {
                MVector vec = (MVector)b;
                pc = new VectorPropertyContainer(vec);
            }

            pc.OnUpdate += Pc_OnUpdate;

            var xprop = pc.GetType().GetProperty("XProp");
            var yprop = pc.GetType().GetProperty("YProp");
            var zprop = pc.GetType().GetProperty("ZProp");
            var wprop = pc.GetType().GetProperty("WProp");

            XPos.Set(ntype, pc, xprop);
            YPos.Set(ntype, pc, yprop);
            ZPos.Set(ntype, pc, zprop);
            WPos.Set(ntype, pc, wprop);
        }

        private void Pc_OnUpdate()
        {
            property.SetValue(propertyOwner, pc.Vector);
        }
    }
}
