using System;
using System.Collections.Generic;
using Materia.Rendering.Attributes;
using Materia.Rendering.Imaging.Processing;
using Newtonsoft.Json;
using Materia.Rendering.Textures;
using Materia.Rendering.Extensions;
using Materia.Graph;

namespace Materia.Nodes.Atomic
{
    public class AONode : ImageNode
    {
        BlurProcessor blur;
        OcclusionProcessor processor;

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

            previewProcessor = new BasicImageRenderer();
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

            if(processor != null)
            {
                processor.Dispose();
                processor = null;
            }

            if(blur != null)
            {
                blur.Dispose();
                blur = null;
            }
        }

        public override void TryAndProcess()
        {
            GetParams();
            Process();
        }

        private void GetParams()
        {
            if (!input.HasInput) return;

            prays = rays;

            if (ParentGraph != null && ParentGraph.HasParameterValue(Id, "Rays"))
            {
                prays = ParentGraph.GetParameterValue(Id, "Rays").ToFloat();
            }
        }

        float prays;
        void Process()
        {
            if (!input.HasInput) return;

            GLTexture2D i1 = (GLTexture2D)input.Reference.Data;

            if (i1 == null) return;
            if (i1.Id == 0) return;

            CreateBufferIfNeeded();

            blur.TileX = 1;
            blur.TileY = 1;
            blur.Intensity = (int)prays;
            
            blur.Process(width, height, i1, buffer);
            blur.Complete();
            
            processor.TileX = tileX;
            processor.TileY = tileY;
            
            processor.Process(width, height, buffer, i1, buffer);
            processor.Complete();

            Output.Data = buffer;
            TriggerTextureChange();
        }
    }
}
