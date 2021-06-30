using System;
using System.Collections.Generic;
using Materia.Rendering.Attributes;
using Materia.Rendering.Mathematics;
using Materia.Graph;
using Newtonsoft.Json;
using Materia.Graph.IO;

namespace Materia.Nodes.MathNodes
{
    public class Float3ConstantNode : MathNode
    {
        NodeOutput output;

        MVector vec = MVector.Zero;
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
                TriggerValueChange();
            }
        }

        public Float3ConstantNode(int w, int h, GraphPixelType p = GraphPixelType.RGBA)
        {
            //we ignore w,h,p

            CanPreview = false;

            defaultName = Name = "Float3 Constant";
     
            shaderId = "S" + Id.Split('-')[0];

            Outputs.Clear();
            Inputs.Clear();

            ExecuteInput = null;

            output = new NodeOutput(NodeType.Float3, this);
            Outputs.Add(output);
        }

        public override string GetDescription()
        {
            return string.Format("{0:0.00},{1:0.00},{2:0.00}", vec.X, vec.Y, vec.Z);
        }

        public class Float3ConstantData : NodeData
        {
            public float x;
            public float y;
            public float z;

            public override void Write(Writer w)
            {
                base.Write(w);
                w.Write(x);
                w.Write(y);
                w.Write(z);
            }

            public override void Parse(Reader r)
            {
                base.Parse(r);
                x = r.NextFloat();
                y = r.NextFloat();
                z = r.NextFloat();
            }
        }

        public override void GetBinary(Writer w)
        {
            Float3ConstantData d = new Float3ConstantData();
            FillBaseNodeData(d);
            d.x = vec.X;
            d.y = vec.Y;
            d.z = vec.Z;
            d.Write(w);
        }

        public override void FromBinary(Reader r)
        {
            Float3ConstantData d = new Float3ConstantData();
            d.Parse(r);
            SetBaseNodeDate(d);
            vec.X = d.x;
            vec.Y = d.y;
            vec.Z = d.z;
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

            return "vec3 " + s + " = vec3(" + px.ToCodeString() + "," + py.ToCodeString() + "," + pz.ToCodeString() + ");\r\n";
        }

        public override void TryAndProcess()
        {
            output.Data = vec;
        }
    }
}
