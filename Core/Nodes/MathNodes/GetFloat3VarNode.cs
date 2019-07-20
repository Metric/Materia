using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Materia.Nodes.MathNodes
{
    public class GetFloat3VarNode : GetVarNode
    {
        public GetFloat3VarNode(int w, int h, GraphPixelType p = GraphPixelType.RGBA) : base(w, h, p)
        {
            Name = "Get Float3 Var";
            output.Type = NodeType.Float3;
        }
    }
}
