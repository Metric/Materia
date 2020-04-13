using System;
using System.Collections.Generic;
using Materia.Rendering.Attributes;
using Newtonsoft.Json;
using Materia.Rendering.Textures;
using Materia.Rendering.Imaging.Processing;
using Materia.Graph;

namespace Materia.Nodes.Atomic
{
    public enum OutputType
    {
        basecolor,
        height,
        occlusion,
        roughness,
        metallic,
        normal,
        thickness,
        emission
    }

    /// <summary>
    /// An output node simply takes in 
    /// an input to distribute to other graphs
    /// or to export the final texture
    /// they can only have one input
    /// and no actual outputs
    /// </summary>
    public class OutputNode : ImageNode
    {
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

            Id = Guid.NewGuid().ToString();

            width = height = 16;
            tileX = tileY = 1;

            internalPixelType = p;

            previewProcessor = new BasicImageRenderer();

            input = new NodeInput(NodeType.Color | NodeType.Gray, this);
            Inputs.Add(input);

            Outputs = new List<NodeOutput>();
        }

        public override void TryAndProcess()
        {
            Process();
        }

        void Process()
        {
            if (!input.HasInput) return;

            GLTexture2D i1 = (GLTexture2D)input.Reference.Data;

            if (i1 == null) return;
            if (i1.Id == 0) return;

            height = i1.Height;
            width = i1.Width;

            CreateBufferIfNeeded();

            previewProcessor.Process(width, height, i1, buffer);
            previewProcessor.Complete();

            if (Outputs.Count > 0)
            {
                Outputs[0].Data = buffer;
            }
        }


        public class OutputNodeData : NodeData
        {
            public OutputType outType;
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
    }
}
