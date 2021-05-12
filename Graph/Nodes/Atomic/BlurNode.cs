using System;
using Newtonsoft.Json;
using Materia.Rendering.Imaging.Processing;
using Materia.Rendering.Textures;
using Materia.Rendering.Attributes;
using Materia.Rendering.Extensions;
using Materia.Graph;
using Materia.Rendering.Mathematics;

namespace Materia.Nodes.Atomic
{
    public class BlurNode : ImageNode
    {
        NodeInput input;

        int intensity;

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

            Id = Guid.NewGuid().ToString();

            tileX = tileY = 1;

            width = w;
            height = h;

            intensity = 10;

            internalPixelType = p;

            processor = new BlurProcessor();

            input = new NodeInput(NodeType.Color | NodeType.Gray, this, "Image Input");
            Output = new NodeOutput(NodeType.Color | NodeType.Gray, this);

            Inputs.Add(input);
            Outputs.Add(Output);
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
