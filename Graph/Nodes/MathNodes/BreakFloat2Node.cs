using System;
using Materia.Rendering.Attributes;
using Materia.Rendering.Mathematics;
using Materia.Graph;

namespace Materia.Nodes.MathNodes
{
    public class BreakFloat2Node : MathNode
    {
        NodeInput input;
        NodeOutput output;
        NodeOutput output2;

        public BreakFloat2Node(int w, int h, GraphPixelType p = GraphPixelType.RGBA) : base()
        {
            //we ignore w,h,p

            CanPreview = false;

            Name = "Break Float2";
            Id = Guid.NewGuid().ToString();
            shaderId = "S" + Id.Split('-')[0];

            input = new NodeInput(NodeType.Float2, this, "Float2 Type");
            output = new NodeOutput(NodeType.Float, this, "X");
            output2 = new NodeOutput(NodeType.Float, this, "Y");

            Inputs.Add(input);

            Outputs.Add(output);
            Outputs.Add(output2);
        }

        public override string GetShaderPart(string currentFrag)
        {
            if (!Inputs[1].HasInput) return "";
            var s1 = shaderId + "1";
            var s2 = shaderId + "2";

            var n1id = (Inputs[1].Reference.Node as MathNode).ShaderId;

            var index = Inputs[1].Reference.Node.Outputs.IndexOf(Inputs[1].Reference);

            n1id += index;

            string compute = "";
            compute += "float " + s1 + " = " + n1id + ".x;\r\n";
            compute += "float " + s2 + " = " + n1id + ".y;\r\n";

            return compute;
        }

        public override void TryAndProcess()
        {
            NodeInput input = Inputs[1];

            if (!input.IsValid) return;

            try
            {
                MVector v = (MVector)input.Data;
                output.Data = v.X;
                output2.Data = v.Y;
                result = result = v.X + "," + v.Y;
            }
            catch (Exception e)
            {

            }
        }
    }
}
