using System;
using System.Collections.Generic;
using Materia.Rendering.Attributes;
using Newtonsoft.Json;
using Materia.Rendering.Imaging.Processing;
using Materia.Rendering.Extensions;
using Materia.Graph;

namespace Materia.Nodes.Atomic
{
    public class CircleNode : ImageNode
    {
        protected float radius;
        CircleProcessor processor;

        NodeOutput Output;

        [Promote(NodeType.Float)]
        [Editable(ParameterInputType.FloatSlider, "Radius")]
        public float Radius
        {
            get
            {
                return radius;
            }
            set
            {
                radius = value;
                TriggerValueChange();
            }
        }

        protected float outline;

        [Promote(NodeType.Float)]
        [Editable(ParameterInputType.FloatSlider, "Outline")]
        public float Outline
        {
            get
            {
                return outline;
            }
            set
            {
                outline = value;
                TriggerValueChange();
            }
        }

        public CircleNode(int w, int h, GraphPixelType p = GraphPixelType.RGBA) : base()
        {
            Name = "Circle";

            Id = Guid.NewGuid().ToString();

            Inputs = new List<NodeInput>();

            tileX = tileY = 1;

            previewProcessor = new BasicImageRenderer();

            processor = new CircleProcessor();

            outline = 0;

            internalPixelType = p;

            width = w;
            height = h;

            Output = new NodeOutput(NodeType.Gray, this);

            radius = 1;

            Outputs = new List<NodeOutput>();
            Outputs.Add(Output);
        }

        private void GetParams()
        {
            pradius = radius;
            poutline = outline;

            if (ParentGraph != null && ParentGraph.HasParameterValue(Id, "Radius"))
            {
                pradius = ParentGraph.GetParameterValue(Id, "Radius").ToFloat();
            }
            if (ParentGraph != null && ParentGraph.HasParameterValue(Id, "Outline"))
            {
                poutline = ParentGraph.GetParameterValue(Id, "Outline").ToFloat();
            }
        }

        public override void TryAndProcess()
        {
            GetParams();
            Process();
        }

        float pradius;
        float poutline;
        void Process()
        {
            CreateBufferIfNeeded();

            processor.TileX = 1;
            processor.TileY = 1;
            processor.Radius = pradius;
            processor.Outline = poutline;

            processor.Process(width, height, null, buffer);
            processor.Complete();

            //have to do this to tile properly
            previewProcessor.TileX = tileX;
            previewProcessor.TileY = tileY;

            previewProcessor.Process(width, height, buffer, buffer);
            previewProcessor.Complete();

            previewProcessor.TileX = 1;
            previewProcessor.TileY = 1;

            Output.Data = buffer;
            TriggerTextureChange();
        }

        public override void Dispose()
        {
            base.Dispose();

            if(processor != null)
            {
                processor.Dispose();
                processor = null;
            }
        }

        public class CircleData : NodeData
        {
            public float radius;
            public float outline;
        }

        public override string GetJson()
        {
            CircleData d = new CircleData();
            FillBaseNodeData(d);
            d.radius = radius;
            d.outline = outline;

            return JsonConvert.SerializeObject(d);
        }

        public override void FromJson(string data)
        {
            CircleData d = JsonConvert.DeserializeObject<CircleData>(data);
            SetBaseNodeDate(d);
            radius = d.radius;
            outline = d.outline;
        }
    }
}
