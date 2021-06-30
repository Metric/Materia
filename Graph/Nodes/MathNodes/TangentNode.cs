using System;
using Materia.Rendering.Attributes;
using Materia.Rendering.Extensions;
using Materia.Graph;

namespace Materia.Nodes.MathNodes
{
    public class TangentNode : MathNode
    {
        NodeInput input;
        NodeOutput output;

        public TangentNode(int w, int h, GraphPixelType p = GraphPixelType.RGBA) : base()
        {
            //we ignore w,h,p

            CanPreview = false;

            defaultName = Name = "Tangent";
 
            shaderId = "S" + Id.Split('-')[0];

            input = new NodeInput(NodeType.Float, this, "Float Input");
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

            return "float " + s + " = tan(" + n1id + ");\r\n"; 
        }

        public override void TryAndProcess()
        {
            if (!input.IsValid) return;
            float f = input.Data.ToFloat();
            output.Data = MathF.Tan(f);
            result = output.Data?.ToString();
        }
    }
}
