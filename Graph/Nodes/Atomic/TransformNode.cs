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

        private void GetParams()
        {
            if (!input.HasInput) return;

            pangle = angle;

            pscaleX = this.scale.X;
            pscaleY = this.scale.Y;

            pxoffset = offset.X;
            pyoffset = offset.Y;

            if (ParentGraph != null && ParentGraph.HasParameterValue(Id, "XOffset"))
            {
                pxoffset = ParentGraph.GetParameterValue(Id, "XOffset").ToFloat();
            }

            if (ParentGraph != null && ParentGraph.HasParameterValue(Id, "YOffset"))
            {
                pyoffset = ParentGraph.GetParameterValue(Id, "YOffset").ToFloat();
            }

            if (ParentGraph != null && ParentGraph.HasParameterValue(Id, "Offset"))
            {
                MVector v = ParentGraph.GetParameterValue<MVector>(Id, "Offset");
                pxoffset = v.X;
                pyoffset = v.Y;
            }

            if (ParentGraph != null && ParentGraph.HasParameterValue(Id, "ScaleX"))
            {
                pscaleX = ParentGraph.GetParameterValue(Id, "ScaleX").ToFloat();
            }

            if (ParentGraph != null && ParentGraph.HasParameterValue(Id, "ScaleY"))
            {
                pscaleY = ParentGraph.GetParameterValue(Id, "ScaleY").ToFloat();
            }

            if (parentGraph != null && ParentGraph.HasParameterValue(Id, "Scale"))
            {
                MVector v = ParentGraph.GetParameterValue<MVector>(Id, "Scale");
                pscaleX = v.X;
                pscaleY = v.Y;
            }

            if (ParentGraph != null && ParentGraph.HasParameterValue(Id, "Angle"))
            {
                pangle = ParentGraph.GetParameterValue(Id, "Angle").ToFloat();
            }
        }

        public override void TryAndProcess()
        {
            GetParams();
            Process();
        }

        float pxoffset;
        float pyoffset;
        float pscaleX;
        float pscaleY;
        float pangle;
        void Process()
        {
            if (processor == null) return;
            if (!input.HasInput) return;

            GLTexture2D i1 = (GLTexture2D)input.Reference.Data;

            if (i1 == null) return;
            if (i1.Id == 0) return;

            CreateBufferIfNeeded();

            processor.PrepareView(buffer);

            Matrix3 rot = Matrix3.CreateRotationZ(pangle * (float)(Math.PI / 180.0));
            Matrix3 scale = Matrix3.CreateScale(1.0f / pscaleX, 1.0f / pscaleY, 1);
            Vector3 trans = new Vector3(pxoffset * width, pyoffset * height, 0);

            processor.Tiling = new Vector2(TileX, TileY);
            processor.Rotation = rot;
            processor.Scale = scale;
            processor.Translation = trans;

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
