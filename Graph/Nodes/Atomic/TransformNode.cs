using System;
using Materia.Rendering.Attributes;
using Newtonsoft.Json;
using Materia.Rendering.Textures;
using Materia.Rendering.Imaging.Processing;
using Materia.Rendering.Mathematics;
using Materia.Rendering.Extensions;
using Materia.Graph;
using Materia.Graph.IO;

namespace Materia.Nodes.Atomic
{
    public class TransformNode : ImageNode
    {
        TransformProcessor processor;

        protected float xoffset;

        protected MVector offset = MVector.Zero;
        [Promote(NodeType.Float2)]
        [Editable(ParameterInputType.Float2Input, "Offset")]
        public MVector Offset
        {
            get
            {
                return offset;
            }
            set
            {
                offset = value;
                TriggerValueChange();
            }
        }

        protected int angle = 0;
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

        protected MVector scale = new MVector(1,1);
        [Promote(NodeType.Float2)]
        [Editable(ParameterInputType.Float2Input, "Scale")]
        public MVector Scale
        {
            get
            {
                return scale;
            }
            set
            {
                scale = value;
                TriggerValueChange();
            }
        }

        NodeOutput Output;
        NodeInput input;

        public TransformNode(int w, int h, GraphPixelType p = GraphPixelType.RGBA) : base()
        {
            defaultName = Name = "Transform";

            width = w;
            height = h;

            internalPixelType = p;

            input = new NodeInput(NodeType.Color | NodeType.Gray, this, "Image Input");
            Output = new NodeOutput(NodeType.Color | NodeType.Gray, this);

            Inputs.Add(input);
            Outputs.Add(Output);
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

            Vector2 pscale = GetParameter("Scale", scale).ToVector2();
            float pangle = GetParameter("Angle", angle) * MathHelper.Deg2Rad;
            Vector2 poffset = GetParameter("Offset", offset).ToVector2();

            Matrix3 irot = Matrix3.CreateRotationZ(pangle);
            Matrix3 iscale = Matrix3.CreateScale(1.0f / pscale.X, 1.0f / pscale.Y, 1);
            Vector3 itrans = new Vector3(poffset.X * width, poffset.Y * height, 0);

            processor ??= new TransformProcessor();

            processor.Tiling = GetTiling();
            processor.Rotation = irot;
            processor.Scale = iscale;
            processor.Translation = itrans;

            processor.PrepareView(buffer);
            processor.Process(i1);
            processor.Complete();

            Output.Data = buffer;
            TriggerTextureChange();
        }

        public override void Dispose()
        {
            base.Dispose();

            //we always release just in case
            //we ever add anything to it
            processor?.Dispose();
            processor = null;
        }

        public class TransformData : NodeData
        {
            public float xOffset;
            public float yOffset;
            public ushort angle;
            public float scaleX;
            public float scaleY;

            public override void Write(Writer w)
            {
                base.Write(w);
                w.Write(xOffset);
                w.Write(yOffset);
                w.Write(angle);
                w.Write(scaleX);
                w.Write(scaleY);
            }

            public override void Parse(Reader r)
            {
                base.Parse(r);
                xOffset = r.NextFloat();
                yOffset = r.NextFloat();
                angle = r.NextUShort();
                scaleX = r.NextFloat();
                scaleY = r.NextFloat();
            }
        }

        private void FillData(TransformData d)
        {
            d.xOffset = offset.X;
            d.yOffset = offset.Y;
            d.angle = (ushort)angle;
            d.scaleX = scale.X;
            d.scaleY = scale.Y;
        }

        private void SetData(TransformData d)
        { 
            offset.X = d.xOffset;
            offset.Y = d.yOffset;
            angle = d.angle;
            scale.X = d.scaleX;
            scale.Y = d.scaleY;
        }

        public override void GetBinary(Writer w)
        {
            TransformData d = new TransformData();
            FillBaseNodeData(d);
            FillData(d);
            d.Write(w);
        }

        public override void FromBinary(Reader r)
        {
            TransformData d = new TransformData();
            d.Parse(r);
            SetBaseNodeDate(d);
            SetData(d);
        }

        public override string GetJson()
        {
            TransformData d = new TransformData();
            FillBaseNodeData(d);
            FillData(d);

            return JsonConvert.SerializeObject(d);
        }

        public override void FromJson(string data)
        {
            TransformData d = JsonConvert.DeserializeObject<TransformData>(data);
            SetBaseNodeDate(d);
            SetData(d);
        }
    }
}
