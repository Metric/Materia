using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Materia.Nodes.MathNodes
{
    public class GetFloat4VarNode : GetVarNode
    {
        public GetFloat4VarNode(int w, int h, GraphPixelType p = GraphPixelType.RGBA) : base(w, h, p)
        {
            Name = "Get Float4 Var";
            output.Type = NodeType.Float4;
        }
    }
}
