using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Materia.Nodes.Containers;
using Materia.Nodes.Attributes;
using Materia.Imaging;
using Materia.Nodes.Helpers;
using Newtonsoft.Json;
using Materia.Imaging.GLProcessing;
using Materia.Textures;
using Materia.MathHelpers;
using Materia.GLInterfaces;

namespace Materia.Nodes.Atomic
{
    public class GradientMapNode : ImageNode
    {
        NodeInput input;
        NodeInput input2;

        NodeOutput Output;

        GradientMapProcessor processor;

        GLTextuer2D colorLUT;
        FloatBitmap LUT;

        protected Gradient gradient;
        [Editable(ParameterInputType.Gradient, "Gradient")]
        public Gradient Gradient
        {
            get
            {
                return gradient;
            }
            set
            {
                gradient = value;
                TriggerValueChange();
            }
        }

        public GradientMapNode(int w, int h, GraphPixelType p = GraphPixelType.RGBA) : base()
        {
            Name = "Gradient Map";
            Id = Guid.NewGuid().ToString();
            width = w;
            height = h;

            gradient = new Gradient();

            tileX = tileY = 1;

            previewProcessor = new BasicImageRenderer();
            processor = new GradientMapProcessor();

            internalPixelType = p;

            input = new NodeInput(NodeType.Gray, this, "Image Input");
            input2 = new NodeInput(NodeType.Gray, this, "Mask Input");
            Output = new NodeOutput(NodeType.Color, this);

            Inputs.Add(input);
            Inputs.Add(input2);

            Outputs.Add(Output);
        }

        public override void Dispose()
        {
            base.Dispose();

            if (processor != null)
            {
                processor.Release();
                processor = null;
            }

            if(colorLUT != null)
            {
                colorLUT.Release();
                colorLUT = null;
            }

            LUT = null;
        }

        private void FillLUT()
        {
            if (!input.HasInput) return;

            if (LUT == null)
            {
                LUT = new FloatBitmap(256, 2);
            }

            //generate gradient
            Utils.CreateGradient(LUT, gradient.positions, gradient.colors);
        }

        public override void TryAndProcess()
        {
            FillLUT();
            Process();
        }

        void Process()
        {
            if (!input.HasInput) return;

            GLTextuer2D i1 = (GLTextuer2D)input.Reference.Data;
            GLTextuer2D i2 = null;

            if(input2.HasInput)
            {
                i2 = (GLTextuer2D)input2.Reference.Data;
            }

            if (i1 == null) return;
            if (i1.Id == 0) return;

            if (processor == null) return;

            if(colorLUT == null || colorLUT.Id == 0)
            {
                colorLUT = new GLTextuer2D(PixelInternalFormat.Rgba8);
            }

            colorLUT.Bind();
            colorLUT.SetData(LUT.Image, PixelFormat.Rgba, 256, 2);
            colorLUT.SetFilter((int)TextureMinFilter.Linear, (int)TextureMagFilter.Linear);
            GLTextuer2D.Unbind();

            CreateBufferIfNeeded();

            processor.TileX = tileX;
            processor.TileY = tileY;

            processor.ColorLUT = colorLUT;
            processor.Mask = i2;
            processor.Process(width, height, i1, buffer);
            processor.Complete();

            Output.Data = buffer;
            TriggerTextureChange();
        }

        public class GradientMapData : NodeData
        {
            public List<float[]> colors;
            public float[] positions;
        }

        public override void FromJson(string data)
        {
            GradientMapData d = JsonConvert.DeserializeObject<GradientMapData>(data);
            SetBaseNodeDate(d);

            gradient = new Gradient();

            if(d.colors != null)
            {
                gradient.colors = new MVector[d.colors.Count];
            }

            for(int i = 0; i < d.colors.Count; ++i)
            {
                gradient.colors[i] = MVector.FromArray(d.colors[i]);
            }

            if(d.positions != null && d.positions.Length == d.colors.Count)
            {
                gradient.positions = d.positions;
            }
        }

        public override string GetJson()
        {
            GradientMapData d = new GradientMapData();
            FillBaseNodeData(d);
            d.colors = new List<float[]>();

            if (gradient != null)
            {
                for(int j = 0; j < gradient.colors.Length; ++j)
                {
                    MVector m = gradient.colors[j];
                    d.colors.Add(m.ToArray());
                }

                d.positions = gradient.positions;
            }

            return JsonConvert.SerializeObject(d);
        }
    }
}
