using System;
using Materia.Graph;


namespace Materia.Nodes.MathNodes
{
    public class ExecuteNode : MathNode
    {
        public ExecuteNode(int w, int h, GraphPixelType p = GraphPixelType.RGBA) : base()
        {
            //we ignore w,h,p

            shaderId = "S" + Id.Split('-')[0];

            CanPreview = false;
            Name = "Execute";

            //remove execute input nodes
            Inputs.Clear();
            ExecuteInput = null;
        }

        public override string GetDescription()
        {
            return "Execute";
        }
    }
}
