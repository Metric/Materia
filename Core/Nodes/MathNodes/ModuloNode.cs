using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Materia.MathHelpers;
using Materia.Nodes.Helpers;

namespace Materia.Nodes.MathNodes
{
    public class ModuloNode : MathNode
    {
        NodeInput input;
        NodeInput input2;
        NodeOutput output;

        public ModuloNode(int w, int h, GraphPixelType p = GraphPixelType.RGBA) : base()
        {
            //we ignore w,h,p

            CanPreview = false;

            Name = "Modulo";
            Id = Guid.NewGuid().ToString();
            shaderId = "S" + Id.Split('-')[0];

            input = new NodeInput(NodeType.Float | NodeType.Float2 | NodeType.Float3 | NodeType.Float4, this, "Any Float Input");
            input2 = new NodeInput(NodeType.Float, this, "Mod (Float) Input");

            output = new NodeOutput(NodeType.Float | NodeType.Float2 | NodeType.Float3 | NodeType.Float4, this);

            Inputs.Add(input);
            Inputs.Add(input2);

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
            if (!input.HasInput || !input2.HasInput) return "";
            var s = shaderId + "1";
            var n1id = (input.Reference.Node as MathNode).ShaderId;
            var n2id = (input2.Reference.Node as MathNode).ShaderId;

            var index = input.Reference.Node.Outputs.IndexOf(input.Reference);

            n1id += index;

            var index2 = input2.Reference.Node.Outputs.IndexOf(input2.Reference);

            n2id += index2;

            var type = input.Reference.Type;

            if (type == NodeType.Float)
            {
                return "float " + s + " = mod(" + n1id + "," + n2id + ");\r\n";
            }
            else if(type == NodeType.Float2)
            {
                return "vec2 " + s + " = mod(" + n1id + "," + n2id + ");\r\n";
            }
            else if (type == NodeType.Float3)
            {
                return "vec3 " + s + " = mod(" + n1id + "," + n2id + ");\r\n";
            }
            else if (type == NodeType.Float4)
            {
                return "vec4 " + s + " = mod(" + n1id + "," + n2id + ");\r\n";
            }

            return "";
        }

        public override void TryAndProcess()
        {
            if (!input.IsValid || !input2.IsValid) return;

            NodeType t = input.Reference.Type;

            float mod = input2.Data.ToFloat();

            try
            {
                if (t == NodeType.Float)
                {
                    float f = input.Data.ToFloat();
                    output.Data = f % mod;
                }
                else if (t == NodeType.Float2 || t == NodeType.Float3 || t == NodeType.Float4)
                {
                    MVector v = (MVector)input.Data;
                    output.Data = v.Mod(mod);
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
