using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Materia.Nodes.MathNodes
{
    public class PolarNode : MathNode
    {
        NodeInput input;
        NodeInput input2;
        NodeOutput output;
        NodeOutput output2;

        public PolarNode(int w, int h, GraphPixelType p = GraphPixelType.RGBA) : base()
        {
            //we ignore w,h,p

            CanPreview = false;

            Name = "Polar";
            Id = Guid.NewGuid().ToString();
            shaderId = "S" + Id.Split('-')[0];

            input = new NodeInput(NodeType.Float, this, "X Float Input");
            input2 = new NodeInput(NodeType.Float, this, "Y Float Input");
            output = new NodeOutput(NodeType.Float, this, "Radius Output");
            output2 = new NodeOutput(NodeType.Float, this, "Angle Output");

            Inputs.Add(input);
            Inputs.Add(input2);

            input.OnInputAdded += Input_OnInputAdded;
            input.OnInputChanged += Input_OnInputChanged;

            input2.OnInputAdded += Input_OnInputAdded;
            input2.OnInputChanged += Input_OnInputChanged;

            Outputs.Add(output);
            Outputs.Add(output2);
        }

        private void Input_OnInputChanged(NodeInput n)
        {
            TryAndProcess();
        }

        private void Input_OnInputAdded(NodeInput n)
        {
            Updated();
        }

        public override void TryAndProcess()
        {
            if (input.HasInput && input2.HasInput)
            {
                Process();
            }
        }

        public override string GetShaderPart(string currentFrag)
        {
            if (!Inputs[0].HasInput || !Inputs[1].HasInput) return "";
            var s1 = shaderId + "1";
            var s2 = shaderId + "2";

            var n1id = (Inputs[0].Input.Node as MathNode).ShaderId;
            var n2id = (Inputs[1].Input.Node as MathNode).ShaderId;

            var index = Inputs[0].Input.Node.Outputs.IndexOf(Inputs[0].Input);

            n1id += index;

            var index2 = Inputs[1].Input.Node.Outputs.IndexOf(Inputs[1].Input);

            n2id += index2;

            string compute = "";
            compute += "float " + s1 + " = sqrt(" + n1id + " * " + n1id + " + " + n2id + " * " + n2id + ");\r\n";
            compute += "float " + s2 + " = tan(" + n2id + " / " + n1id + ") + PI;\r\n";

            return compute;
        }

        void Process()
        {
            if (input.Input.Data == null || input2.Input.Data == null) return;

            float x = Convert.ToSingle(input.Input.Data);
            float y = Convert.ToSingle(input2.Input.Data);

            float radius = (float)Math.Sqrt(x * x + y * y);
            float theta = (float)Math.Tan(y / x) + (float)Math.PI;

            output.Data = radius;
            output2.Data = theta;

            result = $"{radius},{theta}";

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
