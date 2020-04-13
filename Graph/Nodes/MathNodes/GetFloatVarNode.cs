using Materia.Rendering.Attributes;

using Materia.Graph;

namespace Materia.Nodes.MathNodes
{
   public class GetFloatVarNode : GetVarNode
    {
        public GetFloatVarNode(int w, int h, GraphPixelType p = GraphPixelType.RGBA) : base(w,h,p)
        {
            Name = "Get Float Var";
            output.Type = NodeType.Float;
        }
    }
}
