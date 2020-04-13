using System;
namespace Materia.Rendering.Attributes
{
    public class DropdownAttribute : Attribute
    {
        public object[] Values
        {
            get; set;
        }
        public string OutputProperty { get; set; }
        public bool IsEditable { get; set; }

        public DropdownAttribute(string outputProperty, bool isEditable = false, params object[] values)
        {
            Values = values;
            IsEditable = isEditable;
            OutputProperty = outputProperty;
        }
    }
}
