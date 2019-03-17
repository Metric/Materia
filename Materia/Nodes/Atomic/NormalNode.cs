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

        [Slider(IsInt = false, Max = 32, Min = 0.001f, Snap = false, Ticks = new float[0])]
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
            if(input.HasInput)
            {
                Process();
            }
        }

        void Process() 
        {
            GLTextuer2D i1 = (GLTextuer2D)input.Input.Data;

            if (i1 == null) return;
            if (i1.Id == 0) return;

            CreateBufferIfNeeded();

            processor.TileX = tileX;
            processor.TileY = tileY;
            processor.DirectX = directx;
            processor.Intensity = intensity;
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

        public override void FromJson(Dictionary<string, Node> nodes, string data)
        {
            NormalData d = JsonConvert.DeserializeObject<NormalData>(data);
            SetBaseNodeDate(d);
            intensity = d.intensity;
            directx = d.directx;

            SetConnections(nodes, d.outputs);

            OnWidthHeightSet();
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
