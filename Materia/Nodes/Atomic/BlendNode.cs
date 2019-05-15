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
using Materia.Textures;
using Materia.Imaging.GLProcessing;

namespace Materia.Nodes.Atomic
{
    public enum BlendType
    {
        AddSub = 0,
        Copy = 1,
        Multiply = 2,
        Screen = 3,
        Overlay = 4,
        HardLight = 5,
        SoftLight = 6,
        ColorDodge = 7,
        LinearDodge = 8,
        ColorBurn = 9,
        LinearBurn = 10,
        VividLight = 11,
        Divide = 12,
        Subtract = 13,
        Difference = 14,
        Darken = 15,
        Lighten = 16,
        Hue = 17,
        Saturation = 18,
        Color = 19,
        Luminosity = 20,
        LinearLight = 21
    }

    public class BlendNode : ImageNode
    {
        NodeInput first;
        NodeInput second;
        NodeInput mask;

        NodeOutput Output;

        BlendProcessor processor;

        float alpha;
        [Promote(NodeType.Float)]
        [Slider(IsInt = false, Max = 1, Min = 0, Snap = false, Ticks = new float[0])]
        public float Alpha
        {
            get
            {
                return alpha;
            }
            set
            {
                if (value < 0) alpha = 0;
                if (value > 1) alpha = 1;

                alpha = value;
                TryAndProcess();
            }
        }

        BlendType mode;
        [Promote(NodeType.Float)]
        [Title(Title = "Blend Mode")]
        public BlendType Mode
        {
            get
            {
                return mode;
            }
            set
            {
                mode = value;
                TryAndProcess();
            }
        }

        public BlendNode(int w, int h, GraphPixelType p = GraphPixelType.RGBA)
        {
            Name = "Blend";

            width = w;
            height = h;

            alpha = 1;
            mode = BlendType.Copy;

            previewProcessor = new BasicImageRenderer();
            processor = new BlendProcessor();

            internalPixelType = p;

            tileX = tileY = 1;

            Id = Guid.NewGuid().ToString();
            Inputs = new List<NodeInput>();
            Output = new NodeOutput(NodeType.Color | NodeType.Gray, this);

            first = new NodeInput(NodeType.Color | NodeType.Gray, this, "Foreground");
            second = new NodeInput(NodeType.Color | NodeType.Gray, this, "Background");
            mask = new NodeInput(NodeType.Gray, this, "Mask");

            first.OnInputAdded += OnInputAdded;
            first.OnInputRemoved += OnInputRemoved;
            first.OnInputChanged += OnInputChanged;
            second.OnInputRemoved += OnInputRemoved;
            second.OnInputAdded += OnInputAdded;
            second.OnInputChanged += OnInputChanged;

            mask.OnInputRemoved += OnInputRemoved;
            mask.OnInputAdded += OnInputAdded;
            mask.OnInputChanged += OnInputChanged;

            Inputs.Add(first);
            Inputs.Add(second);
            Inputs.Add(mask);

            Outputs = new List<NodeOutput>();
            Outputs.Add(Output);
        }

        private void OnInputChanged(NodeInput n)
        {
            TryAndProcess();
        }

        private void OnInputRemoved(NodeInput n)
        {
            TryAndProcess();
        }

        private void OnInputAdded(NodeInput n)
        {
            TryAndProcess();
        }

        public override void TryAndProcess()
        {
            if(first.HasInput && second.HasInput)
            {
                Process();
            }
        }

        void Process()
        {
            GLTextuer2D i1 = (GLTextuer2D)first.Input.Data;
            GLTextuer2D i2 = (GLTextuer2D)second.Input.Data;
            GLTextuer2D i3 = null;

            if(mask.HasInput)
            {
                i3 = (GLTextuer2D)mask.Input.Data;
            }

            if (i1 == null || i2 == null) return;
            if (i1.Id == 0) return;
            if (i2.Id == 0) return;

            CreateBufferIfNeeded();

            int pmode = (int)mode;
            float palpha = alpha;

            if(ParentGraph != null && ParentGraph.HasParameterValue(Id, "Mode"))
            {
                pmode = Convert.ToInt32(ParentGraph.GetParameterValue(Id, "Mode"));
            }
            if(ParentGraph != null && ParentGraph.HasParameterValue(Id, "Alpha"))
            {
                palpha = Convert.ToSingle(ParentGraph.GetParameterValue(Id, "Alpha"));
            }

            processor.TileX = tileX;
            processor.TileY = tileY;
            processor.Alpha = palpha;
            processor.BlendMode = pmode;
            processor.Process(width, height, i1, i2, i3, buffer);
            processor.Complete();

            Updated();
            Output.Data = buffer;
            Output.Changed();
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

        protected override void OnWidthHeightSet()
        {
            TryAndProcess();
        }

        public class BlendData : NodeData
        {
            public string mode;
            public float alpha;
        }

        public override string GetJson()
        {
            BlendData d = new BlendData();
            FillBaseNodeData(d);
            d.mode = mode.ToString();
            d.alpha = alpha;

            return JsonConvert.SerializeObject(d);
        }

        public override void FromJson(Dictionary<string, Node> nodes, string data)
        {
            BlendData d = JsonConvert.DeserializeObject<BlendData>(data);
            SetBaseNodeDate(d);
            Enum.TryParse<BlendType>(d.mode, out mode);
            alpha = d.alpha;
        }
    }
}
