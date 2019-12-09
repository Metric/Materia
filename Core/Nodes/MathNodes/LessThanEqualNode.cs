using Materia.MathHelpers;
using Materia.Nodes.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Materia.Nodes.MathNodes
{
    public class LessThanEqualNode : MathNode
    {
        NodeInput input;
        NodeInput input2;
        NodeOutput output;

        public LessThanEqualNode(int w, int h, GraphPixelType p = GraphPixelType.RGBA) : base()
        {
            //we ignore w,h,p

            CanPreview = false;

            Name = "Less Equal";
            Id = Guid.NewGuid().ToString();
            shaderId = "S" + Id.Split('-')[0];

            input = new NodeInput(NodeType.Float, this, "A (Float)");
            input2 = new NodeInput(NodeType.Float, this, "B (Float)");

            output = new NodeOutput(NodeType.Bool, this);

            Inputs.Add(input);
            Inputs.Add(input2);

            Outputs.Add(output);
        }

        public override string GetShaderPart(string currentFrag)
        {
            if (!input.HasInput || !input2.HasInput) return "";
            var s = shaderId + "1";
            var n1id = (input.Reference.Node as MathNode).ShaderId;
            var n2id = (input2.Reference.Node as MathNode).ShaderId;

            var index = input.Reference.Node.Outputs.IndexOf(input.Reference);

            n1id += index;

            var index2 = input2.Reference.Node.Outputs.IndexOf(input2.Reference);

            n2id += index2;

            return "float " + s + " = (" + n1id + " <= " + n2id + ") ? 1 : 0;\r\n";
        }

        public override void TryAndProcess()
        {
            if (!input.IsValid || !input2.IsValid) return;

            float v1 = input.Data.ToFloat();
            float v2 = input2.Data.ToFloat();

            output.Data = v1 <= v2 ? 1 : 0;
            result = output.Data?.ToString();
        }
    }
}
