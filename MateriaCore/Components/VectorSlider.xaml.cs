using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using MateriaCore.Utils;
using System.Reflection;
using Materia.Rendering.Mathematics;
using Materia.Rendering.Attributes;

namespace MateriaCore.Components
{
    public class VectorSlider : UserControl
    {
        Grid zview;
        Grid wview;

        NumberSlider xvalue;
        NumberSlider yvalue;
        NumberSlider zvalue;
        NumberSlider wvalue;

        PropertyInfo property;
        object propertyOwner;
        VectorPropertyContainer pc;

        public VectorSlider()
        {
            this.InitializeComponent();
            pc = new VectorPropertyContainer(new MVector());
            pc.OnUpdate += Pc_OnUpdate;
        }

        private void Pc_OnUpdate()
        {
            property?.SetValue(propertyOwner, pc.Vector);
        }

        public VectorSlider(PropertyInfo p, object owner, float min = 0, float max = 1, NodeType type = NodeType.Float4, bool isInt = false) : this()
        {
            property = p;
            propertyOwner = owner;

            switch (type)
            {
                case NodeType.Float2:
                    zview.IsVisible = false;
                    wview.IsVisible = false;
                    break;
                case NodeType.Float3:
                    zview.IsVisible = true;
                    wview.IsVisible = false;
                    break;
                case NodeType.Float4:
                    zview.IsVisible = true;
                    wview.IsVisible = true;
                    break;
            }

            object b = p.GetValue(owner);

            if (b != null)
            {
                MVector m = (MVector)b;
                pc.Vector = m;
            }

            var xprop = pc.GetType().GetProperty("XProp");
            var yprop = pc.GetType().GetProperty("YProp");
            var zprop = pc.GetType().GetProperty("ZProp");
            var wprop = pc.GetType().GetProperty("WProp");

            xvalue.IsInt = isInt;
            yvalue.IsInt = isInt;
            zvalue.IsInt = isInt;
            wvalue.IsInt = isInt;

            xvalue.Set(min, max, xprop, pc);
            yvalue.Set(min, max, yprop, pc);
            zvalue.Set(min, max, zprop, pc);
            wvalue.Set(min, max, wprop, pc);
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            xvalue = this.FindControl<NumberSlider>("XValue");
            yvalue = this.FindControl<NumberSlider>("YValue");
            zvalue = this.FindControl<NumberSlider>("ZValue");
            wvalue = this.FindControl<NumberSlider>("WValue");
            zview = this.FindControl<Grid>("ZView");
            wview = this.FindControl<Grid>("WView");
        }
    }
}
