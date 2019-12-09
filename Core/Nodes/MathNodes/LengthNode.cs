using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Materia.MathHelpers;

namespace Materia.Nodes.MathNodes
{
    public class LengthNode : MathNode
    {
        NodeInput input;
        NodeOutput output;

        public LengthNode(int w, int h, GraphPixelType p = GraphPixelType.RGBA) : base()
        {
            //we ignore w,h,p

            CanPreview = false;

            Name = "Length";
            Id = Guid.NewGuid().ToString();
            shaderId = "S" + Id.Split('-')[0];

            input = new NodeInput(NodeType.Float2 | NodeType.Float3 | NodeType.Float4, this, "Any Vector Type");
            output = new NodeOutput(NodeType.Float, this);

            Inputs.Add(input);
            Outputs.Add(output);
        }

        public override string GetShaderPart(string currentFrag)
        {
            if (!input.HasInput) return "";
            var s = shaderId + "1";
            var n1id = (input.Reference.Node as MathNode).ShaderId;

            var index = input.Reference.Node.Outputs.IndexOf(input.Reference);

            n1id += index;

            return "float " + s + " = length(" + n1id + ");\r\n";
        }

        public override void TryAndProcess()
        {
            if (!input.IsValid) return;
            try
            {
                MVector v = (MVector)input.Data;
                output.Data = v.Length;

                result = output.Data?.ToString();
            }
            catch (Exception e)
            {

            }
        }
    }
}
