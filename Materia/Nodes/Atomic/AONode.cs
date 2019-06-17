using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Materia.Imaging;
using Materia.Nodes.Attributes;
using Newtonsoft.Json;
using Materia.Imaging.GLProcessing;
using Materia.Textures;

namespace Materia.Nodes.Atomic
{
    public class AONode : ImageNode
    {
        CancellationTokenSource ctk;
        BlurProcessor blur;
        OcclusionProcessor processor;

        NodeInput input;

        NodeOutput Output;

        int rays;

        [Promote(NodeType.Float)]
        [Slider(IsInt = true, Max = 128, Min = 1, Snap = false, Ticks = new float[0])]
        public int Rays
        {
            get
            {
                return rays;
            }
            set
            {
                rays = value;

                if (rays <= 0) rays = 1;
                TryAndProcess();
            }
        }

        public AONode(int w, int h, GraphPixelType p = GraphPixelType.RGBA)
        {
            Name = "AO";

            Id = Guid.NewGuid().ToString();

            tileX = tileY = 1;

            width = w;
            height = h;
            rays = 4;

            internalPixelType = p;

            previewProcessor = new BasicImageRenderer();
            processor = new OcclusionProcessor();
            blur = new BlurProcessor();

            input = new NodeInput(NodeType.Gray, this, "Gray Input");
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

        public class AOData : NodeData
        {
            public int rays;
        }

        public override void FromJson(string data)
        {
            AOData d = JsonConvert.DeserializeObject<AOData>(data);
            SetBaseNodeDate(d);
            rays = d.rays;
        }

        public override string GetJson()
        {
            AOData d = new AOData();
            FillBaseNodeData(d);
            d.rays = rays;

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

            if(blur != null)
            {
                blur.Release();
                blur = null;
            }
        }

        public override void TryAndProcess()
        {
            if(ctk != null)
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

            float prays = rays;

            if(ParentGraph != null && ParentGraph.HasParameterValue(Id, "Rays"))
            {
                prays = Convert.ToSingle(ParentGraph.GetParameterValue(Id, "Rays"));
            }

            blur.TileX = 1;
            blur.TileY = 1;
            blur.Intensity = (int)prays;
            blur.Process(width, height, i1, buffer);
            blur.Complete();
            processor.TileX = tileX;
            processor.TileY = tileY;
            processor.Process(width, height, buffer, i1, buffer);
            processor.Complete();

            Updated();
            Output.Data = buffer;
            Output.Changed();
        }
    }
}
