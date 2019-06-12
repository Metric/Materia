using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Materia.MathHelpers;

namespace Materia.Nodes.MathNodes
{
    public class RoundNode : MathNode
    {
        NodeInput input;
        NodeOutput output;

        public RoundNode(int w, int h, GraphPixelType p = GraphPixelType.RGBA) : base()
        {
            //we ignore w,h,p

            CanPreview = false;

            Name = "Round";
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
            if(input.HasInput)
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

            NodeType t = input.Input.Type;

            if (t == NodeType.Float)
            {
                return "float " + s + " = round(" + n1id + ");\r\n";
            }
            else if(t == NodeType.Float2)
            {
                return "vec2 " + s + " = round(" + n1id + ");\r\n";
            }
            else if(t == NodeType.Float3)
            {
                return "vec3 " + s + " = round(" + n1id + ");\r\n";
            }
            else if(t == NodeType.Float4)
            {
                return "vec4 " + s + " = round(" + n1id + ");\r\n";
            }

            return "";
        }

        void Process()
        {
            if (input.Input.Data == null) return;

            object o = input.Input.Data;

            if (o is float || o is int || o is double || o is long)
            {
                float v = Convert.ToSingle(o);
                output.Data = (float)Math.Round(v);
            }
            else if(o is MVector)
            {
                MVector m = (MVector)o;
                m.X = (float)Math.Round(m.X);
                m.Y = (float)Math.Round(m.Y);
                m.Z = (float)Math.Round(m.Z);
                m.W = (float)Math.Round(m.W);

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
