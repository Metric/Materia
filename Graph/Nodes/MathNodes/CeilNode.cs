using System;
using Materia.Rendering.Attributes;
using Materia.Rendering.Extensions;
using Materia.Rendering.Mathematics;
using Materia.Graph;
using MLog;

namespace Materia.Nodes.MathNodes
{
    public class CeilNode : MathNode
    {
        NodeInput input;
        NodeOutput output;
        
        public CeilNode(int w, int h, GraphPixelType p = GraphPixelType.RGBA) : base()
        {
            //we ignore w,h,p

            CanPreview = false;

            Name = "Ceil";
            Id = Guid.NewGuid().ToString();
            shaderId = "S" + Id.Split('-')[0];

            input = new NodeInput(NodeType.Float | NodeType.Float2 | NodeType.Float3 | NodeType.Float4, this, "Any Float Input");
            output = new NodeOutput(NodeType.Float | NodeType.Float2 | NodeType.Float3 | NodeType.Float4, this);

            Inputs.Add(input);
            Outputs.Add(output);
        }

        public override void UpdateOutputType()
        {
            if (input.HasInput)
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
                return "vec4 " + s + " = ceil(" + n1id + ");\r\n";
            }
            else if (input.Reference.Type == NodeType.Float3)
            {
                return "vec3 " + s + " = ceil(" + n1id + ");\r\n";
            }
            else if (input.Reference.Type == NodeType.Float2)
            {
                return "vec2 " + s + " = ceil(" + n1id + ");\r\n";
            }
            else if (input.Reference.Type == NodeType.Float)
            {
                return "float " + s + " = ceil(" + n1id + ");\r\n";
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
                    output.Data = v.Ceil();
                }
                else
                {
                    float v = input.Data.ToFloat();
                    output.Data = MathF.Ceiling(v);
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
