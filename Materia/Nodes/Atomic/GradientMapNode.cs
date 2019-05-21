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
using OpenTK.Graphics.OpenGL;

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
        public Gradient Gradient
        {
            get
            {
                return gradient;
            }
            set
            {
                gradient = value;
                TryAndProcess();
            }
        }

        public GradientMapNode(int w, int h, GraphPixelType p = GraphPixelType.RGBA)
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

            input.OnInputAdded += Input_OnInputAdded;
            input.OnInputChanged += Input_OnInputChanged;

            input2.OnInputAdded += Input_OnInputAdded;
            input2.OnInputChanged += Input_OnInputChanged;

            Inputs = new List<NodeInput>();
            Inputs.Add(input);
            Inputs.Add(input2);

            Outputs = new List<NodeOutput>();
            Outputs.Add(Output);
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
            if (input.HasInput && gradient != null)
            {
                Process();
            }
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

        void Process()
        {
            GLTextuer2D i1 = (GLTextuer2D)input.Input.Data;
            GLTextuer2D i2 = null;

            if(input2.HasInput)
            {
                if(input2.Input.Data != null)
                {
                    i2 = (GLTextuer2D)input2.Input.Data;
                }
            }

            if (i1 == null) return;
            if (i1.Id == 0) return;

            if(colorLUT == null || colorLUT.Id == 0)
            {
                colorLUT = new GLTextuer2D(PixelInternalFormat.Rgba8);
            }
            if(LUT == null)
            {
                LUT = new FloatBitmap(256, 2);
            }

            //generate gradient
            Utils.CreateGradient(LUT, gradient.positions, gradient.colors);

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
            Output.Changed();
            Updated();
        }

        public class GradientMapData : NodeData
        {
            public List<float[]> colors;
            public float[] positions;
        }

        public override void FromJson(Dictionary<string, Node> nodes, string data)
        {
            GradientMapData d = JsonConvert.DeserializeObject<GradientMapData>(data);
            SetBaseNodeDate(d);

            gradient = new Gradient();

            if(d.colors != null)
            {
                gradient.colors = new MVector[d.colors.Count];
            }

            for(int i = 0; i < d.colors.Count; i++)
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
                foreach (MVector m in gradient.colors)
                {
                    d.colors.Add(m.ToArray());
                }

                d.positions = gradient.positions;
            }

            return JsonConvert.SerializeObject(d);
        }

        protected override void OnWidthHeightSet()
        {
            TryAndProcess();
        }
    }
}
