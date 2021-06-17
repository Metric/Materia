using System;
using System.Collections.Generic;
using Materia.Rendering.Attributes;
using Newtonsoft.Json;
using Materia.Rendering.Imaging.Processing;
using Materia.Rendering.Extensions;
using Materia.Graph;
using Materia.Rendering.Mathematics;
using Materia.Rendering.Textures;
using Materia.Graph.IO;

namespace Materia.Nodes.Atomic
{
    public class CircleNode : ImageNode
    {
        CircleProcessor processor;
        protected GLTexture2D buffer2;

        NodeOutput Output;

        protected float radius = 1;
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

        protected float outline = 0;
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

            internalPixelType = p;

            width = w;
            height = h;

            Output = new NodeOutput(NodeType.Gray, this);

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
            if (isDisposing) return;

            CreateBufferIfNeeded();

            processor ??= new CircleProcessor();

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

            public override void Write(Writer w)
            {
                base.Write(w);
                w.Write(radius);
                w.Write(outline);
            }

            public override void Parse(Reader r)
            {
                base.Parse(r);
                radius = r.NextFloat();
                outline = r.NextFloat();
            }
        }

        public override void GetBinary(Writer w)
        {
            CircleData d = new CircleData();
            FillBaseNodeData(d);
            d.radius = radius;
            d.outline = outline;
            d.Write(w);
        }

        public override void FromBinary(Reader r)
        {
            CircleData d = new CircleData();
            d.Parse(r);
            SetBaseNodeDate(d);
            radius = d.radius;
            outline = d.outline;
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
