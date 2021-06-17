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
using Materia.Graph.IO;

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

        protected Containers.Gradient gradient = new Containers.Gradient();
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

            width = w;
            height = h;

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
            if (isDisposing) return;
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
            if (isDisposing) return;
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
            colorLUT.SetData(LUT.Image, PixelFormat.Bgra, 256, 2);
            colorLUT.Linear();
            colorLUT.Repeat();
            GLTexture2D.Unbind();

            CreateBufferIfNeeded();


            processor ??= new GradientMapProcessor();

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

            public override void Write(Writer w)
            {
                base.Write(w);

                w.Write(colors.Count);

                for (int i = 0; i < colors.Count; ++i)
                {
                    w.WriteObjectList(colors[i]);
                }

                w.WriteObjectList(positions);
            }

            public override void Parse(Reader r)
            {
                base.Parse(r);

                int count = r.NextInt();

                colors = new List<float[]>();

                for (int i = 0; i < count; ++i)
                {
                    colors.Add(r.NextList<float>());
                }

                positions = r.NextList<float>();
            }
        }

        private void FillData(GradientMapData d)
        {
            d.colors = new List<float[]>();

            if (gradient != null)
            {
                for (int j = 0; j < gradient.colors.Length; ++j)
                {
                    MVector m = gradient.colors[j];
                    d.colors.Add(m.ToArray());
                }

                d.positions = gradient.positions;
            }
        }

        private void SetData(GradientMapData d)
        {
            gradient = new Containers.Gradient();

            if (d.colors != null)
            {
                gradient.colors = new MVector[d.colors.Count];
            }

            for (int i = 0; i < d.colors.Count; ++i)
            {
                gradient.colors[i] = MVector.FromArray(d.colors[i]);
            }

            if (d.positions != null && d.positions.Length == d.colors.Count)
            {
                gradient.positions = d.positions;
            }
        }

        public override void GetBinary(Writer w)
        {
            GradientMapData d = new GradientMapData();
            FillBaseNodeData(d);
            FillData(d);
            d.Write(w);
        }

        public override void FromBinary(Reader r)
        {
            GradientMapData d = new GradientMapData();
            d.Parse(r);
            SetBaseNodeDate(d);
            SetData(d);
        }

        public override void FromJson(string data)
        {
            GradientMapData d = JsonConvert.DeserializeObject<GradientMapData>(data);
            SetBaseNodeDate(d);
            SetData(d);
        }

        public override string GetJson()
        {
            GradientMapData d = new GradientMapData();
            FillBaseNodeData(d);
            FillData(d);

            return JsonConvert.SerializeObject(d);
        }
    }
}
