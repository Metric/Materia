using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Materia.MathHelpers;
using Materia.Nodes.Helpers;

namespace Materia.Nodes.MathNodes
{
    public class LerpNode : MathNode
    {
        NodeInput input;
        NodeInput input2;
        NodeInput input3;
        NodeOutput output;

        public LerpNode(int w, int h, GraphPixelType p = GraphPixelType.RGBA) : base()
        {
            //we ignore w,h,p

            CanPreview = false;

            Name = "Linear Interpolation";
            Id = Guid.NewGuid().ToString();
            shaderId = "S" + Id.Split('-')[0];

            input = new NodeInput(NodeType.Float | NodeType.Float2 | NodeType.Float3 | NodeType.Float4, this, "From");
            input2 = new NodeInput(NodeType.Float | NodeType.Float2 | NodeType.Float3 | NodeType.Float4, this, "To");
            input3 = new NodeInput(NodeType.Float, this, "Delta");

            output = new NodeOutput(NodeType.Float | NodeType.Float2 | NodeType.Float3 | NodeType.Float4, this);

            Inputs.Add(input);
            Inputs.Add(input2);
            Inputs.Add(input3);

            Outputs.Add(output);
        }

        public override void UpdateOutputType()
        {
            if (Inputs.Count == 0) return;
            if (Inputs[1].HasInput && Inputs[2].HasInput)
            {
                NodeType t1 = Inputs[1].Reference.Type;
                NodeType t2 = Inputs[2].Reference.Type;

                if (t1 == NodeType.Float && t2 == NodeType.Float)
                {
                    output.Type = NodeType.Float;
                }
                else if ((t1 == NodeType.Float && t2 == NodeType.Float2) || (t1 == NodeType.Float2 && t2 == NodeType.Float))
                {
                    output.Type = NodeType.Float2;
                }
                else if ((t1 == NodeType.Float && t2 == NodeType.Float3) || (t1 == NodeType.Float3 && t2 == NodeType.Float))
                {
                    output.Type = NodeType.Float3;
                }
                else if ((t1 == NodeType.Float && t2 == NodeType.Float4) || (t1 == NodeType.Float4 && t2 == NodeType.Float))
                {
                    output.Type = NodeType.Float4;
                }
                else if (t1 == NodeType.Float2 && t2 == NodeType.Float2)
                {
                    output.Type = NodeType.Float2;
                }
                else if (t1 == NodeType.Float3 && t2 == NodeType.Float3)
                {
                    output.Type = NodeType.Float3;
                }
                else if (t1 == NodeType.Float4 && t2 == NodeType.Float4)
                {
                    output.Type = NodeType.Float4;
                }
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

            n1id += index;

            var index2 = input2.Reference.Node.Outputs.IndexOf(input2.Reference);

            n2id += index2;

            var index3 = input3.Reference.Node.Outputs.IndexOf(input3.Reference);

            n3id += index3;

            if(input.Reference.Type == NodeType.Float && input2.Reference.Type == NodeType.Float)
            {
                return "float " + s + " = mix(" + n1id + "," + n2id + "," + n3id + ");\r\n"; 
            }
            else if(input.Reference.Type == NodeType.Float2 && input2.Reference.Type == NodeType.Float2)
            {
                return "vec2 " + s + " = mix(" + n1id + "," + n2id + "," + n3id + ");\r\n";
            }
            else if (input.Reference.Type == NodeType.Float3 && input2.Reference.Type == NodeType.Float3)
            {
                return "vec3 " + s + " = mix(" + n1id + "," + n2id + "," + n3id + ");\r\n";
            }
            else if (input.Reference.Type == NodeType.Float4 && input2.Reference.Type == NodeType.Float4)
            {
                return "vec4 " + s + " = mix(" + n1id + "," + n2id + "," + n3id + ");\r\n";
            }

            return "";
        }

        public override void TryAndProcess()
        {
            if (!input.IsValid || !input2.IsValid || !input3.IsValid) return;

            NodeType t = input.Reference.Type;
            NodeType t2 = input2.Reference.Type;

            float delta = input3.Data.ToFloat();

            try
            {

                if (t == NodeType.Float && t2 == NodeType.Float)
                {
                    float v1 = input.Data.ToFloat();
                    float v2 = input2.Data.ToFloat();
                    output.Data = Utils.Lerp(v1, v2, delta);
                }
                else if ((t == NodeType.Float2 || t == NodeType.Float3 || t == NodeType.Float4)
                    && (t2 == NodeType.Float2 || t2 == NodeType.Float3 || t2 == NodeType.Float4))
                {
                    MVector v1 = (MVector)input.Data;
                    MVector v2 = (MVector)input2.Data;

                    output.Data = MVector.Lerp(v1, v2, delta);
                }
                else if (t == NodeType.Float && (t2 == NodeType.Float2 || t2 == NodeType.Float3 || t2 == NodeType.Float4))
                {
                    float f = input.Data.ToFloat();
                    MVector v1 = new MVector(f, f, f, f);
                    MVector v2 = (MVector)input2.Data;
                    output.Data = MVector.Lerp(v1, v2, delta);
                }
                else if ((t == NodeType.Float2 || t == NodeType.Float3 || t == NodeType.Float4) && t2 == NodeType.Float)
                {
                    float f = input2.Data.ToFloat();
                    MVector v2 = new MVector(f, f, f, f);
                    MVector v1 = (MVector)input.Data;
                    output.Data = MVector.Lerp(v1, v2, delta);
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
