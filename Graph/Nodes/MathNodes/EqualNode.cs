using System;
using Materia.Rendering.Attributes;
using Materia.Graph;

namespace Materia.Nodes.MathNodes
{
    public class EqualNode : MathNode
    {
        NodeInput input;
        NodeInput input2;
        NodeOutput output;

        public EqualNode(int w, int h, GraphPixelType p = GraphPixelType.RGBA) : base()
        {
            //we ignore w,h,p

            CanPreview = false;

            Name = "Equal";
            Id = Guid.NewGuid().ToString();
            shaderId = "S" + Id.Split('-')[0];

            input = new NodeInput(NodeType.Bool | NodeType.Float | NodeType.Float2 | NodeType.Float3 | NodeType.Float4, this, "A");
            input2 = new NodeInput(NodeType.Bool |  NodeType.Float | NodeType.Float2 | NodeType.Float3 | NodeType.Float4, this, "B");

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

            return "float " + s + " = (" + n1id + " == " + n2id + ") ? 1 : 0;\r\n";
        }

        public override void TryAndProcess()
        {
            if (!input.IsValid || !input2.IsValid) return;

            output.Data = input.Data.Equals(input2.Data) ? 1 : 0;
        }
    }
}
