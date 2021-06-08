using System;
using System.Collections.Generic;
using Materia.Rendering.Attributes;
using Materia.Rendering.Imaging.Processing;
using Newtonsoft.Json;
using Materia.Rendering.Textures;
using Materia.Rendering.Extensions;
using Materia.Graph;
using Materia.Rendering.Mathematics;

namespace Materia.Nodes.Atomic
{
    public class AONode : ImageNode
    {
        BlurProcessor blur;
        OcclusionProcessor processor;
        protected GLTexture2D buffer2;

        NodeInput input;

        NodeOutput Output;

        int rays;

        [Promote(NodeType.Float)]
        [Editable(ParameterInputType.IntSlider, "Rays", "Default", 1, 128)]
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
            Name = "AO";

            Id = Guid.NewGuid().ToString();

            tileX = tileY = 1;

            width = w;
            height = h;
            rays = 4;

            internalPixelType = p;

            processor = new OcclusionProcessor();
            blur = new BlurProcessor();

            input = new NodeInput(NodeType.Gray, this, "Gray Input");
            Output = new NodeOutput(NodeType.Gray, this);

            Inputs = new List<NodeInput>();
            Inputs.Add(input);

            Outputs = new List<NodeOutput>();
            Outputs.Add(Output);
        }

        public class AOData : NodeData
        {
            public int rays;
        }

        public override void FromJson(string data)
        {
            AOData d = JsonConvert.DeserializeObject<AOData>(data);
            SetBaseNodeDate(d);
            rays = d.rays;
        }

        public override string GetJson()
        {
            AOData d = new AOData();
            FillBaseNodeData(d);
            d.rays = rays;

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
            if (buffer2 == null || buffer2.Id == 0)
            {
                buffer2 = buffer.Copy();
            }
        }

        void Process()
        {
            if (processor == null || blur == null) return;
            if (!input.HasInput) return;

            GLTexture2D i1 = (GLTexture2D)input.Reference.Data;

            if (i1 == null) return;
            if (i1.Id == 0) return;

            CreateBufferIfNeeded();

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
