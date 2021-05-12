using System;

using Materia.Rendering.Imaging.Processing;
using Materia.Rendering.Attributes;
using Materia.Rendering.Textures;
using Newtonsoft.Json;
using Materia.Rendering.Extensions;
using Materia.Graph;
using Materia.Rendering.Mathematics;

namespace Materia.Nodes.Atomic
{
    public class GammaNode : ImageNode
    {
        protected NodeInput input;

        protected float gamma;

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
            Id = Guid.NewGuid().ToString();

            width = w;
            height = h;
            gamma = 2.2f;

            tileX = tileY = 1;

            processor = new GammaProcessor();

            internalPixelType = p;

            input = new NodeInput(NodeType.Color | NodeType.Gray, this, "Image Input");
            output = new NodeOutput(NodeType.Color | NodeType.Gray, this);

            Inputs.Add(input);
            Outputs.Add(output);
        }

        private void GetParams()
        {
            if (!input.HasInput) return;

            pgamma = gamma;

            if (ParentGraph != null && ParentGraph.HasParameterValue(Id, "Gamma"))
            {
                pgamma = ParentGraph.GetParameterValue(Id, "Gamma").ToFloat();
            }
        }

        public override void TryAndProcess()
        {
            GetParams();
            Process();
        }

        float pgamma;
        void Process()
        {
            if (processor == null) return;
            if (!input.HasInput) return;

            GLTexture2D i1 = (GLTexture2D)input.Reference.Data;

            if (i1 == null) return;
            if (i1.Id == 0) return;

            CreateBufferIfNeeded();

            processor.PrepareView(buffer);

            processor.Tiling = new Vector2(TileX, TileY);
            processor.Gamma = pgamma;
            processor.Process(i1);
            processor.Complete();

            output.Data = buffer;
            TriggerTextureChange();
        }

        public class GammaData : NodeData
        {
            public float gamma;
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
