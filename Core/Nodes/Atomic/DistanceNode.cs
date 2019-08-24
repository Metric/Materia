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
        NodeInput input2;
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

        protected bool sourceOnly;
        [Promote(NodeType.Bool)]
        [Editable(ParameterInputType.Toggle, "Source Only")]
        public bool SourceOnly
        {
            get
            {
                return sourceOnly;
            }
            set
            {
                sourceOnly = value;
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
            input2 = new NodeInput(NodeType.Gray | NodeType.Color, this, "Source");
            Output = new NodeOutput(NodeType.Gray, this);

            input.OnInputAdded += Input_OnInputAdded;
            input.OnInputChanged += Input_OnInputChanged;
            input2.OnInputAdded += Input_OnInputAdded;
            input2.OnInputChanged += Input_OnInputChanged;

            Inputs = new List<NodeInput>();
            Inputs.Add(input);
            Inputs.Add(input2);

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
            psourceonly = sourceOnly;

            if(ParentGraph != null && ParentGraph.HasParameterValue(Id, "MaxDistance"))
            {
                pmaxDistance = Convert.ToSingle(ParentGraph.GetParameterValue(Id, "MaxDistance"));
            }

            if (ParentGraph != null && ParentGraph.HasParameterValue(Id, "SourceOnly"))
            {
                psourceonly = Convert.ToBoolean(ParentGraph.GetParameterValue(Id, "SourceOnly"));
            }
        }

        float pmaxDistance;
        bool psourceonly;
        void Process()
        {
            GLTextuer2D i1 = (GLTextuer2D)input.Input.Data;
            GLTextuer2D i2 = null;

            if(input2.HasInput)
            {
                i2 = (GLTextuer2D)input2.Input.Data;
            }

            if (i1 == null) return;
            if (i1.Id == 0) return;

            if (processor == null) return;

            CreateBufferIfNeeded();

            processor.TileX = tileX;
            processor.TileY = tileY;

            processor.SourceOnly = psourceonly;
            processor.Distance = pmaxDistance;
            processor.Process(width, height, i1, i2, buffer);
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
            public bool sourceOnly;
        }

        public override string GetJson()
        {
            DistanceNodeData d = new DistanceNodeData();
            FillBaseNodeData(d);
            d.maxDistance = distance;
            d.sourceOnly = sourceOnly;
            return JsonConvert.SerializeObject(d);
        }

        public override void FromJson(string data)
        {
            DistanceNodeData d = JsonConvert.DeserializeObject<DistanceNodeData>(data);
            SetBaseNodeDate(d);
            distance = d.maxDistance;
            sourceOnly = d.sourceOnly;
        }
    }
}
