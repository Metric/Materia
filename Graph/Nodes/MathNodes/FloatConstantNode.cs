using System;
using System.Collections.Generic;
using Materia.Rendering.Attributes;
using Materia.Rendering.Mathematics;
using Materia.Graph;
using Newtonsoft.Json;
using Materia.Graph.IO;

namespace Materia.Nodes.MathNodes
{
    public class FloatConstantNode : MathNode
    {
        NodeOutput output;

        protected float val = 0;
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
                TriggerValueChange();
            }
        }

        public FloatConstantNode(int w, int h, GraphPixelType p = GraphPixelType.RGBA)
        {
            //we ignore w,h,p

            CanPreview = false;

            defaultName = Name = "Float Constant";

            shaderId = "S" + Id.Split('-')[0];

            Outputs.Clear();
            Inputs.Clear();

            ExecuteInput = null;

            output = new NodeOutput(NodeType.Float, this);
            Outputs.Add(output);
        }

        public override string GetDescription()
        {
            return string.Format("{0:0.000}", val);
        }

        public class FloatConstantData : NodeData
        {
            public float val;

            public override void Write(Writer w)
            {
                base.Write(w);
                w.Write(val);
            }

            public override void Parse(Reader r)
            {
                base.Parse(r);
                val = r.NextFloat();
            }
        }

        public override void GetBinary(Writer w)
        {
            FloatConstantData d = new FloatConstantData();
            FillBaseNodeData(d);
            d.val = val;
            d.Write(w);
        }

        public override void FromBinary(Reader r)
        {
            FloatConstantData d = new FloatConstantData();
            d.Parse(r);
            SetBaseNodeDate(d);
            val = d.val;
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

            return "float " + s + " = " + v.ToCodeString() + ";\r\n";
        }

        public override void TryAndProcess()
        {
            output.Data = val;
        }
    }
}
