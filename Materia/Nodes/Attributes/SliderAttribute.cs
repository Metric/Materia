using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Materia.Nodes.Attributes
{
    public class SliderAttribute : Attribute
    {
        public bool IsInt { get; set; }
        public bool Snap { get; set; }
        public float Min { get; set; }
        public float Max { get; set; }
        public float[] Ticks { get; set; }
    }
}
