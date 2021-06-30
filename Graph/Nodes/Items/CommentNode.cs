using System;
using System.Collections.Generic;

namespace Materia.Nodes.Items
{
    public class CommentNode : ItemNode
    {
        public CommentNode()
        {
            defaultName = name = "Comment";
            content = "Comment...";
        }
    }
}
