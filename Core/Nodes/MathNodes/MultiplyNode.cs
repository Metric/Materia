using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Materia.MathHelpers;
using Materia.Math3D;
using Materia.Nodes.Helpers;

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
     
            output = new NodeOutput(NodeType.Float | NodeType.Float2 | NodeType.Float3 | NodeType.Float4 | NodeType.Matrix, this);

            for (int i = 0; i < 2; i++)
            {
                var input = new NodeInput(NodeType.Float | NodeType.Float2 | NodeType.Float3 | NodeType.Float4 | NodeType.Matrix, this, "Input " + i);
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
                else if((t1 == NodeType.Matrix && t2 == NodeType.Float2) || (t1 == NodeType.Float2 && t2 == NodeType.Matrix))
                {
                    output.Type = NodeType.Float2;
                }
                else if((t1 == NodeType.Matrix && t2 == NodeType.Float3) || (t1 == NodeType.Float3 && t2 == NodeType.Matrix))
                {
                    output.Type = NodeType.Float3;
                }
                else if ((t1 == NodeType.Matrix && t2 == NodeType.Float4) || (t1 == NodeType.Float4 && t2 == NodeType.Matrix))
                {
                    output.Type = NodeType.Float4;
                }
                else if(t1 == NodeType.Matrix && t2 == NodeType.Matrix)
                { 
                    output.Type = NodeType.Matrix;
                }
            }
        }

        protected override void AddPlaceholderInput()
        {
            var input = new NodeInput(NodeType.Float | NodeType.Float2 | NodeType.Float3 | NodeType.Float4 | NodeType.Matrix, this, "Input " + Inputs.Count);

            input.OnInputAdded += Input_OnInputAdded;
            input.OnInputChanged += Input_OnInputChanged;
            input.OnInputRemoved += Input_OnInputRemoved;

            Inputs.Add(input);
            AddedInput(input);
        }

        public override void TryAndProcess()
        {
            int validInputs = 0;

            foreach (NodeInput inp in Inputs)
            {
                if (inp != executeInput)
                {
                    if (inp.HasInput)
                    {
                        validInputs++;
                    }
                }
            }

            if (validInputs >= 2)
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
                return "float " + s + " = " + n1id + " * " + n2id + ";\r\n";
            }
            else if ((t1 == NodeType.Float && t2 == NodeType.Float2) || (t1 == NodeType.Float2 && t2 == NodeType.Float))
            {
                return "vec2 " + s + " = " + n1id + " * " + n2id + ";\r\n";
            }
            else if ((t1 == NodeType.Float && t2 == NodeType.Float3) || (t1 == NodeType.Float3 && t2 == NodeType.Float))
            {
                return "vec3 " + s + " = " + n1id + " * " + n2id + ";\r\n";
            }
            else if ((t1 == NodeType.Float && t2 == NodeType.Float4) || (t1 == NodeType.Float4 && t2 == NodeType.Float))
            {
                return "vec4 " + s + " = " + n1id + " * " + n2id + ";\r\n";
            }
            else if (t1 == NodeType.Float2 && t2 == NodeType.Float2)
            {
                return "vec2 " + s + " = " + n1id + " * " + n2id + ";\r\n";
            }
            else if (t1 == NodeType.Float3 && t2 == NodeType.Float3)
            {
                return "vec3 " + s + " = " + n1id + " * " + n2id + ";\r\n";
            }
            else if (t1 == NodeType.Float4 && t2 == NodeType.Float4)
            {
                return "vec4 " + s + " = " + n1id + " * " + n2id + ";\r\n";
            }
            else if(t1 == NodeType.Float2 && t2 == NodeType.Matrix)
            {
                return "vec2 " + s + " = (vec4(" + n1id + ",0,1) * " + n2id + ").xy;\r\n";
            }
            else if (t1 == NodeType.Matrix && t2 == NodeType.Float2)
            {
                return "vec2 " + s + " = (" + n1id + " * vec4(" + n2id + ",0,1)).xy;\r\n"; 
            }
            else if (t1 == NodeType.Float3 && t2 == NodeType.Matrix)
            {
                return "vec3 " + s + " = (vec4(" + n1id + ",1) * " + n2id + ").xyz;\r\n";
            }
            else if (t1 == NodeType.Matrix && t2 == NodeType.Float3)
            {
                return "vec3 " + s + " = (" + n1id + " * vec4(" + n2id + ",1)).xyz;\r\n";
            }
            else if ((t1 == NodeType.Float4 && t2 == NodeType.Matrix) || (t1 == NodeType.Matrix && t2 == NodeType.Float4))
            {
                return "vec4 " + s + " = " + n1id + " * " + n2id + ";\r\n";
            }
            else if(t1 == NodeType.Matrix && t2 == NodeType.Matrix)
            {
                return "mat4 " + s + " = " + n1id + " * " + n2id + ";\r\n";
            }

            return "";
        }

        void Process()
        {
            bool matrixOnly = true;
            bool hasMatrix = false;
            bool hasVector = false;
            bool hasSingle = false;

            foreach (NodeInput inp in Inputs)
            {
                if (inp != executeInput)
                {
                    if (inp.HasInput)
                    {
                        if (inp.Input.Data is MVector)
                        {
                            matrixOnly = false;
                            hasVector = true;
                        }
                        else if(Utils.IsNumber(inp.Input.Data))
                        {
                            hasSingle = true;
                            matrixOnly = false;
                        }
                        else if(inp.Input.Data is Matrix4)
                        {
                            hasMatrix = true;
                        }
                    }
                }
            }

            if(hasVector && hasMatrix)
            {
                object i1 = Inputs[1].Input.Data;
                object i2 = Inputs[2].Input.Data;
                NodeType t1 = Inputs[1].Input.Type;
                NodeType t2 = Inputs[2].Input.Type;

                 if(i1 is MVector && i2 is Matrix4)
                {
                    MVector v = (MVector)i1;

                    if(t1 == NodeType.Float2 || t1 == NodeType.Float3)
                    {
                        v.W = 1;
                    } 

                    Vector4 v2 = new Vector4(v.X, v.Y, v.Z, v.W);
                    Matrix4 m2 = (Matrix4)i2;

                    v2 = v2 * m2;

                    output.Data = new MVector(v2.X, v2.Y, v2.Z, v2.W);
                }
                else if(i1 is Matrix4 && i2 is MVector)
                {
                    MVector v = (MVector)i2;

                    if(t2 == NodeType.Float2 || t2 == NodeType.Float3)
                    {
                        v.W = 1;
                    }

                    Vector4 v2 = new Vector4(v.X, v.Y, v.Z, v.W);
                    Matrix4 m2 = (Matrix4)i1;

                    v2 = m2 * v2;

                    output.Data = new MVector(v2.X, v2.Y, v2.Z, v2.W);
                }
                else
                {
                    output.Data = i1;
                }
            }
            else if(hasMatrix && matrixOnly)
            {
                object i1 = Inputs[1].Input.Data;
                object i2 = Inputs[2].Input.Data;

                if(i1 is Matrix4 && i1 is Matrix4)
                {
                    Matrix4 m1 = (Matrix4)i1;
                    Matrix4 m2 = (Matrix4)i2;

                    output.Data = m1 * m2;
                }
                else
                {
                    output.Data = i1;
                }
            }
            else if(hasMatrix && hasSingle)
            {
                object i1 = Inputs[1].Input.Data;
                object i2 = Inputs[2].Input.Data;

                if(Utils.IsNumber(i1) && i2 is Matrix4)
                {
                    float f = Convert.ToSingle(i1);
                    Matrix4 m2 = (Matrix4)i2;

                    output.Data = f * m2;
                }
                else if(i1 is Matrix4 && Utils.IsNumber(i2))
                {
                    float f = Convert.ToSingle(i2);
                    Matrix4 m2 = (Matrix4)i1;

                    output.Data = m2 * f;
                }
                else
                {
                    output.Data = i1;
                }
            }
            else if (hasVector)
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

                            if (Utils.IsNumber(o))
                            {
                                if (i == 0)
                                {
                                    float f = Convert.ToSingle(o);
                                    v.X = v.Y = v.Z = v.W = f;
                                }
                                else
                                {
                                    float f = Convert.ToSingle(o);
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

                            i++;
                        }
                    }
                }

                output.Data = v;
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

                            if (Utils.IsNumber(o))
                            {
                                if (i == 0)
                                {
                                    v = Convert.ToSingle(o);
                                }
                                else
                                {
                                    float f = Convert.ToSingle(o);
                                    v *= f;
                                }
                            }

                            i++;
                        }
                    }
                }

                output.Data = v;
            }

            if (output.Data != null)
            {
                result = output.Data.ToString();
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
