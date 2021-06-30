using Materia.Rendering.Attributes;
using Materia.Graph;

namespace Materia.Nodes.MathNodes
{
    public class GetFloat4VarNode : GetVarNode
    {
        public GetFloat4VarNode(int w, int h, GraphPixelType p = GraphPixelType.RGBA) : base(w, h, p)
        {
            defaultName = Name = "Get Float4 Var";
            output.Type = NodeType.Float4;
        }
    }
}
