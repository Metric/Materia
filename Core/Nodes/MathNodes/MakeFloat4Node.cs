using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Materia.MathHelpers;

namespace Materia.Nodes.MathNodes
{
    public class MakeFloat4Node : MathNode
    {
        NodeInput input;
        NodeInput input2;
        NodeInput input3;
        NodeInput input4;
        NodeOutput output;

        MVector vec;

        public MakeFloat4Node(int w, int h, GraphPixelType p = GraphPixelType.RGBA) : base()
        {
            //we ignore w,h,p

            CanPreview = false;

            Name = "Make Float4";
            Id = Guid.NewGuid().ToString();
            shaderId = "S" + Id.Split('-')[0];

            vec = new MVector();

            input = new NodeInput(NodeType.Float, this, "X (Float)");
            input2 = new NodeInput(NodeType.Float, this, "Y (Float)");
            input3 = new NodeInput(NodeType.Float, this, "Z (Float)");
            input4 = new NodeInput(NodeType.Float, this, "W (Float)");

            output = new NodeOutput(NodeType.Float4, this);

            Inputs.Add(input);
            Inputs.Add(input2);
            Inputs.Add(input3);
            Inputs.Add(input4);

            input.OnInputAdded += Input_OnInputAdded;
            input.OnInputChanged += Input_OnInputChanged;

            input2.OnInputAdded += Input_OnInputAdded;
            input2.OnInputChanged += Input_OnInputChanged;

            input3.OnInputAdded += Input_OnInputAdded;
            input3.OnInputChanged += Input_OnInputChanged;

            input4.OnInputAdded += Input_OnInputAdded;
            input4.OnInputChanged += Input_OnInputChanged;

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
            if (input.HasInput && input2.HasInput && input3.HasInput && input4.HasInput)
            {
                Process();
            }
        }

        public override string GetShaderPart(string currentFrag)
        {
            if (!input.HasInput || !input2.HasInput || !input3.HasInput || !input4.HasInput) return "";
            var s = shaderId + "1";
            var n1id = (input.Input.Node as MathNode).ShaderId;
            var n2id = (input2.Input.Node as MathNode).ShaderId;
            var n3id = (input3.Input.Node as MathNode).ShaderId;
            var n4id = (input4.Input.Node as MathNode).ShaderId;

            var index = input.Input.Node.Outputs.IndexOf(input.Input);

            n1id += index;

            var index2 = input2.Input.Node.Outputs.IndexOf(input2.Input);

            n2id += index2;

            var index3 = input3.Input.Node.Outputs.IndexOf(input3.Input);

            n3id += index3;

            var index4 = input4.Input.Node.Outputs.IndexOf(input4.Input);

            n4id += index4;

            return "vec4 " + s + " = vec4(" + n1id + "," + n2id + "," + n3id + "," + n4id + ");\r\n";
        }

        void Process()
        {
            if (input.Input.Data == null || input2.Input.Data == null || input3.Input.Data == null || input4.Input.Data == null) return;

            if (!Helpers.Utils.IsNumber(input.Input.Data))
            {
                return;
            }
            if (!Helpers.Utils.IsNumber(input2.Input.Data))
            {
                return;
            }
            if (!Helpers.Utils.IsNumber(input3.Input.Data))
            {
                return;
            }
            if (!Helpers.Utils.IsNumber(input4.Input.Data))
            {
                return;
            }

            float x = Convert.ToSingle(input.Input.Data);
            float y = Convert.ToSingle(input2.Input.Data);
            float z = Convert.ToSingle(input3.Input.Data);
            float w = Convert.ToSingle(input4.Input.Data);

            vec.X = x;
            vec.Y = y;
            vec.Z = z;
            vec.W = w;

            output.Data = vec;

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
