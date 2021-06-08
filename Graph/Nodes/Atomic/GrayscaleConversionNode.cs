using System;
using Materia.Rendering.Attributes;
using Materia.Rendering.Imaging.Processing;
using Materia.Rendering.Mathematics;
using Materia.Rendering.Textures;
using Materia.Graph;
using Newtonsoft.Json;

namespace Materia.Nodes.Atomic
{
    public class GrayscaleConversionNode : ImageNode
    {
        NodeInput input;

        GrayscaleConvProcessor processor;

        NodeOutput Output;

        float r;
        [Promote(NodeType.Float)]
        [Editable(ParameterInputType.FloatSlider, "Red")]
        public float Red
        {
            get
            {
                return r;
            }
            set
            {
                r = value;
                TriggerValueChange();
            }
        }

        float g;
        [Promote(NodeType.Float)]
        [Editable(ParameterInputType.FloatSlider, "Green")]
        public float Green
        {
            get
            {
                return g;
            }
            set
            {
                g = value;
                TriggerValueChange();
            }
        }

        float b;
        [Promote(NodeType.Float)]
        [Editable(ParameterInputType.FloatSlider, "Blue")]
        public float Blue
        {
            get
            {
                return b;
            }
            set
            {
                b = value;
                TriggerValueChange();
            }
        }

        float a;
        [Promote(NodeType.Float)]
        [Editable(ParameterInputType.FloatSlider, "Alpha")]
        public float Alpha
        {
            get
            {
                return a;
            }
            set
            {
                a = value;
                TriggerValueChange();
            }
        }

        public GrayscaleConversionNode(int w, int h, GraphPixelType p = GraphPixelType.RGBA) : base()
        {
            Name = "Grayscale Conversion";

            Id = Guid.NewGuid().ToString();

            width = w;
            height = h;

            r = 1;
            g = 1;
            b = 1;
            a = 0;

            tileX = tileY = 1;

            processor = new GrayscaleConvProcessor();

            internalPixelType = p;

            input = new NodeInput(NodeType.Color, this);
            Output = new NodeOutput(NodeType.Gray, this);

            Inputs.Add(input);
            Outputs.Add(Output);
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

            CreateBufferIfNeeded();

            processor.Tiling = GetTiling();
            processor.Weight = new Vector4(
                                    GetParameter("Red", r),
                                    GetParameter("Green", g),
                                    GetParameter("Blue", b),
                                    GetParameter("Alpha", a)
                                );

            processor.PrepareView(buffer);
            processor.Process(i1);
            processor.Complete();

            Output.Data = buffer;
            TriggerTextureChange();
        }

        public class GrayscaleConversionNodeData : NodeData
        {
            public float red;
            public float green;
            public float blue;
            public float alpha;
        }

        public override void FromJson(string data)
        {
            GrayscaleConversionNodeData d = JsonConvert.DeserializeObject<GrayscaleConversionNodeData>(data);
            SetBaseNodeDate(d);
            r = d.red;
            g = d.green;
            b = d.blue;
            a = d.alpha;
        }

        public override string GetJson()
        {
            GrayscaleConversionNodeData d = new GrayscaleConversionNodeData();
            FillBaseNodeData(d);
            d.red = r;
            d.green = g;
            d.blue = b;
            d.alpha = a;

            return JsonConvert.SerializeObject(d);
        }

        public override void Dispose()
        {
            base.Dispose();

            processor?.Dispose();
            processor = null;
        }
    }
}
