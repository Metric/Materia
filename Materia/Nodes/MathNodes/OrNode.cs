using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Materia.Nodes.MathNodes
{
    public class OrNode : MathNode
    {
        NodeOutput output;

        public OrNode(int w, int h, GraphPixelType p = GraphPixelType.RGBA)
        {
            //we ignore w,h,p

            CanPreview = false;

            Name = "Or";
            Id = Guid.NewGuid().ToString();
            shaderId = "S" + Id.Split('-')[0];

            Inputs = new List<NodeInput>();

            output = new NodeOutput(NodeType.Bool, this);


            for (int i = 0; i < 2; i++)
            {
                var input = new NodeInput(NodeType.Bool, this, "Bool Input " + i);
                Inputs.Add(input);

                input.OnInputAdded += Input_OnInputAdded;
                input.OnInputChanged += Input_OnInputChanged;
                input.OnInputRemoved += Input_OnInputRemoved;
            }

            Outputs = new List<NodeOutput>();
            Outputs.Add(output);
        }

        private void Input_OnInputRemoved(NodeInput n)
        {
            var noinputs = Inputs.FindAll(m => !m.HasInput);

            if (noinputs != null && noinputs.Count >= 2 && Inputs.Count > 2)
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

            if (!HasEmptyInput)
            {
                AddPlaceholderInput();
            }
        }

        protected override void AddPlaceholderInput()
        {
            var input = new NodeInput(NodeType.Bool, this, "Bool Input " + Inputs.Count);

            input.OnInputAdded += Input_OnInputAdded;
            input.OnInputChanged += Input_OnInputChanged;
            input.OnInputRemoved += Input_OnInputRemoved;

            Inputs.Add(input);
            AddedInput(input);
        }

        public override string GetShaderPart()
        {
            var s = shaderId + "0";

            string compute = "";
            string sep = "";

            foreach(var inp in Inputs)
            {
                if(inp.HasInput)
                {
                    var index = inp.Input.Node.Outputs.IndexOf(inp.Input);
                    var n1id = (inp.Input.Node as MathNode).ShaderId;

                    n1id += index;

                    compute += sep + n1id;
                    sep = " || ";
                }
            }

            if (string.IsNullOrEmpty(compute)) return "";

            return "bool " + s + " = " + compute + ";\r\n";
        }

        public override void TryAndProcess()
        {
            bool hasInput = false;

            foreach (NodeInput inp in Inputs)
            {
                if (inp.HasInput)
                {
                    hasInput = true;
                    break;
                }
            }

            if (hasInput)
            {
                Process();
            }
        }

        void Process()
        {
            bool v = false;
            foreach (NodeInput inp in Inputs)
            {
                if (inp.HasInput)
                {
                    object o = inp.Input.Data;
                    if (o == null) continue;

                    if (o is bool)
                    {
                        bool f = (bool)o;
                        if (f)
                        {
                            v = true;
                            break;
                        }
                    }
                }
            }

            output.Data = v;
            output.Changed();

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
