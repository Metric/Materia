using System;
using Materia.Rendering.Attributes;
using Materia.Rendering.Textures;
using Materia.Rendering.Imaging.Processing;
using Newtonsoft.Json;
using Materia.Rendering.Extensions;
using Materia.Graph;
using Materia.Rendering.Mathematics;
using Materia.Graph.IO;

namespace Materia.Nodes.Atomic
{
    public class EmbossNode : ImageNode
    {
        NodeInput input;

        NodeOutput Output;

        EmbossProcessor processor;

        int angle = 0;
        [Promote(NodeType.Float)]
        [Editable(ParameterInputType.IntSlider, "Angle", "Default", 0, 360)]
        public int Angle
        {
            get
            {
                return angle;
            }
            set
            {
                angle = value;
                TriggerValueChange();
            }
        }

        int elevation = 2;
        [Promote(NodeType.Float)]
        [Editable(ParameterInputType.IntSlider, "Elevation", "Default", 0, 90)]
        public int Elevation
        {
            get
            {
                return elevation;
            }
            set
            {
                elevation = value;
                TriggerValueChange();
            }
        }

        public EmbossNode(int w, int h, GraphPixelType p = GraphPixelType.RGBA)
        {
            defaultName = Name = "Emboss";

            width = w;
            height = h;

            internalPixelType = p;

            input = new NodeInput(NodeType.Color | NodeType.Gray, this, "Image Input");
            Output = new NodeOutput(NodeType.Gray, this);

            Inputs.Add(input);
            Outputs.Add(Output);
        }

        public class EmbossNodeData : NodeData
        {
            public ushort angle;
            public byte elevation;

            public override void Write(Writer w)
            {
                base.Write(w);
                w.Write(angle);
                w.Write(elevation);
            }

            public override void Parse(Reader r)
            {
                base.Parse(r);
                angle = r.NextUShort();
                elevation = r.NextByte();
            }
        }

        public override void GetBinary(Writer w)
        {
            EmbossNodeData d = new EmbossNodeData();
            FillBaseNodeData(d);
            d.angle = (ushort)angle;
            d.elevation = (byte)elevation;
            d.Write(w);
        }

        public override void FromBinary(Reader r)
        {
            EmbossNodeData d = new EmbossNodeData();
            d.Parse(r);
            SetBaseNodeDate(d);
            angle = d.angle;
            elevation = d.elevation;
        }

        public override void FromJson(string data)
        {
            EmbossNodeData d = JsonConvert.DeserializeObject<EmbossNodeData>(data);
            SetBaseNodeDate(d);
            angle = d.angle;
            elevation = d.elevation;
        }

        public override string GetJson()
        {
            EmbossNodeData d = new EmbossNodeData();

            FillBaseNodeData(d);
            d.angle = (ushort)angle;
            d.elevation = (byte)elevation;

            return JsonConvert.SerializeObject(d);
        }

        public override void TryAndProcess()
        {
            Process();
        }

        void Process()
        {
            if (isDisposing) return;
            if (!input.HasInput) return;

            GLTexture2D i1 = (GLTexture2D)input.Reference.Data;

            if (i1 == null) return;
            if (i1.Id == 0) return;

            CreateBufferIfNeeded();

            processor ??= new EmbossProcessor();

            processor.Tiling = GetTiling();
            processor.Azimuth = GetParameter("Angle", angle) * MathHelper.Deg2Rad;
            processor.Elevation = GetParameter("Elevation", elevation) * MathHelper.Deg2Rad;

            processor.PrepareView(buffer);
            processor.Process(i1);
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
    }
}
