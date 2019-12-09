using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Materia.MathHelpers;
using Materia.Nodes.Helpers;

namespace Materia.Nodes.MathNodes
{
    public class Pow2Node : MathNode
    {
        NodeInput input;
        NodeOutput output;

        public Pow2Node(int w, int h, GraphPixelType p = GraphPixelType.RGBA) : base()
        {
            //we ignore w,h,p

            CanPreview = false;

            Name = "Pow2";
            Id = Guid.NewGuid().ToString();
            shaderId = "S" + Id.Split('-')[0];

            input = new NodeInput(NodeType.Float, this, "Value");
            output = new NodeOutput(NodeType.Float, this);

            Inputs.Add(input);

            Outputs.Add(output);
        }

        public override string GetShaderPart(string currentFrag)
        {
            if (!input.HasInput) return "";
            var s = shaderId + "1";
            var n1id = (input.Reference.Node as MathNode).ShaderId;

            var index = input.Reference.Node.Outputs.IndexOf(input.Reference);

            n1id += index;

            return "float " + s + " = pow(" + n1id + ", 2);\r\n";
        }

        public override void TryAndProcess()
        {
            if (!input.IsValid) return;
            float f = input.Data.ToFloat();
            output.Data = (float)Math.Pow(f, 2);
            result = output.Data?.ToString();
        }
    }
}
