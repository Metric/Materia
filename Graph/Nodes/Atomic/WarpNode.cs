﻿using System;
using Materia.Rendering.Textures;
using Materia.Rendering.Imaging.Processing;
using Newtonsoft.Json;
using Materia.Rendering.Attributes;
using Materia.Rendering.Extensions;
using Materia.Graph;
using Materia.Rendering.Mathematics;

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

        protected float intensity;
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

        protected float blurIntensity;
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

        public WarpNode(int w, int h, GraphPixelType p = GraphPixelType.RGBA) : base()
        {
            Name = "Warp";
            Id = Guid.NewGuid().ToString();

            width = w;
            height = h;

            intensity = 1;

            tileX = tileY = 1;

            processor = new WarpProcessor();
            blur = new BlurProcessor();

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
            if (buffer2 == null || buffer2.Id == 0)
            {
                buffer2 = buffer.Copy();
            }
        }

        void Process()
        {
            if (processor == null || blur == null) return;
            if (!input.HasInput || !input1.HasInput) return;

            GLTexture2D i1 = (GLTexture2D)input.Reference.Data;
            GLTexture2D i2 = (GLTexture2D)input1.Reference.Data;

            if (i1 == null) return;
            if (i1.Id == 0) return;

            if (i2 == null) return;
            if (i2.Id == 0) return;

            CreateBufferIfNeeded();

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
            public float intensity;
            public float blurIntensity;
        }

        public override string GetJson()
        {
            WarpData d = new WarpData();
            FillBaseNodeData(d);
            d.intensity = intensity;
            d.blurIntensity = blurIntensity;

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
