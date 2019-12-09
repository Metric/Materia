using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Materia.MathHelpers;

namespace Materia.Nodes.MathNodes
{
    public class BreakFloat3Node : MathNode
    {
        NodeInput input;
        NodeOutput output;
        NodeOutput output2;
        NodeOutput output3;

        public BreakFloat3Node(int w, int h, GraphPixelType p = GraphPixelType.RGBA) : base()
        {
            //we ignore w,h,p

            CanPreview = false;

            Name = "Break Float3";
            Id = Guid.NewGuid().ToString();
            shaderId = "S" + Id.Split('-')[0];

            input = new NodeInput(NodeType.Float3, this, "Float3 Type");
            output = new NodeOutput(NodeType.Float, this, "X");
            output2 = new NodeOutput(NodeType.Float, this, "Y");
            output3 = new NodeOutput(NodeType.Float, this, "Z");

            Inputs.Add(input);

            Outputs.Add(output);
            Outputs.Add(output2);
            Outputs.Add(output3);
        }

        public override string GetShaderPart(string currentFrag)
        {
            if (!Inputs[1].HasInput) return "";
            var s1 = shaderId + "1";
            var s2 = shaderId + "2";
            var s3 = shaderId + "3";

            var n1id = (Inputs[1].Reference.Node as MathNode).ShaderId;

            var index = Inputs[1].Reference.Node.Outputs.IndexOf(Inputs[1].Reference);

            n1id += index;

            string compute = "";
            compute += "float " + s1 + " = " + n1id + ".x;\r\n";
            compute += "float " + s2 + " = " + n1id + ".y;\r\n";
            compute += "float " + s3 + " = " + n1id + ".z;\r\n";

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
                output3.Data = v.Z;
                result = v.X + "," + v.Y + "," + v.Z;
            }
            catch (Exception e)
            {

            }
        }
    }
}
