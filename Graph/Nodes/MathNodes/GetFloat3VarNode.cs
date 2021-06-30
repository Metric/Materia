using Materia.Rendering.Attributes;
using Materia.Graph;

namespace Materia.Nodes.MathNodes
{
    public class GetFloat3VarNode : GetVarNode
    {
        public GetFloat3VarNode(int w, int h, GraphPixelType p = GraphPixelType.RGBA) : base(w, h, p)
        {
            defaultName = Name = "Get Float3 Var";
            output.Type = NodeType.Float3;
        }
    }
}
