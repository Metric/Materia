using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows;
using Materia.Nodes.Attributes;
using Materia.Imaging;
using Materia.Nodes.Helpers;
using Newtonsoft.Json;
using Materia.Textures;
using Materia.MathHelpers;
using Materia.Imaging.GLProcessing;
using NLog;

namespace Materia.Nodes.Atomic
{
    public class CurvesNode : ImageNode
    {
        private static ILogger Log = LogManager.GetCurrentClassLogger();

        CancellationTokenSource ctk;

        NodeInput input;

        FloatBitmap lutBrush;
        GLTextuer2D curveLUT;
        CurvesProcessor processor;

        NodeOutput Output;

        Dictionary<int, List<Point>> points;
        [Editable(ParameterInputType.Curves, "Points")]
        public Dictionary<int, List<Point>> Points
        {
            get
            {
                return points;
            }
            set
            {
                points = value;
                TryAndProcess();
            }
        }

        public CurvesNode(int w, int h, GraphPixelType p = GraphPixelType.RGBA)
        {
            Name = "Curves";
            Id = Guid.NewGuid().ToString();

            width = w;
            height = h;
    
            points = new Dictionary<int, List<Point>>();

            previewProcessor = new BasicImageRenderer();

            tileX = tileY = 1;

            lutBrush = new FloatBitmap(256, 2);

            internalPixelType = p;

            curveLUT = new GLTextuer2D(GLInterfaces.PixelInternalFormat.Rgba8);
            curveLUT.Bind();
            curveLUT.SetFilter((int)GLInterfaces.TextureMinFilter.Nearest, (int)GLInterfaces.TextureMagFilter.Nearest);
            curveLUT.SetWrap((int)GLInterfaces.TextureWrapMode.Repeat);
            GLTextuer2D.Unbind();

            processor = new CurvesProcessor(curveLUT);

            //set defaults
            List<Point> pts = new List<Point>();
            pts.Add(new Point(0, 1)); 
            pts.Add(new Point(1, 0));

            points[0] = pts;
            points[1] = pts;
            points[2] = pts;
            points[3] = pts;

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
            if(!Async)
            {
                if(input.HasInput)
                {
                    FillLUT();
                    Process();
                }

                return;
            }

            //if (ctk != null)
            //{
            //    ctk.Cancel();
            //}

            //ctk = new CancellationTokenSource();

            //Task.Delay(25, ctk.Token).ContinueWith(t =>
            //{
            //    if (t.IsCanceled) return;

                if (input.HasInput)
                {
                    if (ParentGraph != null)
                    {
                        ParentGraph.Schedule(this);
                    }
                }
            //}, Context);
        }

        List<Point> GetNormalizedCurve(List<Point> pts)
        {
            List<Point> points = new List<Point>();
            List<Point> normalized = new List<Point>();

            int w = 255;
            int h = 255;

            for(int i = 0; i < pts.Count; i++)
            {
                Point p = pts[i];
                points.Add(new Point(p.X * w, p.Y * h));
            }

            points.Sort((Point p1, Point p2) =>
            {
                return (int)p1.X - (int)p2.X;
            });

            List<Point> curve = new List<Point>();

            //make sure we have x points on edges
            if (points.Count >= 2)
            {
                Point f = points[0];

                if (f.X > 0)
                {
                    points.Insert(0, new Point(0, f.Y));
                }

                Point l = points[points.Count - 1];

                if (l.X < w)
                {
                    points.Add(new Point(w, l.Y));
                }
            }

            double[] sd = Curves.SecondDerivative(points.ToArray());

            for (int i = 0; i < points.Count - 1; i++)
            {
                Point cur = points[i];
                Point next = points[i + 1];

                for (double x = cur.X; x < next.X; x++)
                {
                    double t = (double)(x - cur.X) / (next.X - cur.X);

                    double a = 1 - t;
                    double b = t;
                    double hn = next.X - cur.X;

                    double y = a * cur.Y + b * next.Y + (hn * hn / 6) * ((a * a * a - a) * sd[i] + (b * b * b - b) * sd[i + 1]);


                    if (y < 0) y = 0;
                    if (y > h) y = h;

                    normalized.Add(new Point(x / w, y / h));
                }
            }

            Point lp = points[points.Count - 1];

            normalized.Add(new Point(lp.X / w, lp.Y / h));

            return normalized;
        }

        private void FillLUT()
        {
            try
            {
                List<Point> mids = GetNormalizedCurve(points[0]);
                List<Point> reds = GetNormalizedCurve(points[1]);
                List<Point> greens = GetNormalizedCurve(points[2]);
                List<Point> blues = GetNormalizedCurve(points[3]);
                for (int j = 0; j < reds.Count; j++)
                {
                    Point p = reds[j];
                    int x = 255 - (int)Math.Floor(Math.Min(255, Math.Max(0, p.X * 255)));
                    for (int i = 0; i < 2; i++)
                    {
                        int idx2 = (x + i * 256) * 4;
                        lutBrush.Image[idx2] = (float)p.Y;
                    }
                }

                for (int j = 0; j < greens.Count; j++)
                {
                    Point p = greens[j];
                    int x = 255 - (int)Math.Floor(Math.Min(255, Math.Max(0, p.X * 255)));
                    for (int i = 0; i < 2; i++)
                    {
                        int idx2 = (x + i * 256) * 4;
                        lutBrush.Image[idx2 + 1] = (float)p.Y;
                    }
                }

                for (int j = 0; j < blues.Count; j++)
                {
                    Point p = blues[j];
                    int x = 255 - (int)Math.Floor(Math.Min(255, Math.Max(0, p.X * 255)));
                    for (int i = 0; i < 2; i++)
                    {
                        int idx2 = (x + i * 256) * 4;
                        lutBrush.Image[idx2 + 2] = (float)p.Y;
                    }
                }

                for (int j = 0; j < mids.Count; j++)
                {
                    Point p = mids[j];
                    int x = 255 - (int)Math.Floor(Math.Min(255, Math.Max(0, p.X * 255)));
                    for (int i = 0; i < 2; i++)
                    {
                        int idx2 = (x + i * 256) * 4;
                        lutBrush.Image[idx2 + 3] = (float)p.Y;
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }

        public override Task GetTask()
        {
            return Task.Factory.StartNew(() =>
            {
                FillLUT();
            })
            .ContinueWith(t =>
            {
                if(input.HasInput)
                {
                    Process();
                }
            }, Context);
        }

        void Process()
        {
            GLTextuer2D i1 = (GLTextuer2D)input.Input.Data;

            if (i1 == null) return;
            if (i1.Id == 0) return;

            CreateBufferIfNeeded();

            curveLUT.Bind();
            curveLUT.SetData(lutBrush.Image, GLInterfaces.PixelFormat.Rgba, 256, 2);
            GLTextuer2D.Unbind();

            processor.TileX = tileX;
            processor.TileY = tileY;
            processor.Process(width, height, i1, buffer);
            processor.Complete();

            Updated();
            Output.Data = buffer;
            Output.Changed();
        }

        public override void Dispose()
        {
            base.Dispose();

            if(processor != null)
            {
                processor.Release();
                processor = null;
            }

            if(curveLUT != null)
            {
                curveLUT.Release();
                curveLUT = null;
            }
        }

        public class CurvesData : NodeData
        {
            public Dictionary<int, List<Graph.GPoint>> points;
        }

        public override void FromJson(string data)
        {
            CurvesData d = JsonConvert.DeserializeObject<CurvesData>(data);
            SetBaseNodeDate(d);

            points = new Dictionary<int, List<Point>>();

            foreach(int k in d.points.Keys)
            {
                List<Graph.GPoint> pts = d.points[k];
                points[k] = new List<Point>();

                Parallel.For(0, pts.Count, i =>
                {
                    Graph.GPoint gp = pts[i];
                    points[k].Add(gp.ToPoint());
                });
            }
        }


        public override string GetJson()
        {
            CurvesData d = new CurvesData();
            FillBaseNodeData(d);
            d.points = new Dictionary<int, List<Graph.GPoint>>();

            foreach(int k in points.Keys)
            {
                List<Point> pts = points[k];
                d.points[k] = new List<Graph.GPoint>();

                Parallel.For(0, pts.Count, i =>
                {
                    Point p = pts[i];
                    Graph.GPoint gp = new Graph.GPoint(p);
                    d.points[k].Add(gp);
                });
            } 

            return JsonConvert.SerializeObject(d);
        }

        protected override void OnWidthHeightSet()
        {
            TryAndProcess();
        }
    }
}
