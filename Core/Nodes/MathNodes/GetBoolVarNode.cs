using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Materia.Nodes.MathNodes
{
    public class GetBoolVarNode : GetVarNode
    {
        public GetBoolVarNode(int w, int h, GraphPixelType p = GraphPixelType.RGBA) : base(w, h, p)
        {
            Name = "Get Bool Var";
            output.Type = NodeType.Bool;
        }
    }
}