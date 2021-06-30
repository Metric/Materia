using Materia.Rendering.Attributes;
using Materia.Graph;

namespace Materia.Nodes.MathNodes
{
    public class GetFloat2VarNode : GetVarNode
    {
        public GetFloat2VarNode(int w, int h, GraphPixelType p = GraphPixelType.RGBA) : base(w, h, p)
        {
            defaultName = Name = "Get Float2 Var";
            output.Type = NodeType.Float2;
        }
    }
}
