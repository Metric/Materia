using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Materia.Imaging;
using Materia.Nodes.Helpers;
using Materia.Nodes.Attributes;
using System.Drawing;
using Newtonsoft.Json;
using Materia.Textures;
using Materia.Imaging.GLProcessing;

namespace Materia.Nodes.Atomic
{
    public class CircleNode : ImageNode
    {
        protected float radius;
        CircleProcessor processor;

        NodeOutput Output;

        [Slider(IsInt = false, Max = 1, Min = 0.001f, Snap = false, Ticks = new float[0])]
        public float Radius
        {
            get
            {
                return radius;
            }
            set
            {
                radius = value;
                Process();
            }
        }

        protected float outline;

        [Slider(IsInt = false, Max = 1, Min = 0, Snap = false, Ticks = new float[0])]
        public float Outline
        {
            get
            {
                return outline;
            }
            set
            {
                outline = value;
                Process();
            }
        }

        public CircleNode(int w, int h, GraphPixelType p = GraphPixelType.RGBA)
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

            Process();
        }

        protected override void OnWidthHeightSet()
        {
            Process();
        }

        public override void TryAndProcess()
        {
            Process();
        }

        void Process()
        {
            CreateBufferIfNeeded();

            processor.TileX = 1;
            processor.TileY = 1;
            processor.Radius = radius;
            processor.Outline = outline;

            processor.Process(width, height, null, buffer);
            processor.Complete();

            //have to do this to tile properly
            previewProcessor.TileX = tileX;
            previewProcessor.TileY = tileY;

            previewProcessor.Process(width, height, buffer, buffer);
            previewProcessor.Complete();

            previewProcessor.TileX = 1;
            previewProcessor.TileY = 1;

            Updated();
            Output.Data = buffer;
            Output.Changed();
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

        public override void FromJson(Dictionary<string, Node> nodes, string data)
        {
            CircleData d = JsonConvert.DeserializeObject<CircleData>(data);
            SetBaseNodeDate(d);
            radius = d.radius;
            outline = d.outline;

            SetConnections(nodes, d.outputs);

            OnWidthHeightSet();
        }
    }
}
