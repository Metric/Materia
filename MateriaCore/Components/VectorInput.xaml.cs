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

            xpos.Set(ntype, pc, xprop);
            ypos.Set(ntype, pc, yprop);
            zpos.Set(ntype, pc, zprop);
            wpos.Set(ntype, pc, wprop);
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
