using System;
using Materia.Rendering.Imaging.Processing;
using Materia.Rendering.Attributes;
using Materia.Rendering.Textures;
using Newtonsoft.Json;
using Materia.Rendering.Extensions;
using Materia.Graph;
using Materia.Rendering.Mathematics;

namespace Materia.Nodes.Atomic
{
    public class HSLNode : ImageNode 
    {
        NodeInput input;
        NodeOutput Output;

        HSLProcessor processor;

        protected float hue;
        [Promote(NodeType.Float)]
        [Editable(ParameterInputType.FloatSlider, "Hue")]
        public float Hue
        {
            get
            {
                return hue;
            }
            set
            {
                hue = value;
                TriggerValueChange();
            }
        }

        protected float saturation;
        [Promote(NodeType.Float)]
        [Editable(ParameterInputType.FloatSlider, "Saturation", "Default", -1, 1)]
        public float Saturation
        {
            get
            {
                return saturation;
            }
            set
            {
                saturation = value;
                TriggerValueChange();
            }
        }

        protected float lightness;
        [Promote(NodeType.Float)]
        [Editable(ParameterInputType.FloatSlider, "Lightness", "Default", -1, 1)]
        public float Lightness
        {
            get
            {
                return lightness;
            }
            set
            {
                lightness = value;
                TriggerValueChange();
            }
        }

        public HSLNode(int w, int h, GraphPixelType p = GraphPixelType.RGBA) : base()
        {
            Name = "HSL";
            Id = Guid.NewGuid().ToString();
            width = w;
            height = h;

            tileX = tileY = 1;

            processor = new HSLProcessor();

            hue = 0;
            saturation = 0;
            lightness = 0;

            internalPixelType = p;

            input = new NodeInput(NodeType.Gray | NodeType.Color, this, "Image");
            Output = new NodeOutput(NodeType.Color, this);

            Inputs.Add(input);
            Outputs.Add(Output);
        }

        public override void Dispose()
        {
            base.Dispose();

            processor?.Dispose();
            processor = null;
        }


        public override void TryAndProcess()
        {
            Process();
        }

        void Process()
        {
            if (processor == null) return;
            if (!input.HasInput) return;

            GLTexture2D i1 = (GLTexture2D)input.Reference.Data;

            if (i1 == null) return;
            if (i1.Id == 0) return;

            if (processor == null) return;

            CreateBufferIfNeeded();

            processor.Tiling = GetTiling();

            processor.Hue = GetParameter("Hue", hue) * 6.0f;
            processor.Saturation = GetParameter("Saturation", saturation);
            processor.Lightness = GetParameter("Lightness", lightness);

            processor.PrepareView(buffer);
            processor.Process(i1);
            processor.Complete();

            Output.Data = buffer;
            TriggerTextureChange();
        }

        public class HSLData : NodeData
        {
            public float hue;
            public float saturation;
            public float lightness;
        }

        public override void FromJson(string data)
        {
            HSLData d = JsonConvert.DeserializeObject<HSLData>(data);
            SetBaseNodeDate(d);
            hue = d.hue;
            saturation = d.saturation;
            lightness = d.lightness;
        }

        public override string GetJson()
        {
            HSLData d = new HSLData();
            FillBaseNodeData(d);
            d.hue = hue;
            d.saturation = saturation;
            d.lightness = lightness;

            return JsonConvert.SerializeObject(d);
        }
    }
}
