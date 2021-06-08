using System;
using System.Collections.Generic;
using Materia.Rendering.Attributes;
using Newtonsoft.Json;
using Materia.Rendering.Imaging.Processing;
using Materia.Rendering.Extensions;
using Materia.Graph;
using Materia.Rendering.Mathematics;
using Materia.Rendering.Textures;

namespace Materia.Nodes.Atomic
{
    public class CircleNode : ImageNode
    {
        protected float radius;
        CircleProcessor processor;
        protected GLTexture2D buffer2;

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

        public override void TryAndProcess()
        {
            Process();
        }

        //will probably need to do this
        //so we can do tiling properly
        /*protected override void CreateBufferIfNeeded()
        {
            base.CreateBufferIfNeeded();
            buffer2?.Dispose();
            buffer2 = buffer.Copy();
        }*/

        void Process()
        {
            if (processor == null) return;

            CreateBufferIfNeeded();

            processor.Tiling = GetTiling();
            processor.Radius = GetParameter("Radius", radius);
            processor.Outline = GetParameter("Outline", outline);

            processor.PrepareView(buffer);
            processor.Process();
            processor.Complete();

            //note might need to add back in
            //another processor here
            //for tiling

            Output.Data = buffer;
            TriggerTextureChange();
        }

        public override void Dispose()
        {
            base.Dispose();

            buffer2?.Dispose();
            buffer2 = null;

            processor?.Dispose();
            processor = null;
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
