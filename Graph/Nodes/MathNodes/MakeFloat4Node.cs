using System;
using Materia.Rendering.Attributes;
using Materia.Rendering.Extensions;
using Materia.Rendering.Mathematics;
using Materia.Graph;

namespace Materia.Nodes.MathNodes
{
    public class MakeFloat4Node : MathNode
    {
        NodeInput input;
        NodeInput input2;
        NodeInput input3;
        NodeInput input4;
        NodeOutput output;

        public MakeFloat4Node(int w, int h, GraphPixelType p = GraphPixelType.RGBA) : base()
        {
            //we ignore w,h,p

            CanPreview = false;

            defaultName = Name = "Make Float4";

            shaderId = "S" + Id.Split('-')[0];

            input = new NodeInput(NodeType.Float, this, "X (Float)");
            input2 = new NodeInput(NodeType.Float, this, "Y (Float)");
            input3 = new NodeInput(NodeType.Float, this, "Z (Float)");
            input4 = new NodeInput(NodeType.Float, this, "W (Float)");

            output = new NodeOutput(NodeType.Float4, this);

            Inputs.Add(input);
            Inputs.Add(input2);
            Inputs.Add(input3);
            Inputs.Add(input4);

            Outputs.Add(output);
        }

        public override string GetShaderPart(string currentFrag)
        {
            if (!input.HasInput || !input2.HasInput || !input3.HasInput || !input4.HasInput) return "";
            var s = shaderId + "1";
            var n1id = (input.Reference.Node as MathNode).ShaderId;
            var n2id = (input2.Reference.Node as MathNode).ShaderId;
            var n3id = (input3.Reference.Node as MathNode).ShaderId;
            var n4id = (input4.Reference.Node as MathNode).ShaderId;

            var index = input.Reference.Node.Outputs.IndexOf(input.Reference);

            n1id += index;

            var index2 = input2.Reference.Node.Outputs.IndexOf(input2.Reference);

            n2id += index2;

            var index3 = input3.Reference.Node.Outputs.IndexOf(input3.Reference);

            n3id += index3;

            var index4 = input4.Reference.Node.Outputs.IndexOf(input4.Reference);

            n4id += index4;

            return "vec4 " + s + " = vec4(" + n1id + "," + n2id + "," + n3id + "," + n4id + ");\r\n";
        }

        public override void TryAndProcess()
        {
            if (!input.IsValid || !input2.IsValid || !input3.IsValid || !input4.IsValid) return;
            float x = input.Data.ToFloat();
            float y = input2.Data.ToFloat();
            float z = input3.Data.ToFloat();
            float w = input4.Data.ToFloat();

            output.Data = new MVector(x, y, z, w);
            result = output.Data?.ToString();
        }
    }
}
