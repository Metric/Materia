using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Materia.Imaging.GLProcessing;
using Materia.Nodes.Attributes;
using Materia.Textures;
using Newtonsoft.Json;

namespace Materia.Nodes.Atomic
{
    public class SharpenNode : ImageNode
    {
        CancellationTokenSource ctk;

        NodeInput input;
        NodeOutput Output;

        SharpenProcessor processor;

        protected float intensity;
        [Promote(NodeType.Float)]
        [Editable(ParameterInputType.FloatSlider, "Intensity", "Default", 1, 10)]
        public float Intensity
        {
            get
            {
                return intensity;
            }
            set
            {
                intensity = value;
                TryAndProcess();
            }
        }

        public SharpenNode(int w, int h, GraphPixelType p = GraphPixelType.RGBA)
        {
            Name = "Sharpen";
            Id = Guid.NewGuid().ToString();
            width = w;
            height = h;

            tileX = tileY = 1;

            previewProcessor = new BasicImageRenderer();
            processor = new SharpenProcessor();

            intensity = 1;

            internalPixelType = p;

            input = new NodeInput(NodeType.Gray | NodeType.Color, this, "Image");
            Output = new NodeOutput(NodeType.Gray | NodeType.Color, this);

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
                if(input.HasInput)
                {
                    Process();
                }
            }, Context);
        }

        private void GetParams()
        {
            pintensity = intensity;

            if (ParentGraph != null && ParentGraph.HasParameterValue(Id, "Intensity"))
            {
                pintensity = Convert.ToSingle(ParentGraph.GetParameterValue(Id, "Intensity"));
            }
        }

        float pintensity;
        void Process()
        {
            GLTextuer2D i1 = (GLTextuer2D)input.Input.Data;

            if (i1 == null) return;
            if (i1.Id == 0) return;

            if (processor == null) return;

            CreateBufferIfNeeded();

            processor.TileX = tileX;
            processor.TileY = tileY;

            processor.Intensity = pintensity;

            processor.Process(width, height, i1, buffer);
            processor.Complete();

            Output.Data = buffer;
            Output.Changed();
            Updated();
        }

        public class SharpenData : NodeData
        {
            public float intensity;
        }

        public override void FromJson(string data)
        {
            SharpenData d = JsonConvert.DeserializeObject<SharpenData>(data);
            SetBaseNodeDate(d);
            intensity = d.intensity;
        }

        public override string GetJson()
        {
            SharpenData d = new SharpenData();
            FillBaseNodeData(d);
            d.intensity = intensity;

            return JsonConvert.SerializeObject(d);
        }

        protected override void OnWidthHeightSet()
        {
            TryAndProcess();
        }
    }
}
