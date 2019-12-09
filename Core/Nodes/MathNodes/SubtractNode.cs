using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Materia.MathHelpers;

namespace Materia.Nodes.MathNodes
{
    public class SubtractNode : MathNode
    {
        NodeOutput output;

        public SubtractNode(int w, int h, GraphPixelType p = GraphPixelType.RGBA) : base()
        {
            //we ignore w,h,p

            CanPreview = false;

            Name = "Subtract";
            Id = Guid.NewGuid().ToString();
            shaderId = "S" + Id.Split('-')[0];
            
            output = new NodeOutput(NodeType.Float | NodeType.Float2 | NodeType.Float3 | NodeType.Float4, this);

            for (int i = 0; i < 2; ++i)
            {
                var input = new NodeInput(NodeType.Float | NodeType.Float2 | NodeType.Float3 | NodeType.Float4, this, "Any Float Input " + i);
                Inputs.Add(input);
            }

            Outputs.Add(output);
        }

        public override void UpdateOutputType()
        {
            if (Inputs.Count == 0) return;
            if(Inputs[1].HasInput && Inputs[2].HasInput)
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
            if (!Inputs[1].HasInput || !Inputs[2].HasInput) return "";

            var s = shaderId + "1";
            var n1id = (Inputs[1].Reference.Node as MathNode).ShaderId;
            var n2id = (Inputs[2].Reference.Node as MathNode).ShaderId;

            var index = Inputs[1].Reference.Node.Outputs.IndexOf(Inputs[1].Reference);

            n1id += index;

            var index2 = Inputs[2].Reference.Node.Outputs.IndexOf(Inputs[2].Reference);

            n2id += index2;

            var t1 = Inputs[1].Reference.Type;
            var t2 = Inputs[2].Reference.Type;

            if(t1 == NodeType.Float && t2 == NodeType.Float)
            {
                return "float " + s + " = " + n1id + " - " + n2id + ";\r\n";
            }
            else if((t1 == NodeType.Float && t2 == NodeType.Float2) || (t1 == NodeType.Float2 && t2 == NodeType.Float))
            {
                return "vec2 " + s + " = " + n1id + " - " + n2id + ";\r\n";
            }
            else if ((t1 == NodeType.Float && t2 == NodeType.Float3) || (t1 == NodeType.Float3 && t2 == NodeType.Float))
            {
                return "vec3 " + s + " = " + n1id + " - " + n2id + ";\r\n";
            }
            else if ((t1 == NodeType.Float && t2 == NodeType.Float4) || (t1 == NodeType.Float4 && t2 == NodeType.Float))
            {
                return "vec4 " + s + " = " + n1id + " - " + n2id + ";\r\n";
            }
            else if(t1 == NodeType.Float2 && t2 == NodeType.Float2)
            {
                return "vec2 " + s + " = " + n1id + " - " + n2id + ";\r\n";
            }
            else if(t1 == NodeType.Float3 && t2 == NodeType.Float3)
            {
                return "vec3 " + s + " = " + n1id + " - " + n2id + ";\r\n";
            }
            else if(t1  == NodeType.Float4 && t2 == NodeType.Float4)
            {
                return "vec4 " + s + " = " + n1id + " - " + n2id + ";\r\n";
            }

            return "";
        }

        public override void TryAndProcess()
        {
            NodeInput input = Inputs[1];
            NodeInput input2 = Inputs[2];

            if (!input.IsValid || !input2.IsValid) return;

            NodeType t1 = input.Reference.Type;
            NodeType t2 = input2.Reference.Type;

            try
            {

                if (t1 == NodeType.Float && t2 == NodeType.Float)
                {
                    float v1 = input.Data.ToFloat();
                    float v2 = input2.Data.ToFloat();
                    output.Data = v1 - v2;
                }
                else if ((t1 == NodeType.Float2 || t1 == NodeType.Float3 || t1 == NodeType.Float4) && (t2 == NodeType.Float2 || t2 == NodeType.Float3 || t2 == NodeType.Float4))
                {
                    MVector v = (MVector)input.Data;
                    MVector v2 = (MVector)input2.Data;

                    output.Data = v - v2;
                }
                else if (t1 == NodeType.Float && (t2 == NodeType.Float2 || t2 == NodeType.Float3 || t2 == NodeType.Float4))
                {
                    float v1 = input.Data.ToFloat();
                    MVector v2 = (MVector)input2.Data;
                    output.Data = v1 - v2;
                }
                else if ((t1 == NodeType.Float2 || t1 == NodeType.Float3 || t1 == NodeType.Float4) && t2 == NodeType.Float)
                {
                    MVector v1 = (MVector)input.Data;
                    float v2 = input2.Data.ToFloat();
                    output.Data = v1 - v2;
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
