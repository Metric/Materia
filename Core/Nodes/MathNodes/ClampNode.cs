using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Materia.MathHelpers;

namespace Materia.Nodes.MathNodes
{
    public class ClampNode : MathNode
    {
        NodeInput input;
        NodeInput input2;
        NodeInput input3;
        NodeOutput output;

        public ClampNode(int w, int h, GraphPixelType p = GraphPixelType.RGBA) : base()
        {
            //we ignore w,h,p

            CanPreview = false;

            Name = "Clamp";
            Id = Guid.NewGuid().ToString();
            shaderId = "S" + Id.Split('-')[0];

            input = new NodeInput(NodeType.Float | NodeType.Float2 | NodeType.Float3 | NodeType.Float4, this, "Value");
            input2 = new NodeInput(NodeType.Float | NodeType.Float2 | NodeType.Float3 | NodeType.Float4, this, "Min");
            input3 = new NodeInput(NodeType.Float | NodeType.Float2 | NodeType.Float3 | NodeType.Float4, this, "Max");
            output = new NodeOutput(NodeType.Float | NodeType.Float2 | NodeType.Float3 | NodeType.Float4, this);

            Inputs.Add(input);
            Inputs.Add(input2);
            Inputs.Add(input3);

            input.OnInputAdded += Input_OnInputAdded;
            input.OnInputChanged += Input_OnInputChanged;

            input2.OnInputAdded += Input_OnInputAdded;
            input2.OnInputChanged += Input_OnInputChanged;

            input3.OnInputAdded += Input_OnInputAdded;
            input3.OnInputChanged += Input_OnInputChanged;

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
            if (input.HasInput && input2.HasInput && input3.HasInput)
            {
                Process();
            }
        }

        public override string GetShaderPart(string currentFrag)
        {
            if (!input.HasInput || !input2.HasInput || !input3.HasInput) return "";
            var s = shaderId + "1";
            var n1id = (input.Input.Node as MathNode).ShaderId;
            var n2id = (input2.Input.Node as MathNode).ShaderId;
            var n3id = (input3.Input.Node as MathNode).ShaderId;

            var index = input.Input.Node.Outputs.IndexOf(input.Input);
            var index2 = input2.Input.Node.Outputs.IndexOf(input2.Input);
            var index3 = input3.Input.Node.Outputs.IndexOf(input3.Input);

            n1id += index;
            n2id += index2;
            n3id += index3;

            if (input.Input.Type == NodeType.Float4)
            {
                return "vec4 " + s + " = clamp(" + n1id + "," + n2id + "," + n3id +");\r\n";
            }
            else if (input.Input.Type == NodeType.Float3)
            {
                return "vec3 " + s + " = clamp(" + n1id + "," + n2id + "," + n3id + ");\r\n";
            }
            else if (input.Input.Type == NodeType.Float2)
            {
                return "vec2 " + s + " = clamp(" + n1id + "," + n2id + "," + n3id + ");\r\n";
            }
            else if (input.Input.Type == NodeType.Float)
            {
                return "float " + s + " = clamp(" + n1id + "," + n2id + "," + n3id + ");\r\n";
            }

            return "";
        }

        void Process()
        {
            if (input.Input.Data == null || input2.Input.Data == null || input3.Input.Data == null) return;

            object o = input.Input.Data;
            object o2 = input2.Input.Data;
            object o3 = input3.Input.Data;

            if (Helpers.Utils.IsNumber(o) && Helpers.Utils.IsNumber(o2) && Helpers.Utils.IsNumber(o3))
            {
                float v = Convert.ToSingle(o);
                float min = Convert.ToSingle(o2);
                float max = Convert.ToSingle(o3);

                output.Data = (float)Math.Min(max, Math.Max(min, v));
            }
            else if (o is MVector && o2 is MVector && o3 is MVector)
            {
                MVector v = (MVector)o;
                MVector min = (MVector)o2;
                MVector max = (MVector)o3;

                MVector d = new MVector();
                d.X = (float)Math.Min(max.X, Math.Max(min.X, v.X));
                d.Y = (float)Math.Min(max.Y, Math.Max(min.Y, v.Y));
                d.Z = (float)Math.Min(max.Z, Math.Max(min.Z, v.Z));
                d.W = (float)Math.Min(max.W, Math.Max(min.W, v.W));

                output.Data = d;
            }
            else if(o is MVector && Helpers.Utils.IsNumber(o2) && Helpers.Utils.IsNumber(o3))
            {
                MVector v = (MVector)o;
                float min = Convert.ToSingle(o2);
                float max = Convert.ToSingle(o3);
                MVector d = new MVector();

                d.X = (float)Math.Min(max, Math.Max(min, v.X));
                d.Y = (float)Math.Min(max, Math.Max(min, v.Y));
                d.Z = (float)Math.Min(max, Math.Max(min, v.Z));
                d.W = (float)Math.Min(max, Math.Max(min, v.W));

                output.Data = d;
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
