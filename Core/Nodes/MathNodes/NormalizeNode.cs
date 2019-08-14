using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Materia.MathHelpers;

namespace Materia.Nodes.MathNodes
{
    public class NormalizeNode : MathNode
    {
        NodeInput input;
        NodeOutput output;

        public NormalizeNode(int w, int h, GraphPixelType p = GraphPixelType.RGBA) : base()
        {
            //we ignore w,h,p

            CanPreview = false;

            Name = "Normalize";
            Id = Guid.NewGuid().ToString();
            shaderId = "S" + Id.Split('-')[0];

            input = new NodeInput(NodeType.Float2 | NodeType.Float3 | NodeType.Float4, this, "Any Vector Type");
            output = new NodeOutput(NodeType.Float2 | NodeType.Float3 | NodeType.Float4, this);

            Inputs.Add(input);

            input.OnInputAdded += Input_OnInputAdded;
            input.OnInputChanged += Input_OnInputChanged;
            Outputs.Add(output);
        }

        private void Input_OnInputChanged(NodeInput n)
        {
            TryAndProcess();
        }

        public override void UpdateOutputType()
        {
            if (input.HasInput)
            {
                output.Type = input.Input.Type;
            }
        }

        private void Input_OnInputAdded(NodeInput n)
        {
            UpdateOutputType();
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
            if (!input.HasInput) return "";
            var s = shaderId + "1";
            var n1id = (input.Input.Node as MathNode).ShaderId;

            var index = input.Input.Node.Outputs.IndexOf(input.Input);

            n1id += index;

            if (input.Input.Type == NodeType.Float4)
            {
                return "vec4 " + s + " = normalize(" + n1id + ");\r\n";
            }
            else if (input.Input.Type == NodeType.Float3)
            {
                return "vec3 " + s + " = normalize(" + n1id + ");\r\n";
            }
            else if (input.Input.Type == NodeType.Float2)
            {
                return "vec2 " + s + " = normalize(" + n1id + ");\r\n";
            }
            else if (input.Input.Type == NodeType.Float)
            {
                return "float " + s + " = normalize(" + n1id + ");\r\n";
            }

            return "";
        }

        void Process()
        {
            if (input.Input.Data == null) return;

            object o = input.Input.Data;

            if (o is MVector)
            {
                MVector v = (MVector)o;
                MVector d = v.Normalized;

                output.Data = d;
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
