using System;
using Materia.Rendering.Attributes;
using Materia.Rendering.Extensions;
using Materia.Graph;

namespace Materia.Nodes.MathNodes
{
    public class PolarNode : MathNode
    {
        NodeInput input;
        NodeInput input2;
        NodeOutput output;
        NodeOutput output2;

        public PolarNode(int w, int h, GraphPixelType p = GraphPixelType.RGBA) : base()
        {
            //we ignore w,h,p

            CanPreview = false;

            Name = "Polar";
            Id = Guid.NewGuid().ToString();
            shaderId = "S" + Id.Split('-')[0];

            input = new NodeInput(NodeType.Float, this, "X Float Input");
            input2 = new NodeInput(NodeType.Float, this, "Y Float Input");
            output = new NodeOutput(NodeType.Float, this, "Radius Output");
            output2 = new NodeOutput(NodeType.Float, this, "Angle Output");

            Inputs.Add(input);
            Inputs.Add(input2);

            Outputs.Add(output);
            Outputs.Add(output2);
        }

        public override string GetShaderPart(string currentFrag)
        {
            if (!Inputs[0].HasInput || !Inputs[1].HasInput) return "";
            var s1 = shaderId + "1";
            var s2 = shaderId + "2";

            var n1id = (Inputs[0].Reference.Node as MathNode).ShaderId;
            var n2id = (Inputs[1].Reference.Node as MathNode).ShaderId;

            var index = Inputs[0].Reference.Node.Outputs.IndexOf(Inputs[0].Reference);

            n1id += index;

            var index2 = Inputs[1].Reference.Node.Outputs.IndexOf(Inputs[1].Reference);

            n2id += index2;

            string compute = "";
            compute += "float " + s1 + " = sqrt(" + n1id + " * " + n1id + " + " + n2id + " * " + n2id + ");\r\n";
            compute += "float " + s2 + " = tan(" + n2id + " / " + n1id + ") + PI;\r\n";

            return compute;
        }

        public override void TryAndProcess()
        {
            if (!input.IsValid || !input2.IsValid) return;
            
            float x = input.Data.ToFloat();
            float y = input2.Data.ToFloat();

            float radius = MathF.Sqrt(x * x + y * y);
            float angle = MathF.Tan(y / x);

            output.Data = radius;
            output2.Data = angle;

            result = output.Data?.ToString() + "," + output2.Data?.ToString();
        }
    }
}
