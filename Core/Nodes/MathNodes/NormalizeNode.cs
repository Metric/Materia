using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Materia.MathHelpers;

namespace Materia.Nodes.MathNodes
{
    public class NormalizeNode : MathNode
    {
        NodeInput input;
        NodeOutput output;

        public NormalizeNode(int w, int h, GraphPixelType p = GraphPixelType.RGBA) : base()
        {
            //we ignore w,h,p

            CanPreview = false;

            Name = "Normalize";
            Id = Guid.NewGuid().ToString();
            shaderId = "S" + Id.Split('-')[0];

            input = new NodeInput(NodeType.Float2 | NodeType.Float3 | NodeType.Float4, this, "Any Vector Type");
            output = new NodeOutput(NodeType.Float2 | NodeType.Float3 | NodeType.Float4, this);

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
                return "vec4 " + s + " = normalize(" + n1id + ");\r\n";
            }
            else if (input.Reference.Type == NodeType.Float3)
            {
                return "vec3 " + s + " = normalize(" + n1id + ");\r\n";
            }
            else if (input.Reference.Type == NodeType.Float2)
            {
                return "vec2 " + s + " = normalize(" + n1id + ");\r\n";
            }
            else if (input.Reference.Type == NodeType.Float)
            {
                return "float " + s + " = normalize(" + n1id + ");\r\n";
            }

            return "";
        }

        public override void TryAndProcess()
        {
            if (!input.IsValid) return;
            try
            {
                MVector v = (MVector)input.Data;
                output.Data = v.Normalized;
                result = output.Data?.ToString();
            }
            catch (Exception e)
            {

            }
            UpdateOutputType();
        }
    }
}
