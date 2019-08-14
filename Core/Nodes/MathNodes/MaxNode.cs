using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Materia.Nodes.MathNodes
{
    public class MaxNode : MathNode
    {
        NodeOutput output;

        public MaxNode(int w, int h, GraphPixelType p = GraphPixelType.RGBA) : base()
        {
            //we ignore w,h,p

            CanPreview = false;

            Name = "Max";
            Id = Guid.NewGuid().ToString();
            shaderId = "S" + Id.Split('-')[0];

            output = new NodeOutput(NodeType.Float, this);


            for (int i = 0; i < 2; i++)
            {
                var input = new NodeInput(NodeType.Float, this, "Float Input " + i);
                Inputs.Add(input);

                input.OnInputAdded += Input_OnInputAdded;
                input.OnInputChanged += Input_OnInputChanged;
                input.OnInputRemoved += Input_OnInputRemoved;
            }

            Outputs.Add(output);
        }

        private void Input_OnInputRemoved(NodeInput n)
        {
            var noinputs = Inputs.FindAll(m => !m.HasInput);

            if (noinputs != null && noinputs.Count >= 3 && Inputs.Count > 3)
            {
                var inp = noinputs[noinputs.Count - 1];

                inp.OnInputChanged -= Input_OnInputChanged;
                inp.OnInputRemoved -= Input_OnInputRemoved;
                inp.OnInputAdded -= Input_OnInputAdded;

                Inputs.Remove(inp);
                RemovedInput(inp);
            }
        }

        private void Input_OnInputChanged(NodeInput n)
        {
            TryAndProcess();
        }

        private void Input_OnInputAdded(NodeInput n)
        {
            Updated();
        }

        protected override void AddPlaceholderInput()
        {
            var input = new NodeInput(NodeType.Float, this, "Float Input " + Inputs.Count);

            input.OnInputAdded += Input_OnInputAdded;
            input.OnInputChanged += Input_OnInputChanged;
            input.OnInputRemoved += Input_OnInputRemoved;

            Inputs.Add(input);
            AddedInput(input);
        }

        public override void TryAndProcess()
        {
            bool hasInput = false;

            foreach (NodeInput inp in Inputs)
            {
                if (inp != executeInput)
                {
                    if (inp.HasInput)
                    {
                        hasInput = true;
                        break;
                    }
                }
            }

            if (hasInput)
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

            return "float " + s + " = max(" + n1id + "," + n2id + ");\r\n";
        }

        void Process()
        {
            float v = 0;

            foreach (NodeInput inp in Inputs)
            {
                if (inp != executeInput)
                {
                    if (inp.HasInput)
                    {
                        object o = inp.Input.Data;
                        if (o == null) continue;

                        if (o is float || o is int || o is double || o is long)
                        {
                            float f = Convert.ToSingle(o);
                            v = Math.Max(f, v);
                        }
                    }
                }
            }

            output.Data = v;

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
