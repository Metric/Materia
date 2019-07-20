using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Materia.MathHelpers;

namespace Materia.Nodes.MathNodes
{
    public class BreakFloat4Node : MathNode
    {
        NodeInput input;
        NodeOutput output;
        NodeOutput output2;
        NodeOutput output3;
        NodeOutput output4;

        public BreakFloat4Node(int w, int h, GraphPixelType p = GraphPixelType.RGBA) : base()
        {
            //we ignore w,h,p

            CanPreview = false;

            Name = "Break Float4";
            Id = Guid.NewGuid().ToString();
            shaderId = "S" + Id.Split('-')[0];

            input = new NodeInput(NodeType.Float4, this, "Float4 Type");
            output = new NodeOutput(NodeType.Float, this, "X");
            output2 = new NodeOutput(NodeType.Float, this, "Y");
            output3 = new NodeOutput(NodeType.Float, this, "Z");
            output4 = new NodeOutput(NodeType.Float, this, "W");

            Inputs.Add(input);

            input.OnInputAdded += Input_OnInputAdded;
            input.OnInputChanged += Input_OnInputChanged;

            Outputs.Add(output);
            Outputs.Add(output2);
            Outputs.Add(output3);
            Outputs.Add(output4);
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
            if (input.HasInput)
            {
                Process();
            }
        }

        public override string GetShaderPart(string currentFrag)
        {
            if (!Inputs[1].HasInput) return "";
            var s1 = shaderId + "1";
            var s2 = shaderId + "2";
            var s3 = shaderId + "3";
            var s4 = shaderId + "4";

            var n1id = (Inputs[1].Input.Node as MathNode).ShaderId;

            var index = Inputs[1].Input.Node.Outputs.IndexOf(Inputs[1].Input);

            n1id += index;

            string compute = "";
            compute += "float " + s1 + " = " + n1id + ".x;\r\n";
            compute += "float " + s2 + " = " + n1id + ".y;\r\n";
            compute += "float " + s3 + " = " + n1id + ".z;\r\n";
            compute += "float " + s4 + " = " + n1id + ".w;\r\n";

            return compute;
        }

        void Process()
        {
            if (input.Input.Data == null) return;

            MVector v = (MVector)input.Input.Data;

            output.Data = v.X;
            output2.Data = v.Y;
            output3.Data = v.Z;
            output4.Data = v.W;
        }
    }
}
