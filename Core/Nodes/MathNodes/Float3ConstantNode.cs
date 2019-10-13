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
        [Promote(NodeType.Float3)]
        [Editable(ParameterInputType.Float3Input, "Vector")]
        public MVector Vector
        {
            get
            {
                return vec;
            }
            set
            {
                vec = value;
                OnDescription($"{vec.X},{vec.Y},{vec.Z}");
                Updated();
            }
        }

        public Float3ConstantNode(int w, int h, GraphPixelType p = GraphPixelType.RGBA)
        {
            //we ignore w,h,p

            CanPreview = false;

            vec = new MVector(0,0,0);

            Name = "Float3 Constant";
            Id = Guid.NewGuid().ToString();
            shaderId = "S" + Id.Split('-')[0];

            output = new NodeOutput(NodeType.Float3, this);

            Inputs = new List<NodeInput>();

            Outputs = new List<NodeOutput>();
            Outputs.Add(output);
        }

        public override string GetDescription()
        {
            return $"{vec.X},{vec.Y},{vec.Z}";
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

        public override void FromJson(string data)
        {
            Float3ConstantData d = JsonConvert.DeserializeObject<Float3ConstantData>(data);
            SetBaseNodeDate(d);
            vec.X = d.x;
            vec.Y = d.y;
            vec.Z = d.z;
        }

        public override string GetJson()
        {
            Float3ConstantData d = new Float3ConstantData();
            FillBaseNodeData(d);
            d.x = vec.X;
            d.y = vec.Y;
            d.z = vec.Z;

            return JsonConvert.SerializeObject(d);
        }

        public override string GetShaderPart(string currentFrag)
        {
            var s = shaderId + "0";

            float px = vec.X;
            float py = vec.Y;
            float pz = vec.Z;

            var p = TopGraph();
            if (p != null && p.HasParameterValue(Id, "Vector"))
            {
                MVector v = p.GetParameterValue<MVector>(Id, "Vector");
                px = v.X;
                py = v.Y;
                pz = v.Z;
            }


            return "vec3 " + s + " = vec3(" + px.ToCodeString() + "," + py.ToCodeString() + "," + pz.ToCodeString() + ");\r\n";
        }

        void Process()
        {

            MVector v = vec;

            var p = TopGraph();
            if (p != null && p.HasParameterValue(Id, "Vector"))
            {
                v = p.GetParameterValue<MVector>(Id, "Vector");
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
