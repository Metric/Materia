using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Materia.MathHelpers;
using Materia.Nodes.Attributes;

namespace Materia.Nodes.MathNodes
{
    public class Float2ConstantNode : MathNode
    {
        NodeOutput output;

        MVector vec;

        protected float x;
        [Promote(NodeType.Float)]
        public float X
        {
            get
            {
                return x;
            }
            set
            {
                x = value;
                Updated();
            }
        }

        protected float y;
        [Promote(NodeType.Float)]
        public float Y
        {
            get
            {
                return y;
            }
            set
            {
                y = value;
                Updated();
            }
        }

        public Float2ConstantNode(int w, int h, GraphPixelType p = GraphPixelType.RGBA)
        {
            //we ignore w,h,p

            vec = new MVector();

            x = y = 0;

            CanPreview = false;

            Name = "Float2 Constant";
            Id = Guid.NewGuid().ToString();
            shaderId = "S" + Id.Split('-')[0];

            output = new NodeOutput(NodeType.Float2, this);

            Inputs = new List<NodeInput>();

            Outputs = new List<NodeOutput>();
            Outputs.Add(output);
        }

        public override void TryAndProcess()
        {
            Process();
        }

        public class Float2ConstantData : NodeData
        {
            public float x;
            public float y;
        }

        public override void FromJson(Dictionary<string, Node> nodes, string data)
        {
            Float2ConstantData d = JsonConvert.DeserializeObject<Float2ConstantData>(data);
            SetBaseNodeDate(d);
            x = d.x;
            y = d.y;
        }

        public override string GetJson()
        {
            Float2ConstantData d = new Float2ConstantData();
            FillBaseNodeData(d);
            d.x = x;
            d.y = y;

            return JsonConvert.SerializeObject(d);
        }

        public override string GetShaderPart(string currentFrag)
        {
            var s = shaderId + "0";

            float px = x;
            float py = y;

            var p = TopGraph();
            if (p != null && p.HasParameterValue(Id, "X"))
            {
                px = Convert.ToSingle(p.GetParameterValue(Id, "X"));
            }
            if(p != null && p.HasParameterValue(Id, "Y"))
            {
                py = Convert.ToSingle(p.GetParameterValue(Id, "Y"));
            }

            return "vec2 " + s + " = vec2(" + px + "," + py + ");\r\n";
        }

        void Process()
        {
            float px = x;
            float py = y;

            var p = TopGraph();
            if (p != null && p.HasParameterValue(Id, "X"))
            {
                px = Convert.ToSingle(p.GetParameterValue(Id, "X"));
            }
            if (p != null && p.HasParameterValue(Id, "Y"))
            {
                py = Convert.ToSingle(p.GetParameterValue(Id, "Y"));
            }

            vec.X = px;
            vec.Y = py;
            output.Data = vec;
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
