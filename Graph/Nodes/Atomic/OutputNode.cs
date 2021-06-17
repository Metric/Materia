using System;
using System.Collections.Generic;
using Materia.Rendering.Attributes;
using Newtonsoft.Json;
using Materia.Rendering.Textures;
using Materia.Rendering.Imaging.Processing;
using Materia.Graph;
using Materia.Graph.IO;

namespace Materia.Nodes.Atomic
{
    /// <summary>
    /// An output node simply takes in 
    /// an input to distribute to other graphs
    /// or to export the final texture
    /// they can only have one input
    /// and no actual outputs
    /// </summary>
    public class OutputNode : ImageNode
    {
        ImageProcessor processor;
        NodeInput input;

        OutputType outtype;
        [Editable(ParameterInputType.Dropdown, "Out Type")]
        public OutputType OutType
        {
            get
            {
                return outtype;
            }
            set
            {
                outtype = value;
            }
        }

       
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

        public OutputNode(GraphPixelType p = GraphPixelType.RGBA) : base()
        {
            OutType = OutputType.basecolor;

            Name = "Output";

            width = height = 16;

            internalPixelType = p;

            input = new NodeInput(NodeType.Color | NodeType.Gray, this);
            Inputs.Add(input);

            Outputs.Clear();
        }

        public override void TryAndProcess()
        {
            Process();
        }

        public override GLTexture2D GetActiveBuffer()
        {
            return buffer;
        }

        void Process()
        {
            if (isDisposing) return;
            if (!input.HasInput) return;

            GLTexture2D i1 = (GLTexture2D)input.Reference.Data;

            if (i1 == null) return;
            if (i1.Id == 0) return;

            height = i1.Height;
            width = i1.Width;

            CreateBufferIfNeeded();

            processor ??= new ImageProcessor();

            processor.PrepareView(buffer);
            processor.Process(i1);
            processor.Complete();

            //there should only ever ben 1 nodeoutput
            //on an output node if it is part of a graph instance
            //otherwise it will have none
            if (Outputs.Count > 0)
            {
                Outputs[0].Data = buffer;
                //do this here for the parent graph instance
                //so we don't have to worry about it later
                Outputs[0]?.Node.TriggerTextureChange();
            }

            TriggerTextureChange();
        }


        public class OutputNodeData : NodeData
        {
            public OutputType outType;

            public override void Write(Writer w)
            {
                base.Write(w);
                w.Write((int)outType);
            }

            public override void Parse(Reader r)
            {
                base.Parse(r);
                outType = (OutputType)r.NextInt();
            }
        }

        public override void GetBinary(Writer w)
        {
            OutputNodeData d = new OutputNodeData();
            FillBaseNodeData(d);
            d.outType = outtype;
            d.Write(w);
        }

        public override void FromBinary(Reader r)
        {
            OutputNodeData d = new OutputNodeData();
            d.Parse(r);
            SetBaseNodeDate(d);
            outtype = d.outType;
        }

        public override void FromJson(string data)
        {
            OutputNodeData d = JsonConvert.DeserializeObject<OutputNodeData>(data);
            SetBaseNodeDate(d);
            outtype = d.outType;
        }

        public override string GetJson()
        {
            OutputNodeData d = new OutputNodeData();
            FillBaseNodeData(d);
            d.outputs = new List<NodeConnection>();
            d.outType = OutType;

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
