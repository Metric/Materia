using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Materia.Nodes.MathNodes
{
    public class PowNode : MathNode
    {
        NodeInput input;
        NodeInput input2;
        NodeOutput output;

        public PowNode(int w, int h, GraphPixelType p = GraphPixelType.RGBA) : base()
        {
            //we ignore w,h,p

            CanPreview = false;

            Name = "Pow";
            Id = Guid.NewGuid().ToString();
            shaderId = "S" + Id.Split('-')[0];

            input = new NodeInput(NodeType.Float, this, "Value");
            input2 = new NodeInput(NodeType.Float, this, "Raise");
            output = new NodeOutput(NodeType.Float, this);

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
            if (!input.HasInput) return "";
            var s = shaderId + "1";
            var n1id = (input.Input.Node as MathNode).ShaderId;
            var n2id = (input2.Input.Node as MathNode).ShaderId;

            var index = input.Input.Node.Outputs.IndexOf(input.Input);
            var index2 = input2.Input.Node.Outputs.IndexOf(input2.Input);

            n1id += index;
            n2id += index2;

            return "float " + s + " = pow(" + n1id + ", " + n2id + ");\r\n";
        }

        void Process()
        {
            if (input.Input.Data == null || input2.Input.Data == null) return;

            object o = input.Input.Data;
            object o2 = input2.Input.Data;

            if ((o is float || o is int || o is double || o is long) && (o2 is float || o2 is int || o2 is double || o2 is long))
            {
                float v = Convert.ToSingle(o);
                float v2 = Convert.ToSingle(o2);

                output.Data = (float)Math.Pow(v, v2);
            }
            else
            {
                output.Data = 0;
            }

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
