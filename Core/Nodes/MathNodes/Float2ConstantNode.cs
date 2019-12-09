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
        [Editable(ParameterInputType.Float2Input, "Vector")]
        public MVector Vector
        {
            get
            {
                return vec;
            }
            set
            {
                vec = value;
                TriggerValueChange();
            }
        }

        public Float2ConstantNode(int w, int h, GraphPixelType p = GraphPixelType.RGBA)
        {
            //we ignore w,h,p

            vec = new MVector(0,0);

            CanPreview = false;

            Name = "Float2 Constant";
            Id = Guid.NewGuid().ToString();
            shaderId = "S" + Id.Split('-')[0];

            output = new NodeOutput(NodeType.Float2, this);

            Inputs = new List<NodeInput>();

            Outputs = new List<NodeOutput>();
            Outputs.Add(output);
        }

        public override string GetDescription()
        {
            return string.Format("{0:0.00},{1:0.00}", vec.X, vec.Y);
        }

        public class Float2ConstantData : NodeData
        {
            public float x;
            public float y;
        }

        public override void FromJson(string data)
        {
            Float2ConstantData d = JsonConvert.DeserializeObject<Float2ConstantData>(data);
            SetBaseNodeDate(d);
            vec.X = d.x;
            vec.Y = d.y;
        }

        public override string GetJson()
        {
            Float2ConstantData d = new Float2ConstantData();
            FillBaseNodeData(d);
            d.x = vec.X;
            d.y = vec.Y;

            return JsonConvert.SerializeObject(d);
        }

        public override string GetShaderPart(string currentFrag)
        {
            var s = shaderId + "0";

            float px = vec.X;
            float py = vec.Y;

            return "vec2 " + s + " = vec2(" + px.ToCodeString() + "," + py.ToCodeString() + ");\r\n";
        }

        public override void TryAndProcess()
        {
            output.Data = vec;
        }
    }
}
