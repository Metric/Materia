using System;
using Materia.Nodes.Containers;
using Materia.Rendering.Attributes;
using Newtonsoft.Json;
using Materia.Rendering.Imaging.Processing;
using Materia.Rendering.Textures;
using Materia.Rendering.Mathematics;
using Materia.Graph;

namespace Materia.Nodes.Atomic
{
    public class LevelsNode : ImageNode
    {
        NodeInput input;
        MultiRange range;

        NodeOutput Output;

        LevelsProcessor processor;

        [Promote(NodeType.Float4)]
        [Editable(ParameterInputType.Levels, "Range")]
        public MultiRange Range
        {
            get
            {
                return range;
            }
            set
            {
                range = value;
                TriggerValueChange();
            }
        }

        public LevelsNode(int w, int h, GraphPixelType p = GraphPixelType.RGBA) : base()
        {
            Name = "Levels";
            Id = Guid.NewGuid().ToString();
            width = w;
            height = h;

            tileX = tileY = 1;

            range = new MultiRange();

            processor = new LevelsProcessor();

            internalPixelType = p;

            input = new NodeInput(NodeType.Color | NodeType.Gray, this, "Image Input");
            Output = new NodeOutput(NodeType.Color | NodeType.Gray, this);


            Inputs.Add(input);
            Outputs.Add(Output);
        }

        public override void Dispose()
        {
            base.Dispose();

            processor?.Dispose();
            processor = null;
        }

        private void GetParams()
        {
            if (!input.HasInput) return;

            prange = range;

            object value = GetParameter("Range");
            if (value == null || !(value is MVector)) return;
            MVector v = (MVector)value;
            prange = new MultiRange();

            if(v.W <= 0)
            {
                prange.min[0] = prange.min[1] = prange.min[2] = v.X;
                prange.mid[0] = prange.mid[1] = prange.mid[2] = v.Y;
                prange.max[0] = prange.max[1] = prange.max[2] = v.Z;
            }
            else if(v.W <= 1)
            {
                prange.min[0] = v.X;
                prange.mid[0] = v.Y;
                prange.max[0] = v.Z;
            }
            else if(v.W <= 2)
            {
                prange.min[1] = v.X;
                prange.mid[1] = v.Y;
                prange.max[1] = v.Z;
            }
            else if(v.W <= 3)
            {
                prange.min[2] = v.X;
                prange.mid[2] = v.Y;
                prange.max[2] = v.Z;
            }
            else
            {
                prange.min[0] = prange.min[1] = prange.min[2] = v.X;
                prange.mid[0] = prange.mid[1] = prange.mid[2] = v.Y;
                prange.max[0] = prange.max[1] = prange.max[2] = v.Z;
            }
        }

        public override void TryAndProcess()
        {
            GetParams();
            Process();
        }

        MultiRange prange;
        void Process()
        {
            if (processor == null) return;
            if (!input.HasInput) return;

            GLTexture2D i1 = (GLTexture2D)input.Reference.Data;

            if (i1 == null) return;
            if (i1.Id == 0) return;

            CreateBufferIfNeeded();

            processor.Tiling = GetTiling();
            processor.Min = new Vector3(prange.min[0], prange.min[1], prange.min[2]);
            processor.Max = new Vector3(prange.max[0], prange.max[1], prange.max[2]);
            processor.Mid = new Vector3(prange.mid[0], prange.mid[1], prange.mid[2]);
            processor.Value = new Vector2(prange.min[3], prange.max[3]);

            processor.PrepareView(buffer);
            processor.Process(i1);
            processor.Complete();

            Output.Data = buffer;
            TriggerTextureChange();
        }

        public class LevelsData : NodeData
        {
            public MultiRange range;
        }

        public override void FromJson(string data)
        {
            LevelsData d = JsonConvert.DeserializeObject<LevelsData>(data);
            SetBaseNodeDate(d);
            //to ensure backwards compat with older multi range size
            range = new MultiRange(d.range.min, d.range.mid, d.range.max);
        }

        public override string GetJson()
        {
            LevelsData d = new LevelsData();
            FillBaseNodeData(d);
            d.range = range;

            return JsonConvert.SerializeObject(d);
        }
    }
}
