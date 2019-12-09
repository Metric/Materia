using Materia.MathHelpers;
using Materia.Nodes.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Materia.Nodes.MathNodes
{
    public class IfElseNode : MathNode
    {
        NodeInput input;
        NodeInput input2;
        NodeInput input3;
        NodeOutput output;

        public IfElseNode(int w, int h, GraphPixelType p = GraphPixelType.RGBA) : base()
        {
            //we ignore w,h,p

            CanPreview = false;

            Name = "If Else";
            Id = Guid.NewGuid().ToString();
            shaderId = "S" + Id.Split('-')[0];

            input = new NodeInput(NodeType.Bool, this, "Comparison");
            input2 = new NodeInput(NodeType.Float | NodeType.Float2 | NodeType.Float3 | NodeType.Float4, this, "If");
            input3 = new NodeInput(NodeType.Float | NodeType.Float2 | NodeType.Float3 | NodeType.Float4, this, "Else");

            output = new NodeOutput(NodeType.Float | NodeType.Float2 | NodeType.Float3 | NodeType.Float4, this);

            Inputs.Add(input);
            Inputs.Add(input2);
            Inputs.Add(input3);

            Outputs.Add(output);
        }

        public override void UpdateOutputType()
        {
            if (Inputs.Count == 0) return;
            if (Inputs[2].HasInput && Inputs[3].HasInput)
            {
                NodeType t1 = Inputs[2].Reference.Type;
                NodeType t2 = Inputs[3].Reference.Type;

                if (t1 == NodeType.Float && t2 == NodeType.Float)
                {
                    output.Type = NodeType.Float;
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


            string compute = ""; 

            if(input2.Reference.Type == NodeType.Float && input3.Reference.Type == NodeType.Float)
            {
                compute = "float " + s + ";\r\n" + " if(" + n1id + " > 0) { \r\n";
                compute += s + " = " + n2id + ";\r\n} else {\r\n";
                compute += s + " = " + n3id + ";}\r\n";
            }
            else if(input2.Reference.Type == NodeType.Float2 && input3.Reference.Type == NodeType.Float2)
            {
                compute = "vec2 " + s + ";\r\n" + "if(" + n1id + " > 0) { \r\n";
                compute += s + " = " + n2id + ";\r\n} else {\r\n";
                compute += s + " = " + n3id + ";}\r\n";
            }
            else if (input2.Reference.Type == NodeType.Float3 && input3.Reference.Type == NodeType.Float3)
            {
                compute = "vec3 " + s + ";\r\n" + "if(" + n1id + " > 0) { \r\n";
                compute += s + " = " + n2id + ";\r\n} else {\r\n";
                compute += s + " = " + n3id + ";}\r\n";
            }
            else if (input2.Reference.Type == NodeType.Float4 && input3.Reference.Type == NodeType.Float4)
            {
                compute = "vec4 " + s + ";\r\n" + "if(" + n1id + " > 0) { \r\n";
                compute += s + " = " + n2id + ";\r\n} else {\r\n";
                compute += s + " = " + n3id + ";}\r\n";
            }

            return compute;
        }

        public override void TryAndProcess()
        {
            if (!input.IsValid || !input2.IsValid || !input3.IsValid) return;

            NodeType t1 = input2.Reference.Type;
            NodeType t2 = input3.Reference.Type;

            float r = input.Data.ToFloat();

            try
            {
                if (t1 == NodeType.Float && t2 == NodeType.Float)
                {
                    float v1 = input2.Data.ToFloat();
                    float v2 = input3.Data.ToFloat();

                    output.Data = r <= 0 ? v2 : v1;
                }
                else if ((t1 == NodeType.Float2 || t1 == NodeType.Float3 || t1 == NodeType.Float4)
                    && (t2 == NodeType.Float2 || t2 == NodeType.Float3 || t2 == NodeType.Float4))
                {
                    MVector v1 = (MVector)input2.Data;
                    MVector v2 = (MVector)input3.Data;

                    output.Data = r <= 0 ? v2 : v1;
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
