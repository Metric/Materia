using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Materia.Rendering.Attributes;
using Materia.Rendering.Mathematics;
using Materia.Graph;
using Materia.Graph.IO;

namespace Materia.Nodes.MathNodes
{
    public class Float4ConstantNode : MathNode
    {
        NodeOutput output;

        MVector vec = MVector.Zero;

        protected float x;
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
                TriggerValueChange();
            }
        }

        public Float4ConstantNode(int w, int h, GraphPixelType p = GraphPixelType.RGBA)
        {
            //we ignore w,h,p

            CanPreview = false;

            defaultName = Name = "Float4 Constant";

            shaderId = "S" + Id.Split('-')[0];

            Outputs.Clear();
            Inputs.Clear();

            ExecuteInput = null;

            output = new NodeOutput(NodeType.Float4, this);
            Outputs.Add(output);
        }

        public override string GetDescription()
        {
            return string.Format("{0:0.00},{1:0.00},{2:0.00},{3:0.00}", vec.X, vec.Y, vec.Z, vec.W);
        }

        public class Float4ConstantData : NodeData
        {
            public float x;
            public float y;
            public float z;
            public float w;

            public override void Write(Writer wr)
            {
                base.Write(wr);
                wr.Write(x);
                wr.Write(y);
                wr.Write(z);
                wr.Write(w);
            }

            public override void Parse(Reader r)
            {
                base.Parse(r);
                x = r.NextFloat();
                y = r.NextFloat();
                z = r.NextFloat();
                w = r.NextFloat();
            }
        }

        public override void GetBinary(Writer w)
        {
            Float4ConstantData d = new Float4ConstantData();
            FillBaseNodeData(d);
            d.x = vec.X;
            d.y = vec.Y;
            d.z = vec.Z;
            d.w = vec.W;
            d.Write(w);
        }

        public override void FromBinary(Reader r)
        {
            Float4ConstantData d = new Float4ConstantData();
            d.Parse(r);
            SetBaseNodeDate(d);
            vec.X = d.x;
            vec.Y = d.y;
            vec.Z = d.z;
            vec.W = d.w;
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

            return "vec4 " + s + " = vec4(" + px.ToCodeString() + "," + py.ToCodeString() + "," + pz.ToCodeString() + "," + pw.ToCodeString() + ");\r\n";
        }

        public override void TryAndProcess()
        {
            output.Data = vec;
        }
    }
}
