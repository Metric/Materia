using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Materia.MathHelpers;
using Materia.Nodes.Helpers;

namespace Materia.Nodes.MathNodes
{
    public class LerpNode : MathNode
    {
        NodeInput input;
        NodeInput input2;
        NodeInput input3;
        NodeOutput output;

        public LerpNode(int w, int h, GraphPixelType p = GraphPixelType.RGBA)
        {
            //we ignore w,h,p

            CanPreview = false;

            Name = "Linear Interpolation";
            Id = Guid.NewGuid().ToString();
            shaderId = "S" + Id.Split('-')[0];

            input = new NodeInput(NodeType.Float | NodeType.Float2 | NodeType.Float3 | NodeType.Float4, this, "From");
            input2 = new NodeInput(NodeType.Float | NodeType.Float2 | NodeType.Float3 | NodeType.Float4, this, "To");
            input3 = new NodeInput(NodeType.Float, this, "Delta");

            output = new NodeOutput(NodeType.Float | NodeType.Float2 | NodeType.Float3 | NodeType.Float4, this);

            Inputs = new List<NodeInput>();
            Inputs.Add(input);
            Inputs.Add(input2);
            Inputs.Add(input3);

            input.OnInputAdded += Input_OnInputAdded;
            input.OnInputChanged += Input_OnInputChanged;

            input2.OnInputAdded += Input_OnInputAdded;
            input2.OnInputChanged += Input_OnInputChanged;

            input3.OnInputAdded += Input_OnInputAdded;
            input3.OnInputChanged += Input_OnInputChanged;

            Outputs = new List<NodeOutput>();
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

        public override string GetShaderPart()
        {
            if (!input.HasInput || !input2.HasInput || !input3.HasInput) return "";
            var s = shaderId + "0";
            var n1id = (input.Input.Node as MathNode).ShaderId;
            var n2id = (input2.Input.Node as MathNode).ShaderId;
            var n3id = (input3.Input.Node as MathNode).ShaderId;

            var index = input.Input.Node.Outputs.IndexOf(input.Input);

            n1id += index;

            var index2 = input2.Input.Node.Outputs.IndexOf(input2.Input);

            n2id += index2;

            var index3 = input3.Input.Node.Outputs.IndexOf(input3.Input);

            n3id += index3;

            if(input.Input.Type == NodeType.Float && input2.Input.Type == NodeType.Float)
            {
                output.Type = NodeType.Float;
                return "float " + s + " = mix(" + n1id + "," + n2id + "," + n3id + ");\r\n"; 
            }
            else if(input.Input.Type == NodeType.Float2 && input2.Input.Type == NodeType.Float2)
            {
                output.Type = NodeType.Float2;
                return "vec2 " + s + " = mix(" + n1id + "," + n2id + "," + n3id + ");\r\n";
            }
            else if (input.Input.Type == NodeType.Float3 && input2.Input.Type == NodeType.Float3)
            {
                output.Type = NodeType.Float3;
                return "vec3 " + s + " = mix(" + n1id + "," + n2id + "," + n3id + ");\r\n";
            }
            else if (input.Input.Type == NodeType.Float4 && input2.Input.Type == NodeType.Float4)
            {
                output.Type = NodeType.Float4;
                return "vec4 " + s + " = mix(" + n1id + "," + n2id + "," + n3id + ");\r\n";
            }

            return "";
        }

        void Process()
        {
            if (input.Input.Data == null || input2.Input.Data == null || input3.Input.Data == null) return;

            object from = input.Input.Data;
            object to = input.Input.Data;
            float delta = (float)input.Input.Data;


            if (from is float && to is MVector)
            {
                MVector f = new MVector((float)from, (float)from, (float)from, (float)from);
                MVector r = MVector.Lerp(f, (MVector)to, delta);
                output.Data = r;
                output.Changed();
            }
            else if (from is float && to is float)
            {
                float r = Utils.Lerp((float)from, (float)to, delta);
                output.Data = r;
                output.Changed();
            }
            else if (from is MVector && to is float)
            {
                MVector f = new MVector((float)from, (float)from, (float)from, (float)from);
                MVector r = MVector.Lerp((MVector)from, f, delta);
                output.Data = r;
                output.Changed();
            }
            else if(from is MVector && to is MVector)
            {
                output.Data = MVector.Lerp((MVector)from, (MVector)to, delta);
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
