using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Materia.Nodes.Attributes;
using Materia.Nodes.Helpers;
using Newtonsoft.Json;

namespace Materia.Nodes.MathNodes
{
    public class BooleanConstantNode : MathNode
    {
        NodeOutput output;

        protected bool val;
        [Editable(ParameterInputType.Toggle, "True")]
        public bool True
        {
            get
            {
                return val;
            }
            set
            {
                val = value;
                TriggerValueChange();
            }
        }

        public BooleanConstantNode(int w, int h, GraphPixelType p = GraphPixelType.RGBA)
        {
            //we ignore w,h,p

            val = false;

            CanPreview = false;

            Name = "Boolean Constant";
            Id = Guid.NewGuid().ToString();
            shaderId = "S" + Id.Split('-')[0];

            output = new NodeOutput(NodeType.Bool, this);

            Inputs = new List<NodeInput>();

            Outputs = new List<NodeOutput>();
            Outputs.Add(output);
        }

        public override string GetDescription()
        {
            return val.ToString();
        }

        public class BoolConstantData : NodeData
        {
            public bool val;
        }

        public override void FromJson(string data)
        {
            BoolConstantData d = JsonConvert.DeserializeObject<BoolConstantData>(data);
            SetBaseNodeDate(d);
            val = d.val;
        }

        public override string GetJson()
        {
            BoolConstantData d = new BoolConstantData();
            FillBaseNodeData(d);
            d.val = val;

            return JsonConvert.SerializeObject(d);
        }

        public override string GetShaderPart(string currentFrag)
        {
            var s = shaderId + "0";

            bool t = val;

            return "float " + s + " = " + (t ? 1 : 0 ) + ";\r\n";
        }

        public override void TryAndProcess()
        {
            output.Data = val ? 1 : 0;
        }
    }
}
