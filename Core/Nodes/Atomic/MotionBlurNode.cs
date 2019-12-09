using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Materia.Imaging.GLProcessing;
using Materia.Nodes.Attributes;
using Materia.Textures;
using Materia.Math3D;
using Newtonsoft.Json;
using System.Threading;
using Materia.Nodes.Helpers;

namespace Materia.Nodes.Atomic
{
    public class MotionBlurNode : ImageNode
    {
        MotionBlurProcessor processor;

        int magnitude;
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

        int direction;
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

            Id = Guid.NewGuid().ToString();

            width = w;
            height = h;

            internalPixelType = p;

            previewProcessor = new BasicImageRenderer();
            processor = new MotionBlurProcessor();

            tileX = tileY = 1;

            direction = 0;
            magnitude = 10;

            input = new NodeInput(NodeType.Color | NodeType.Gray, this, "Image Input");
            Inputs.Add(input);
            output = new NodeOutput(NodeType.Color | NodeType.Gray, this);
            Outputs.Add(output);
        }

        private void GetParams()
        {
            if (!input.HasInput) return;

            pintensity = magnitude;
            pdirection = direction;

            if (ParentGraph != null && ParentGraph.HasParameterValue(Id, "Intensity"))
            {
                pintensity = Utils.ConvertToFloat(ParentGraph.GetParameterValue(Id, "Intensity"));
            }

            if (ParentGraph != null && ParentGraph.HasParameterValue(Id, "Direction"))
            {
                pdirection = Utils.ConvertToFloat(ParentGraph.GetParameterValue(Id, "Direction"));
            }
        }

        public override void TryAndProcess()
        {
            GetParams();
            Process();
        }

        float pintensity;
        float pdirection;
        void Process()
        {
            if (!input.HasInput) return;

            GLTextuer2D i1 = (GLTextuer2D)input.Reference.Data;

            if (i1 == null) return;

            CreateBufferIfNeeded();

            processor.TileX = 1;
            processor.TileY = 1;
            processor.Direction = (float)pdirection * (float)(Math.PI / 180.0f);
            processor.Magnitude = pintensity;
            processor.Process(width, height, i1, buffer);
            processor.Complete();

            previewProcessor.TileX = tileX;
            previewProcessor.TileY = tileY;
            previewProcessor.Process(width, height, buffer, buffer);
            previewProcessor.Complete();
            previewProcessor.TileX = 1;
            previewProcessor.TileY = 1;

            output.Data = buffer;
            TriggerTextureChange();
        }

        public class MotionBlurData : NodeData
        {
            public int intensity;
            public int direction;
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

            if(processor != null)
            {
                processor.Release();
            }
        }
    }
}
