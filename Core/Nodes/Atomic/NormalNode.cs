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

            previewProcessor = new BasicImageRenderer();
            processor = new NormalsProcessor();

            internalPixelType = p;

            input = new NodeInput(NodeType.Gray, this, "Gray Input");
            Output = new NodeOutput(NodeType.Color, this);

            Inputs.Add(input);
            Outputs.Add(Output);
        }

        private void GetParams()
        {
            if (!input.HasInput) return;

            pintensity = intensity;
            pnoiseReduction = noiseReduction;
            pdirectx = directx;

            if (ParentGraph != null && ParentGraph.HasParameterValue(Id, "Intensity"))
            {
                pintensity = Utils.ConvertToFloat(ParentGraph.GetParameterValue(Id, "Intensity"));
            }

            if (ParentGraph != null && ParentGraph.HasParameterValue(Id, "DirectX"))
            {
                pdirectx = Utils.ConvertToBool(ParentGraph.GetParameterValue(Id, "DirectX"));
            }

            if (ParentGraph != null && ParentGraph.HasParameterValue(Id, "NoiseReduction"))
            {
                pnoiseReduction = Utils.ConvertToFloat(ParentGraph.GetParameterValue(Id, "NoiseReduction"));
            }
        }

        public override void TryAndProcess()
        {
            GetParams();
            Process();
        }

        float pintensity;
        float pnoiseReduction;
        bool pdirectx;
        void Process() 
        {
            if (!input.HasInput) return;

            GLTextuer2D i1 = (GLTextuer2D)input.Reference.Data;

            if (i1 == null) return;
            if (i1.Id == 0) return;

            CreateBufferIfNeeded();

            processor.TileX = tileX;
            processor.TileY = tileY;
            processor.DirectX = pdirectx;
            processor.NoiseReduction = pnoiseReduction;
            processor.Intensity = pintensity;
            processor.Process(width, height, i1, buffer);
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

            if(processor != null)
            {
                processor.Release();
                processor = null;
            }
        }
    }
}
