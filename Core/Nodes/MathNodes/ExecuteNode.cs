using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Materia.Nodes.MathNodes
{
    public class ExecuteNode : MathNode
    {
        public ExecuteNode(int w, int h, GraphPixelType p = GraphPixelType.RGBA) : base()
        {
            //we ignore w,h,p

            Id = Guid.NewGuid().ToString();
            shaderId = "S" + Id.Split('-')[0];

            CanPreview = false;
            Name = "Execute";

            //remove execute input nodes
            Inputs.Clear();
            executeInput = null;
        }

        public override string GetDescription()
        {
            return "Execute";
        }
    }
}
