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
                OnDescription(varName);
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

            Outputs = new List<NodeOutput>();
            Inputs = new List<NodeInput>();

            output = new NodeOutput(NodeType.Bool | NodeType.Float | NodeType.Float2 | NodeType.Float3 | NodeType.Float4, this);
            Outputs.Add(output);
        }

        public override string GetDescription()
        {
            return varName;
        }

        public override void TryAndProcess()
        {
            Process();
        }

        public override string GetShaderPart(string currentFrag)
        {
            var s = shaderId + "0";

            string prefix = "";

            if (output.Type == NodeType.Float)
            {
                 prefix = "float ";
            }
            else if(output.Type == NodeType.Float2)
            {
                 prefix = "vec2 ";
            }
            else if(output.Type == NodeType.Float3)
            {
                 prefix = "vec3 ";
            }
            else if(output.Type == NodeType.Float4)
            {
                 prefix = "vec4 ";
            }
            else if(output.Type == NodeType.Bool)
            {
                 prefix = "bool ";
            }

           
            if (currentFrag.IndexOf(prefix + s + " = ") > -1)
            {
                prefix = s + " = " + varName + ";\r\n";
            }
            else
            {
                prefix += s + " = " + varName + ";\r\n";
            }

            return prefix;
        }

        protected virtual void Process()
        {
            object d = null;
            if (ParentGraph != null)
            {
                d = ParentGraph.GetVar(varName);
            }

            output.Data = d;

            if (ParentGraph != null)
            {
                FunctionGraph g = (FunctionGraph)ParentGraph;

                if (g != null && g.OutputNode == this)
                {
                    g.Result = output.Data;
                }
            }
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
