using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Materia.Nodes.Attributes;
using Newtonsoft.Json;

namespace Materia.Nodes.MathNodes
{
    public class BooleanConstantNode : MathNode
    {
        NodeOutput output;

        protected bool val;
        public bool True
        {
            get
            {
                return val;
            }
            set
            {
                val = value;
                Updated();
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

        public override void TryAndProcess()
        {
            Process();
        }

        public class BoolConstantData : NodeData
        {
            public bool val;
        }

        public override void FromJson(Dictionary<string, Node> nodes, string data)
        {
            BoolConstantData d = JsonConvert.DeserializeObject<BoolConstantData>(data);
            SetBaseNodeDate(d);
            val = d.val;

            SetConnections(nodes, d.outputs);

            Updated();
        }

        public override string GetJson()
        {
            BoolConstantData d = new BoolConstantData();
            FillBaseNodeData(d);
            d.val = val;

            return JsonConvert.SerializeObject(d);
        }

        public override string GetShaderPart()
        {
            var s = shaderId + "0";

            return "bool " + s + " = " + val.ToString().ToLower() + ";\r\n";
        }

        void Process()
        {
            output.Data = val;
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
    }
}
