using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Materia.Nodes.Attributes;
using Newtonsoft.Json;

namespace Materia.Nodes.MathNodes
{
    public class GetVarNode : MathNode
    {
        protected NodeOutput output;

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
                if (!string.IsNullOrEmpty(varName))
                {
                    if (ParentGraph != null)
                    {
                        ParentGraph.RemoveVar(varName);
                    }
                }

                varName = value;
                TryAndProcess();
                Updated();
            }
        }

        public GetVarNode(int w, int h, GraphPixelType p = GraphPixelType.RGBA)
        {
            //we ignore w,h,p

            CanPreview = false;

            Name = "Get Var";
            Id = Guid.NewGuid().ToString();
            shaderId = "S" + Id.Split('-')[0];

            output = new NodeOutput(NodeType.Bool | NodeType.Float | NodeType.Float2 | NodeType.Float3 | NodeType.Float4, this);

            Inputs = new List<NodeInput>();

            Outputs = new List<NodeOutput>();
            Outputs.Add(output);
        }

        public override void TryAndProcess()
        {
            Process();
        }

        public override string GetShaderPart()
        {
            var s = shaderId + "0";

            if (output.Type == NodeType.Float)
            {
                return "float " + s + " = " + varName + ";\r\n";
            }
            else if(output.Type == NodeType.Float2)
            {
                return "vec2 " + s + " = " + varName + ";\r\n";
            }
            else if(output.Type == NodeType.Float3)
            {
                return "vec3 " + s + " = " + varName + ";\r\n";
            }
            else if(output.Type == NodeType.Float4)
            {
                return "vec4 " + s + " = " + varName + ";\r\n";
            }

            return "";
        }

        protected virtual void Process()
        {
            object d = null;
            if (ParentGraph != null)
            {
                d = ParentGraph.GetVar(varName);
            }

            output.Data = d;
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

        public override void FromJson(Dictionary<string, Node> nodes, string data)
        {
            VarData d = JsonConvert.DeserializeObject<VarData>(data);
            SetBaseNodeDate(d);
            varName = d.varName;

            SetConnections(nodes, d.outputs);

            TryAndProcess();
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
