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

        float Min = 0;
        float Max = 1;

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

            Min = min;
            Max = max;

            xvalue.IsInt = isInt;
            yvalue.IsInt = isInt;
            zvalue.IsInt = isInt;
            wvalue.IsInt = isInt;

            UpdateValuesFromProperty();
        }

        private void UpdateValuesFromProperty()
        {
            if (property == null || propertyOwner == null) return;

            object b = property.GetValue(propertyOwner);

            if (b != null)
            {
                MVector m = (MVector)b;
                pc.Vector = m;
            }

            var xprop = pc.GetType().GetProperty("XProp");
            var yprop = pc.GetType().GetProperty("YProp");
            var zprop = pc.GetType().GetProperty("ZProp");
            var wprop = pc.GetType().GetProperty("WProp");


            xvalue.Set(Min, Max, xprop, pc);
            yvalue.Set(Min, Max, yprop, pc);
            zvalue.Set(Min, Max, zprop, pc);
            wvalue.Set(Min, Max, wprop, pc);
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
            UpdateValuesFromProperty();
        }

        protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnDetachedFromVisualTree(e);
            GlobalEvents.Off(GlobalEvent.UpdateParameters, OnUpdateParameter);
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
