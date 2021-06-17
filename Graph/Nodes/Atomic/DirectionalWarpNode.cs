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
    public class DirectionalWarpNode : ImageNode
    {
        protected DirectionalWarpProcessor processor;
        protected BlurProcessor blur;

        protected GLTexture2D buffer2;

        protected NodeInput input;
        protected NodeInput input1;

        protected NodeOutput output;

        protected float angle = 0;
        [Promote(NodeType.Float)]
        [Editable(ParameterInputType.IntSlider, "Angle", "Default", 0, 360)]
        public float Angle
        {
            get
            {
                return angle;
            }
            set
            {
                angle = value;
                TriggerValueChange();
            }
        }

        protected float intensity = 1;
        [Promote(NodeType.Float)]
        [Editable(ParameterInputType.FloatSlider, "Intensity")]
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

        protected float blurIntensity = 1;
        [Promote(NodeType.Float)]
        [Editable(ParameterInputType.IntSlider, "Blur Intensity", "Default", 0, 128)]
        public float BlurIntensity
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

        public DirectionalWarpNode(int w, int h, GraphPixelType p = GraphPixelType.RGBA) : base()
        {
            Name = "Directional Warp";

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

            processor ??= new DirectionalWarpProcessor();
            blur ??= new BlurProcessor();

            processor.Tiling = GetTiling();
            processor.Angle = GetParameter("Angle", angle) * MathHelper.Deg2Rad;
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
            public float intensity;
            public float blurIntensity;
            public float angle;

            public override void Write(Writer w)
            {
                base.Write(w);
                w.Write(intensity);
                w.Write(blurIntensity);
                w.Write(angle);
            }

            public override void Parse(Reader r)
            {
                base.Parse(r);
                intensity = r.NextFloat();
                blurIntensity = r.NextFloat();
                angle = r.NextFloat();
            }
        }

        public override void GetBinary(Writer w)
        {
            WarpData d = new WarpData();
            FillBaseNodeData(d);
            d.intensity = intensity;
            d.blurIntensity = blurIntensity;
            d.angle = angle;
            d.Write(w);
        }

        public override void FromBinary(Reader r)
        {
            WarpData d = new WarpData();
            d.Parse(r);
            SetBaseNodeDate(d);
            intensity = d.intensity;
            blurIntensity = d.blurIntensity;
            angle = d.angle;
        }

        public override string GetJson()
        {
            WarpData d = new WarpData();
            FillBaseNodeData(d);
            d.intensity = intensity;
            d.blurIntensity = blurIntensity;
            d.angle = angle;

            return JsonConvert.SerializeObject(d);
        }

        public override void FromJson(string data)
        {
            WarpData d = JsonConvert.DeserializeObject<WarpData>(data);
            SetBaseNodeDate(d);
            intensity = d.intensity;
            blurIntensity = d.blurIntensity;
            angle = d.angle;
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
