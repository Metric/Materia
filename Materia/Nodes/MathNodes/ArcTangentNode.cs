using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace Materia.Nodes.MathNodes
{
    public class ArcTangentNode : MathNode
    {
        NodeInput input;
        NodeInput input2;
        NodeOutput output;

        public ArcTangentNode(int w, int h, GraphPixelType p = GraphPixelType.RGBA)
        {
            //we ignore w,h,p

            CanPreview = false;

            Name = "Arc Tangent 2";
            Id = Guid.NewGuid().ToString();
            shaderId = "S" + Id.Split('-')[0];

            input = new NodeInput(NodeType.Float, this, "X (Float)");
            input2 = new NodeInput(NodeType.Float, this, "Y (Float)");
        
            output = new NodeOutput(NodeType.Float, this);

            Inputs = new List<NodeInput>();
            Inputs.Add(input);
            Inputs.Add(input2);

            input.OnInputAdded += Input_OnInputAdded;
            input.OnInputChanged += Input_OnInputChanged;

            input2.OnInputAdded += Input_OnInputAdded;
            input2.OnInputChanged += Input_OnInputChanged;

            Outputs = new List<NodeOutput>();
            Outputs.Add(output);
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
            var s = shaderId + "0";
            var n1id = (Inputs[0].Input.Node as MathNode).ShaderId;
            var n2id = (Inputs[1].Input.Node as MathNode).ShaderId;

            var index = Inputs[0].Input.Node.Outputs.IndexOf(Inputs[0].Input);

            n1id += index;

            var index2 = Inputs[1].Input.Node.Outputs.IndexOf(Inputs[1].Input);

            n2id += index2;

            return "float " + s + " = atan(" + n2id + "," + n1id + ");\r\n";
        }

        void Process()
        {
            if (input.Input.Data == null || input2.Input.Data == null) return;

            float x = (float)input.Input.Data;
            float y = (float)input2.Input.Data;

            output.Data = (float)Math.Atan2(y, x);
            output.Changed();

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
