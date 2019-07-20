using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Materia.Math3D;
using Materia.Nodes.Attributes;

namespace Materia.MathHelpers
{
    public class Light : Transform
    {
        public Vector3 Color { get; set; }
        public float Power { get; set; }
    }
}
