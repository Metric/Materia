using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Materia.Nodes.Attributes
{
    public class VectorAttribute : Attribute
    {
        public NodeType Type { get; set; }

        public VectorAttribute(NodeType t)
        {
            Type = t;
        }
    }
}
