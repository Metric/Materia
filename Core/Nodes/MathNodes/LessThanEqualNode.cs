using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Materia.Nodes.MathNodes
{
    public class LessThanEqualNode : MathNode
    {
        NodeInput input;
        NodeInput input2;
        NodeOutput output;

        public LessThanEqualNode(int w, int h, GraphPixelType p = GraphPixelType.RGBA) : base()
        {
            //we ignore w,h,p

            CanPreview = false;

            Name = "Less Equal";
            Id = Guid.NewGuid().ToString();
            shaderId = "S" + Id.Split('-')[0];

            input = new NodeInput(NodeType.Float, this, "A (Float)");
            input2 = new NodeInput(NodeType.Float, this, "B (Float)");

            output = new NodeOutput(NodeType.Bool, this);

            Inputs.Add(input);
            Inputs.Add(input2);

            input.OnInputAdded += Input_OnInputAdded;
            input.OnInputChanged += Input_OnInputChanged;

            input2.OnInputAdded += Input_OnInputAdded;
            input2.OnInputChanged += Input_OnInputChanged;

            Outputs.Add(output);
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
            if (!input.HasInput || !input2.HasInput) return "";
            var s = shaderId + "1";
            var n1id = (input.Input.Node as MathNode).ShaderId;
            var n2id = (input2.Input.Node as MathNode).ShaderId;

            var index = input.Input.Node.Outputs.IndexOf(input.Input);

            n1id += index;

            var index2 = input2.Input.Node.Outputs.IndexOf(input2.Input);

            n2id += index2;

            return "bool " + s + " = " + n1id + " <= " + n2id + ";\r\n";
        }

        void Process()
        {
            if (input.Input.Data == null || input2.Input.Data == null) return;

            float x = Convert.ToSingle(input.Input.Data);
            float y = Convert.ToSingle(input2.Input.Data);

            output.Data = x <= y;

            result = output.Data.ToString();

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
