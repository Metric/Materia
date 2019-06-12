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

        public ArcTangentNode(int w, int h, GraphPixelType p = GraphPixelType.RGBA) : base()
        {
            //we ignore w,h,p

            CanPreview = false;

            Name = "Arc Tangent 2";
            Id = Guid.NewGuid().ToString();
            shaderId = "S" + Id.Split('-')[0];

            input = new NodeInput(NodeType.Float, this, "X (Float)");
            input2 = new NodeInput(NodeType.Float, this, "Y (Float)");
        
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
            if (!Inputs[1].HasInput || !Inputs[2].HasInput) return "";
            var s = shaderId + "1";
            var n1id = (Inputs[1].Input.Node as MathNode).ShaderId;
            var n2id = (Inputs[2].Input.Node as MathNode).ShaderId;

            var index = Inputs[1].Input.Node.Outputs.IndexOf(Inputs[1].Input);

            n1id += index;

            var index2 = Inputs[2].Input.Node.Outputs.IndexOf(Inputs[2].Input);

            n2id += index2;

            return "float " + s + " = atan(" + n2id + "," + n1id + ");\r\n";
        }

        void Process()
        {
            if (input.Input.Data == null || input2.Input.Data == null) return;

            float x = Convert.ToSingle(input.Input.Data);
            float y = Convert.ToSingle(input2.Input.Data);

            output.Data = (float)Math.Atan2(y, x);

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
