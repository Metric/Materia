using System;
using Newtonsoft.Json;
using Materia.Rendering.Attributes;
using Materia.Rendering.Textures;
using Materia.Graph;

namespace Materia.Nodes.Atomic
{ 
    public class SequenceNode : ImageNode
    {
        public new bool AbsoluteSize { get; set; }

        static int MIN_OUTPUTS = 4;

        public new int Height
        {
            get
            {
                return height;
            }
            set
            {
                height = value;
            }
        }

        public new int Width
        {
            get
            {
                return width;
            }
            set
            {
                width = value;
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

        public new GraphPixelType InternalPixelFormat
        {
            get
            {
                return internalPixelType;
            }
            set
            {
                internalPixelType = value;
            }
        }

        NodeInput input;

        public SequenceNode(int w, int h, GraphPixelType p = GraphPixelType.RGBA) : base()
        {
            CanPreview = false;

            Name = "Sequence";
            Id = Guid.NewGuid().ToString();

            input = new NodeInput(NodeType.Bool | NodeType.Color | NodeType.Gray | NodeType.Float | NodeType.Float2 | NodeType.Float3 | NodeType.Float4, this, "Any Input");

            Inputs.Add(input);

            for(int i = 0; i < 4; ++i)
            {
                var output = new NodeOutput(NodeType.Bool | NodeType.Color | NodeType.Gray | NodeType.Float | NodeType.Float2 | NodeType.Float3 | NodeType.Float4, this, String.Format("{0:0}", Outputs.Count));
                output.OnOutputChanged += Output_OnOutputChanged;
                Outputs.Add(output);
            }
        }

        private void Output_OnOutputChanged(NodeOutput output)
        {
            int outputsConnected = 0;
            for (int i = 0; i < Outputs.Count; ++i)
            {
                var op = Outputs[i];
                if (op.To.Count > 0)
                {
                    ++outputsConnected;
                }
            }

            // minus 1 for execute pin
            if (outputsConnected >= Outputs.Count - 1)
            {
                AddPlaceholderOutput();
            }
            else if(outputsConnected  < Outputs.Count - 2 && outputsConnected > MIN_OUTPUTS + 1)
            {
                for(int i = MIN_OUTPUTS + 1; i < Outputs.Count; ++i)
                {
                    var op = Outputs[i];
                    Outputs.RemoveAt(i);
                    --i;
                    RemovedOutput(op);
                }
            }
        }

        protected override void AddPlaceholderOutput()
        {
            var output = new NodeOutput(NodeType.Bool | NodeType.Color | NodeType.Gray | NodeType.Float | NodeType.Float2 | NodeType.Float3 | NodeType.Float4, this, String.Format("{0:0}", Outputs.Count));
            Outputs.Add(output);
            AddedOutput(output);
        }

        public override void TryAndProcess()
        {
            Process();
        }
        void Process()
        {
            if (!input.HasInput) return;
            if (input.Reference.Data == null) return;

            GLTexture2D i1 = (GLTexture2D)input.Reference.Data;

            if (i1 == null) return;
            if (i1.Id == 0) return;

            width = i1.Width;
            height = i1.Height;

            int c = Outputs.Count;

            for(int i = 0; i < c; ++i)
            {
                Outputs[i].Data = i1;
            }

            TriggerTextureChange();
        }

        public override GLTexture2D GetActiveBuffer()
        {
            if(input.HasInput)
            {
                return input.Reference.GetActiveBuffer();
            }

            return null;
        }

        public override byte[] Export()
        {
            if(input.HasInput)
            {
                return input.Reference.Export();
            }

            return null;
        }

        public override string GetJson()
        {
            NodeData d = new NodeData();
            FillBaseNodeData(d);

            return JsonConvert.SerializeObject(d);
        }

        public override void FromJson(string data)
        {
            NodeData d = JsonConvert.DeserializeObject<NodeData>(data);
            SetBaseNodeDate(d);
        }
    }
}
