using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Materia.MathHelpers;

namespace Materia.Nodes.MathNodes
{
    public class ModuloNode : MathNode
    {
        NodeInput input;
        NodeInput input2;
        NodeOutput output;

        public ModuloNode(int w, int h, GraphPixelType p = GraphPixelType.RGBA) : base()
        {
            //we ignore w,h,p

            CanPreview = false;

            Name = "Modulo";
            Id = Guid.NewGuid().ToString();
            shaderId = "S" + Id.Split('-')[0];

            input = new NodeInput(NodeType.Float | NodeType.Float2 | NodeType.Float3 | NodeType.Float4, this, "Any Float Input");
            input2 = new NodeInput(NodeType.Float, this, "Mod (Float) Input");

            output = new NodeOutput(NodeType.Float | NodeType.Float2 | NodeType.Float3 | NodeType.Float4, this);

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
            UpdateOutputType();
            Updated();
        }

        public override void UpdateOutputType()
        {
            if(input.HasInput)
            {
                output.Type = input.Input.Type;
            }
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

            var type = input.Input.Type;

            if (type == NodeType.Float)
            {
                return "float " + s + " = mod(" + n1id + "," + n2id + ");\r\n";
            }
            else if(type == NodeType.Float2)
            {
                return "vec2 " + s + " = mod(" + n1id + "," + n2id + ");\r\n";
            }
            else if (type == NodeType.Float3)
            {
                return "vec3 " + s + " = mod(" + n1id + "," + n2id + ");\r\n";
            }
            else if (type == NodeType.Float4)
            {
                return "vec4 " + s + " = mod(" + n1id + "," + n2id + ");\r\n";
            }

            return "";
        }

        void Process()
        {
            if (input.Input.Data == null || input2.Input.Data == null) return;

            object x = input.Input.Data;
            float y = Convert.ToSingle(input2.Input.Data);

            if (x is float || x is int || x is double || x is long)
            {
                output.Data = Convert.ToSingle(x) % y;
            }
            else if(x is MVector)
            {
                MVector m = (MVector)x;

                m.X = m.X % y;
                m.Y = m.Y % y;
                m.Z = m.Z % y;
                m.W = m.W % y;

                output.Data = m;
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
