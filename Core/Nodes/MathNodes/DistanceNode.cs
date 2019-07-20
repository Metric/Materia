using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Materia.MathHelpers;

namespace Materia.Nodes.MathNodes
{
    public class DistanceNode : MathNode
    {
        NodeOutput output;

        public DistanceNode(int w, int h, GraphPixelType p = GraphPixelType.RGBA) : base()
        {
            //we ignore w,h,p

            CanPreview = false;

            Name = "Distance";
            Id = Guid.NewGuid().ToString();
            shaderId = "S" + Id.Split('-')[0];

            output = new NodeOutput(NodeType.Float, this);

            for (int i = 0; i < 2; i++)
            {
                var input = new NodeInput(NodeType.Float2 | NodeType.Float3 | NodeType.Float4, this, "Vector Input " + i);
                Inputs.Add(input);

                input.OnInputAdded += Input_OnInputAdded;
                input.OnInputChanged += Input_OnInputChanged;

            }
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

        protected override void AddPlaceholderInput()
        {
            var input = new NodeInput(NodeType.Float2 | NodeType.Float3 | NodeType.Float4, this, "Vector Input " + Inputs.Count);

            input.OnInputAdded += Input_OnInputAdded;
            input.OnInputChanged += Input_OnInputChanged;

            Inputs.Add(input);
            AddedInput(input);
        }

        public override void TryAndProcess()
        {
            if(Inputs[1].HasInput && Inputs[2].HasInput)
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

            var t1 = Inputs[1].Input.Type;
            var t2 = Inputs[2].Input.Type;

            if (t1 == NodeType.Float2 && t2 == NodeType.Float2)
            {
                output.Type = NodeType.Float;
                return "float " + s + " = distance(" + n1id + ", " + n2id + ");\r\n";
            }
            else if (t1 == NodeType.Float3 && t2 == NodeType.Float3)
            {
                output.Type = NodeType.Float;
                return "float " + s + " = distance(" + n1id + ", " + n2id + ");\r\n";
            }
            else if (t1 == NodeType.Float4 && t2 == NodeType.Float4)
            {
                output.Type = NodeType.Float;
                return "float " + s + " = distance(" + n1id + ", " + n2id + ");\r\n";
            }

            return "float " + s + " = 0;\r\n";
        }

        void Process()
        {
            object d1 = Inputs[1].Input.Data;
            object d2 = Inputs[2].Input.Data;

            if (d1 == null || d2 == null) return;

            var t1 = Inputs[1].Input.Type;
            var t2 = Inputs[2].Input.Type;

            if((t1 == NodeType.Float2 && t2 == NodeType.Float2)
                || (t1 == NodeType.Float3 && t2 == NodeType.Float3)
                || (t1 == NodeType.Float4 && t2 == NodeType.Float4))
            {
                MVector f1 = (MVector)d1;
                MVector f2 = (MVector)d2;

                output.Data = (f1 - f2).Length;
            }
            else
            {
                output.Data = 0;
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
