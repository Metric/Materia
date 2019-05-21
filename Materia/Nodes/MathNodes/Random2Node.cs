using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Materia.MathHelpers;

namespace Materia.Nodes.MathNodes
{
    public class Random2Node : MathNode
    {
        NodeInput input;
        NodeInput input2;
        NodeOutput output;

        public Random2Node(int w, int h, GraphPixelType p = GraphPixelType.RGBA) : base()
        {
            //we ignore w,h,p

            CanPreview = false;

            Name = "Random2";
            Id = Guid.NewGuid().ToString();
            shaderId = "S" + Id.Split('-')[0];

            input = new NodeInput(NodeType.Float2, this, "Float2 Input");
            input2 = new NodeInput(NodeType.Float, this, "Float Input");
            output = new NodeOutput(NodeType.Float, this);

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
            Updated();
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

            int seed = 0;

            if (ParentGraph != null)
            {
                seed = ParentGraph.RandomSeed;
            }

            var s = shaderId + "1";
            var n1id = (input.Input.Node as MathNode).ShaderId;
            var n2id = (input2.Input.Node as MathNode).ShaderId;

            var index = input.Input.Node.Outputs.IndexOf(input.Input);
            var index2 = input2.Input.Node.Outputs.IndexOf(input2.Input);

            n1id += index;
            n2id += index2;
            
            return "float " + s + " = rand(vec2(rand(" + n1id + " + " + seed + ")," + n2id + ") + " + seed + ");\r\n";
        }

        float fract(float f)
        {
            return f - (float)Math.Floor(f);
        }

        float rand(ref MVector vec2)
        {
            return fract((float)Math.Sin(MVector.Dot(vec2, new MVector(12.9898f, 78.233f))) * 43758.5453f);
        }

        void Process()
        {
            if (input.Input.Data == null || input2.Input.Data == null) return;

            float seed = 0;

            if(ParentGraph != null)
            {
                seed = ParentGraph.RandomSeed;
            }

            object o = input.Input.Data;
            object o2 = input2.Input.Data;

            if (o is MVector && (o2 is float || o2 is int || o2 is double))
            {
                MVector v = (MVector)o + seed;
                MVector v2 = new MVector(rand(ref v), Convert.ToSingle(o2)) + seed;
                output.Data = rand(ref v2);
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
