using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Materia.MathHelpers;

namespace Materia.Nodes.MathNodes
{
    public class PowNode : MathNode
    {
        NodeInput input;
        NodeInput input2;
        NodeOutput output;

        public PowNode(int w, int h, GraphPixelType p = GraphPixelType.RGBA) : base()
        {
            //we ignore w,h,p

            CanPreview = false;

            Name = "Pow";
            Id = Guid.NewGuid().ToString();
            shaderId = "S" + Id.Split('-')[0];

            input = new NodeInput(NodeType.Float, this, "Value");
            input2 = new NodeInput(NodeType.Float, this, "Raise");
            output = new NodeOutput(NodeType.Float, this);

            Inputs.Add(input);
            Inputs.Add(input2);

            Outputs.Add(output);
        }

        public override string GetShaderPart(string currentFrag)
        {
            if (!input.HasInput) return "";
            var s = shaderId + "1";
            var n1id = (input.Reference.Node as MathNode).ShaderId;
            var n2id = (input2.Reference.Node as MathNode).ShaderId;

            var index = input.Reference.Node.Outputs.IndexOf(input.Reference);
            var index2 = input2.Reference.Node.Outputs.IndexOf(input2.Reference);

            n1id += index;
            n2id += index2;

            return "float " + s + " = pow(" + n1id + ", " + n2id + ");\r\n";
        }

        public override void TryAndProcess()
        {
            if (!input.IsValid || !input2.IsValid) return;
            float v = input.Data.ToFloat();
            float r = input2.Data.ToFloat();

            output.Data = (float)Math.Pow(v, r);
            result = output.Data?.ToString();
        }
    }
}
