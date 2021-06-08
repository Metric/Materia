using System;
using Materia.Rendering.Attributes;
using Newtonsoft.Json;
using Materia.Rendering.Imaging.Processing;
using Materia.Rendering.Textures;
using Materia.Rendering.Extensions;
using Materia.Graph;
using Materia.Rendering.Mathematics;

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

                TriggerValueChange();
            }
        }

        bool directx;
        [Promote(NodeType.Bool)]
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
                TriggerValueChange();
            }
        }

        float noiseReduction;
        [Promote(NodeType.Float)]
        [Editable(ParameterInputType.FloatInput, "Noise Reduction")]
        public float NoiseReduction
        {
            get
            {
                return noiseReduction;
            }
            set
            {
                noiseReduction = value;
                TriggerValueChange();
            }
        }

        public NormalNode(int w, int h, GraphPixelType p = GraphPixelType.RGBA) : base()
        {
            Name = "Normal";
            Id = Guid.NewGuid().ToString();

            directx = false;
            noiseReduction = 0.004f;

            width = w;
            height = h;

            intensity = 8;

            tileX = tileY = 1;

            processor = new NormalsProcessor();

            internalPixelType = p;

            input = new NodeInput(NodeType.Gray, this, "Gray Input");
            Output = new NodeOutput(NodeType.Color, this);

            Inputs.Add(input);
            Outputs.Add(Output);
        }

        public override void TryAndProcess()
        {
            Process();
        }

        void Process() 
        {
            if (processor == null) return;
            if (!input.HasInput) return;

            GLTexture2D i1 = (GLTexture2D)input.Reference.Data;

            if (i1 == null) return;
            if (i1.Id == 0) return;

            CreateBufferIfNeeded();

            processor.Tiling = GetTiling();
            processor.DirectX = GetParameter("DirectX", directx);
            processor.NoiseReduction = GetParameter("NoiseReduction", noiseReduction);
            processor.Intensity = GetParameter("Intensity", intensity);

            processor.PrepareView(buffer);
            processor.Process(i1);
            processor.Complete();

            Output.Data = buffer;
            TriggerTextureChange();
        }

        public class NormalData : NodeData
        {
            public float intensity;
            public bool directx;
            public float noiseReduction = 0.004f;
        }

        public override string GetJson()
        {
            NormalData d = new NormalData();
            FillBaseNodeData(d);
            d.intensity = intensity;
            d.directx = directx;
            d.noiseReduction = noiseReduction;

            return JsonConvert.SerializeObject(d);
        }

        public override void FromJson(string data)
        {
            NormalData d = JsonConvert.DeserializeObject<NormalData>(data);
            SetBaseNodeDate(d);
            intensity = d.intensity;
            directx = d.directx;
            noiseReduction = d.noiseReduction;
        }

        public override void Dispose()
        {
            base.Dispose();

            processor?.Dispose();
            processor = null;
        }
    }
}
