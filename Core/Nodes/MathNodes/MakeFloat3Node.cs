using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Materia.MathHelpers;
using Materia.Nodes.Helpers;

namespace Materia.Nodes.MathNodes
{
    public class MakeFloat3Node : MathNode
    {
        NodeInput input;
        NodeInput input2;
        NodeInput input3;
        NodeOutput output;

        MVector vec;

        public MakeFloat3Node(int w, int h, GraphPixelType p = GraphPixelType.RGBA) : base()
        {
            //we ignore w,h,p

            CanPreview = false;

            vec = new MVector();

            Name = "Make Float3";
            Id = Guid.NewGuid().ToString();
            shaderId = "S" + Id.Split('-')[0];

            input = new NodeInput(NodeType.Float, this, "X (Float)");
            input2 = new NodeInput(NodeType.Float, this, "Y (Float)");
            input3 = new NodeInput(NodeType.Float, this, "Z (Float)");

            output = new NodeOutput(NodeType.Float3, this);

            Inputs.Add(input);
            Inputs.Add(input2);
            Inputs.Add(input3);
            Outputs.Add(output);
        }

        public override string GetShaderPart(string currentFrag)
        {
            if (!input.HasInput || !input2.HasInput || !input3.HasInput) return "";
            var s = shaderId + "1";
            var n1id = (input.Reference.Node as MathNode).ShaderId;
            var n2id = (input2.Reference.Node as MathNode).ShaderId;
            var n3id = (input3.Reference.Node as MathNode).ShaderId;

            var index = input.Reference.Node.Outputs.IndexOf(input.Reference);

            n1id += index;

            var index2 = input2.Reference.Node.Outputs.IndexOf(input2.Reference);

            n2id += index2;

            var index3 = input3.Reference.Node.Outputs.IndexOf(input3.Reference);

            n3id += index3;


            return "vec3 " + s + " = vec3(" + n1id + "," + n2id + "," + n3id + ");\r\n";
        }

        public override void TryAndProcess()
        {
            if (!input.IsValid || !input2.IsValid || !input3.IsValid) return;
            float x = input.Data.ToFloat();
            float y = input2.Data.ToFloat();
            float z = input3.Data.ToFloat();

            output.Data = new MVector(x, y, z);
            result = output.Data?.ToString();
        }
    }
}
