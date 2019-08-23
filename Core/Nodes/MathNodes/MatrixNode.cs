using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Materia.Math3D;

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

            Name = "Matrix";
            Id = Guid.NewGuid().ToString();
            shaderId = "S" + Id.Split('-')[0];

            input = new NodeInput(NodeType.Float | NodeType.Float2, this, "Float Input");
            output = new NodeOutput(NodeType.Matrix, this);

            Inputs.Add(input);

            input.OnInputAdded += Input_OnInputAdded;
            input.OnInputChanged += Input_OnInputChanged;

            Outputs.Add(output);
        }

        private void Input_OnInputChanged(NodeInput n)
        {
            TryAndProcess();
        }

        private void Input_OnInputAdded(NodeInput n)
        {
            UpdateOutputType();
            Updated();
        }

        public override void UpdateOutputType()
        {

        }

        public override void TryAndProcess()
        {
            if (input.HasInput)
            {
                Process();
            }
        }

        public override string GetShaderPart(string currentFrag)
        {
            if (input == null || !input.HasInput) return "";
            var s = shaderId + "1";
            var n1id = (input.Input.Node as MathNode).ShaderId;

            var index = input.Input.Node.Outputs.IndexOf(input.Input);

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

        protected virtual void Process()
        {
            if (input.Input.Data == null) return;

            object o = input.Input.Data;

            CalculateMatrix(o);

            output.Data = matrix;

            if (output.Data != null)
            {
                result = output.Data.ToString();
            }

            if (ParentGraph != null)
            {
                FunctionGraph g = (FunctionGraph)ParentGraph;

                if (g != null && g.OutputNode == this)
                {
                    g.Result = output.Data;
                }
            }
        }
    }
}
