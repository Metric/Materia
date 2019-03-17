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

namespace Materia.Nodes.Atomic
{
    public class LevelsNode : ImageNode
    {
        NodeInput input;
        MultiRange range;

        NodeOutput Output;

        LevelsProcessor processor;

        [LevelEditor]
        public MultiRange Range
        {
            get
            {
                return range;
            }
            set
            {
                range = value;
                TryAndProcess();
            }
        }

        public LevelsNode(int w, int h, GraphPixelType p = GraphPixelType.RGBA)
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

            input.OnInputAdded += Input_OnInputAdded;
            input.OnInputChanged += Input_OnInputChanged;
            input.OnInputRemoved += Input_OnInputRemoved;

            Inputs = new List<NodeInput>();
            Inputs.Add(input);

            Outputs = new List<NodeOutput>();
            Outputs.Add(Output);
        }

        private void Input_OnInputRemoved(NodeInput n)
        {
            Output.Data = null;
            Output.Changed();
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
            if(input.HasInput)
            {
                Process();    
            }
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

        void Process()
        {
            GLTextuer2D i1 = (GLTextuer2D)input.Input.Data;

            if (i1 == null) return;
            if (i1.Id == 0) return;

            CreateBufferIfNeeded();

            processor.TileX = tileX;
            processor.TileY = tileY;
            processor.Min = new OpenTK.Vector3(range.min[0], range.min[1], range.min[2]);
            processor.Max = new OpenTK.Vector3(range.max[0], range.max[1], range.max[2]);
            processor.Mid = new OpenTK.Vector3(range.mid[0], range.mid[1], range.mid[2]);

            processor.Process(width, height, i1, buffer);
            processor.Complete();

            Updated();
            Output.Data = buffer;
            Output.Changed();
        }

        public class LevelsData : NodeData
        {
            public MultiRange range;
        }

        public override void FromJson(Dictionary<string, Node> nodes, string data)
        {
            LevelsData d = JsonConvert.DeserializeObject<LevelsData>(data);
            SetBaseNodeDate(d);
            range = d.range;

            SetConnections(nodes, d.outputs);

            OnWidthHeightSet();
        }

        public override string GetJson()
        {
            LevelsData d = new LevelsData();
            FillBaseNodeData(d);
            d.range = range;

            return JsonConvert.SerializeObject(d);
        }

        protected override void OnWidthHeightSet()
        {
            TryAndProcess();
        }
    }
}
