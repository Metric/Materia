using System;
using Materia.Rendering.Imaging.Processing;
using Materia.Rendering.Attributes;
using Materia.Rendering.Textures;
using Newtonsoft.Json;
using Materia.Rendering.Extensions;
using Materia.Graph;
using Materia.Rendering.Mathematics;
using Materia.Graph.IO;

namespace Materia.Nodes.Atomic
{
    public class SharpenNode : ImageNode
    {
        NodeInput input;
        NodeOutput Output;

        SharpenProcessor processor;

        protected float intensity = 1;
        [Promote(NodeType.Float)]
        [Editable(ParameterInputType.FloatSlider, "Intensity", "Default", 1, 10)]
        public float Intensity
        {
            get
            {
                return intensity;
            }
            set
            {
                intensity = value;
                TriggerValueChange();
            }
        }

        public SharpenNode(int w, int h, GraphPixelType p = GraphPixelType.RGBA) : base()
        {
            defaultName = Name = "Sharpen";

            width = w;
            height = h;

            internalPixelType = p;

            input = new NodeInput(NodeType.Gray | NodeType.Color, this, "Image");
            Output = new NodeOutput(NodeType.Gray | NodeType.Color, this);

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

        float pintensity;
        void Process()
        {
            if (isDisposing) return;
            if (!input.HasInput) return;

            GLTexture2D i1 = (GLTexture2D)input.Reference.Data;

            if (i1 == null) return;
            if (i1.Id == 0) return;

            CreateBufferIfNeeded();

            processor ??= new SharpenProcessor();

            processor.Tiling = GetTiling();
            processor.Intensity = GetParameter("Intensity", intensity);

            processor.PrepareView(buffer);
            processor.Process(i1);
            processor.Complete();

            Output.Data = buffer;
            TriggerTextureChange();
        }

        public class SharpenData : NodeData
        {
            public float intensity;

            public override void Write(Writer w)
            {
                base.Write(w);
                w.Write(intensity);
            }

            public override void Parse(Reader r)
            {
                base.Parse(r);
                intensity = r.NextFloat();
            }
        }

        public override void GetBinary(Writer w)
        {
            SharpenData d = new SharpenData();
            FillBaseNodeData(d);
            d.intensity = intensity;
            d.Write(w);
        }

        public override void FromBinary(Reader r)
        {
            SharpenData d = new SharpenData();
            d.Parse(r);
            SetBaseNodeDate(d);
            intensity = d.intensity;
        }

        public override void FromJson(string data)
        {
            SharpenData d = JsonConvert.DeserializeObject<SharpenData>(data);
            SetBaseNodeDate(d);
            intensity = d.intensity;
        }

        public override string GetJson()
        {
            SharpenData d = new SharpenData();
            FillBaseNodeData(d);
            d.intensity = intensity;

            return JsonConvert.SerializeObject(d);
        }
    }
}
