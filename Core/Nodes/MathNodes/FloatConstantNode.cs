using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Materia.Nodes.Attributes;

namespace Materia.Nodes.MathNodes
{
    public class FloatConstantNode : MathNode
    {
        NodeOutput output;

        protected float val;
        [Promote(NodeType.Float)]
        [Editable(ParameterInputType.FloatInput, "Value")]
        public float Value
        {
            get
            {
                return val;
            }
            set
            {
                val = value;
                OnDescription(string.Format("{0:0.000}", val));
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

        public override string GetDescription()
        {
            return string.Format("{0:0.000}", val);
        }

        public override void TryAndProcess()
        {
            Process();
        }

        public class FloatConstantData : NodeData
        {
            public float val;
        }

        public override void FromJson(string data)
        {
            FloatConstantData d = JsonConvert.DeserializeObject<FloatConstantData>(data);
            SetBaseNodeDate(d);
            val = d.val;
        }

        public override string GetJson()
        {
            FloatConstantData d = new FloatConstantData();
            FillBaseNodeData(d);
            d.val = val;

            return JsonConvert.SerializeObject(d);
        }

        public override string GetShaderPart(string currentFrag)
        {
            var s = shaderId + "0";

            float v = val;

            var p = TopGraph();
            if (p != null && p.HasParameterValue(Id, "Value"))
            {
                v = Convert.ToSingle(p.GetParameterValue(Id, "Value"));
            }

            return "float " + s + " = " + v + ";\r\n";
        }

        void Process()
        {
            float v = val;

            var p = TopGraph();
            if (p != null && p.HasParameterValue(Id, "Value"))
            {
                v = Convert.ToSingle(p.GetParameterValue(Id, "Value"));
            }

            output.Data = v;

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
