using System;
using Materia.Rendering.Attributes;
using Newtonsoft.Json;
using Materia.Rendering.Imaging.Processing;
using Materia.Rendering.Textures;
using Materia.Graph;
using Materia.Graph.IO;

namespace Materia.Nodes.Atomic
{
    public class InputNode : ImageNode
    {
        ImageProcessor processor;

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

        NodeOutput Output;

        public InputNode(GraphPixelType p = GraphPixelType.RGBA) : base()
        {
            defaultName = Name = "Input";

            internalPixelType = p;

            //this actually does nothing for this node
            width = 16;
            height = 16;

            //only an output is present
            Output = new NodeOutput(NodeType.Color | NodeType.Gray, this);
            Outputs.Add(Output);
        }

        public override GLTexture2D GetActiveBuffer()
        {
            return buffer;
        }

        public override void TryAndProcess()
        {
            Process();
        }

        void Process()
        {
            if (isDisposing) return;
            if (Inputs.Count == 0 || !Inputs[0].HasInput) return;

            GLTexture2D i1 = (GLTexture2D)Inputs[0].Reference.Data;

            if (i1 == null) return;
            if (i1.Id == 0) return;

            width = i1.Width;
            height = i1.Height;

            CreateBufferIfNeeded();

            processor ??= new ImageProcessor();

            processor.PrepareView(buffer);
            processor.Process(i1);
            processor.Complete();

            Output.Data = buffer;
            TriggerTextureChange();
        }

        public override void Dispose()
        {
            base.Dispose();
            processor?.Dispose();
            processor = null;
        }
    }
}
