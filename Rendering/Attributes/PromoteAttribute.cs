using System;

namespace Materia.Rendering.Attributes
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
