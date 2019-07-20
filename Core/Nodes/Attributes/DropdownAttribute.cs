using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Materia.Nodes.Attributes
{
    public class DropdownAttribute : Attribute
    {
        public object[] Values
        {
            get; set;
        }
        public string OutputProperty { get; set; }
        public DropdownAttribute(string outputProperty, params object[] values)
        {
            Values = values;
            OutputProperty = outputProperty;
        }
    }
}
