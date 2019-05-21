using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Materia.MathHelpers;

namespace Materia.Nodes.MathNodes
{
    public class MultiplyNode : MathNode
    {
        NodeOutput output;

        public MultiplyNode(int w, int h, GraphPixelType p = GraphPixelType.RGBA) : base()
        {
            //we ignore w,h,p

            CanPreview = false;

            Name = "Multiply";
            Id = Guid.NewGuid().ToString();
            shaderId = "S" + Id.Split('-')[0];
     
            output = new NodeOutput(NodeType.Float | NodeType.Float2 | NodeType.Float3 | NodeType.Float4, this);

            for (int i = 0; i < 2; i++)
            {
                var input = new NodeInput(NodeType.Float | NodeType.Float2 | NodeType.Float3 | NodeType.Float4, this, "Any Float Input " + i);
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
            UpdateOutputType();
            Updated();
        }

        public override void UpdateOutputType()
        {
            if (Inputs.Count == 0) return;
            if (Inputs[1].HasInput && Inputs[2].HasInput)
            {
                NodeType t1 = Inputs[1].Input.Type;
                NodeType t2 = Inputs[2].Input.Type;

                if (t1 == NodeType.Float && t2 == NodeType.Float)
                {
                    output.Type = NodeType.Float;
                }
                else if ((t1 == NodeType.Float && t2 == NodeType.Float2) || (t1 == NodeType.Float2 && t2 == NodeType.Float))
                {
                    output.Type = NodeType.Float2;
                }
                else if ((t1 == NodeType.Float && t2 == NodeType.Float3) || (t1 == NodeType.Float3 && t2 == NodeType.Float))
                {
                    output.Type = NodeType.Float3;
                }
                else if ((t1 == NodeType.Float && t2 == NodeType.Float4) || (t1 == NodeType.Float4 && t2 == NodeType.Float))
                {
                    output.Type = NodeType.Float4;
                }
                else if (t1 == NodeType.Float2 && t2 == NodeType.Float2)
                {
                    output.Type = NodeType.Float2;
                }
                else if (t1 == NodeType.Float3 && t2 == NodeType.Float3)
                {
                    output.Type = NodeType.Float3;
                }
                else if (t1 == NodeType.Float4 && t2 == NodeType.Float4)
                {
                    output.Type = NodeType.Float4;
                }
            }
        }

        protected override void AddPlaceholderInput()
        {
            var input = new NodeInput(NodeType.Float | NodeType.Float2 | NodeType.Float3 | NodeType.Float4, this, "Any Float Input " + Inputs.Count);

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

            var t1 = Inputs[1].Input.Type;
            var t2 = Inputs[2].Input.Type;

            if (t1 == NodeType.Float && t2 == NodeType.Float)
            {
                output.Type = NodeType.Float;
                return "float " + s + " = " + n1id + " * " + n2id + ";\r\n";
            }
            else if ((t1 == NodeType.Float && t2 == NodeType.Float2) || (t1 == NodeType.Float2 && t2 == NodeType.Float))
            {
                output.Type = NodeType.Float2;
                return "vec2 " + s + " = " + n1id + " * " + n2id + ";\r\n";
            }
            else if ((t1 == NodeType.Float && t2 == NodeType.Float3) || (t1 == NodeType.Float3 && t2 == NodeType.Float))
            {
                output.Type = NodeType.Float3;
                return "vec3 " + s + " = " + n1id + " * " + n2id + ";\r\n";
            }
            else if ((t1 == NodeType.Float && t2 == NodeType.Float4) || (t1 == NodeType.Float4 && t2 == NodeType.Float))
            {
                output.Type = NodeType.Float4;
                return "vec4 " + s + " = " + n1id + " * " + n2id + ";\r\n";
            }
            else if (t1 == NodeType.Float2 && t2 == NodeType.Float2)
            {
                output.Type = NodeType.Float2;
                return "vec2 " + s + " = " + n1id + " * " + n2id + ";\r\n";
            }
            else if (t1 == NodeType.Float3 && t2 == NodeType.Float3)
            {
                output.Type = NodeType.Float3;
                return "vec3 " + s + " = " + n1id + " * " + n2id + ";\r\n";
            }
            else if (t1 == NodeType.Float4 && t2 == NodeType.Float4)
            {
                output.Type = NodeType.Float4;
                return "vec4 " + s + " = " + n1id + " * " + n2id + ";\r\n";
            }

            return "";
        }

        void Process()
        {
            bool hasVector = false;

            foreach (NodeInput inp in Inputs)
            {
                if (inp != executeInput)
                {
                    if (inp.HasInput)
                    {
                        if (inp.Input.Data is MVector)
                        {
                            hasVector = true;
                            break;
                        }
                    }
                }
            }

            if (hasVector)
            {
                MVector v = new MVector();

                int i = 0;
                foreach (NodeInput inp in Inputs)
                {
                    if (inp != executeInput)
                    {
                        if (inp.HasInput)
                        {
                            object o = inp.Input.Data;
                            if (o == null) continue;

                            if (o is float || o is int)
                            {
                                if (i == 0)
                                {
                                    float f = (float)o;
                                    v.X = v.Y = v.Z = v.W = f;
                                }
                                else
                                {
                                    float f = (float)o;
                                    v.X *= f;
                                    v.Y *= f;
                                    v.Z *= f;
                                    v.W *= f;
                                }
                            }
                            else if (o is MVector)
                            {
                                if (i == 0)
                                {
                                    var d = (MVector)o;
                                    v.X = d.X;
                                    v.Y = d.Y;
                                    v.Z = d.Z;
                                    v.W = d.W;
                                }
                                else
                                {
                                    MVector f = (MVector)o;
                                    v.X *= f.X;
                                    v.Y *= f.Y;
                                    v.Z *= f.Z;
                                    v.W *= f.W;
                                }
                            }
                        }

                        i++;
                    }
                }

                output.Data = v;
                if (Outputs.Count > 0)
                {
                    Outputs[0].Changed();
                }
            }
            else
            {
                float v = 0;
                int i = 0;
                foreach (NodeInput inp in Inputs)
                {
                    if (inp != executeInput)
                    {
                        if (inp.HasInput)
                        {
                            object o = inp.Input.Data;
                            if (o == null) continue;

                            if (o is float || o is int)
                            {
                                if (i == 0)
                                {
                                    v = (float)o;
                                }
                                else
                                {
                                    float f = (float)o;
                                    if (f == 0) continue;
                                    v *= f;
                                }
                            }
                        }

                        i++;
                    }
                }

                output.Data = v;
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
