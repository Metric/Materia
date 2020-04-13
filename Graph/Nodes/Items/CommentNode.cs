using System;
using System.Collections.Generic;

namespace Materia.Nodes.Items
{
    public class CommentNode : ItemNode
    {
        public CommentNode()
        {
            Id = Guid.NewGuid().ToString();
            Outputs = new List<NodeOutput>();
            Inputs = new List<NodeInput>();
            name = "Comment";
            content = "Comment...";
        }
    }
}
