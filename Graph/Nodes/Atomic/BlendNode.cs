using System;
using Newtonsoft.Json;
using Materia.Rendering.Imaging.Processing;
using Materia.Rendering.Attributes;
using Materia.Rendering.Extensions;
using Materia.Rendering.Textures;
using Materia.Graph;
using Materia.Rendering.Mathematics;
using Materia.Graph.IO;

namespace Materia.Nodes.Atomic
{
    public class BlendNode : ImageNode
    {
        NodeInput first;
        NodeInput second;
        NodeInput mask;

        NodeOutput Output;

        BlendProcessor processor;

        float alpha = 1;
        [Promote(NodeType.Float)]
        [Editable(ParameterInputType.FloatSlider, "Alpha")]
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
                TriggerValueChange();
            }
        }

        BlendType mode = BlendType.Copy;
        [Promote(NodeType.Float)]
        [Editable(ParameterInputType.Dropdown, "Mode")]
        public BlendType Mode
        {
            get
            {
                return mode;
            }
            set
            {
                mode = value;
                TriggerValueChange();
            }
        }

        AlphaModeType alphaMode = AlphaModeType.Add;
        [Promote(NodeType.Float)]
        [Editable(ParameterInputType.Dropdown, "Alpha Mode")]
        public AlphaModeType AlphaMode
        {
            get
            {
                return alphaMode;
            }
            set
            {
                alphaMode = value;
                TriggerValueChange();
            }
        }

        public BlendNode(int w, int h, GraphPixelType p = GraphPixelType.RGBA) : base()
        {
            //Do not create processors here as it can hurt loading times
            //also it allows us to load on another thread if needed
            //and not worry about GL calls

            Name = "Blend";

            width = w;
            height = h;

            internalPixelType = p;

            Output = new NodeOutput(NodeType.Color | NodeType.Gray, this);
            first = new NodeInput(NodeType.Color | NodeType.Gray, this, "Foreground");
            second = new NodeInput(NodeType.Color | NodeType.Gray, this, "Background");
            mask = new NodeInput(NodeType.Gray, this, "Mask");

            Inputs.Add(first);
            Inputs.Add(second);
            Inputs.Add(mask);
            Outputs.Add(Output);
        }

        public override void TryAndProcess()
        {
            Process();
        }

        void Process()
        {
            if (isDisposing) return;
            if (!first.HasInput || !second.HasInput) return;

            GLTexture2D i1 = (GLTexture2D)first.Reference.Data;
            GLTexture2D i2 = (GLTexture2D)second.Reference.Data;
            GLTexture2D i3 = null;

            if(mask.HasInput)
            {
                i3 = (GLTexture2D)mask.Reference.Data;
            }

            if (i1 == null || i2 == null) return;
            if (i1.Id == 0) return;
            if (i2.Id == 0) return;

            CreateBufferIfNeeded();

            processor ??= new BlendProcessor();

            processor.Tiling = GetTiling();
            processor.Alpha = GetParameter("Alpha", alpha);
            processor.BlendMode = GetParameter("Mode", (int)mode);
            processor.AlphaMode = GetParameter("AlphaMode", (int)alphaMode);

            processor.PrepareView(buffer);
            processor.Process(i1, i2, i3);
            processor.Complete();

            Output.Data = buffer;
            TriggerTextureChange();
        }

        public override void Dispose()
        {
            base.Dispose();

            processor?.Dispose();
            processor = null;
        }

        public class BlendData : NodeData
        {
            public string mode;
            public float alpha;
            public string alphaMode;

            public override void Write(Writer w)
            {
                base.Write(w);
                w.Write(mode);
                w.Write(alpha);
                w.Write(alphaMode);
            }

            public override void Parse(Reader r)
            {
                base.Parse(r);
                mode = r.NextString();
                alpha = r.NextFloat();
                alphaMode = r.NextString();
            }
        }

        public override void GetBinary(Writer w)
        {
            BlendData d = new BlendData();
            FillBaseNodeData(d);
            d.mode = mode.ToString(); //todo: eventually convert to integer
            d.alphaMode = alphaMode.ToString(); //todo: convert to integer
            d.alpha = alpha;
            d.Write(w);
        }

        public override void FromBinary(Reader r)
        {
            BlendData d = new BlendData();
            d.Parse(r);
            SetBaseNodeDate(d);
            Enum.TryParse<AlphaModeType>(d.alphaMode, out alphaMode);
            Enum.TryParse<BlendType>(d.mode, out mode);
            alpha = d.alpha;
        }

        public override string GetJson()
        {
            BlendData d = new BlendData();
            FillBaseNodeData(d);
            d.mode = mode.ToString();
            d.alphaMode = alphaMode.ToString();
            d.alpha = alpha;

            return JsonConvert.SerializeObject(d);
        }

        public override void FromJson(string data)
        {
            BlendData d = JsonConvert.DeserializeObject<BlendData>(data);
            SetBaseNodeDate(d);
            Enum.TryParse<AlphaModeType>(d.alphaMode, out alphaMode);
            Enum.TryParse<BlendType>(d.mode, out mode);
            alpha = d.alpha;
        }
    }
}
