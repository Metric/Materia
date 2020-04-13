using Materia.Rendering.Attributes;
using Materia.Graph;

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