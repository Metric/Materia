using System;
using Materia.Rendering.Attributes;
using Materia.Rendering.Extensions;
using Materia.Rendering.Mathematics;
using Materia.Graph;
using MLog;

namespace Materia.Nodes.MathNodes
{
    public class MakeFloat2Node : MathNode
    {
        

        NodeInput input;
        NodeInput input2;
        NodeOutput output;

        public MakeFloat2Node(int w, int h, GraphPixelType p = GraphPixelType.RGBA) : base()
        {
            //we ignore w,h,p

            CanPreview = false;

            Name = "Make Float2";
    
            shaderId = "S" + Id.Split('-')[0];

            input = new NodeInput(NodeType.Float, this, "X (Float)");
            input2 = new NodeInput(NodeType.Float, this, "Y (Float)");

            output = new NodeOutput(NodeType.Float2, this);

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


            return "vec2 " + s + " = vec2(" + n1id + "," + n2id + ");\r\n";
        }

        public override void TryAndProcess()
        {
            if (!input.IsValid || !input2.IsValid) return;
            float x = input.Data.ToFloat();
            float y = input2.Data.ToFloat();

            output.Data = new MVector(x, y);
            result = output.Data?.ToString();
        }
    }
}
