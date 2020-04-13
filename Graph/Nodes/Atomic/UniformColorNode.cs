﻿using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Materia.Rendering.Imaging.Processing;
using Materia.Rendering.Attributes;
using Materia.Rendering.Mathematics;
using Materia.Graph;

namespace Materia.Nodes.Atomic
{
    public class UniformColorNode : ImageNode
    {
        MVector color;

        [Promote(NodeType.Float4)]
        [Editable(ParameterInputType.Color, "Color")]
        public MVector Color
        {
            get
            {
                return color;
            }
            set
            {
                color = value;
                TriggerValueChange();
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

        NodeOutput output;
        UniformColorProcessor processor;

        public UniformColorNode(int w, int h, GraphPixelType p = GraphPixelType.RGBA) : base()
        {
            Name = "Uniform Color";

            Id = Guid.NewGuid().ToString();

            width = w;
            height = h;

            tileX = tileY = 1;

            color = new MVector(0, 0, 0, 1);

            processor = new UniformColorProcessor();
            previewProcessor = new BasicImageRenderer();

            internalPixelType = p;

            Inputs = new List<NodeInput>();
            output = new NodeOutput(NodeType.Color | NodeType.Gray, this);
            Outputs = new List<NodeOutput>();
            Outputs.Add(output);
        }


        private void GetParams()
        {
            pcolor = new Vector4(color.X, color.Y, color.Z, color.W);

            if (ParentGraph != null && ParentGraph.HasParameterValue(Id, "Color"))
            {
                object obj = ParentGraph.GetParameterValue(Id, "Color");

                if (obj is MVector)
                {
                    MVector m = (MVector)obj;

                    pcolor.X = m.X;
                    pcolor.Y = m.Y;
                    pcolor.Z = m.Z;
                    pcolor.W = m.W;
                }
                else if (obj is Vector4)
                {
                    pcolor = (Vector4)obj;
                }
            }
        }

        public override void TryAndProcess()
        {
            GetParams();
            Process();
        }

        Vector4 pcolor;
        void Process()
        {
            CreateBufferIfNeeded();

            processor.Color = pcolor;
            processor.Process(width, height, null, buffer);
            processor.Complete();

            output.Data = buffer;
            TriggerTextureChange();
        }

        public class UniformColorNodeData : NodeData
        {
            public float[] color;
        }

        public override void FromJson(string data)
        {
            UniformColorNodeData d = JsonConvert.DeserializeObject<UniformColorNodeData>(data);
            SetBaseNodeDate(d);
            float[] c = d.color;
            color = new MVector(c[0], c[1], c[2], c[3]);
        }

        public override string GetJson()
        {
            UniformColorNodeData d = new UniformColorNodeData();
            FillBaseNodeData(d);

            d.color = new float[] { color.X, color.Y, color.Z, color.W };

            return JsonConvert.SerializeObject(d);
        }

        public override void Dispose()
        {
            base.Dispose();

            if(processor != null)
            {
                processor.Dispose();
            }
        }
    }
}
