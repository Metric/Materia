using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Materia.Rendering.Imaging.Processing;
using Materia.Rendering.Attributes;
using Materia.Rendering.Mathematics;
using Materia.Graph;
using Materia.Graph.IO;

namespace Materia.Nodes.Atomic
{
    public class UniformColorNode : ImageNode
    {
        MVector color = new MVector(0, 0, 0, 1);

        [Promote(NodeType.Float4)]
        [Editable(ParameterInputType.Color, "Color")]
        public MVector Color
        {
            get
            {
                return color;
            }
            set
            {
                color = value;
                TriggerValueChange();
            }
        }

        public new float TileX
        {
            get
            {
                return tileX;
            }
            set
            {
                tileX = value;
            }
        }

        public new float TileY
        {
            get
            {
                return tileY;
            }
            set
            {
                tileY = value;
            }
        }

        NodeOutput output;
        UniformColorProcessor processor;

        public UniformColorNode(int w, int h, GraphPixelType p = GraphPixelType.RGBA) : base()
        {
            defaultName = Name = "Uniform Color";

            width = w;
            height = h;

            internalPixelType = p;

            Inputs = new List<NodeInput>();
            output = new NodeOutput(NodeType.Color | NodeType.Gray, this);
            Outputs = new List<NodeOutput>();
            Outputs.Add(output);
        }


        public override void TryAndProcess()
        {
            Process();
        }

        void Process()
        {
            if (isDisposing) return;

            CreateBufferIfNeeded();

            processor ??= new UniformColorProcessor();

            processor.Color = GetParameter("Color", color).ToVector4();

            processor.PrepareView(buffer);
            processor.Process();
            processor.Complete();

            output.Data = buffer;
            TriggerTextureChange();
        }

        public class UniformColorNodeData : NodeData
        {
            public float[] color;

            public override void Write(Writer w)
            {
                base.Write(w);
                w.WriteObjectList(color);
            }

            public override void Parse(Reader r)
            {
                base.Parse(r);
                color = r.NextList<float>();
            }
        }

        public override void GetBinary(Writer w)
        {
            UniformColorNodeData d = new UniformColorNodeData();
            FillBaseNodeData(d);
            d.color = new float[] { color.X, color.Y, color.Z, color.W };
            d.Write(w);
        }

        public override void FromBinary(Reader r)
        {
            UniformColorNodeData d = new UniformColorNodeData();
            d.Parse(r);
            SetBaseNodeDate(d);
            float[] c = d.color;
            color = new MVector(c[0], c[1], c[2], c[3]);
        }

        public override void FromJson(string data)
        {
            UniformColorNodeData d = JsonConvert.DeserializeObject<UniformColorNodeData>(data);
            SetBaseNodeDate(d);
            float[] c = d.color;
            color = new MVector(c[0], c[1], c[2], c[3]);
        }

        public override string GetJson()
        {
            UniformColorNodeData d = new UniformColorNodeData();
            FillBaseNodeData(d);

            d.color = new float[] { color.X, color.Y, color.Z, color.W };

            return JsonConvert.SerializeObject(d);
        }

        public override void Dispose()
        {
            base.Dispose();
            processor?.Dispose();
            processor = null;
        }
    }
}
