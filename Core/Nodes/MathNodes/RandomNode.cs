using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Materia.MathHelpers;
using Materia.Nodes.Helpers;

namespace Materia.Nodes.MathNodes
{
    public class RandomNode : MathNode
    {
        NodeInput input;
        NodeOutput output;

        public RandomNode(int w, int h, GraphPixelType p = GraphPixelType.RGBA) : base()
        {
            //we ignore w,h,p

            CanPreview = false;

            Name = "Random";
            Id = Guid.NewGuid().ToString();
            shaderId = "S" + Id.Split('-')[0];

            input = new NodeInput(NodeType.Float | NodeType.Float2, this, "Float Input");
            output = new NodeOutput(NodeType.Float, this);

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

            int seed = 0;

            if (ParentGraph != null)
            {
                seed = ParentGraph.RandomSeed;
            }

            var s = shaderId + "1";
            var n1id = (input.Input.Node as MathNode).ShaderId;

            var index = input.Input.Node.Outputs.IndexOf(input.Input);

            n1id += index;

            if (input.Input.Type == NodeType.Float2)
            {
                return "float " + s + " = rand(" + n1id + " + " + seed.ToCodeString() + ");\r\n";
            }
            else
            { 
                return "float " + s + " = rand(vec2(" + n1id + ", 1.0 - " + n1id + ") + " + seed.ToCodeString() + ");\r\n";
            }
        }

        void Process()
        {
            if (input.Input.Data == null) return;

            object o = input.Input.Data;

            float seed = 0;

            if(ParentGraph != null)
            {
                seed = ParentGraph.RandomSeed;
            }

            if (o is float || o is int || o is double || o is long)
            {
                float v = Convert.ToSingle(o);
                MVector v2 = new MVector(v, 1.0f - v) + seed;
                output.Data = Utils.Rand(ref v2);
            }
            else if(o is MVector)
            {
                MVector v = (MVector)o + seed;
                output.Data = Utils.Rand(ref v);
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
