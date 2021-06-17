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
    public class GammaNode : ImageNode
    {
        protected NodeInput input;

        protected float gamma = 2.2f;

        [Promote(NodeType.Float)]
        [Editable(ParameterInputType.FloatInput, "Gamma")]
        public float Gamma
        {
            get
            {
                return gamma;
            }
            set
            {
                gamma = value;
                TriggerValueChange();
            }
        }

        NodeOutput output;
        GammaProcessor processor;

        public GammaNode(int w, int h, GraphPixelType p = GraphPixelType.RGBA)
        {
            Name = "Gamma";

            width = w;
            height = h;

            internalPixelType = p;

            input = new NodeInput(NodeType.Color | NodeType.Gray, this, "Image Input");
            output = new NodeOutput(NodeType.Color | NodeType.Gray, this);

            Inputs.Add(input);
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
            if (i1.Id == 0) return;

            CreateBufferIfNeeded();

            processor ??= new GammaProcessor();

            processor.Tiling = GetTiling();
            processor.Gamma = GetParameter("Gamma", gamma);

            processor.PrepareView(buffer);
            processor.Process(i1);
            processor.Complete();

            output.Data = buffer;
            TriggerTextureChange();
        }

        public class GammaData : NodeData
        {
            public float gamma;
            public override void Write(Writer w)
            {
                base.Write(w);
                w.Write(gamma);
            }
            public override void Parse(Reader r)
            {
                base.Parse(r);
                gamma = r.NextFloat();
            }
        }

        public override void GetBinary(Writer w)
        {
            GammaData d = new GammaData();
            FillBaseNodeData(d);
            d.gamma = gamma;
            d.Write(w);
        }

        public override void FromBinary(Reader r)
        {
            GammaData d = new GammaData();
            d.Parse(r);
            SetBaseNodeDate(d);
            gamma = d.gamma;
        }

        public override string GetJson()
        {
            GammaData d = new GammaData();
            FillBaseNodeData(d);
            d.gamma = gamma;

            return JsonConvert.SerializeObject(d);
        }

        public override void FromJson(string data)
        {
            GammaData d = JsonConvert.DeserializeObject<GammaData>(data);
            SetBaseNodeDate(d);
            gamma = d.gamma;
        }

        public override void Dispose()
        {
            base.Dispose();

            processor?.Dispose();
            processor = null;
        }
    }
}
