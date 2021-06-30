using System;
using System.Collections.Generic;
using Materia.Rendering.Attributes;
using Materia.Rendering.Imaging.Processing;
using Newtonsoft.Json;
using Materia.Rendering.Textures;
using Materia.Rendering.Extensions;
using Materia.Graph;
using Materia.Rendering.Mathematics;
using Materia.Graph.IO;

namespace Materia.Nodes.Atomic
{
    public class AONode : ImageNode
    {
        BlurProcessor blur;
        OcclusionProcessor processor;
        protected GLTexture2D buffer2;

        NodeInput input;

        NodeOutput Output;

        int rays = 4;

        [Promote(NodeType.Float)]
        [Editable(ParameterInputType.IntSlider, "Rays", "Default", 1, 255)]
        public int Rays
        {
            get
            {
                return rays;
            }
            set
            {
                rays = value;
                TriggerValueChange();
            }
        }

        public AONode(int w, int h, GraphPixelType p = GraphPixelType.RGBA) : base()
        {
            //Do not create processors here as it can hurt loading times
            //also it allows us to load on another thread if needed
            //and not worry about GL calls

            defaultName = Name = "AO";

            width = w;
            height = h;

            internalPixelType = p;

            input = new NodeInput(NodeType.Gray, this, "Gray Input");
            Output = new NodeOutput(NodeType.Gray, this);

            Inputs = new List<NodeInput>();
            Inputs.Add(input);

            Outputs = new List<NodeOutput>();
            Outputs.Add(Output);
        }

        public class AOData : NodeData
        {
            public byte rays;

            public override void Write(Writer w)
            {
                base.Write(w);
                w.Write(rays);
            }

            public override void Parse(Reader r)
            {
                base.Parse(r);
                rays = r.NextByte();
            }
        }

        public override void FromBinary(Reader r)
        {

            AOData d = new AOData();
            d.Parse(r);
            SetBaseNodeDate(d);
            rays = d.rays;
        }

        public override void FromJson(string data)
        {
            AOData d = JsonConvert.DeserializeObject<AOData>(data);
            SetBaseNodeDate(d);
            rays = d.rays;
        }

        public override void GetBinary(Writer w)
        {
            AOData d = new AOData();
            FillBaseNodeData(d);
            d.rays = (byte)rays;
            d.Write(w);
        }

        public override string GetJson()
        {
            AOData d = new AOData();
            FillBaseNodeData(d);
            d.rays = (byte)rays;

            return JsonConvert.SerializeObject(d);
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
            if (!input.HasInput) return;

            GLTexture2D i1 = (GLTexture2D)input.Reference.Data;

            if (i1 == null) return;
            if (i1.Id == 0) return;

            CreateBufferIfNeeded();

            processor ??= new OcclusionProcessor();
            blur ??= new BlurProcessor();

            blur.Tiling = GetTiling();
            blur.Intensity = GetParameter("Rays", rays);

            blur.PrepareView(buffer2);
            blur.Process(i1);
            blur.Complete();

            processor.Tiling = GetTiling();

            processor.PrepareView(buffer);            
            processor.Process(buffer2, i1);
            processor.Complete();

            Output.Data = buffer;
            TriggerTextureChange();
        }
    }
}
