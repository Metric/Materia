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
        [Promote(NodeType.Float4)]
        [Editable(ParameterInputType.Float4Input, "Vector")]
        public MVector Vector
        {
            get
            {
                return vec;
            }
            set
            {
                vec = value;
                OnDescription($"{vec.X},{vec.Y},{vec.Z},{vec.W}");
                Updated();
            }
        }

        public Float4ConstantNode(int w, int h, GraphPixelType p = GraphPixelType.RGBA)
        {
            //we ignore w,h,p

            CanPreview = false;

            vec = new MVector(0,0,0,0);

            Name = "Float4 Constant";
            Id = Guid.NewGuid().ToString();
            shaderId = "S" + Id.Split('-')[0];

            output = new NodeOutput(NodeType.Float4, this);

            Inputs = new List<NodeInput>();

            Outputs = new List<NodeOutput>();
            Outputs.Add(output);
        }

        public override string GetDescription()
        {
            return $"{vec.X},{vec.Y},{vec.Z},{vec.W}";
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

        public override void FromJson(string data)
        {
            Float4ConstantData d = JsonConvert.DeserializeObject<Float4ConstantData>(data);
            SetBaseNodeDate(d);
            vec.X = d.x;
            vec.Y = d.y;
            vec.Z = d.z;
            vec.W = d.w;
        }

        public override string GetJson()
        {
            Float4ConstantData d = new Float4ConstantData();
            FillBaseNodeData(d);
            d.x = vec.X;
            d.y = vec.Y;
            d.z = vec.Z;
            d.w = vec.W;

            return JsonConvert.SerializeObject(d);
        }

        public override string GetShaderPart(string currentFrag)
        {
            var s = shaderId + "0";


            float px = vec.X;
            float py = vec.Y;
            float pz = vec.Z;
            float pw = vec.W;

            var p = TopGraph();
            if (p != null && p.HasParameterValue(Id, "Vector"))
            {
                MVector v = p.GetParameterValue<MVector>(Id, "Vector");
                px = v.X;
                py = v.Y;
                pz = v.Z;
                pw = v.W;
            }


            return "vec4 " + s + " = vec4(" + px + "," + py + "," + pz + "," + pw + ");\r\n";
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
