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
    public class SharpenNode : ImageNode
    {
        NodeInput input;
        NodeOutput Output;

        SharpenProcessor processor;

        protected float intensity;
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
            Name = "Sharpen";
            Id = Guid.NewGuid().ToString();
            width = w;
            height = h;

            tileX = tileY = 1;

            processor = new SharpenProcessor();

            intensity = 1;

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

        private void GetParams()
        {
            if (!input.HasInput) return;

            pintensity = intensity;

            if (ParentGraph != null && ParentGraph.HasParameterValue(Id, "Intensity"))
            {
                pintensity = ParentGraph.GetParameterValue(Id, "Intensity").ToFloat();
            }
        }

        public override void TryAndProcess()
        {
            GetParams();
            Process();
        }

        float pintensity;
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
            processor.Intensity = pintensity;

            processor.Process(i1);
            processor.Complete();

            Output.Data = buffer;
            TriggerTextureChange();
        }

        public class SharpenData : NodeData
        {
            public float intensity;
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
