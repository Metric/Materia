using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Materia.Nodes.Attributes
{
    public class PromoteAttribute : Attribute
    {
        public NodeType ExpectedType { get; set; }
        public PromoteAttribute(NodeType expected)
        {
            ExpectedType = expected;
        }
    }
}
