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
    public class MotionBlurNode : ImageNode
    {
        MotionBlurProcessor processor;

        int magnitude = 10;
        [Promote(NodeType.Float)]
        [Editable(ParameterInputType.IntSlider, "Intensity", "Default", 1, 128)]
        public int Intensity
        {
            get
            {
                return magnitude;
            }
            set
            {
                magnitude = value;
                TriggerValueChange();
            }
        }

        int direction = 0;
        [Promote(NodeType.Float)]
        [Editable(ParameterInputType.IntSlider, "Direction", "Default", 0, 180)]
        public int Direction
        {
            get
            {
                return direction;
            }
            set
            {
                direction = value;
                TriggerValueChange();
            }
        }

        NodeInput input;
        NodeOutput output;

        public MotionBlurNode(int w, int h, GraphPixelType p = GraphPixelType.RGBA) : base()
        {
            Name = "Motion Blur";

            width = w;
            height = h;

            internalPixelType = p;

            input = new NodeInput(NodeType.Color | NodeType.Gray, this, "Image Input");
            Inputs.Add(input);
            output = new NodeOutput(NodeType.Color | NodeType.Gray, this);
            Outputs.Add(output);
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

            CreateBufferIfNeeded();

            processor ??= new MotionBlurProcessor();

            processor.Tiling = GetTiling();
            processor.Direction = GetParameter("Direction", direction) * MathHelper.Deg2Rad;
            processor.Magnitude = GetParameter("Intensity", magnitude);

            processor.PrepareView(buffer);
            processor.Process(i1);
            processor.Complete();

            output.Data = buffer;
            TriggerTextureChange();
        }

        public class MotionBlurData : NodeData
        {
            public int intensity;
            public int direction;

            public override void Write(Writer w)
            {
                base.Write(w);
                w.Write(intensity);
                w.Write(direction);
            }

            public override void Parse(Reader r)
            {
                base.Parse(r);
                intensity = r.NextInt();
                direction = r.NextInt();
            }
        }

        public override void GetBinary(Writer w)
        {
            MotionBlurData d = new MotionBlurData();
            FillBaseNodeData(d);
            d.intensity = magnitude;
            d.direction = direction;
            d.Write(w);
        }

        public override void FromBinary(Reader r)
        {
            MotionBlurData d = new MotionBlurData();
            d.Parse(r);
            SetBaseNodeDate(d);
            magnitude = d.intensity;
            direction = d.direction;
        }

        public override void FromJson(string data)
        {
            MotionBlurData d = JsonConvert.DeserializeObject<MotionBlurData>(data);
            SetBaseNodeDate(d);
            magnitude = d.intensity;
            direction = d.direction;
        }

        public override string GetJson()
        {
            MotionBlurData d = new MotionBlurData();
            FillBaseNodeData(d);
            d.intensity = magnitude;
            d.direction = direction;

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
