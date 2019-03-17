using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Materia.MathHelpers;

namespace Materia.Nodes.MathNodes
{
    public class Float3ConstantNode : MathNode
    {
        NodeOutput output;

        MVector vec;

        protected float x;
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

            SetConnections(nodes, d.outputs);

            Updated();
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

        public override string GetShaderPart()
        {
            var s = shaderId + "0";

            return "vec3 " + s + " = vec3(" + x + "," + y + "," + z + ");\r\n";
        }

        void Process()
        {
            vec.X = x;
            vec.Y = y;
            vec.Z = z;
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
