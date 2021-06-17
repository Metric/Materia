using System;
using Materia.Rendering.Attributes;
using Materia.Rendering.Extensions;
using Materia.Rendering.Mathematics;
using Materia.Graph;
using Materia.Nodes.Helpers;

namespace Materia.Nodes.MathNodes
{
    public class Random2Node : MathNode
    {
        NodeInput input;
        NodeInput input2;
        NodeOutput output;

        public Random2Node(int w, int h, GraphPixelType p = GraphPixelType.RGBA) : base()
        {
            //we ignore w,h,p

            CanPreview = false;

            Name = "Random2";

            shaderId = "S" + Id.Split('-')[0];

            input = new NodeInput(NodeType.Float2, this, "Float2 Input");
            input2 = new NodeInput(NodeType.Float, this, "Float Input");
            output = new NodeOutput(NodeType.Float, this);

            Inputs.Add(input);
            Inputs.Add(input2);

            Outputs.Add(output);
        }

        public override string GetShaderPart(string currentFrag)
        {
            if (!input.HasInput || !input2.HasInput) return "";

            int seed = parentGraph.RandomSeed;

            var s = shaderId + "1";
            var n1id = (input.Reference.Node as MathNode).ShaderId;
            var n2id = (input2.Reference.Node as MathNode).ShaderId;

            var index = input.Reference.Node.Outputs.IndexOf(input.Reference);
            var index2 = input2.Reference.Node.Outputs.IndexOf(input2.Reference);

            n1id += index;
            n2id += index2;
            
            return "float " + s + " = rand(vec2(rand(" + n1id + " + " + seed.ToCodeString() + ")," + n2id + ") + " + seed.ToCodeString() + ");\r\n";
        }

        public override void TryAndProcess()
        {
            if (!input.IsValid || !input2.IsValid) return;

            int seed = parentGraph.RandomSeed;
            try
            {
                MVector v1 = (MVector)input.Data;
                v1 += seed;
                float v2 = input2.Data.ToFloat();
                float v3 = Utils.Rand(ref v1);
                MVector v4 = new MVector(v3, v2) + seed;
                output.Data = Utils.Rand(ref v4);
                result = output.Data?.ToString();
            }
            catch (Exception e)
            {

            }
        }
    }
}
