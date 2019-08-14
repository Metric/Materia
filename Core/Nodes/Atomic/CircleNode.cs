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

        CancellationTokenSource ctk;

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
                TryAndProcess();
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
                TryAndProcess();
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
            TryAndProcess();
        }

        public override void TryAndProcess()
        {
            if(!Async)
            {
                GetParams();
                Process();
                return;
            }

            //if (ctk != null)
            //{
            //    ctk.Cancel();
            //}

            //ctk = new CancellationTokenSource();

            //Task.Delay(25, ctk.Token).ContinueWith(t =>
            //{
            //    if (t.IsCanceled) return;

                if (ParentGraph != null)
                {
                    ParentGraph.Schedule(this);
                }
            //}, Context);
        }

        public override Task GetTask()
        {
            return Task.Factory.StartNew(() =>
            {
                GetParams();
            })
            .ContinueWith(t =>
            {
                Process();
            }, Context);
        }

        private void GetParams()
        {
            pradius = radius;
            poutline = outline;

            if (ParentGraph != null && ParentGraph.HasParameterValue(Id, "Radius"))
            {
                pradius = Convert.ToSingle(ParentGraph.GetParameterValue(Id, "Radius"));
            }
            if (ParentGraph != null && ParentGraph.HasParameterValue(Id, "Outline"))
            {
                poutline = Convert.ToSingle(ParentGraph.GetParameterValue(Id, "Outline"));
            }
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

        public override void FromJson(string data)
        {
            CircleData d = JsonConvert.DeserializeObject<CircleData>(data);
            SetBaseNodeDate(d);
            radius = d.radius;
            outline = d.outline;
        }
    }
}
