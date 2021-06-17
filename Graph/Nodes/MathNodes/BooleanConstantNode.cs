using System;
using System.Collections.Generic;
using Materia.Rendering.Attributes;
using Newtonsoft.Json;
using Materia.Graph;
using Materia.Graph.IO;

namespace Materia.Nodes.MathNodes
{
    public class BooleanConstantNode : MathNode
    {
        NodeOutput output;

        protected bool val;
        [Editable(ParameterInputType.Toggle, "True")]
        public bool True
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

        public BooleanConstantNode(int w, int h, GraphPixelType p = GraphPixelType.RGBA)
        {
            //we ignore w,h,p
            CanPreview = false;

            Name = "Boolean Constant";

            shaderId = "S" + Id.Split('-')[0];

            //remove default exection pins
            Inputs.Clear();
            Outputs.Clear();

            ExecuteInput = null;

            output = new NodeOutput(NodeType.Bool, this);
            Outputs.Add(output);
        }

        public override string GetDescription()
        {
            return val.ToString();
        }

        public class BoolConstantData : NodeData
        {
            public bool val;

            public override void Write(Writer w)
            {
                base.Write(w);
                w.Write(val);
            }

            public override void Parse(Reader r)
            {
                base.Parse(r);
                val = r.NextBool();
            }
        }

        public override void GetBinary(Writer w)
        {
            BoolConstantData d = new BoolConstantData();
            FillBaseNodeData(d);
            d.val = val;
            d.Write(w);
        }

        public override void FromBinary(Reader r)
        {
            BoolConstantData d = new BoolConstantData();
            d.Parse(r);
            SetBaseNodeDate(d);
            val = d.val;
        }

        public override void FromJson(string data)
        {
            BoolConstantData d = JsonConvert.DeserializeObject<BoolConstantData>(data);
            SetBaseNodeDate(d);
            val = d.val;
        }

        public override string GetJson()
        {
            BoolConstantData d = new BoolConstantData();
            FillBaseNodeData(d);
            d.val = val;

            return JsonConvert.SerializeObject(d);
        }

        public override string GetShaderPart(string currentFrag)
        {
            var s = shaderId + "0";

            bool t = val;

            return "float " + s + " = " + (t ? 1 : 0 ) + ";\r\n";
        }

        public override void TryAndProcess()
        {
            output.Data = val ? 1 : 0;
        }
    }
}
