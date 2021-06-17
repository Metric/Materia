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
using Materia.Graph.IO;

namespace Materia.Nodes.Atomic
{
    public class CurvesNode : ImageNode
    {
        NodeInput input;

        FloatBitmap lutBrush;
        GLTexture2D curveLUT;
        CurvesProcessor processor;

        NodeOutput Output;

        Dictionary<int, List<PointF>> points = new Dictionary<int, List<PointF>>();
        [Editable(ParameterInputType.Curves, "Points")]
        public Dictionary<int, List<PointF>> Points
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

        float minValue = 0;
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
                    minValue = value.Clamp(0, 1);
                }

                TriggerValueChange();
            }
        }

        //TODO: come back and make maxValue editable via function graph

        float maxValue = 1;
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
                    maxValue = value.Clamp(0, 1);
                }
                TriggerValueChange();
            }
        }

        public CurvesNode(int w, int h, GraphPixelType p = GraphPixelType.RGBA) : base()
        {
            Name = "Curves";

            width = w;
            height = h;

            lutBrush = new FloatBitmap(256, 2);

            internalPixelType = p;

            //set defaults
            List<PointF> pts = new List<PointF>();
            pts.Add(new PointF(0, 1)); 
            pts.Add(new PointF(1, 0));

            points[0] = pts;
            points[1] = pts;
            points[2] = pts;
            points[3] = pts;

            input = new NodeInput(NodeType.Color | NodeType.Gray, this, "Image Input");
            Output = new NodeOutput(NodeType.Color | NodeType.Gray, this);

            Inputs.Add(input);
            Outputs.Add(Output);
        }

        List<PointF> GetNormalizedCurve(List<PointF> pts)
        {
            List<PointF> points = new List<PointF>();
            List<PointF> normalized = new List<PointF>();

            int w = 255;
            int h = 255;

            for(int i = 0; i < pts.Count; ++i)
            {
                PointF p = pts[i];
                points.Add(new PointF(p.x * w, p.y * h));
            }

            points.Sort((PointF p1, PointF p2) =>
            {
                return (int)p1.x - (int)p2.x;
            });

            List<PointF> curve = new List<PointF>();

            //make sure we have x points on edges
            if (points.Count >= 2)
            {
                PointF f = points[0];

                if (f.x > 0)
                {
                    points.Insert(0, new PointF(0, f.y));
                }

                PointF l = points[points.Count - 1];

                if (l.x < w)
                {
                    points.Add(new PointF(w, l.y));
                }
            }

            float[] sd = Curves.SecondDerivative(points.ToArray());

            for (int i = 0; i < points.Count - 1; ++i)
            {
                PointF cur = points[i];
                PointF next = points[i + 1];

                for (float x = cur.x; x < next.x; ++x)
                {
                    float t = (float)(x - cur.x) / (next.x - cur.x);

                    float a = 1 - t;
                    float b = t;
                    float hn = next.x - cur.x;

                    float y = a * cur.y + b * next.y + (hn * hn / 6) * ((a * a * a - a) * sd[i] + (b * b * b - b) * sd[i + 1]);


                    if (y < 0) y = 0;
                    if (y > h) y = h;

                    normalized.Add(new PointF(x / w, y / h));
                }
            }

            PointF lp = points[points.Count - 1];

            normalized.Add(new PointF(lp.x / w, lp.y / h));

            return normalized;
        }

        private void FillLUT()
        {
            if (!input.HasInput) return;

            try
            {
                List<PointF> mids = GetNormalizedCurve(points[0]);
                List<PointF> reds = GetNormalizedCurve(points[1]);
                List<PointF> greens = GetNormalizedCurve(points[2]);
                List<PointF> blues = GetNormalizedCurve(points[3]);
                for (int j = 0; j < reds.Count; ++j)
                {
                    PointF p = reds[j];
                    int x = 255 - (int)Math.Floor(Math.Min(255, Math.Max(0, p.x * 255)));
                    for (int i = 0; i < 2; ++i)
                    {
                        int idx2 = (x + i * 256) * 4;
                        lutBrush.Image[idx2] = Math.Min(1, Math.Max(0, (float)p.y * (maxValue - minValue) + minValue));
                    }
                }

                for (int j = 0; j < greens.Count; ++j)
                {
                    PointF p = greens[j];
                    int x = 255 -(int)Math.Floor(Math.Min(255, Math.Max(0, p.x * 255)));
                    for (int i = 0; i < 2; i++)
                    {
                        int idx2 = (x + i * 256) * 4;
                        lutBrush.Image[idx2 + 1] = Math.Min(1, Math.Max(0, (float)p.y * (maxValue - minValue) + minValue));
                    }
                }

                for (int j = 0; j < blues.Count; ++j)
                {
                    PointF p = blues[j];
                    int x = 255 - (int)Math.Floor(Math.Min(255, Math.Max(0, p.x * 255)));
                    for (int i = 0; i < 2; i++)
                    {
                        int idx2 = (x + i * 256) * 4;
                        lutBrush.Image[idx2 + 2] = Math.Min(1, Math.Max(0, (float)p.y * (maxValue - minValue) + minValue));
                    }
                }

                for (int j = 0; j < mids.Count; ++j)
                {
                    PointF p = mids[j];
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
            if (isDisposing) return;
            if (!input.HasInput) return;

            GLTexture2D i1 = (GLTexture2D)input.Reference.Data;

            if (i1 == null) return;
            if (i1.Id == 0) return;

            CreateBufferIfNeeded();

            processor ??= new CurvesProcessor(curveLUT);

            curveLUT ??= new GLTexture2D(PixelInternalFormat.Rgba8);
            curveLUT.Bind();
            curveLUT.SetData(lutBrush.Image, PixelFormat.Rgba, 256, 2);
            curveLUT.Nearest();
            curveLUT.Repeat();
            GLTexture2D.Unbind();

            processor.Tiling = GetTiling();

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

            curveLUT?.Dispose();
            curveLUT = null;
        }

        public class CurvesData : NodeData
        {
            public Dictionary<int, List<PointF>> points;
            public float min = 0;
            public float max = 1;

            public override void Write(Writer w)
            {
                base.Write(w);
                w.Write(min);
                w.Write(max);
                w.Write(points.Count);

                foreach(int k in points.Keys)
                {
                    var list = points[k];
                    w.Write(k);
                    w.WriteObjectList(list.ToArray());
                }
            }

            public override void Parse(Reader r)
            {
                base.Parse(r);
                min = r.NextFloat();
                max = r.NextFloat();

                int maxGroups = r.NextInt();

                points = new Dictionary<int, List<PointF>>();
                for (int i = 0; i < maxGroups; ++i)
                {
                    int k = r.NextInt();
                    points[k] = new List<PointF>(r.NextList<PointF>());
                }
            }
        }

        public override void GetBinary(Writer w)
        {
            CurvesData d = new CurvesData();
            FillBaseNodeData(d);
            d.points = points;
            d.min = minValue;
            d.max = maxValue;
            d.Write(w);
        }

        public override void FromBinary(Reader r)
        {
            CurvesData d = new CurvesData();
            d.Parse(r);
            SetBaseNodeDate(d);
            points = new Dictionary<int, List<PointF>>();
            minValue = d.min;
            maxValue = d.max;

            foreach (int k in d.points.Keys)
            {
                List<PointF> pts = d.points[k];
                points[k] = pts ?? new List<PointF>();
            }
        }

        public override void FromJson(string data)
        {
            CurvesData d = JsonConvert.DeserializeObject<CurvesData>(data);
            SetBaseNodeDate(d);

            points = new Dictionary<int, List<PointF>>();
            minValue = d.min;
            maxValue = d.max;

            foreach(int k in d.points.Keys)
            {
                List<PointF> pts = d.points[k];
                points[k] = pts ?? new List<PointF>();
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
