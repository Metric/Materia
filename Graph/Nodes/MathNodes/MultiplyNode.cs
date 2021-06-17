using System;
using Materia.Rendering.Attributes;
using Materia.Rendering.Extensions;
using Materia.Rendering.Mathematics;
using Materia.Graph;
using MLog;

namespace Materia.Nodes.MathNodes
{
    public class MultiplyNode : MathNode
    {
        NodeOutput output;

        public MultiplyNode(int w, int h, GraphPixelType p = GraphPixelType.RGBA) : base()
        {
            //we ignore w,h,p

            CanPreview = false;

            Name = "Multiply";
 
            shaderId = "S" + Id.Split('-')[0];
     
            output = new NodeOutput(NodeType.Float | NodeType.Float2 | NodeType.Float3 | NodeType.Float4 | NodeType.Matrix, this);

            for (int i = 0; i < 2; ++i)
            {
                var input = new NodeInput(NodeType.Float | NodeType.Float2 | NodeType.Float3 | NodeType.Float4 | NodeType.Matrix, this, "Input " + i);
                Inputs.Add(input);
            }

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
                else if((t1 == NodeType.Matrix && t2 == NodeType.Float2) || (t1 == NodeType.Float2 && t2 == NodeType.Matrix))
                {
                    output.Type = NodeType.Float2;
                }
                else if((t1 == NodeType.Matrix && t2 == NodeType.Float3) || (t1 == NodeType.Float3 && t2 == NodeType.Matrix))
                {
                    output.Type = NodeType.Float3;
                }
                else if ((t1 == NodeType.Matrix && t2 == NodeType.Float4) || (t1 == NodeType.Float4 && t2 == NodeType.Matrix))
                {
                    output.Type = NodeType.Float4;
                }
                else if(t1 == NodeType.Matrix && t2 == NodeType.Matrix)
                { 
                    output.Type = NodeType.Matrix;
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

            if (t1 == NodeType.Float && t2 == NodeType.Float)
            {
                return "float " + s + " = " + n1id + " * " + n2id + ";\r\n";
            }
            else if ((t1 == NodeType.Float && t2 == NodeType.Float2) || (t1 == NodeType.Float2 && t2 == NodeType.Float))
            {
                return "vec2 " + s + " = " + n1id + " * " + n2id + ";\r\n";
            }
            else if ((t1 == NodeType.Float && t2 == NodeType.Float3) || (t1 == NodeType.Float3 && t2 == NodeType.Float))
            {
                return "vec3 " + s + " = " + n1id + " * " + n2id + ";\r\n";
            }
            else if ((t1 == NodeType.Float && t2 == NodeType.Float4) || (t1 == NodeType.Float4 && t2 == NodeType.Float))
            {
                return "vec4 " + s + " = " + n1id + " * " + n2id + ";\r\n";
            }
            else if (t1 == NodeType.Float2 && t2 == NodeType.Float2)
            {
                return "vec2 " + s + " = " + n1id + " * " + n2id + ";\r\n";
            }
            else if (t1 == NodeType.Float3 && t2 == NodeType.Float3)
            {
                return "vec3 " + s + " = " + n1id + " * " + n2id + ";\r\n";
            }
            else if (t1 == NodeType.Float4 && t2 == NodeType.Float4)
            {
                return "vec4 " + s + " = " + n1id + " * " + n2id + ";\r\n";
            }
            else if(t1 == NodeType.Float2 && t2 == NodeType.Matrix)
            {
                return "vec2 " + s + " = (vec4(" + n1id + ",0,1) * " + n2id + ").xy;\r\n";
            }
            else if (t1 == NodeType.Matrix && t2 == NodeType.Float2)
            {
                return "vec2 " + s + " = (" + n1id + " * vec4(" + n2id + ",0,1)).xy;\r\n"; 
            }
            else if (t1 == NodeType.Float3 && t2 == NodeType.Matrix)
            {
                return "vec3 " + s + " = (vec4(" + n1id + ",1) * " + n2id + ").xyz;\r\n";
            }
            else if (t1 == NodeType.Matrix && t2 == NodeType.Float3)
            {
                return "vec3 " + s + " = (" + n1id + " * vec4(" + n2id + ",1)).xyz;\r\n";
            }
            else if ((t1 == NodeType.Float4 && t2 == NodeType.Matrix) || (t1 == NodeType.Matrix && t2 == NodeType.Float4))
            {
                return "vec4 " + s + " = " + n1id + " * " + n2id + ";\r\n";
            }
            else if(t1 == NodeType.Matrix && t2 == NodeType.Matrix)
            {
                return "mat4 " + s + " = " + n1id + " * " + n2id + ";\r\n";
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

                    output.Data = v1 * v2;
                }
                else if ((t1 == NodeType.Float2 || t1 == NodeType.Float3 || t1 == NodeType.Float4)
                    && (t2 == NodeType.Float2 || t2 == NodeType.Float3 || t2 == NodeType.Float4))
                {
                    MVector v1 = (MVector)input.Data;
                    MVector v2 = (MVector)input2.Data;

                    output.Data = v1 * v2;
                }
                else if (t1 == NodeType.Float && (t2 == NodeType.Float2 || t2 == NodeType.Float3 || t2 == NodeType.Float4))
                {
                    float v1 = input.Data.ToFloat();
                    MVector v2 = (MVector)input2.Data;

                    output.Data = v1 * v2;
                }
                else if ((t1 == NodeType.Float2 || t1 == NodeType.Float3 || t1 == NodeType.Float4)
                    && t2 == NodeType.Float)
                {
                    float v2 = input2.Data.ToFloat();
                    MVector v1 = (MVector)input.Data;

                    output.Data = v1 * v2;
                }
                else if ((t1 == NodeType.Float2 || t1 == NodeType.Float3 || t1 == NodeType.Float4) && t2 == NodeType.Matrix)
                {
                    MVector v1 = (MVector)input.Data;
                    Vector4 vec = new Vector4(v1.X, v1.Y, v1.Z, v1.W);
                    if (t1 == NodeType.Float2 || t1 == NodeType.Float3)
                    {
                        vec.W = 1;
                    }
                    Matrix4 m = (Matrix4)input2.Data;
                    output.Data = m * vec;
                }
                else if (t1 == NodeType.Matrix && (t2 == NodeType.Float2 || t2 == NodeType.Float3 || t2 == NodeType.Float4))
                {
                    MVector v1 = (MVector)input2.Data;
                    Vector4 vec = new Vector4(v1.X, v1.Y, v1.Z, v1.W);
                    Matrix4 m = (Matrix4)input.Data;
                    if (t2 == NodeType.Float2 || t2 == NodeType.Float3)
                    {
                        vec.W = 1;
                    }
                    output.Data = vec * m;
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
