using System;
using Materia.Rendering.Attributes;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using Materia.Graph;
using Materia.Graph.IO;

namespace Materia.Nodes.MathNodes
{
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
                TriggerValueChange();
            }
        }

        public SetVarNode(int w, int h, GraphPixelType p = GraphPixelType.RGBA) : base()
        {
            //we ignore w,h,p

            CanPreview = false;

            defaultName = Name = "Set Var";

            shaderId = "S" + Id.Split('-')[0];

            input = new NodeInput(NodeType.Bool | NodeType.Float | NodeType.Float2 | NodeType.Float3 | NodeType.Float4 | NodeType.Matrix, this, "Any");
            output = new NodeOutput(NodeType.Bool | NodeType.Float | NodeType.Float2 | NodeType.Float3 | NodeType.Float4 | NodeType.Matrix, this);

            Inputs.Add(input);
            Outputs.Add(output);
        }

        public override void UpdateOutputType()
        {
            if(input.HasInput)
            {
                output.Type = input.Reference.Type;
            }
        }

        public override string GetDescription()
        {
            return varName;
        }

        public override string GetShaderPart(string currentFrag)
        {
            if (!input.HasInput) return "";
            if (string.IsNullOrEmpty(varName)) return "";
            var s = shaderId + "1";
            var n1id = (input.Reference.Node as MathNode).ShaderId;
            var t = input.Reference.Type;
            var index = input.Reference.Node.Outputs.IndexOf(input.Reference);

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
                string op = "float " + varName + " = " + n1id + ";\r\n float " + s + " = " + n1id + ";\r\n";
                Regex ftest = new Regex(string.Format(FunctionArgTest, "float " + varName), RegexOptions.Multiline);
                if (currentFrag.IndexOf("float " + VarName + " = ") > -1 || ftest.Match(currentFrag).Success)
                {
                    op = varName + " = " + n1id + ";\r\n float " + s + " = " + n1id + ";\r\n";
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
            else if (t == NodeType.Matrix)
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

        public override void TryAndProcess()
        {
            if (!input.IsValid) return;

            UpdateOutputType();
            output.Data = input.Data;
            parentGraph.SetVar(varName, output.Data, output.Type);
            result = output.Data?.ToString();
        }

        public override void Dispose()
        {
            if (ParentGraph != null && !string.IsNullOrEmpty(varName))
            {
                parentGraph.RemoveVar(varName);
            }

            base.Dispose();
        }

        public override void GetBinary(Writer w)
        {
            VarData d = new VarData();
            FillBaseNodeData(d);
            d.varName = varName;
            d.Write(w);
        }

        public override void FromBinary(Reader r)
        {
            VarData d = new VarData();
            d.Parse(r);
            SetBaseNodeDate(d);
            varName = d.varName;
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
