using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Materia.Nodes.Attributes;
using System.Text.RegularExpressions;

namespace Materia.Nodes.MathNodes
{
    public class VarData : Node.NodeData
    {
        public string varName;
    }

    public class SetVarNode : MathNode
    {
        NodeInput input;
        NodeOutput output;

        private static string FunctionArgTest = "[A-Za-z0-9_]+\\(.*{0}.*\\)";

        protected string varName;

        [Editable(ParameterInputType.Text, "Variable Name")]
        public string VarName
        {
            get
            {
                return varName;
            }
            set
            {
                if(!string.IsNullOrEmpty(varName))
                {
                    if(ParentGraph != null)
                    {
                        ParentGraph.RemoveVar(varName);
                    }
                }

                varName = value;
                OnDescription(varName);
                TryAndProcess();
                Updated();
            }
        }

        public SetVarNode(int w, int h, GraphPixelType p = GraphPixelType.RGBA) : base()
        {
            //we ignore w,h,p

            CanPreview = false;

            Name = "Set Var";
            Id = Guid.NewGuid().ToString();
            shaderId = "S" + Id.Split('-')[0];

            input = new NodeInput(NodeType.Bool | NodeType.Float | NodeType.Float2 | NodeType.Float3 | NodeType.Float4, this, "Any");
            output = new NodeOutput(NodeType.Bool | NodeType.Float | NodeType.Float2 | NodeType.Float3 | NodeType.Float4, this);

            Inputs.Add(input);

            input.OnInputAdded += Input_OnInputAdded;
            input.OnInputChanged += Input_OnInputChanged;
            input.OnInputRemoved += Input_OnInputRemoved;

            Outputs.Add(output);
        }

        private void Input_OnInputRemoved(NodeInput n)
        {
            if(ParentGraph != null && !string.IsNullOrEmpty(varName))
            {
                ParentGraph.SetVar(varName, null);
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
            if(input.HasInput)
            {
                output.Type = input.Input.Type;
            }
        }

        public override string GetDescription()
        {
            return varName;
        }

        public override void TryAndProcess()
        {
            if (input.HasInput)
            {
                Process();
            }
        }

        void Process()
        {
            if (string.IsNullOrEmpty(varName)) return;

            if(ParentGraph != null)
            {
                ParentGraph.SetVar(varName, input.Input.Data);
            }

            output.Data = input.Input.Data;
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

        public override string GetShaderPart(string currentFrag)
        {
            if (!input.HasInput) return "";
            if (string.IsNullOrEmpty(varName)) return "";
            var s = shaderId + "1";
            var n1id = (input.Input.Node as MathNode).ShaderId;
            var t = input.Input.Type;
            var index = input.Input.Node.Outputs.IndexOf(input.Input);

            n1id += index;

            if(t == NodeType.Float)
            {
                string op = "float " + varName + " = " + n1id + ";\r\n float " + s + " = " + n1id + ";\r\n";
                Regex ftest = new Regex(string.Format(FunctionArgTest, "float " + varName), RegexOptions.Multiline);
                if(currentFrag.IndexOf("float " + VarName + " = ") > -1 || ftest.Match(currentFrag).Success)
                {
                    op = varName + " = " + n1id + ";\r\n float " + s + " = " + n1id + ";\r\n";
                }
                return op;
            }
            else if(t == NodeType.Bool)
            {
                string op = "bool " + varName + " = " + n1id + ";\r\n bool " + s + " = " + n1id + ";\r\n";
                Regex ftest = new Regex(string.Format(FunctionArgTest, "bool " + varName), RegexOptions.Multiline);
                if (currentFrag.IndexOf("bool " + VarName + " = ") > -1 || ftest.Match(currentFrag).Success)
                {
                    op = varName + " = " + n1id + ";\r\n bool " + s + " = " + n1id + ";\r\n";
                }
                return op;
            }
            else if(t == NodeType.Float2)
            {
                string op = "vec2 " + varName + " = " + n1id + ";\r\n vec2 " + s + " = " + n1id + ";\r\n";
                Regex ftest = new Regex(string.Format(FunctionArgTest, "vec2 " + varName), RegexOptions.Multiline);
                if (currentFrag.IndexOf("vec2 " + VarName + " = ") > -1 || ftest.Match(currentFrag).Success)
                {
                    op = varName + " = " + n1id + ";\r\n vec2 " + s + " = " + n1id + ";\r\n";
                }
                return op;
            }
            else if(t == NodeType.Float3)
            {
                string op = "vec3 " + varName + " = " + n1id + ";\r\n vec3 " + s + " = " + n1id + ";\r\n";
                Regex ftest = new Regex(string.Format(FunctionArgTest, "vec3 " + varName), RegexOptions.Multiline);
                if (currentFrag.IndexOf("vec3 " + VarName + " = ") > -1 || ftest.Match(currentFrag).Success)
                {
                    op = varName + " = " + n1id + ";\r\n vec3 " + s + " = " + n1id + ";\r\n";
                }
                return op;
            }
            else if(t == NodeType.Float4)
            {
                string op = "vec4 " + varName + " = " + n1id + ";\r\n vec4 " + s + " = " + n1id + ";\r\n";
                Regex ftest = new Regex(string.Format(FunctionArgTest, "vec4 " + varName), RegexOptions.Multiline);
                if (currentFrag.IndexOf("vec4 " + VarName + " = ") > -1 || ftest.Match(currentFrag).Success)
                {
                    op = varName + " = " + n1id + ";\r\n vec4 " + s + " = " + n1id + ";\r\n";
                }
                return op;
            }
            else if(t == NodeType.Matrix2)
            {
                string op = "mat2 " + varName + " = " + n1id + ";\r\n mat2 " + s + " = " + n1id + ";\r\n";
                Regex ftest = new Regex(string.Format(FunctionArgTest, "mat2 " + varName), RegexOptions.Multiline);
                if (currentFrag.IndexOf("mat2 " + VarName + " = ") > -1 || ftest.Match(currentFrag).Success)
                {
                    op = varName + " = " + n1id + ";\r\n mat2 " + s + " = " + n1id + ";\r\n";
                }
                return op;
            }
            else if (t == NodeType.Matrix3)
            {
                string op = "mat3 " + varName + " = " + n1id + ";\r\n mat3 " + s + " = " + n1id + ";\r\n";
                Regex ftest = new Regex(string.Format(FunctionArgTest, "mat3 " + varName), RegexOptions.Multiline);
                if (currentFrag.IndexOf("mat3 " + VarName + " = ") > -1 || ftest.Match(currentFrag).Success)
                {
                    op = varName + " = " + n1id + ";\r\n mat3 " + s + " = " + n1id + ";\r\n";
                }
                return op;
            }
            else if (t == NodeType.Matrix4)
            {
                string op = "mat4 " + varName + " = " + n1id + ";\r\n mat4 " + s + " = " + n1id + ";\r\n";
                Regex ftest = new Regex(string.Format(FunctionArgTest, "mat4 " + varName), RegexOptions.Multiline);
                if (currentFrag.IndexOf("mat4 " + VarName + " = ") > -1 || ftest.Match(currentFrag).Success)
                {
                    op = varName + " = " + n1id + ";\r\n mat4 " + s + " = " + n1id + ";\r\n";
                }
                return op;
            }

            return "";
        }

        public override void Dispose()
        {
            if (ParentGraph != null)
            {
                parentGraph.RemoveVar(varName);
            }

            base.Dispose();
        }

        public override void FromJson(string data)
        {
            VarData d = JsonConvert.DeserializeObject<VarData>(data);
            SetBaseNodeDate(d);
            varName = d.varName;
        }

        public override string GetJson()
        {
            VarData d = new VarData();
            FillBaseNodeData(d);
            d.varName = varName;

            return JsonConvert.SerializeObject(d);
        }
    }
}
