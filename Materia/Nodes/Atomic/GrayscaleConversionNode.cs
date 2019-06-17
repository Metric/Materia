using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Materia.Imaging;
using Materia.Nodes.Attributes;
using System.Threading;
using Materia.Nodes.Helpers;
using Materia.Imaging.GLProcessing;
using Materia.Textures;
using Newtonsoft.Json;

namespace Materia.Nodes.Atomic
{
    public class GrayscaleConversionNode : ImageNode
    {
        CancellationTokenSource ctk;

        NodeInput input;

        GrayscaleConvProcessor processor;

        NodeOutput Output;

        float r;
        [Slider(IsInt = false, Max = 1, Min = 0, Snap = false, Ticks = new float[0])]
        public float Red
        {
            get
            {
                return r;
            }
            set
            {
                r = value;
                TryAndProcess();
            }
        }

        float g;
        [Slider(IsInt = false, Max = 1, Min = 0, Snap = false, Ticks = new float[0])]
        public float Green
        {
            get
            {
                return g;
            }
            set
            {
                g = value;
                TryAndProcess();
            }
        }

        float b;
        [Slider(IsInt = false, Max = 1, Min = 0, Snap = false, Ticks = new float[0])]
        public float Blue
        {
            get
            {
                return b;
            }
            set
            {
                b = value;
                TryAndProcess();
            }
        }

        float a;
        [Slider(IsInt = false, Max = 1, Min = 0, Snap = false, Ticks = new float[0])]
        public float Alpha
        {
            get
            {
                return a;
            }
            set
            {
                a = value;
                TryAndProcess();
            }
        }

        public GrayscaleConversionNode(int w, int h, GraphPixelType p = GraphPixelType.RGBA)
        {
            Name = "Grayscale Conversion";

            Id = Guid.NewGuid().ToString();

            width = w;
            height = h;

            r = 1;
            g = 1;
            b = 1;
            a = 0;

            tileX = tileY = 1;

            previewProcessor = new BasicImageRenderer();
            processor = new GrayscaleConvProcessor();

            internalPixelType = p;

            input = new NodeInput(NodeType.Color, this);
            Output = new NodeOutput(NodeType.Gray, this);

            input.OnInputAdded += Input_OnInputAdded;
            input.OnInputChanged += Input_OnInputChanged;
            input.OnInputRemoved += Input_OnInputRemoved;

            Inputs = new List<NodeInput>();
            Inputs.Add(input);

            Outputs = new List<NodeOutput>();
            Outputs.Add(Output);
        }

        private void Input_OnInputRemoved(NodeInput n)
        {
            Output.Data = null;
            Output.Changed();
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
            if (ctk != null)
            {
                ctk.Cancel();
            }

            ctk = new CancellationTokenSource();

            Task.Delay(100, ctk.Token).ContinueWith(t =>
            {
                if (t.IsCanceled) return;

                App.Current.Dispatcher.Invoke(() =>
                {
                    if (input.HasInput)
                    {
                        Process();
                    }
                });
            });
        }

        void Process()
        {
            GLTextuer2D i1 = (GLTextuer2D)input.Input.Data;

            if (i1 == null) return;
            if (i1.Id == 0) return;

            CreateBufferIfNeeded();

            processor.TileX = tileX;
            processor.TileY = tileY;
            processor.Weight = new OpenTK.Vector4(r, g, b, a);
            processor.Process(width, height, i1, buffer);
            processor.Complete();

            Updated();
            Output.Data = buffer;
            Output.Changed();
        }

        public class GrayscaleConversionNodeData : NodeData
        {
            public float red;
            public float green;
            public float blue;
            public float alpha;
        }

        public override void FromJson(string data)
        {
            GrayscaleConversionNodeData d = JsonConvert.DeserializeObject<GrayscaleConversionNodeData>(data);
            SetBaseNodeDate(d);
            r = d.red;
            g = d.green;
            b = d.blue;
            a = d.alpha;
        }

        public override string GetJson()
        {
            GrayscaleConversionNodeData d = new GrayscaleConversionNodeData();
            FillBaseNodeData(d);
            d.red = r;
            d.green = g;
            d.blue = b;
            d.alpha = a;

            return JsonConvert.SerializeObject(d);
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
