using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Materia.MathHelpers;
using Materia.Nodes.Attributes;

namespace Materia.Nodes.MathNodes
{
    public class Float4ConstantNode : MathNode
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

        protected float w;
        [Promote(NodeType.Float)]
        public float W
        {
            get
            {
                return w;
            }
            set
            {
                w = value;
                Updated();
            }
        }

        public Float4ConstantNode(int w, int h, GraphPixelType p = GraphPixelType.RGBA)
        {
            //we ignore w,h,p

            x = y = z = 0;

            CanPreview = false;

            vec = new MVector();

            Name = "Float4 Constant";
            Id = Guid.NewGuid().ToString();
            shaderId = "S" + Id.Split('-')[0];

            output = new NodeOutput(NodeType.Float4, this);

            Inputs = new List<NodeInput>();

            Outputs = new List<NodeOutput>();
            Outputs.Add(output);
        }

        public override void TryAndProcess()
        {
            Process();
        }

        public class Float4ConstantData : NodeData
        {
            public float x;
            public float y;
            public float z;
            public float w;
        }

        public override void FromJson(Dictionary<string, Node> nodes, string data)
        {
            Float4ConstantData d = JsonConvert.DeserializeObject<Float4ConstantData>(data);
            SetBaseNodeDate(d);
            x = d.x;
            y = d.y;
            z = d.z;
            w = d.w;
        }

        public override string GetJson()
        {
            Float4ConstantData d = new Float4ConstantData();
            FillBaseNodeData(d);
            d.x = x;
            d.y = y;
            d.z = z;
            d.w = w;

            return JsonConvert.SerializeObject(d);
        }

        public override string GetShaderPart(string currentFrag)
        {
            var s = shaderId + "0";


            float px = x;
            float py = y;
            float pz = z;
            float pw = w;

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
            if (p != null && p.HasParameterValue(Id, "W"))
            {
                pw = Convert.ToSingle(p.GetParameterValue(Id, "W"));
            }

            return "vec4 " + s + " = vec4(" + px + "," + py + "," + pz + "," + pw + ");\r\n";
        }

        void Process()
        {
            float px = x;
            float py = y;
            float pz = z;
            float pw = w;

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
            if (p != null && p.HasParameterValue(Id, "W"))
            {
                pw = Convert.ToSingle(p.GetParameterValue(Id, "W"));
            }

            vec.X = px;
            vec.Y = py;
            vec.Z = pz;
            vec.W = pw;
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
