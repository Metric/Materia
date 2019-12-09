using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Materia.Imaging;
using Materia.Nodes.Helpers;
using Newtonsoft.Json;
using Materia.Nodes.Attributes;
using Materia.Imaging.GLProcessing;
using Materia.Textures;

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

            previewProcessor = new BasicImageRenderer();
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
                pintensity = Utils.ConvertToFloat(ParentGraph.GetParameterValue(Id, "Intensity"));
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
            if (!input.HasInput) return;

            GLTextuer2D i1 = (GLTextuer2D)input.Reference.Data;

            if (i1 == null) return;
            if (i1.Id == 0) return;

            CreateBufferIfNeeded();

            processor.TileX = 1;
            processor.TileY = 1;

            processor.Intensity = pintensity;
            processor.Process(width, height, i1, buffer);
            processor.Complete();

            previewProcessor.TileX = tileX;
            previewProcessor.TileY = tileY;

            previewProcessor.Process(width, height, buffer, buffer);
            previewProcessor.Complete();

            previewProcessor.TileY = 1;
            previewProcessor.TileX = 1;

            Output.Data = buffer;
            TriggerTextureChange();
        }

        public override void Dispose()
        {
            base.Dispose();

            if(processor != null)
            {
                processor.Release();
                processor = null;
            }
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
