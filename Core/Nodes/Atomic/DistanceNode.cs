using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Materia.Imaging.GLProcessing;
using Materia.Textures;
using Materia.Nodes.Attributes;
using Newtonsoft.Json;

namespace Materia.Nodes.Atomic
{
    public class DistanceNode : ImageNode
    {
        NodeInput input;
        NodeOutput Output;

        DistanceProcessor processor;

        protected float distance;
        [Promote(NodeType.Float)]
        [Editable(ParameterInputType.FloatSlider, "Max Distance", "Default")]
        public float MaxDistance
        {
            get
            {
                return distance;
            }
            set
            {
                distance = value;
                TryAndProcess();
            }
        }

        public DistanceNode(int w, int h, GraphPixelType p = GraphPixelType.RGBA)
        {
            Name = "Distance";
            Id = Guid.NewGuid().ToString();
            width = w;
            height = h;

            tileX = tileY = 1;
            distance = 0.2f;

            previewProcessor = new BasicImageRenderer();
            processor = new DistanceProcessor();

            internalPixelType = p;

            input = new NodeInput(NodeType.Gray, this, "Mask");
            Output = new NodeOutput(NodeType.Gray, this);

            input.OnInputAdded += Input_OnInputAdded;
            input.OnInputChanged += Input_OnInputChanged;

            Inputs = new List<NodeInput>();
            Inputs.Add(input);

            Outputs = new List<NodeOutput>();
            Outputs.Add(Output);
        }

        private void Input_OnInputChanged(NodeInput n)
        {
            TryAndProcess();
        }

        private void Input_OnInputAdded(NodeInput n)
        {
            TryAndProcess();
        }

        public override void TryAndProcess()
        {
            if (!Async)
            {
                if (input.HasInput)
                {
                    GetParams();
                    Process();
                }

                return;
            }

            if (input.HasInput)
            {
                if (ParentGraph != null)
                {
                    ParentGraph.Schedule(this);
                }
            }
        }

        public override void Dispose()
        {
            base.Dispose();

            if (processor != null)
            {
                processor.Release();
                processor = null;
            }
        }

        public override Task GetTask()
        {
            return Task.Factory.StartNew(() =>
            {
                GetParams();
            })
            .ContinueWith(t =>
            {
                if (input.HasInput)
                {
                    Process();
                }
            }, Context);
        }

        void GetParams()
        {
            pmaxDistance = distance;

            if(ParentGraph != null && ParentGraph.HasParameterValue(Id, "MaxDistance"))
            {
                pmaxDistance = Convert.ToSingle(ParentGraph.GetParameterValue(Id, "MaxDistance"));
            }
        }

        float pmaxDistance;
        void Process()
        {
            GLTextuer2D i1 = (GLTextuer2D)input.Input.Data;

            if (i1 == null) return;
            if (i1.Id == 0) return;

            if (processor == null) return;

            CreateBufferIfNeeded();

            processor.TileX = tileX;
            processor.TileY = tileY;

            processor.Distance = pmaxDistance;
            processor.Process(width, height, i1, null, buffer);
            processor.Complete();

            Output.Data = buffer;
            Output.Changed();
            Updated();
        }

        protected override void OnWidthHeightSet()
        {
            TryAndProcess();
        }

        public class DistanceNodeData : NodeData
        {
            public float maxDistance;
        }

        public override string GetJson()
        {
            DistanceNodeData d = new DistanceNodeData();
            FillBaseNodeData(d);
            d.maxDistance = distance;
            return JsonConvert.SerializeObject(d);
        }

        public override void FromJson(string data)
        {
            DistanceNodeData d = JsonConvert.DeserializeObject<DistanceNodeData>(data);
            SetBaseNodeDate(d);
            distance = d.maxDistance;
        }
    }
}
