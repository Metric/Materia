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

namespace Materia.Nodes.Atomic
{
    public class BitmapNode : ImageNode
    {
        NodeOutput Output;

        string relativePath;

        string path;
        [FileSelector(Filter = "Image Files (*.png;*.jpg;*.jpeg;*.bmp;*.tif;*.gif)|*.png;*.jpg;*.jpeg;*.bmp;*.tif;*.gif")]
        [Title(Title ="Image File")]
        [Section(Section = "Content")]
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

                Process();
            }
        }

        [Section(Section = "Content")]
        public bool Resource
        {
            get; set;
        }

        //we declare these new to hide them

        [HideProperty]
        public new int Width
        {
            get
            {
                return width;
            }
        }

        [HideProperty]
        public new int Height
        {
            get
            {
                return height;
            }
        }

        [HideProperty]
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

        [HideProperty]
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
            Process();
        }

        void Process()
        {
            Task.Run(() =>
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
                    else if(!string.IsNullOrEmpty(relativePath) && ParentGraph != null && !string.IsNullOrEmpty(ParentGraph.CWD) && File.Exists(System.IO.Path.Combine(ParentGraph.CWD, relativePath)))
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
                    Console.WriteLine(e.StackTrace);
                }

                System.GC.Collect();

            })
            .ContinueWith(t =>
            {
                App.Current.Dispatcher.Invoke(() =>
                {
                    if (brush == null) return;

                    CreateBufferIfNeeded();

                    buffer.Bind();
                    buffer.SetData(brush.Image, OpenTK.Graphics.OpenGL.PixelFormat.Rgba, width, height);
                    GLTextuer2D.Unbind();

                    brush = null;

                    Updated();
                    Output.Data = buffer;
                    Output.Changed();
                });
            });
        }

        public class BitmapNodeData : NodeData
        {
            public string path;
            public string relativePath;
            public bool resource;
        }

        public override void FromJson(Dictionary<string, Node> nodes, string data)
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

            if(string.IsNullOrEmpty(relativePath) || string.IsNullOrEmpty(path))
            {
                return;
            }

            string cpath = System.IO.Path.Combine(CWD, relativePath);
            string opath = System.IO.Path.Combine(ParentGraph.CWD, relativePath);
            if (!Directory.Exists(cpath))
            {
                Directory.CreateDirectory(cpath);
            }


            if (File.Exists(path))
            {
                File.Copy(path, cpath);
            }
            else if (File.Exists(opath) && !opath.ToLower().Equals(cpath.ToLower()))
            {
                File.Copy(opath, cpath);
            }
        }

        public override void Dispose()
        {
            base.Dispose();  
        }

        protected override void OnWidthHeightSet()
        {
            //we don't do anything here in this one
        }
    }
}
