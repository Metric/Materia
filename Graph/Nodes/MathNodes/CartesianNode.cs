using System;
using Materia.Rendering.Attributes;
using Materia.Rendering.Extensions;
using Materia.Graph;

namespace Materia.Nodes.MathNodes
{
    public class CartesianNode : MathNode
    {
        NodeInput input;
        NodeInput input2;
        NodeOutput output;
        NodeOutput output2;

        public CartesianNode(int w, int h, GraphPixelType p = GraphPixelType.RGBA) : base()
        {
            //we ignore w,h,p

            CanPreview = false;

            defaultName = Name = "Cartesian";

            shaderId = "S" + Id.Split('-')[0];

            input = new NodeInput(NodeType.Float, this, "Angle (Deg) Float Input");
            input2 = new NodeInput(NodeType.Float, this, "Radius Float Input");
            output = new NodeOutput(NodeType.Float, this, "X Output");
            output2 = new NodeOutput(NodeType.Float, this, "Y Output");

            Inputs.Add(input);
            Inputs.Add(input2);

            Outputs.Add(output);
            Outputs.Add(output2);
        }

        public override string GetShaderPart(string currentFrag)
        {
            if (!Inputs[1].HasInput || !Inputs[2].HasInput) return "";
            var s1 = shaderId + "1";
            var s2 = shaderId + "2";

            var n1id = (Inputs[1].Reference.Node as MathNode).ShaderId;
            var n2id = (Inputs[2].Reference.Node as MathNode).ShaderId;

            var index = Inputs[1].Reference.Node.Outputs.IndexOf(Inputs[1].Reference);

            n1id += index;

            var index2 = Inputs[2].Reference.Node.Outputs.IndexOf(Inputs[2].Reference);

            n2id += index2;

            string compute = "";
            compute += "float " + s1 + " = " + n2id + " * cos(" + n1id + ");\r\n";
            compute += "float " + s2 + " = " + n2id + " * sin(" + n1id + ");\r\n";

            return compute;
        }

        public override void TryAndProcess()
        {
            if (!input.IsValid || !input2.IsValid) return;

            float angle = input.Data.ToFloat();
            float radius = input2.Data.ToFloat();

            output.Data = radius * MathF.Cos(angle);
            output2.Data = radius * MathF.Sin(angle);
            result = output.Data?.ToString() + "," + output2.Data?.ToString();
        }
    }
}
