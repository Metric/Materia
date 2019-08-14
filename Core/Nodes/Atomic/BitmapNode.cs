using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Materia.Imaging;
using System.IO;
using System.Threading;
using Materia.Nodes.Helpers;
using System.Drawing;
using Materia.Nodes.Attributes;
using Materia.Textures;
using Materia.Imaging.GLProcessing;
using Newtonsoft.Json;
using NLog;

namespace Materia.Nodes.Atomic
{
    public class BitmapNode : ImageNode
    {
        private static ILogger Log = LogManager.GetCurrentClassLogger();

        NodeOutput Output;

        CancellationTokenSource ctk;

        string relativePath;

        string path;
        [Editable(ParameterInputType.ImageFile, "Image", "Content")]
        public string Path
        {
            get
            {
                return path;
            }
            set
            {
                path = value;
                if (string.IsNullOrEmpty(path))
                {
                    relativePath = "";
                }
                else
                {
                    relativePath = System.IO.Path.Combine("resources", System.IO.Path.GetFileName(path));
                }

                TryAndProcess();
            }
        }

        [Editable(ParameterInputType.Toggle, "Resource", "Content")]
        public bool Resource
        {
            get; set;
        }

        //we declare these new to hide them

        public new int Width
        {
            get
            {
                return width;
            }
        }

        public new int Height
        {
            get
            {
                return height;
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

        public BitmapNode(int w, int h, GraphPixelType p = GraphPixelType.RGBA)
        {
            Name = "Bitmap";

            Id = Guid.NewGuid().ToString();

            previewProcessor = new BasicImageRenderer();

            internalPixelType = p;

            tileX = tileY = 1;

            width = w;
            height = h;

            Output = new NodeOutput(NodeType.Color | NodeType.Gray, this);
            Inputs = new List<NodeInput>();

            Outputs = new List<NodeOutput>();
            Outputs.Add(Output);
        }

        public override void TryAndProcess()
        {
            if(!Async)
            {
                LoadBitmap();
                Process();
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

                if (ParentGraph != null)
                {
                    ParentGraph.Schedule(this);
                }
            //}, Context);
        }

        private void LoadBitmap()
        {
            try
            {
                if (!string.IsNullOrEmpty(path) && File.Exists(path))
                {
                    Bitmap bmp = (Bitmap)Bitmap.FromFile(path);

                    if (bmp != null && bmp.Width > 0 && bmp.Height > 0)
                    {
                        width = bmp.Width;
                        height = bmp.Height;

                        brush = FloatBitmap.FromBitmap(bmp);
                    }
                }
                else if (!string.IsNullOrEmpty(relativePath) && ParentGraph != null && !string.IsNullOrEmpty(ParentGraph.CWD) && File.Exists(System.IO.Path.Combine(ParentGraph.CWD, relativePath)))
                {
                    Bitmap bmp = (Bitmap)Bitmap.FromFile(System.IO.Path.Combine(ParentGraph.CWD, relativePath));

                    if (bmp != null && bmp.Width > 0 && bmp.Height > 0)
                    {
                        width = bmp.Width;
                        height = bmp.Height;

                        brush = FloatBitmap.FromBitmap(bmp);
                    }
                }

            }
            catch (Exception e)
            {
                Log.Error(e);
            }

            System.GC.Collect();
        }

        public override Task GetTask()
        {
            return Task.Factory.StartNew(() =>
            {
                LoadBitmap();

            }).ContinueWith(t =>
            {
                Process();
            }, Context);
        }

        void Process()
        {
            if (brush == null) return;

            CreateBufferIfNeeded();

            buffer.Bind();
            buffer.SetData(brush.Image, GLInterfaces.PixelFormat.Rgba, width, height);
            GLTextuer2D.Unbind();

            brush = null;

            Updated();
            Output.Data = buffer;
            Output.Changed();
        }

        public class BitmapNodeData : NodeData
        {
            public string path;
            public string relativePath;
            public bool resource;
        }

        public override void FromJson(string data)
        {
            BitmapNodeData d = JsonConvert.DeserializeObject<BitmapNodeData>(data);
            SetBaseNodeDate(d);
            path = d.path;
            Resource = d.resource;
            relativePath = d.relativePath;
        }

        public override string GetJson()
        {
            BitmapNodeData d = new BitmapNodeData();
            FillBaseNodeData(d);
            d.path = Path;
            d.relativePath = relativePath;
            d.resource = Resource;

            return JsonConvert.SerializeObject(d);
        }

        public override void CopyResources(string CWD)
        {
            if (!Resource) return;

            CopyResourceTo(CWD, relativePath, path);
        }

        protected override void OnWidthHeightSet()
        {
            //we don't do anything here in this one
        }
    }
}
