using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Materia.Nodes.MathNodes
{
    public class CartesianNode : MathNode
    {
        NodeInput input;
        NodeInput input2;
        NodeOutput output;
        NodeOutput output2;

        public CartesianNode(int w, int h, GraphPixelType p = GraphPixelType.RGBA)
        {
            //we ignore w,h,p

            CanPreview = false;

            Name = "Cartesian";
            Id = Guid.NewGuid().ToString();
            shaderId = "S" + Id.Split('-')[0];

            input = new NodeInput(NodeType.Float, this, "Angle (Deg) Float Input");
            input2 = new NodeInput(NodeType.Float, this, "Radius Float Input");
            output = new NodeOutput(NodeType.Float, this, "X Output");
            output2 = new NodeOutput(NodeType.Float, this, "Y Output");

            Inputs = new List<NodeInput>();
            Inputs.Add(input);
            Inputs.Add(input2);
                
            input.OnInputAdded += Input_OnInputAdded;
            input.OnInputChanged += Input_OnInputChanged;

            input2.OnInputAdded += Input_OnInputAdded;
            input2.OnInputChanged += Input_OnInputChanged;

            Outputs = new List<NodeOutput>();
            Outputs.Add(output);
            Outputs.Add(output2);
        }

        private void Input_OnInputChanged(NodeInput n)
        {
            TryAndProcess();
        }

        private void Input_OnInputAdded(NodeInput n)
        {
            TryAndProcess();
        }

        public override void TryAndProcess()
        {
            if (input.HasInput && input2.HasInput)
            {
                Process();
            }
        }

        public override string GetShaderPart()
        {
            if (!Inputs[0].HasInput || !Inputs[1].HasInput) return "";
            var s1 = shaderId + "0";
            var s2 = shaderId + "1";

            var n1id = (Inputs[0].Input.Node as MathNode).ShaderId;
            var n2id = (Inputs[1].Input.Node as MathNode).ShaderId;

            var index = Inputs[0].Input.Node.Outputs.IndexOf(Inputs[0].Input);

            n1id += index;

            var index2 = Inputs[1].Input.Node.Outputs.IndexOf(Inputs[1].Input);

            n2id += index2;

            string compute = "";
            compute += "float " + s1 + " = " + n2id + " * cos(" + n1id + ");\r\n";
            compute += "float " + s2 + " = " + n2id + " * sin(" + n1id + ");\r\n";

            return compute;
        }

        void Process()
        {
            if (input.Input.Data == null || input2.Input.Data == null) return;

            float theta = (float)input.Input.Data;
            float radius = (float)input2.Input.Data;

            theta = (float)(Math.PI / 180.0f) * (theta - 90);


            output.Data = radius * (float)Math.Cos(theta);
            output.Changed();
            output2.Data = radius * (float)Math.Sin(theta);
            output2.Changed();

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
