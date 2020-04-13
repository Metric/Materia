using System;
using Materia.Rendering.Mathematics;
using Materia.Rendering.Extensions;
using Materia.Rendering.Attributes;
using Materia.Graph;
using MLog;

namespace Materia.Nodes.MathNodes
{
    public class AbsoluteNode : MathNode
    {

        NodeInput input;
        NodeOutput output;

        public AbsoluteNode(int w, int h, GraphPixelType p = GraphPixelType.RGBA) : base()
        {
            //we ignore w,h,p

            CanPreview = false;

            Name = "Absolute";
            Id = Guid.NewGuid().ToString();
            shaderId = "S" + Id.Split('-')[0];

            input = new NodeInput(NodeType.Float | NodeType.Float2 | NodeType.Float3 | NodeType.Float4, this, "Any Float Type");
            output = new NodeOutput(NodeType.Float | NodeType.Float2 | NodeType.Float3 | NodeType.Float4, this);

            Inputs.Add(input);
            Outputs.Add(output);
        }

        public override void UpdateOutputType()
        {
            if(input.HasInput)
            {
                output.Type = input.Reference.Type;
            }
        }

        public override string GetShaderPart(string currentFrag)
        {
            if (!input.HasInput) return "";
            var s = shaderId + "1";
            var n1id = (input.Reference.Node as MathNode).ShaderId;

            var index = input.Reference.Node.Outputs.IndexOf(input.Reference);

            n1id += index;

            if (input.Reference.Type == NodeType.Float4)
            {
                return "vec4 " + s + " = abs(" + n1id + ");\r\n";
            }
            else if (input.Reference.Type == NodeType.Float3)
            {
                return "vec3 " + s + " = abs(" + n1id + ");\r\n";
            }
            else if (input.Reference.Type == NodeType.Float2)
            {
                return "vec2 " + s + " = abs(" + n1id + ");\r\n";
            }
            else if (input.Reference.Type == NodeType.Float)
            {
                return "float " + s + " = abs(" + n1id + ");\r\n";
            }

            return "";
        }

        public override void TryAndProcess()
        {
            if (!input.IsValid) return;

            NodeType t = input.Reference.Type;

            try
            {
                if (t == NodeType.Float4 || t == NodeType.Float3 || t == NodeType.Float2)
                {
                    MVector v = (MVector)input.Data;
                    output.Data = v.Abs();
                }
                else if (t == NodeType.Float)
                {
                    float f = input.Data.ToFloat();
                    output.Data = Math.Abs(f);
                }

                result = output.Data?.ToString();
            }
            catch (Exception e)
            {
                Log.Error(e);
            }

            UpdateOutputType();
        }
    }
}
