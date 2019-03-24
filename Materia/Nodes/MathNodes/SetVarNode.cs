using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Materia.Nodes.Attributes;

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

        protected string varName;

        [Title(Title = "Variable Name")]
        [TextInput]
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
                TryAndProcess();
                Updated();
            }
        }

        public SetVarNode(int w, int h, GraphPixelType p = GraphPixelType.RGBA)
        {
            //we ignore w,h,p

            CanPreview = false;

            Name = "Set Var";
            Id = Guid.NewGuid().ToString();
            shaderId = "S" + Id.Split('-')[0];

            input = new NodeInput(NodeType.Bool | NodeType.Float | NodeType.Float2 | NodeType.Float3 | NodeType.Float4, this, "Any");
            output = new NodeOutput(NodeType.Bool | NodeType.Float | NodeType.Float2 | NodeType.Float3 | NodeType.Float4, this);

            Inputs = new List<NodeInput>();
            Inputs.Add(input);

            input.OnInputAdded += Input_OnInputAdded;
            input.OnInputChanged += Input_OnInputChanged;
            input.OnInputRemoved += Input_OnInputRemoved;

            Outputs = new List<NodeOutput>();
            Outputs.Add(output);
        }

        private void Input_OnInputRemoved(NodeInput n)
        {
            if(ParentGraph != null)
            {
                ParentGraph.SetVar(varName, null);
            }

            output.Data = null;
            output.Changed();
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

        void Process()
        {
            if(ParentGraph != null)
            {
                ParentGraph.SetVar(varName, input.Input.Data);
            }

            output.Data = input.Input.Data;
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

        public override string GetShaderPart()
        {
            if (!input.HasInput) return "";
            var s = shaderId + "0";
            var n1id = (input.Input.Node as MathNode).ShaderId;
            var t = input.Input.Type;
            var index = input.Input.Node.Outputs.IndexOf(input.Input);

            n1id += index;

            if(t == NodeType.Float)
            {
                output.Type = NodeType.Float;
                return "float " + varName + " = " + n1id + ";\r\n float " + s + " = " + n1id + ";\r\n";
            }
            else if(t == NodeType.Bool)
            {
                output.Type = NodeType.Bool;
                return "bool " + varName + " = " + n1id + ";\r\n bool " + s + " = " + n1id + ";\r\n";
            }
            else if(t == NodeType.Float2)
            {
                output.Type = NodeType.Float2;
                return "vec2 " + varName + " = " + n1id + ";\r\n vec2 " + s + " = " + n1id + ";\r\n";
            }
            else if(t == NodeType.Float3)
            {
                output.Type = NodeType.Float3;
                return "vec3 " + varName + " = " + n1id + ";\r\n vec3 " + s + " = " + n1id + ";\r\n";
            }
            else if(t == NodeType.Float4)
            {
                output.Type = NodeType.Float4;
                return "vec4 " + varName + " = " + n1id + ";\r\n vec4 " + s + " = " + n1id + ";\r\n";
            }

            return "";
        }


        public override void FromJson(Dictionary<string, Node> nodes, string data)
        {
            VarData d = JsonConvert.DeserializeObject<VarData>(data);
            SetBaseNodeDate(d);
            varName = d.varName;

            SetConnections(nodes, d.outputs);

            TryAndProcess();

            Updated();
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
