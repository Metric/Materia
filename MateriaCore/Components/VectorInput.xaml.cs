using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using MateriaCore.Utils;
using System.Reflection;
using Materia.Rendering.Mathematics;
using Materia.Rendering.Attributes;

namespace MateriaCore.Components
{
    public class VectorInput : UserControl
    {
        NumberInput xpos;
        NumberInput ypos;
        NumberInput zpos;
        NumberInput wpos;

        PropertyInfo property;
        object propertyOwner;
        VectorPropertyContainer pc;

        NumberInputType nType;

        public VectorInput()
        {
            this.InitializeComponent();
            pc = new VectorPropertyContainer(new MVector());
            pc.OnUpdate += Pc_OnUpdate;

        }

        public VectorInput(PropertyInfo p, object owner, NodeType type = NodeType.Float4, NumberInputType ntype = NumberInputType.Float) : this()
        {
            property = p;
            propertyOwner = owner;

            nType = ntype;

            switch (type)
            {
                case NodeType.Float2:
                    zpos.IsVisible = false;
                    wpos.IsVisible = false;
                    break;
                case NodeType.Float3:
                    zpos.IsVisible = true;
                    wpos.IsVisible = false;
                    break;
                case NodeType.Float4:
                    zpos.IsVisible = true;
                    wpos.IsVisible = true;
                    break;
            }

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

            xpos.Set(nType, pc, xprop);
            ypos.Set(nType, pc, yprop);
            zpos.Set(nType, pc, zprop);
            wpos.Set(nType, pc, wprop);
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

        private void Pc_OnUpdate()
        {
            property.SetValue(propertyOwner, pc.Vector);
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            xpos = this.FindControl<NumberInput>("XPos");
            ypos = this.FindControl<NumberInput>("YPos");
            zpos = this.FindControl<NumberInput>("ZPos");
            wpos = this.FindControl<NumberInput>("WPos");
        }
    }
}
