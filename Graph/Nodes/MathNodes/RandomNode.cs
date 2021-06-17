using System;
using Materia.Rendering.Attributes;
using Materia.Rendering.Extensions;
using Materia.Rendering.Mathematics;
using Materia.Nodes.Helpers;
using Materia.Graph;

namespace Materia.Nodes.MathNodes
{
    public class RandomNode : MathNode
    {
        NodeInput input;
        NodeOutput output;

        public RandomNode(int w, int h, GraphPixelType p = GraphPixelType.RGBA) : base()
        {
            //we ignore w,h,p

            CanPreview = false;

            Name = "Random";

            shaderId = "S" + Id.Split('-')[0];

            input = new NodeInput(NodeType.Float | NodeType.Float2, this, "Float Input");
            output = new NodeOutput(NodeType.Float, this);

            Inputs.Add(input);

            Outputs.Add(output);
        }

        public override string GetShaderPart(string currentFrag)
        {
            if (!input.HasInput) return "";

            int seed = parentGraph.RandomSeed;

            var s = shaderId + "1";
            var n1id = (input.Reference.Node as MathNode).ShaderId;

            var index = input.Reference.Node.Outputs.IndexOf(input.Reference);

            n1id += index;

            if (input.Reference.Type == NodeType.Float2)
            {
                return "float " + s + " = rand(" + n1id + " + " + seed.ToCodeString() + ");\r\n";
            }
            else
            { 
                return "float " + s + " = rand(vec2(" + n1id + ", 1.0 - " + n1id + ") + " + seed.ToCodeString() + ");\r\n";
            }
        }

        public override void TryAndProcess()
        {
            if (!input.IsValid) return;
            int seed = parentGraph.RandomSeed;
            NodeType t = input.Reference.Type;

            try
            {
                if (t == NodeType.Float)
                {
                    float f = input.Data.ToFloat();
                    MVector v2 = new MVector(f, 1.0f - f) + seed;
                    output.Data = Utils.Rand(ref v2);
                }
                else if (t == NodeType.Float2 || t == NodeType.Float3 || t == NodeType.Float4)
                {
                    MVector v = (MVector)input.Data;
                    v += seed;
                    output.Data = Utils.Rand(ref v);
                }
                result = output.Data?.ToString();
            }
            catch (Exception e)
            {

            }
        }
    }
}
