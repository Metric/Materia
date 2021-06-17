using System;
using Newtonsoft.Json;
using Materia.Rendering.Imaging.Processing;
using Materia.Rendering.Textures;
using Materia.Rendering.Attributes;
using Materia.Rendering.Extensions;
using Materia.Graph;
using Materia.Rendering.Mathematics;
using Materia.Graph.IO;

namespace Materia.Nodes.Atomic
{
    public class BlurNode : ImageNode
    {
        NodeInput input;

        int intensity = 10;

        NodeOutput Output;

        BlurProcessor processor;

        [Promote(NodeType.Float)]
        [Editable(ParameterInputType.IntSlider, "Intensity", "Default", 1, 128)]
        public int Intensity
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

        public BlurNode(int w, int h, GraphPixelType p = GraphPixelType.RGBA) : base()
        {
            Name = "Blur";

            width = w;
            height = h;

            internalPixelType = p;

            input = new NodeInput(NodeType.Color | NodeType.Gray, this, "Image Input");
            Output = new NodeOutput(NodeType.Color | NodeType.Gray, this);

            Inputs.Add(input);
            Outputs.Add(Output);
        }

        public override void TryAndProcess()
        {
            Process();
        }

        void Process()
        {
            if (isDisposing) return;
            if (!input.HasInput) return;

            GLTexture2D i1 = (GLTexture2D)input.Reference.Data;

            if (i1 == null) return;
            if (i1.Id == 0) return;

            CreateBufferIfNeeded();

            processor ??= new BlurProcessor();

            processor.Tiling = GetTiling();
            processor.Intensity = GetParameter("Intensity", intensity);

            processor.PrepareView(buffer);
            processor.Process(i1);
            processor.Complete();

            //todo: might need another tile processor here

            Output.Data = buffer;
            TriggerTextureChange();
        }

        public override void Dispose()
        {
            base.Dispose();

            processor?.Dispose();
            processor = null;
        }

        public class BlurData : NodeData
        {
            public int intensity;

            public override void Write(Writer w)
            {
                base.Write(w);
                w.Write(intensity);
            }

            public override void Parse(Reader r)
            {
                base.Parse(r);
                intensity = r.NextInt();
            }
        }

        public override void GetBinary(Writer w)
        {
            BlurData d = new BlurData();
            FillBaseNodeData(d);
            d.intensity = intensity;
            d.Write(w);
        }

        public override void FromBinary(Reader r)
        {
            BlurData d = new BlurData();
            d.Parse(r);
            SetBaseNodeDate(d);
            intensity = d.intensity;
        }

        public override string GetJson()
        {
            BlurData d = new BlurData();
            FillBaseNodeData(d);
            d.intensity = intensity;

            return JsonConvert.SerializeObject(d);
        }

        public override void FromJson(string data)
        {
            BlurData d = JsonConvert.DeserializeObject<BlurData>(data);
            SetBaseNodeDate(d);
            intensity = d.intensity;
        }
    }
}
