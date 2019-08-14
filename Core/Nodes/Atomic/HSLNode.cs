using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Materia.Nodes;
using Materia.Imaging;
using Materia.Imaging.GLProcessing;
using System.Threading;
using Materia.Nodes.Attributes;
using Materia.GLInterfaces;
using Materia.Textures;
using Newtonsoft.Json;

namespace Materia.Nodes.Atomic
{
    public class HSLNode : ImageNode 
    {
        CancellationTokenSource ctk;

        NodeInput input;
        NodeOutput Output;

        HSLProcessor processor;

        protected float hue;
        [Promote(NodeType.Float)]
        [Editable(ParameterInputType.FloatSlider, "Hue")]
        public float Hue
        {
            get
            {
                return hue;
            }
            set
            {
                hue = value;
                TryAndProcess();
            }
        }

        protected float saturation;
        [Promote(NodeType.Float)]
        [Editable(ParameterInputType.FloatSlider, "Saturation", "Default", -1, 1)]
        public float Saturation
        {
            get
            {
                return saturation;
            }
            set
            {
                saturation = value;
                TryAndProcess();
            }
        }

        protected float lightness;
        [Promote(NodeType.Float)]
        [Editable(ParameterInputType.FloatSlider, "Lightness", "Default", -1, 1)]
        public float Lightness
        {
            get
            {
                return lightness;
            }
            set
            {
                lightness = value;
                TryAndProcess();
            }
        }

        public HSLNode(int w, int h, GraphPixelType p = GraphPixelType.RGBA)
        {
            Name = "HSL";
            Id = Guid.NewGuid().ToString();
            width = w;
            height = h;

            tileX = tileY = 1;

            previewProcessor = new BasicImageRenderer();
            processor = new HSLProcessor();

            hue = 0;
            saturation = 0;
            lightness = 0;

            internalPixelType = p;

            input = new NodeInput(NodeType.Gray | NodeType.Color, this, "Image");
            Output = new NodeOutput(NodeType.Color, this);

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
                if (input.HasInput)
                {
                    Process();
                }
            }, Context);
        }

        private void GetParams()
        {
            h = hue;
            s = saturation;
            l = lightness;

            if (ParentGraph != null && ParentGraph.HasParameterValue(Id, "Hue"))
            {
                h = Convert.ToSingle(ParentGraph.GetParameterValue(Id, "Hue"));
            }
            if (ParentGraph != null && ParentGraph.HasParameterValue(Id, "Saturation"))
            {
                s = Convert.ToSingle(ParentGraph.GetParameterValue(Id, "Saturation"));
            }
            if (ParentGraph != null && ParentGraph.HasParameterValue(Id, "Lightness"))
            {
                l = Convert.ToSingle(ParentGraph.GetParameterValue(Id, "Lightness"));
            }
        }

        float h;
        float s;
        float l;
        void Process()
        {
            GLTextuer2D i1 = (GLTextuer2D)input.Input.Data;

            if (i1 == null) return;
            if (i1.Id == 0) return;

            if (processor == null) return;

            CreateBufferIfNeeded();

            processor.TileX = tileX;
            processor.TileY = tileY;

            processor.Hue = h * 6.0f;
            processor.Saturation = s;
            processor.Lightness = l;

            processor.Process(width, height, i1, buffer);
            processor.Complete();

            Output.Data = buffer;
            Output.Changed();
            Updated();
        }

        public class HSLData : NodeData
        {
            public float hue;
            public float saturation;
            public float lightness;
        }

        public override void FromJson(string data)
        {
            HSLData d = JsonConvert.DeserializeObject<HSLData>(data);
            SetBaseNodeDate(d);
            hue = d.hue;
            saturation = d.saturation;
            lightness = d.lightness;
        }

        public override string GetJson()
        {
            HSLData d = new HSLData();
            FillBaseNodeData(d);
            d.hue = hue;
            d.saturation = saturation;
            d.lightness = lightness;

            return JsonConvert.SerializeObject(d);
        }

        protected override void OnWidthHeightSet()
        {
            TryAndProcess();
        }
    }
}
