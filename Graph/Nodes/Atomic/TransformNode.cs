using System;
using Materia.Rendering.Attributes;
using Newtonsoft.Json;
using Materia.Rendering.Textures;
using Materia.Rendering.Imaging.Processing;
using Materia.Rendering.Mathematics;
using Materia.Rendering.Extensions;
using Materia.Graph;

namespace Materia.Nodes.Atomic
{
    public class TransformNode : ImageNode
    {
        TransformProcessor processor;

        protected float xoffset;

        protected MVector offset;
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

        protected float angle;
        [Promote(NodeType.Float)]
        [Editable(ParameterInputType.IntSlider, "Angle", "Default", 0, 360)]
        public float Angle
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

        protected MVector scale;
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
            Name = "Transform";

            Id = Guid.NewGuid().ToString();

            angle = 0;
            offset = new MVector(0, 0);
            scale = new MVector(1, 1);

            tileX = tileY = 1;

            width = w;
            height = h;

            processor = new TransformProcessor();

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
            if (processor == null) return;
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
            public float angle;
            public float scaleX;
            public float scaleY;
        }

        public override string GetJson()
        {
            TransformData d = new TransformData();
            FillBaseNodeData(d);
            d.xOffset = offset.X;
            d.yOffset = offset.Y;
            d.angle = angle;
            d.scaleX = scale.X;
            d.scaleY = scale.Y;

            return JsonConvert.SerializeObject(d);
        }

        public override void FromJson(string data)
        {
            TransformData d = JsonConvert.DeserializeObject<TransformData>(data);
            SetBaseNodeDate(d);

            offset.X = d.xOffset;
            offset.Y = d.yOffset;
            angle = d.angle;
            scale.X = d.scaleX;
            scale.Y = d.scaleY;
        }
    }
}
