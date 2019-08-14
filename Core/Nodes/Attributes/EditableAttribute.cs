using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Materia.Nodes;

namespace Materia.Nodes.Attributes
{
    public class EditableAttribute : Attribute
    {
        public ParameterInputType Type { get; set; }
        public float Min { get; set; }
        public float Max { get; set; }
        public string Name { get; set; }
        public string Section { get; set; }
        public float[] Ticks { get; set; }

        public EditableAttribute(ParameterInputType type, string name, string section = "", float min = 0, float max = 1, float[] ticks = null)
        {
            Type = type;
            Name = name;
            Section = section;
            Min = min;
            Max = max;
            Ticks = ticks;
        }
    }
}
