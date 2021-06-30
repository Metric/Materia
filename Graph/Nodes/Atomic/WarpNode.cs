using System;
using Materia.Rendering.Textures;
using Materia.Rendering.Imaging.Processing;
using Newtonsoft.Json;
using Materia.Rendering.Attributes;
using Materia.Rendering.Extensions;
using Materia.Graph;
using Materia.Rendering.Mathematics;
using Materia.Graph.IO;

namespace Materia.Nodes.Atomic
{
    public class WarpNode : ImageNode
    {
        protected WarpProcessor processor;
        protected BlurProcessor blur;

        protected NodeInput input;
        protected NodeInput input1;

        protected NodeOutput output;

        protected GLTexture2D buffer2;

        protected int intensity = 1;
        [Promote(NodeType.Float)]
        [Editable(ParameterInputType.IntSlider, "Intensity", "Default", 0, 255)]
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

        protected int blurIntensity = 1;
        [Promote(NodeType.Float)]
        [Editable(ParameterInputType.IntSlider, "Blur Intensity", "Default", 1, 255)]
        public int BlurIntensity
        {
            get
            {
                return blurIntensity;
            }
            set
            {
                blurIntensity = value;
                TriggerValueChange();
            }
        }

        public WarpNode(int w, int h, GraphPixelType p = GraphPixelType.RGBA) : base()
        {
            defaultName = Name = "Warp";

            width = w;
            height = h;

            internalPixelType = p;

            input = new NodeInput(NodeType.Gray | NodeType.Color, this, "Image Input");
            input1 = new NodeInput(NodeType.Gray, this, "Grayscale Gradients");

            output = new NodeOutput(NodeType.Gray | NodeType.Color, this);

            Inputs.Add(input);
            Inputs.Add(input1);
            Outputs.Add(output);
        }

        public override void TryAndProcess()
        {
            Process();
        }
        public override void ReleaseBuffer()
        {
            base.ReleaseBuffer();
            buffer2?.Dispose();
        }

        protected override void CreateBufferIfNeeded()
        {
            base.CreateBufferIfNeeded();
            if (isDisposing) return;
            if (buffer2 == null || buffer2.Id == 0)
            {
                buffer2 = buffer.Copy();
            }
        }

        void Process()
        {
            if (isDisposing) return;
            if (!input.HasInput || !input1.HasInput) return;

            GLTexture2D i1 = (GLTexture2D)input.Reference.Data;
            GLTexture2D i2 = (GLTexture2D)input1.Reference.Data;

            if (i1 == null) return;
            if (i1.Id == 0) return;

            if (i2 == null) return;
            if (i2.Id == 0) return;

            CreateBufferIfNeeded();

            processor ??= new WarpProcessor();
            blur ??= new BlurProcessor();

            processor.Tiling = GetTiling();
            processor.Intensity = GetParameter("Intensity", intensity).Max(0);

            processor.PrepareView(buffer2);
            processor.Process(i1, i2);
            processor.Complete();

            blur.Tiling = Vector2.One;
            blur.Intensity = GetParameter("BlurIntensity", blurIntensity).Max(1);

            blur.PrepareView(buffer);
            blur.Process(buffer2);
            blur.Complete();

            output.Data = buffer;
            TriggerTextureChange();
        }

        public class WarpData : NodeData
        {
            public byte intensity;
            public byte blurIntensity;

            public override void Write(Writer w)
            {
                base.Write(w);
                w.Write(intensity);
                w.Write(blurIntensity);
            }

            public override void Parse(Reader r)
            {
                base.Parse(r);
                intensity = r.NextByte();
                blurIntensity = r.NextByte();
            }
        }

        public override void GetBinary(Writer w)
        {
            WarpData d = new WarpData();
            FillBaseNodeData(d);
            d.intensity = (byte)intensity;
            d.blurIntensity = (byte)blurIntensity;
            d.Write(w);
        }

        public override void FromBinary(Reader r)
        {
            WarpData d = new WarpData();
            d.Parse(r);
            SetBaseNodeDate(d);
            intensity = d.intensity;
            blurIntensity = d.blurIntensity;
        }

        public override string GetJson()
        {
            WarpData d = new WarpData();
            FillBaseNodeData(d);
            d.intensity = (byte)intensity;
            d.blurIntensity = (byte)blurIntensity;

            return JsonConvert.SerializeObject(d);
        }

        public override void FromJson(string data)
        {
            WarpData d = JsonConvert.DeserializeObject<WarpData>(data);
            SetBaseNodeDate(d);
            intensity = d.intensity;
            blurIntensity = d.blurIntensity;
        }

        public override void Dispose()
        {
            base.Dispose();

            buffer2?.Dispose();
            buffer2 = null;

            processor?.Dispose();
            processor = null;

            blur?.Dispose();
            blur = null;
        }
    }
}
