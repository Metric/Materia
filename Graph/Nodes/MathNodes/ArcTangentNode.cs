using Materia.Rendering.Attributes;
using Materia.Rendering.Extensions;
using Materia.Graph;
using System;


namespace Materia.Nodes.MathNodes
{
    public class ArcTangentNode : MathNode
    {
        NodeInput input;
        NodeInput input2;
        NodeOutput output;

        public ArcTangentNode(int w, int h, GraphPixelType p = GraphPixelType.RGBA) : base()
        {
            //we ignore w,h,p

            CanPreview = false;

            Name = "Arc Tangent";
 
            shaderId = "S" + Id.Split('-')[0];

            input = new NodeInput(NodeType.Float, this, "X (Float)");
            input2 = new NodeInput(NodeType.Float, this, "Y (Float)");
        
            output = new NodeOutput(NodeType.Float, this);

            Inputs.Add(input);
            Inputs.Add(input2);
            Outputs.Add(output);
        }


        public override string GetShaderPart(string currentFrag)
        {
            if (!Inputs[1].HasInput || !Inputs[2].HasInput) return "";
            var s = shaderId + "1";
            var n1id = (Inputs[1].Reference.Node as MathNode).ShaderId;
            var n2id = (Inputs[2].Reference.Node as MathNode).ShaderId;

            var index = Inputs[1].Reference.Node.Outputs.IndexOf(Inputs[1].Reference);

            n1id += index;

            var index2 = Inputs[2].Reference.Node.Outputs.IndexOf(Inputs[2].Reference);

            n2id += index2;

            return "float " + s + " = atan(" + n2id + "," + n1id + ");\r\n";
        }

        public override void TryAndProcess()
        {
            NodeInput input = Inputs[1];
            NodeInput input2 = Inputs[2];

            if (!input.IsValid || !input2.IsValid) return;
            float x = input.Data.ToFloat();
            float y = input2.Data.ToFloat();

            output.Data = MathF.Atan2(y, x);
            result = output.Data?.ToString();
        }
    }
}
