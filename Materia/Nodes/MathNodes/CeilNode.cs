using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Materia.MathHelpers;

namespace Materia.Nodes.MathNodes
{
    public class CeilNode : MathNode
    {
        NodeInput input;
        NodeOutput output;

        public CeilNode(int w, int h, GraphPixelType p = GraphPixelType.RGBA) : base()
        {
            //we ignore w,h,p

            CanPreview = false;

            Name = "Ceil";
            Id = Guid.NewGuid().ToString();
            shaderId = "S" + Id.Split('-')[0];

            input = new NodeInput(NodeType.Float | NodeType.Float2 | NodeType.Float3 | NodeType.Float4, this, "Any Float Input");
            output = new NodeOutput(NodeType.Float | NodeType.Float2 | NodeType.Float3 | NodeType.Float4, this);

            Inputs.Add(input);

            input.OnInputAdded += Input_OnInputAdded;
            input.OnInputChanged += Input_OnInputChanged;

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
            if (input.HasInput)
            {
                output.Type = input.Input.Type;
            }
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
                output.Type = NodeType.Float4;
                return "vec4 " + s + " = ceil(" + n1id + ");\r\n";
            }
            else if (input.Input.Type == NodeType.Float3)
            {
                output.Type = NodeType.Float3;
                return "vec3 " + s + " = ceil(" + n1id + ");\r\n";
            }
            else if (input.Input.Type == NodeType.Float2)
            {
                output.Type = NodeType.Float2;
                return "vec2 " + s + " = ceil(" + n1id + ");\r\n";
            }
            else if (input.Input.Type == NodeType.Float)
            {
                output.Type = NodeType.Float;
                return "float " + s + " = ceil(" + n1id + ");\r\n";
            }

            return "";
        }

        void Process()
        {
            if (input.Input.Data == null) return;

            object o = input.Input.Data;

            if (o is float || o is int)
            {
                float v = (float)o;
                output.Data = (float)Math.Ceiling(v);
                if (Outputs.Count > 0)
                {
                    Outputs[0].Changed();
                }
            }
            else if (o is MVector)
            {
                MVector v = (MVector)o;
                MVector d = new MVector();
                d.X = (float)Math.Ceiling(v.X);
                d.Y = (float)Math.Ceiling(v.Y);
                d.Z = (float)Math.Ceiling(v.Z);
                d.W = (float)Math.Ceiling(v.W);

                output.Data = d;
                if (Outputs.Count > 0)
                {
                    Outputs[0].Changed();
                }
            }
            else
            {
                output.Data = 0;
                if (Outputs.Count > 0)
                {
                    Outputs[0].Changed();
                }
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
