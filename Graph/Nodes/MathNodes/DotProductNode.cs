using System;
using Materia.Rendering.Attributes;
using Materia.Rendering.Mathematics;
using Materia.Graph;
using MLog;

namespace Materia.Nodes.MathNodes
{
    public class DotProductNode : MathNode
    {
        

        NodeInput input;
        NodeInput input2;
        NodeOutput output;

        public DotProductNode(int w, int h, GraphPixelType p = GraphPixelType.RGBA) : base()
        {
            //we ignore w,h,p

            CanPreview = false;

            Name = "Dot Product";
        
            shaderId = "S" + Id.Split('-')[0];

            input = new NodeInput(NodeType.Float2 | NodeType.Float3 | NodeType.Float4, this, "Vector Float 0");
            input2 = new NodeInput(NodeType.Float2 | NodeType.Float3 | NodeType.Float4, this, "Vector Float 1");

            output = new NodeOutput(NodeType.Float, this);

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

            if (input.Reference.Type != input2.Reference.Type) return "";

            return "float " + s + " = dot(" + n1id + "," + n2id + ");\r\n";
        }

        public override void TryAndProcess()
        {
            if (!input.IsValid || !input2.IsValid) return;

            try
            {
                MVector v1 = (MVector)input.Data;
                MVector v2 = (MVector)input2.Data;

                output.Data = MVector.Dot(v1, v2);

                result = output.Data?.ToString();
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }
    }
}
