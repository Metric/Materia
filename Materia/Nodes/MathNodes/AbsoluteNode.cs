using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Materia.MathHelpers;

namespace Materia.Nodes.MathNodes
{
    public class AbsoluteNode : MathNode
    {
        NodeInput input;
        NodeOutput output;

        public AbsoluteNode(int w, int h, GraphPixelType p = GraphPixelType.RGBA)
        {
            //we ignore w,h,p

            CanPreview = false;

            Name = "Absolute";
            Id = Guid.NewGuid().ToString();
            shaderId = "S" + Id.Split('-')[0];

            input = new NodeInput(NodeType.Float | NodeType.Float2 | NodeType.Float3 | NodeType.Float4, this, "Any Float Type");
            output = new NodeOutput(NodeType.Float | NodeType.Float2 | NodeType.Float3 | NodeType.Float4, this);

            Inputs = new List<NodeInput>();
            Inputs.Add(input);

            input.OnInputAdded += Input_OnInputAdded;
            input.OnInputChanged += Input_OnInputChanged;
            input.OnInputRemoved += Input_OnInputRemoved;

            Outputs = new List<NodeOutput>();
            Outputs.Add(output);
        }

        private void Input_OnInputRemoved(NodeInput n)
        {
            Updated();
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
            if(input.HasInput)
            {
                Process();
            }
        }

        public override string GetShaderPart()
        {
            if (!input.HasInput) return "";
            var s = shaderId + "0";
            var n1id = (input.Input.Node as MathNode).ShaderId;

            var index = input.Input.Node.Outputs.IndexOf(input.Input);

            n1id += index;

            if (input.Input.Type == NodeType.Float4)
            {
                output.Type = NodeType.Float4;
                return "vec4 " + s + " = abs(" + n1id + ");\r\n";
            }
            else if (input.Input.Type == NodeType.Float3)
            {
                output.Type = NodeType.Float3;
                return "vec2 " + s + " = abs(" + n1id + ");\r\n";
            }
            else if (input.Input.Type == NodeType.Float2)
            {
                output.Type = NodeType.Float2;
                return "vec2 " + s + " = abs(" + n1id + ");\r\n";
            }
            else if (input.Input.Type == NodeType.Float)
            {
                output.Type = NodeType.Float;
                return "float " + s + " = abs(" + n1id + ");\r\n";
            }

            return "";
        }

        void Process()
        {
            if (input.Input.Data == null) return;

            object o = input.Input.Data;

            if(o is float || o is int)
            {
                float v = (float)o;

                output.Data = Math.Abs(v);
                output.Changed();
            }
            else if(o is MVector)
            {
                MVector v = (MVector)o;
                MVector d = new MVector();
                d.X = Math.Abs(v.X);
                d.Y = Math.Abs(v.Y);
                d.Z = Math.Abs(v.Z);
                d.W = Math.Abs(v.W);

                output.Data = d;
                output.Changed();
            }
            else
            {
                output.Data = 0;
                output.Changed();
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
