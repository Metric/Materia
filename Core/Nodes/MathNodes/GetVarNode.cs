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

        [Dropdown("VarName", true)]
        [Editable(ParameterInputType.Dropdown, "Variable Name")]
        public string[] AvailableVariables
        {
            get
            {
                if(ParentGraph != null)
                {
                    return ParentGraph.GetAvailableVariables(output.Type);
                }

                return new string[0];
            }
        }

        public string VarName
        {
            get
            {
                return varName;
            }
            set
            {
                varName = value;
                TriggerValueChange();
            }
        }

        private void TryAndUpdateVarName()
        {
            if (!string.IsNullOrEmpty(varName))
            {
                if (varName.StartsWith(GraphParameterValue.CODE_PREFIX))
                {
                    string cvar = varName.Replace(GraphParameterValue.CODE_PREFIX, GraphParameterValue.CUSTOM_CODE_PREFIX);
                    if (parentGraph != null && !parentGraph.HasVar(varName) && parentGraph.HasVar(cvar))
                    {
                        varName = cvar;
                    }
                }
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
                 prefix = "float ";
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

        public override void TryAndProcess()
        {
            output.Data = parentGraph.GetVar(VarName);
            result = output.Data?.ToString();
        }

        public override void FromJson(string data)
        {
            VarData d = JsonConvert.DeserializeObject<VarData>(data);
            SetBaseNodeDate(d);
            varName = d.varName;
            TryAndUpdateVarName();
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
