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
        NodeInput input;

        GrayscaleConvProcessor processor;

        NodeOutput Output;

        float r;
        [Editable(ParameterInputType.FloatSlider, "Red")]
        public float Red
        {
            get
            {
                return r;
            }
            set
            {
                r = value;
                TriggerValueChange();
            }
        }

        float g;
        [Editable(ParameterInputType.FloatSlider, "Green")]
        public float Green
        {
            get
            {
                return g;
            }
            set
            {
                g = value;
                TriggerValueChange();
            }
        }

        float b;
        [Editable(ParameterInputType.FloatSlider, "Blue")]
        public float Blue
        {
            get
            {
                return b;
            }
            set
            {
                b = value;
                TriggerValueChange();
            }
        }

        float a;
        [Editable(ParameterInputType.FloatSlider, "Alpha")]
        public float Alpha
        {
            get
            {
                return a;
            }
            set
            {
                a = value;
                TriggerValueChange();
            }
        }

        public GrayscaleConversionNode(int w, int h, GraphPixelType p = GraphPixelType.RGBA) : base()
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

            Inputs.Add(input);
            Outputs.Add(Output);
        }

        public override void TryAndProcess()
        {
            Process();
        }

        void Process()
        {
            if (!input.HasInput) return;

            GLTextuer2D i1 = (GLTextuer2D)input.Reference.Data;

            if (i1 == null) return;
            if (i1.Id == 0) return;

            CreateBufferIfNeeded();

            processor.TileX = tileX;
            processor.TileY = tileY;
            processor.Weight = new Math3D.Vector4(r, g, b, a);
            processor.Process(width, height, i1, buffer);
            processor.Complete();

            Output.Data = buffer;
            TriggerTextureChange();
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
