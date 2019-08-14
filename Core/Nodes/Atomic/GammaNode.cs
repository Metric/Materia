using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Materia.Imaging.GLProcessing;
using Materia.Nodes.Attributes;
using Materia.Textures;
using Newtonsoft.Json;

namespace Materia.Nodes.Atomic
{
    public class GammaNode : ImageNode
    {
        CancellationTokenSource ctk;
        protected NodeInput input;

        protected float gamma;

        [Promote(NodeType.Float)]
        [Editable(ParameterInputType.FloatInput, "Gamma")]
        public float Gamma
        {
            get
            {
                return gamma;
            }
            set
            {
                gamma = value;
                TryAndProcess();
            }
        }

        NodeOutput output;
        GammaProcessor processor;

        public GammaNode(int w, int h, GraphPixelType p = GraphPixelType.RGBA)
        {
            Name = "Gamma";
            Id = Guid.NewGuid().ToString();

            width = w;
            height = h;
            gamma = 2.2f;

            tileX = tileY = 1;

            previewProcessor = new BasicImageRenderer();
            processor = new GammaProcessor();

            internalPixelType = p;

            input = new NodeInput(NodeType.Color | NodeType.Gray, this, "Image Input");
            output = new NodeOutput(NodeType.Color | NodeType.Gray, this);

            input.OnInputAdded += Input_OnInputAdded;
            input.OnInputChanged += Input_OnInputChanged;
            input.OnInputRemoved += Input_OnInputRemoved;

            Inputs = new List<NodeInput>();
            Inputs.Add(input);

            Outputs = new List<NodeOutput>();
            Outputs.Add(output);
        }

        private void Input_OnInputRemoved(NodeInput n)
        {
            output.Data = null;
            output.Changed();
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
            if(!Async)
            {
                if (input.HasInput)
                {
                    GetParams();
                    Process();
                }

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

                if (input.HasInput)
                {
                    if (ParentGraph != null)
                    {
                        ParentGraph.Schedule(this);
                    }
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
                if(input.HasInput)
                {
                    Process();
                }
            }, Context);
        }

        private void GetParams()
        {
            pgamma = gamma;

            if (ParentGraph != null && ParentGraph.HasParameterValue(Id, "Gamma"))
            {
                pgamma = Convert.ToSingle(ParentGraph.GetParameterValue(Id, "Gamma"));
            }
        }

        float pgamma;
        void Process()
        {
            GLTextuer2D i1 = (GLTextuer2D)input.Input.Data;

            if (i1 == null) return;
            if (i1.Id == 0) return;

            CreateBufferIfNeeded();

            processor.TileX = tileX;
            processor.TileY = tileY;
            processor.Gamma = pgamma;
            processor.Process(width, height, i1, buffer);
            processor.Complete();

            Updated();
            output.Data = buffer;
            output.Changed();
        }

        public class GammaData : NodeData
        {
            public float gamma;
        }

        public override string GetJson()
        {
            GammaData d = new GammaData();
            FillBaseNodeData(d);
            d.gamma = gamma;

            return JsonConvert.SerializeObject(d);
        }

        public override void FromJson(string data)
        {
            GammaData d = JsonConvert.DeserializeObject<GammaData>(data);
            SetBaseNodeDate(d);
            gamma = d.gamma;
        }

        protected override void OnWidthHeightSet()
        {
            TryAndProcess();
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
