using System;
using Materia.Rendering.Attributes;
using Materia.Rendering.Extensions;
using Materia.Graph;

namespace Materia.Nodes.MathNodes
{
    public class NotNode : MathNode
    {
        NodeInput input;
        NodeOutput output;

        public NotNode(int w, int h, GraphPixelType p = GraphPixelType.RGBA) : base()
        {
            //we ignore w,h,p

            CanPreview = false;

            Name = "Not";
            Id = Guid.NewGuid().ToString();
            shaderId = "S" + Id.Split('-')[0];

            input = new NodeInput(NodeType.Bool, this, "Bool Input");
            output = new NodeOutput(NodeType.Bool, this);

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

            return "float " + s + " = " + n1id + " != 0 ? 0 : 1;\r\n";
        }

        public override void TryAndProcess()
        {
            if (!input.IsValid) return;
            float f = input.Data.ToFloat();
            if (f <= 0) f = 1;
            else if (f > 0) f = 0;
            output.Data = f;
            result = output.Data?.ToString();
        }
    }
}
