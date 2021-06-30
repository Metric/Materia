using System;
using Materia.Rendering.Attributes;
using Materia.Rendering.Mathematics;
using Materia.Graph;

namespace Materia.Nodes.MathNodes
{
    public class MatrixNode : MathNode 
    {
        protected Matrix4 matrix;
        protected NodeInput input;
        protected NodeOutput output;

        public MatrixNode(int w, int h, GraphPixelType p = GraphPixelType.RGBA) : base()
        {
            //we ignore w,h,p

            CanPreview = false;

            defaultName = Name = "Matrix";
 
            shaderId = "S" + Id.Split('-')[0];

            input = new NodeInput(NodeType.Float | NodeType.Float2, this, "Float Input");
            output = new NodeOutput(NodeType.Matrix, this);

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

            return "mat4 " + s + " = " + GetMatrixPart(n1id) + ";\r\n";
        }

        protected virtual string GetMatrixPart(string inputId)
        {
            return "";
        }

        protected virtual void CalculateMatrix(object o)
        {
            
        }

        public override void TryAndProcess()
        {
            if (!input.IsValid) return;
            CalculateMatrix(input.Data);
            output.Data = matrix;
            result = output.Data?.ToString();
        }
    }
}
