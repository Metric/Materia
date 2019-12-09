using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Materia.MathHelpers;
using Materia.Nodes.Helpers;
using NLog;

namespace Materia.Nodes.MathNodes
{
    public class ClampNode : MathNode
    {
        NodeInput input;
        NodeInput input2;
        NodeInput input3;
        NodeOutput output;

        private static ILogger Log = LogManager.GetCurrentClassLogger();

        public ClampNode(int w, int h, GraphPixelType p = GraphPixelType.RGBA) : base()
        {
            //we ignore w,h,p

            CanPreview = false;

            Name = "Clamp";
            Id = Guid.NewGuid().ToString();
            shaderId = "S" + Id.Split('-')[0];

            input = new NodeInput(NodeType.Float | NodeType.Float2 | NodeType.Float3 | NodeType.Float4, this, "Value");
            input2 = new NodeInput(NodeType.Float | NodeType.Float2 | NodeType.Float3 | NodeType.Float4, this, "Min");
            input3 = new NodeInput(NodeType.Float | NodeType.Float2 | NodeType.Float3 | NodeType.Float4, this, "Max");
            output = new NodeOutput(NodeType.Float | NodeType.Float2 | NodeType.Float3 | NodeType.Float4, this);

            Inputs.Add(input);
            Inputs.Add(input2);
            Inputs.Add(input3);

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
            if (!input.HasInput || !input2.HasInput || !input3.HasInput) return "";
            var s = shaderId + "1";
            var n1id = (input.Reference.Node as MathNode).ShaderId;
            var n2id = (input2.Reference.Node as MathNode).ShaderId;
            var n3id = (input3.Reference.Node as MathNode).ShaderId;

            var index = input.Reference.Node.Outputs.IndexOf(input.Reference);
            var index2 = input2.Reference.Node.Outputs.IndexOf(input2.Reference);
            var index3 = input3.Reference.Node.Outputs.IndexOf(input3.Reference);

            n1id += index;
            n2id += index2;
            n3id += index3;

            if (input.Reference.Type == NodeType.Float4)
            {
                return "vec4 " + s + " = clamp(" + n1id + "," + n2id + "," + n3id +");\r\n";
            }
            else if (input.Reference.Type == NodeType.Float3)
            {
                return "vec3 " + s + " = clamp(" + n1id + "," + n2id + "," + n3id + ");\r\n";
            }
            else if (input.Reference.Type == NodeType.Float2)
            {
                return "vec2 " + s + " = clamp(" + n1id + "," + n2id + "," + n3id + ");\r\n";
            }
            else if (input.Reference.Type == NodeType.Float)
            {
                return "float " + s + " = clamp(" + n1id + "," + n2id + "," + n3id + ");\r\n";
            }

            return "";
        }

        public override void TryAndProcess()
        {
            if (!input.IsValid || !input2.IsValid || !input3.IsValid)
            {
                return;
            }

            NodeType t = input.Reference.Type;

            try
            {
                if (t == NodeType.Float2 || t == NodeType.Float3 || t == NodeType.Float4)
                {
                    MVector min = (MVector)input2.Data;
                    MVector max = (MVector)input3.Data;
                    MVector v = (MVector)input.Data;

                    output.Data = v.Clamp(min, max);
                }
                else if (t == NodeType.Float)
                {
                    float min = input2.Data.ToFloat();
                    float max = input3.Data.ToFloat();
                    float v = input.Data.ToFloat();

                    output.Data = Math.Min(max, Math.Max(min, v));
                }

                result = output.Data?.ToString();
            }
            catch (Exception e)
            {

            }

            UpdateOutputType();
        }
    }
}
