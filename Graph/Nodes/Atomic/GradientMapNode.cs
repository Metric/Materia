using System;
using System.Collections.Generic;
using Materia.Rendering.Attributes;
using Materia.Rendering.Imaging;
using Newtonsoft.Json;
using Materia.Rendering.Imaging.Processing;
using Materia.Rendering.Textures;
using Materia.Rendering.Interfaces;
using Materia.Nodes.Containers;
using Materia.Nodes.Helpers;
using Materia.Rendering.Mathematics;
using Materia.Graph;

namespace Materia.Nodes.Atomic
{
    public class GradientMapNode : ImageNode
    {
        NodeInput input;
        NodeInput input2;

        NodeOutput Output;

        GradientMapProcessor processor;

        GLTexture2D colorLUT;
        FloatBitmap LUT;

        protected Containers.Gradient gradient;
        [Editable(ParameterInputType.Gradient, "Gradient")]
        public Containers.Gradient Gradient
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

            gradient = new Containers.Gradient();

            tileX = tileY = 1;

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

            processor?.Dispose();
            processor = null;

            colorLUT?.Dispose();
            colorLUT = null;

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
            Rendering.Imaging.Gradient.Fill(LUT, gradient.positions, gradient.colors);
        }

        public override void TryAndProcess()
        {
            FillLUT();
            Process();
        }

        void Process()
        {
            if (processor == null) return;
            if (!input.HasInput) return;

            GLTexture2D i1 = (GLTexture2D)input.Reference.Data;
            GLTexture2D i2 = null;

            if(input2.HasInput)
            {
                i2 = (GLTexture2D)input2.Reference.Data;
            }

            if (i1 == null) return;
            if (i1.Id == 0) return;

            if(colorLUT == null || colorLUT.Id == 0)
            {
                colorLUT = new GLTexture2D(PixelInternalFormat.Rgba8);
            }

            colorLUT.Bind();
            colorLUT.SetData(LUT.Image, PixelFormat.Rgba, 256, 2);
            colorLUT.Linear();
            colorLUT.Repeat();
            GLTexture2D.Unbind();

            CreateBufferIfNeeded();

            //setting params must go before PrepareView()
            processor.Tiling = GetTiling();

            processor.ColorLUT = colorLUT;
            processor.Mask = i2;

            processor.PrepareView(buffer);
            processor.Process(i1);
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

            gradient = new Containers.Gradient();

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
