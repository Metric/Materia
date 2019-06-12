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
using Materia.Nodes;
using Materia.MathHelpers;

namespace Materia.UI.Components
{
    public class VectorPropertyContainer
    {
        public delegate void Updated();
        public event Updated OnUpdate;

        protected float xprop;
        public float XProp
        {
            get
            {
                return xprop;
            }
            set
            {
                xprop = value;
                if (OnUpdate != null)
                {
                    OnUpdate.Invoke();
                }
            }
        }

        protected float yprop;
        public float YProp
        {
            get
            {
                return yprop;
            }
            set
            {
                yprop = value;
                if (OnUpdate != null)
                {
                    OnUpdate.Invoke();
                }
            }
        }
        protected float zprop;
        public float ZProp
        {
            get
            {
                return zprop;
            }
            set
            {
                zprop = value;
                if (OnUpdate != null)
                {
                    OnUpdate.Invoke();
                }
            }
        }
        protected float wprop;
        public float WProp
        {
            get
            {
                return xprop;
            }
            set
            {
                xprop = value;
                if (OnUpdate != null)
                {
                    OnUpdate.Invoke();
                }
            }
        }

        public VectorPropertyContainer(MVector v)
        {
            xprop = v.X;
            yprop = v.Y;
            zprop = v.Z;
            wprop = v.W;
        }

        public MVector Vector
        {
            get
            {
                return new MVector(xprop, yprop, zprop, wprop);
            }
        }
    }

    /// <summary>
    /// Interaction logic for VectorSlider.xaml
    /// </summary>
    public partial class VectorSlider : UserControl
    { 

        object propertyOwner;
        float min;
        float max;
        PropertyInfo property;
        VectorPropertyContainer pc;

        public VectorSlider()
        {
            InitializeComponent();
            pc = new VectorPropertyContainer(new MVector());
        }

        public VectorSlider(PropertyInfo prop, object owner, float min = 0, float max = 1, NodeType type = NodeType.Float4)
        {
            InitializeComponent();
            property = prop;
            propertyOwner = owner;
            this.min = min;
            this.max = max;

            switch(type)
            {
                case NodeType.Float2:
                    ZView.Visibility = Visibility.Collapsed;
                    WView.Visibility = Visibility.Collapsed;
                    break;
                case NodeType.Float3:
                    ZView.Visibility = Visibility.Visible;
                    WView.Visibility = Visibility.Collapsed;
                    break;
                case NodeType.Float4:
                    ZView.Visibility = Visibility.Visible;
                    WView.Visibility = Visibility.Visible;
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

            XValue.Set(min, max, xprop, pc);
            YValue.Set(min, max, yprop, pc);
            ZValue.Set(min, max, zprop, pc);
            WValue.Set(min, max, wprop, pc);
        }

        private void Pc_OnUpdate()
        {
            property.SetValue(propertyOwner, pc.Vector);
        }
    }
}
