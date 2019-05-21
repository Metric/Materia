using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Materia.Nodes.MathNodes
{
    public class IfElseNode : MathNode
    {
        NodeInput input;
        NodeInput input2;
        NodeInput input3;
        NodeOutput output;

        public IfElseNode(int w, int h, GraphPixelType p = GraphPixelType.RGBA) : base()
        {
            //we ignore w,h,p

            CanPreview = false;

            Name = "Equal";
            Id = Guid.NewGuid().ToString();
            shaderId = "S" + Id.Split('-')[0];

            input = new NodeInput(NodeType.Bool, this, "Comparison");
            input2 = new NodeInput(NodeType.Float | NodeType.Float2 | NodeType.Float3 | NodeType.Float4, this, "If");
            input3 = new NodeInput(NodeType.Float | NodeType.Float2 | NodeType.Float3 | NodeType.Float4, this, "Else");

            output = new NodeOutput(NodeType.Float | NodeType.Float2 | NodeType.Float3 | NodeType.Float4, this);

            Inputs.Add(input);
            Inputs.Add(input2);
            Inputs.Add(input3);

            input.OnInputAdded += Input_OnInputAdded;
            input.OnInputChanged += Input_OnInputChanged;

            input2.OnInputAdded += Input_OnInputAdded;
            input2.OnInputChanged += Input_OnInputChanged;

            input3.OnInputAdded += Input_OnInputAdded;
            input3.OnInputChanged += Input_OnInputChanged;

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
            if (Inputs.Count == 0) return;
            if (Inputs[2].HasInput && Inputs[3].HasInput)
            {
                NodeType t1 = Inputs[2].Input.Type;
                NodeType t2 = Inputs[3].Input.Type;

                if (t1 == NodeType.Float && t2 == NodeType.Float)
                {
                    output.Type = NodeType.Float;
                }
                else if (t1 == NodeType.Float2 && t2 == NodeType.Float2)
                {
                    output.Type = NodeType.Float2;
                }
                else if (t1 == NodeType.Float3 && t2 == NodeType.Float3)
                {
                    output.Type = NodeType.Float3;
                }
                else if (t1 == NodeType.Float4 && t2 == NodeType.Float4)
                {
                    output.Type = NodeType.Float4;
                }
            }
        }

        public override void TryAndProcess()
        {
            if (input.HasInput && input2.HasInput && input3.HasInput)
            {
                Process();
            }
        }

        public override string GetShaderPart(string currentFrag)
        {
            if (!input.HasInput || !input2.HasInput || !input3.HasInput) return "";
            var s = shaderId + "1";
            var n1id = (input.Input.Node as MathNode).ShaderId;
            var n2id = (input2.Input.Node as MathNode).ShaderId;
            var n3id = (input3.Input.Node as MathNode).ShaderId;

            var index = input.Input.Node.Outputs.IndexOf(input.Input);

            n1id += index;

            var index2 = input2.Input.Node.Outputs.IndexOf(input2.Input);

            n2id += index2;

            var index3 = input3.Input.Node.Outputs.IndexOf(input3.Input);

            n3id += index3;


            string compute = ""; 

            if(input2.Input.Type == NodeType.Float && input3.Input.Type == NodeType.Float)
            {
                output.Type = NodeType.Float;
                compute = "float " + s + ";\r\n" + " if(" + n1id + ") { \r\n";
                compute += s + " = " + n2id + ";\r\n} else {\r\n";
                compute += s + " = " + n3id + ";}\r\n";
            }
            else if(input2.Input.Type == NodeType.Float2 && input3.Input.Type == NodeType.Float2)
            {
                output.Type = NodeType.Float2;
                compute = "vec2 " + s + ";\r\n" + "if(" + n1id + ") { \r\n";
                compute += s + " = " + n2id + ";\r\n} else {\r\n";
                compute += s + " = " + n3id + ";}\r\n";
            }
            else if (input2.Input.Type == NodeType.Float3 && input3.Input.Type == NodeType.Float3)
            {
                output.Type = NodeType.Float3;
                compute = "vec3 " + s + ";\r\n" + "if(" + n1id + ") { \r\n";
                compute += s + " = " + n2id + ";\r\n} else {\r\n";
                compute += s + " = " + n3id + ";}\r\n";
            }
            else if (input2.Input.Type == NodeType.Float4 && input3.Input.Type == NodeType.Float4)
            {
                output.Type = NodeType.Float4;
                compute = "vec4 " + s + ";\r\n" + "if(" + n1id + ") { \r\n";
                compute += s + " = " + n2id + ";\r\n} else {\r\n";
                compute += s + " = " + n3id + ";}\r\n";
            }

            return compute;
        }

        void Process()
        {
            if (input2.Input.Data == null && input3.Input.Data == null) return;

            if (input2.Input.Type != input3.Input.Type) return;

            bool c = false;

            if (input.Input.Data != null && input.Input.Data is bool) c = (bool)input.Input.Data;

            output.Data = c ? input2.Input.Data : input3.Input.Data;
            if (Outputs.Count > 0)
            {
                Outputs[0].Changed();
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
