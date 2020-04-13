using System;
using Materia.Rendering.Attributes;
using Materia.Graph;
using Newtonsoft.Json;

namespace Materia.Nodes.MathNodes
{
    public class SamplerNode : MathNode
    {
        protected NodeInput input;
        protected NodeOutput output;

        protected int sampleIndex;

        [Dropdown(null, false, "Input0", "Input1", "Input2", "Input3")]
        [Editable(ParameterInputType.Dropdown, "Image Input")]
        public int SampleIndex
        {
            get
            {
                return sampleIndex;
            }
            set
            {
                sampleIndex = value;
            }
        }

        public SamplerNode(int w, int h, GraphPixelType p = GraphPixelType.RGBA) : base()
        {
            CanPreview = false;

            Name = "Sampler";

            Id = Guid.NewGuid().ToString();
            shaderId = "S" + Id.Split('-')[0];

            input = new NodeInput(NodeType.Float2, this, "Pos");
            output = new NodeOutput(NodeType.Float4, this);

            sampleIndex = 0;
            Inputs.Add(input);

            Outputs.Add(output);
        }

        public override string GetShaderPart(string currentFrag)
        {
            if (!input.HasInput) return "";
            var s = shaderId + "1";
            var n1id = (input.Reference.Node as MathNode).ShaderId;

            var index = input.Reference.Node.Outputs.IndexOf(input.Reference);
            var samp = "Input" + sampleIndex;
            n1id += index;

            return "vec4 " + s + " = texture(" + samp + "," + n1id + ");\r\n";
        }

        public class SamplerNodeData : NodeData
        {
            public int sampleIndex;
        }

        public override string GetJson()
        {
            SamplerNodeData d = new SamplerNodeData();
            FillBaseNodeData(d);

            d.sampleIndex = sampleIndex;

            return JsonConvert.SerializeObject(d);
        }

        public override void FromJson(string data)
        {
            SamplerNodeData d = JsonConvert.DeserializeObject<SamplerNodeData>(data);
            SetBaseNodeDate(d);
            sampleIndex = d.sampleIndex;
        }
    }
}
