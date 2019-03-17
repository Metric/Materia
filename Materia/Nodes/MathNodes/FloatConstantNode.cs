using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Materia.Nodes.MathNodes
{
    public class FloatConstantNode : MathNode
    {
        NodeOutput output;

        protected float val;
        public float Value
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

        public FloatConstantNode(int w, int h, GraphPixelType p = GraphPixelType.RGBA)
        {
            //we ignore w,h,p

            val = 0;

            CanPreview = false;

            Name = "Float Constant";
            Id = Guid.NewGuid().ToString();
            shaderId = "S" + Id.Split('-')[0];

            output = new NodeOutput(NodeType.Float, this);

            Inputs = new List<NodeInput>();

            Outputs = new List<NodeOutput>();
            Outputs.Add(output);
        }

        public override void TryAndProcess()
        {
            Process();
        }

        public class FloatConstantData : NodeData
        {
            public float val;
        }

        public override void FromJson(Dictionary<string, Node> nodes, string data)
        {
            FloatConstantData d = JsonConvert.DeserializeObject<FloatConstantData>(data);
            SetBaseNodeDate(d);
            val = d.val;

            SetConnections(nodes, d.outputs);

            Updated();
        }

        public override string GetJson()
        {
            FloatConstantData d = new FloatConstantData();
            FillBaseNodeData(d);
            d.val = val;

            return JsonConvert.SerializeObject(d);
        }

        public override string GetShaderPart()
        {
            var s = shaderId + "0";

            return "float " + s + " = " + val + ";\r\n";
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
