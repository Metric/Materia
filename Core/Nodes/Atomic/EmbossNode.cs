using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Materia.Nodes.Attributes;
using Materia.Imaging;
using Materia.Nodes.Helpers;
using Materia.Textures;
using Materia.Imaging.GLProcessing;
using Newtonsoft.Json;

namespace Materia.Nodes.Atomic
{
    public class EmbossNode : ImageNode
    {
        NodeInput input;

        int angle;

        NodeOutput Output;

        EmbossProcessor processor;

        [Promote(NodeType.Float)]
        [Editable(ParameterInputType.IntSlider, "Angle", "Default", 0, 360)]
        public int Angle
        {
            get
            {
                return angle;
            }
            set
            {
                angle = value;
                TriggerValueChange();
            }
        }

        int elevation;

        [Promote(NodeType.Float)]
        [Editable(ParameterInputType.IntSlider, "Elevation", "Default", 0, 90)]
        public int Elevation
        {
            get
            {
                return elevation;
            }
            set
            {
                elevation = value;
                TriggerValueChange();
            }
        }

        public EmbossNode(int w, int h, GraphPixelType p = GraphPixelType.RGBA)
        {
            Name = "Emboss";

            Id = Guid.NewGuid().ToString();

            width = w;
            height = h;

            elevation = 2;
            angle = 0;

            tileX = tileY = 1;

            processor = new EmbossProcessor();

            previewProcessor = new BasicImageRenderer();

            internalPixelType = p;

            input = new NodeInput(NodeType.Color | NodeType.Gray, this, "Image Input");
            Output = new NodeOutput(NodeType.Gray, this);

            Inputs.Add(input);
            Outputs.Add(Output);
        }

        public class EmbossNodeData : NodeData
        {
            public int angle;
            public int elevation;
        }

        public override void FromJson(string data)
        {
            EmbossNodeData d = JsonConvert.DeserializeObject<EmbossNodeData>(data);
            SetBaseNodeDate(d);
            angle = d.angle;
            elevation = d.elevation;
        }

        public override string GetJson()
        {
            EmbossNodeData d = new EmbossNodeData();

            FillBaseNodeData(d);
            d.angle = angle;
            d.elevation = elevation;

            return JsonConvert.SerializeObject(d);
        }

        private void GetParams()
        {
            if (!input.HasInput) return;

            pangle = angle;
            pelevation = elevation;

            if (ParentGraph != null && ParentGraph.HasParameterValue(Id, "Angle"))
            {
                pangle = Utils.ConvertToFloat(ParentGraph.GetParameterValue(Id, "Angle"));
            }

            if (ParentGraph != null && ParentGraph.HasParameterValue(Id, "Elevation"))
            {
                pelevation = Utils.ConvertToFloat(ParentGraph.GetParameterValue(Id, "Elevation"));
            }
        }

        public override void TryAndProcess()
        {
            GetParams();
            Process();
        }

        float pangle;
        float pelevation;
        void Process()
        {
            if (!input.HasInput) return;

            GLTextuer2D i1 = (GLTextuer2D)input.Reference.Data;

            if (i1 == null) return;
            if (i1.Id == 0) return;

            CreateBufferIfNeeded();

            processor.TileX = tileX;
            processor.TileY = tileY;
            processor.Azimuth = pangle * (float)(Math.PI / 180.0f);
            processor.Elevation = pelevation * (float)(Math.PI / 180.0f);
            processor.Process(width, height, i1, buffer);
            processor.Complete();

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
    }
}
