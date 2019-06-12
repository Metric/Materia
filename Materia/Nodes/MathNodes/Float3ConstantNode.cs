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
    public class Float3ConstantNode : MathNode
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

        protected float z;
        [Promote(NodeType.Float)]
        public float Z
        {
            get
            {
                return z;
            }
            set
            {
                z = value;
               Updated();
            }
        }

        public Float3ConstantNode(int w, int h, GraphPixelType p = GraphPixelType.RGBA)
        {
            //we ignore w,h,p

            x = y = z = 0;

            CanPreview = false;

            vec = new MVector();

            Name = "Float3 Constant";
            Id = Guid.NewGuid().ToString();
            shaderId = "S" + Id.Split('-')[0];

            output = new NodeOutput(NodeType.Float3, this);

            Inputs = new List<NodeInput>();

            Outputs = new List<NodeOutput>();
            Outputs.Add(output);
        }

        public override void TryAndProcess()
        {
            Process();
        }

        public class Float3ConstantData : NodeData
        {
            public float x;
            public float y;
            public float z;
        }

        public override void FromJson(Dictionary<string, Node> nodes, string data)
        {
            Float3ConstantData d = JsonConvert.DeserializeObject<Float3ConstantData>(data);
            SetBaseNodeDate(d);
            x = d.x;
            y = d.y;
            z = d.z;
        }

        public override string GetJson()
        {
            Float3ConstantData d = new Float3ConstantData();
            FillBaseNodeData(d);
            d.x = x;
            d.y = y;
            d.z = z;

            return JsonConvert.SerializeObject(d);
        }

        public override string GetShaderPart(string currentFrag)
        {
            var s = shaderId + "0";

            float px = x;
            float py = y;
            float pz = z;

            var p = TopGraph();
            if (p != null && p.HasParameterValue(Id, "X"))
            {
                px = Convert.ToSingle(p.GetParameterValue(Id, "X"));
            }
            if (p != null && p.HasParameterValue(Id, "Y"))
            {
                py = Convert.ToSingle(p.GetParameterValue(Id, "Y"));
            }
            if (p != null && p.HasParameterValue(Id, "Z"))
            {
                pz = Convert.ToSingle(p.GetParameterValue(Id, "Z"));
            }

            return "vec3 " + s + " = vec3(" + px + "," + py + "," + pz + ");\r\n";
        }

        void Process()
        {

            float px = x;
            float py = y;
            float pz = z;

            var p = TopGraph();
            if (p != null && p.HasParameterValue(Id, "X"))
            {
                px = Convert.ToSingle(p.GetParameterValue(Id, "X"));
            }
            if (p != null && p.HasParameterValue(Id, "Y"))
            {
                py = Convert.ToSingle(p.GetParameterValue(Id, "Y"));
            }
            if (p != null && p.HasParameterValue(Id, "Z"))
            {
                pz = Convert.ToSingle(p.GetParameterValue(Id, "Z"));
            }

            vec.X = px;
            vec.Y = py;
            vec.Z = pz;
            output.Data = vec;

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
