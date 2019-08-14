using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Materia.Textures;
using Materia.Imaging.GLProcessing;
using Newtonsoft.Json;
using Materia.Nodes.Attributes;

namespace Materia.Nodes.Atomic
{
    public class WarpNode : ImageNode
    {
        CancellationTokenSource ctk;

        protected WarpProcessor processor;

        protected NodeInput input;
        protected NodeInput input1;

        protected NodeOutput output;

        protected float intensity;

        [Promote(NodeType.Float)]
        [Editable(ParameterInputType.FloatSlider, "Intensity")]
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

        public WarpNode(int w, int h, GraphPixelType p = GraphPixelType.RGBA)
        {
            Name = "Warp";
            Id = Guid.NewGuid().ToString();

            width = w;
            height = h;

            intensity = 1;

            tileX = tileY = 1;

            previewProcessor = new BasicImageRenderer();
            processor = new WarpProcessor();

            internalPixelType = p;

            input = new NodeInput(NodeType.Gray | NodeType.Color, this, "Image Input");
            input1 = new NodeInput(NodeType.Gray, this, "Grayscale Gradients");

            output = new NodeOutput(NodeType.Gray | NodeType.Color, this);

            input.OnInputAdded += Input_OnInputAdded;
            input.OnInputChanged += Input_OnInputChanged;
            input.OnInputRemoved += Input_OnInputRemoved;

            input1.OnInputAdded += Input_OnInputAdded;
            input1.OnInputChanged += Input_OnInputChanged;
            input1.OnInputRemoved += Input_OnInputRemoved;

            Inputs = new List<NodeInput>();
            Outputs = new List<NodeOutput>();

            Inputs.Add(input);
            Inputs.Add(input1);
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
                if (input.HasInput && input1.HasInput)
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

                if (input.HasInput && input1.HasInput)
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
                if(input.HasInput && input1.HasInput)
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
            GLTextuer2D i2 = (GLTextuer2D)input1.Input.Data;

            if (i1 == null) return;
            if (i1.Id == 0) return;

            if (i2 == null) return;
            if (i2.Id == 0) return;

            CreateBufferIfNeeded();

            processor.TileX = tileX;
            processor.TileY = TileY;
            processor.Intensity = pintensity;
            processor.Process(width, height, i1, i2, buffer);
            processor.Complete();

            Updated();
            output.Data = buffer;
            output.Changed();
        }

        public class WarpData : NodeData
        {
            public float intensity;
        }

        public override string GetJson()
        {
            WarpData d = new WarpData();
            FillBaseNodeData(d);
            d.intensity = intensity;

            return JsonConvert.SerializeObject(d);
        }

        public override void FromJson(string data)
        {
            WarpData d = JsonConvert.DeserializeObject<WarpData>(data);
            SetBaseNodeDate(d);
            intensity = d.intensity;
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
