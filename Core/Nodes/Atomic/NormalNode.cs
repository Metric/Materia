using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Materia.Imaging;
using Materia.Nodes.Helpers;
using Materia.Nodes.Attributes;
using Newtonsoft.Json;
using Materia.Imaging.GLProcessing;
using Materia.Textures;

namespace Materia.Nodes.Atomic
{
    public class NormalNode : ImageNode
    {
        protected NodeInput input;

        protected float intensity;

        NodeOutput Output;

        NormalsProcessor processor;

        [Promote(NodeType.Float)]
        [Editable(ParameterInputType.FloatSlider, "Intensity", "Default", 0.001f, 32)]
        public float Intensity
        {
            get
            {
                return intensity;
            }
            set
            {
                intensity = value;

                if(intensity <= 0)
                {
                    intensity = 0.001f;
                }

                TryAndProcess();
            }
        }

        bool directx;
        [Editable(ParameterInputType.Toggle, "DirectX")]
        public bool DirectX
        {
            get
            {
                return directx;
            }
            set
            {
                directx = value;
                TryAndProcess();
            }
        }

        public NormalNode(int w, int h, GraphPixelType p = GraphPixelType.RGBA)
        {
            Name = "Normal";
            Id = Guid.NewGuid().ToString();

            width = w;
            height = h;

            intensity = 8;

            tileX = tileY = 1;

            previewProcessor = new BasicImageRenderer();
            processor = new NormalsProcessor();

            internalPixelType = p;

            input = new NodeInput(NodeType.Gray, this, "Gray Input");
            Output = new NodeOutput(NodeType.Color, this);

            input.OnInputAdded += Input_OnInputAdded;
            input.OnInputRemoved += Input_OnInputRemoved;
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

        private void Input_OnInputRemoved(NodeInput n)
        {
            Output.Data = null;
            Output.Changed();
        }

        private void Input_OnInputAdded(NodeInput n)
        {
            TryAndProcess();
        }

        protected override void OnWidthHeightSet()
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

            if (input.HasInput)
            {
                if (ParentGraph != null)
                {
                    ParentGraph.Schedule(this);
                }
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

            CreateBufferIfNeeded();

            processor.TileX = tileX;
            processor.TileY = tileY;
            processor.DirectX = directx;
            processor.Intensity = pintensity;
            processor.Process(width, height, i1, buffer);
            processor.Complete();

            Updated();
            Output.Data = buffer;
            Output.Changed();
        }

        public class NormalData : NodeData
        {
            public float intensity;
            public bool directx;
        }

        public override string GetJson()
        {
            NormalData d = new NormalData();
            FillBaseNodeData(d);
            d.intensity = intensity;
            d.directx = directx;

            return JsonConvert.SerializeObject(d);
        }

        public override void FromJson(string data)
        {
            NormalData d = JsonConvert.DeserializeObject<NormalData>(data);
            SetBaseNodeDate(d);
            intensity = d.intensity;
            directx = d.directx;
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
