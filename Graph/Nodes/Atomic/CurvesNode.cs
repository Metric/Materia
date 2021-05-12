using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Materia.Rendering.Imaging.Processing;
using Materia.Rendering.Textures;
using Materia.Rendering.Imaging;
using Materia.Rendering.Attributes;
using Materia.Rendering.Mathematics;
using Materia.Rendering.Interfaces;
using Materia.Graph;
using MLog;

namespace Materia.Nodes.Atomic
{
    public class CurvesNode : ImageNode
    {
        NodeInput input;

        FloatBitmap lutBrush;
        GLTexture2D curveLUT;
        CurvesProcessor processor;

        NodeOutput Output;

        Dictionary<int, List<PointD>> points;
        [Editable(ParameterInputType.Curves, "Points")]
        public Dictionary<int, List<PointD>> Points
        {
            get
            {
                return points;
            }
            set
            {
                points = value;
                TriggerValueChange();
            }
        }

        //TODO: come back and make minValue editable via function graph

        float minValue;
        public float MinValue
        {
            get
            {
                return minValue;
            }
            set
            {
                if (minValue != value)
                {
                    minValue = Math.Min(1, Math.Max(0, value));
                }

                TriggerValueChange();
            }
        }

        //TODO: come back and make maxValue editable via function graph

        float maxValue;
        public float MaxValue
        {
            get
            {
                return maxValue;
            }
            set
            {
                if (maxValue != value)
                {
                    maxValue = Math.Min(1, Math.Max(0, value));
                }
                TriggerValueChange();
            }
        }

        public CurvesNode(int w, int h, GraphPixelType p = GraphPixelType.RGBA) : base()
        {
            Name = "Curves";
            Id = Guid.NewGuid().ToString();

            width = w;
            height = h;

            minValue = 0;
            maxValue = 1;

            points = new Dictionary<int, List<PointD>>();

            tileX = tileY = 1;

            lutBrush = new FloatBitmap(256, 2);

            internalPixelType = p;

            curveLUT = new GLTexture2D(PixelInternalFormat.Rgba8);
            curveLUT.Bind();
            curveLUT.SetFilter((int)TextureMinFilter.Nearest, (int)TextureMagFilter.Nearest);
            curveLUT.SetWrap((int)TextureWrapMode.Repeat);
            GLTexture2D.Unbind();

            processor = new CurvesProcessor(curveLUT);

            //set defaults
            List<PointD> pts = new List<PointD>();
            pts.Add(new PointD(0, 1)); 
            pts.Add(new PointD(1, 0));

            points[0] = pts;
            points[1] = pts;
            points[2] = pts;
            points[3] = pts;

            input = new NodeInput(NodeType.Color | NodeType.Gray, this, "Image Input");
            Output = new NodeOutput(NodeType.Color | NodeType.Gray, this);

            Inputs.Add(input);
            Outputs.Add(Output);
        }

        List<PointD> GetNormalizedCurve(List<PointD> pts)
        {
            List<PointD> points = new List<PointD>();
            List<PointD> normalized = new List<PointD>();

            int w = 255;
            int h = 255;

            for(int i = 0; i < pts.Count; ++i)
            {
                PointD p = pts[i];
                points.Add(new PointD(p.x * w, p.y * h));
            }

            points.Sort((PointD p1, PointD p2) =>
            {
                return (int)p1.x - (int)p2.x;
            });

            List<PointD> curve = new List<PointD>();

            //make sure we have x points on edges
            if (points.Count >= 2)
            {
                PointD f = points[0];

                if (f.x > 0)
                {
                    points.Insert(0, new PointD(0, f.y));
                }

                PointD l = points[points.Count - 1];

                if (l.x < w)
                {
                    points.Add(new PointD(w, l.y));
                }
            }

            double[] sd = Curves.SecondDerivative(points.ToArray());

            for (int i = 0; i < points.Count - 1; ++i)
            {
                PointD cur = points[i];
                PointD next = points[i + 1];

                for (double x = cur.x; x < next.x; ++x)
                {
                    double t = (double)(x - cur.x) / (next.x - cur.x);

                    double a = 1 - t;
                    double b = t;
                    double hn = next.x - cur.x;

                    double y = a * cur.y + b * next.y + (hn * hn / 6) * ((a * a * a - a) * sd[i] + (b * b * b - b) * sd[i + 1]);


                    if (y < 0) y = 0;
                    if (y > h) y = h;

                    normalized.Add(new PointD(x / w, y / h));
                }
            }

            PointD lp = points[points.Count - 1];

            normalized.Add(new PointD(lp.x / w, lp.y / h));

            return normalized;
        }

        private void FillLUT()
        {
            if (!input.HasInput) return;

            try
            {
                List<PointD> mids = GetNormalizedCurve(points[0]);
                List<PointD> reds = GetNormalizedCurve(points[1]);
                List<PointD> greens = GetNormalizedCurve(points[2]);
                List<PointD> blues = GetNormalizedCurve(points[3]);
                for (int j = 0; j < reds.Count; ++j)
                {
                    PointD p = reds[j];
                    int x = 255 - (int)Math.Floor(Math.Min(255, Math.Max(0, p.x * 255)));
                    for (int i = 0; i < 2; ++i)
                    {
                        int idx2 = (x + i * 256) * 4;
                        lutBrush.Image[idx2] = Math.Min(1, Math.Max(0, (float)p.y * (maxValue - minValue) + minValue));
                    }
                }

                for (int j = 0; j < greens.Count; ++j)
                {
                    PointD p = greens[j];
                    int x = 255 -(int)Math.Floor(Math.Min(255, Math.Max(0, p.x * 255)));
                    for (int i = 0; i < 2; i++)
                    {
                        int idx2 = (x + i * 256) * 4;
                        lutBrush.Image[idx2 + 1] = Math.Min(1, Math.Max(0, (float)p.y * (maxValue - minValue) + minValue));
                    }
                }

                for (int j = 0; j < blues.Count; ++j)
                {
                    PointD p = blues[j];
                    int x = 255 - (int)Math.Floor(Math.Min(255, Math.Max(0, p.x * 255)));
                    for (int i = 0; i < 2; i++)
                    {
                        int idx2 = (x + i * 256) * 4;
                        lutBrush.Image[idx2 + 2] = Math.Min(1, Math.Max(0, (float)p.y * (maxValue - minValue) + minValue));
                    }
                }

                for (int j = 0; j < mids.Count; ++j)
                {
                    PointD p = mids[j];
                    int x = 255 - (int)Math.Floor(Math.Min(255, Math.Max(0, p.x * 255)));
                    for (int i = 0; i < 2; i++)
                    {
                        int idx2 = (x + i * 256) * 4;
                        lutBrush.Image[idx2 + 3] = Math.Min(1, Math.Max(0, (float)p.y * (maxValue - minValue) + minValue));
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }

        public override void TryAndProcess()
        {
            FillLUT();
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

            curveLUT.Bind();
            curveLUT.SetData(lutBrush.Image, PixelFormat.Rgba, 256, 2);
            GLTexture2D.Unbind();

            processor.PrepareView(buffer);

            processor.Tiling = new Vector2(TileX, TileY);
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

            curveLUT?.Dispose();
            curveLUT = null;
        }

        public class CurvesData : NodeData
        {
            public Dictionary<int, List<PointD>> points;
            public float min = 0;
            public float max = 1;
        }

        public override void FromJson(string data)
        {
            CurvesData d = JsonConvert.DeserializeObject<CurvesData>(data);
            SetBaseNodeDate(d);

            points = new Dictionary<int, List<PointD>>();
            minValue = d.min;
            maxValue = d.max;

            foreach(int k in d.points.Keys)
            {
                List<PointD> pts = d.points[k];
                points[k] = pts ?? new List<PointD>();
            }
        }


        public override string GetJson()
        {
            CurvesData d = new CurvesData();
            FillBaseNodeData(d);
            d.points = points;
            d.min = minValue;
            d.max = maxValue;

            return JsonConvert.SerializeObject(d);
        }
    }
}
