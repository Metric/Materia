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

            previewProcessor = new BasicImageRenderer();
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

            if(processor != null)
            {
                processor.Release();
                processor = null;
            }
        }

        private void GetParams()
        {
            if (!input.HasInput) return;

            prange = range;

            if(ParentGraph != null && ParentGraph.HasParameterValue(Id, "Range"))
            {
                MVector v = ParentGraph.GetParameterValue<MVector>(Id, "Range");

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
            if (!input.HasInput) return;

            GLTextuer2D i1 = (GLTextuer2D)input.Reference.Data;

            if (i1 == null) return;
            if (i1.Id == 0) return;

            CreateBufferIfNeeded();

            processor.TileX = tileX;
            processor.TileY = tileY;
            processor.Min = new Math3D.Vector3(prange.min[0], prange.min[1], prange.min[2]);
            processor.Max = new Math3D.Vector3(prange.max[0], prange.max[1], prange.max[2]);
            processor.Mid = new Math3D.Vector3(prange.mid[0], prange.mid[1], prange.mid[2]);
            processor.Value = new Math3D.Vector2(prange.min[3], prange.max[3]);

            processor.Process(width, height, i1, buffer);
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
