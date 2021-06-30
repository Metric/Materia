using System;
using System.Collections.Generic;
using System.IO;
using System.Drawing;
using Materia.Rendering.Attributes;
using Materia.Rendering.Imaging;
using Materia.Rendering.Interfaces;
using Materia.Rendering.Textures;
using Materia.Graph;
using Newtonsoft.Json;
using MLog;
using Materia.Graph.IO;

namespace Materia.Nodes.Atomic
{
    public class BitmapNode : ImageNode
    {
        NodeOutput Output;

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
                TriggerValueChange();
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

        //we override here as the bitmap is always
        //absolute
        public new bool AbsoluteSize { get; set; }

        private Archive archive;
        private RawBitmap brush;

        public BitmapNode(int w, int h, GraphPixelType p = GraphPixelType.RGBA) : base()
        {
            defaultName = Name = "Bitmap";

            AbsoluteSize = true;

            internalPixelType = p;

            width = w;
            height = h;

            Output = new NodeOutput(NodeType.Color | NodeType.Gray, this);
            Outputs.Add(Output);
        }

        private void LoadBitmap()
        {
            try
            {
                if(archive != null && !string.IsNullOrEmpty(relativePath) && Resource)
                {
                    archive.Open();
                    List<Archive.ArchiveFile> files = archive.GetAvailableFiles();

                    var m = files.Find(f => f.path.Equals(relativePath));
                    if (m != null)
                    {
                        using(Stream ms = m.GetStream())
                        using (Bitmap bmp = (Bitmap)Bitmap.FromStream(ms))
                        {
                            if (bmp != null && bmp.Width > 0 && bmp.Height > 0)
                            {
                                width = bmp.Width;
                                height = bmp.Height;
                                brush = RawBitmap.FromBitmap(bmp);
                                archive.Close();
                                return;
                            }
                        }
                    }

                    archive.Close();
                }

                if (!string.IsNullOrEmpty(path) && File.Exists(path))
                {
                    using (Bitmap bmp = (Bitmap)Bitmap.FromFile(path))
                    {
                        if (bmp != null && bmp.Width > 0 && bmp.Height > 0)
                        {
                            width = bmp.Width;
                            height = bmp.Height;

                            brush = RawBitmap.FromBitmap(bmp);
                        }
                    }
                }
                else if (!string.IsNullOrEmpty(relativePath) && !string.IsNullOrEmpty(CurrentWorkingDirectory) && File.Exists(System.IO.Path.Combine(CurrentWorkingDirectory, relativePath)))
                {
                    using (Bitmap bmp = (Bitmap)Bitmap.FromFile(System.IO.Path.Combine(CurrentWorkingDirectory, relativePath)))
                    {
                        if (bmp != null && bmp.Width > 0 && bmp.Height > 0)
                        {
                            width = bmp.Width;
                            height = bmp.Height;

                            brush = RawBitmap.FromBitmap(bmp);
                        }
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
            LoadBitmap();
            Process();
        }

        void Process()
        {
            if (isDisposing) return;
            if (brush == null) return;

            CreateBufferIfNeeded();

            buffer.Bind();
            PixelFormat format = PixelFormat.Bgra;

            if (brush.BPP == 24)
            {
                format = PixelFormat.Bgr;
            }
            else if(brush.BPP == 16)
            {
                format = PixelFormat.Rg;
            }
            else if(brush.BPP == 8)
            {
                format = PixelFormat.Red;
            }

            buffer.SetData(brush.Image, format, width, height);
            GLTexture2D.Unbind();

            brush = null;
            Output.Data = buffer;
            TriggerTextureChange();
        }

        public class BitmapNodeData : NodeData
        {
            public string path;
            public string relativePath;
            public bool resource;

            public override void Write(Writer w)
            {
                base.Write(w);
                w.Write(path);
                w.Write(relativePath);
                w.Write(resource);
            }

            public override void Parse(Reader r)
            {
                base.Parse(r);
                path = r.NextString();
                relativePath = r.NextString();
                resource = r.NextBool();
            }
        }

        public override void FromBinary(Reader r, Archive arch = null)
        {
            archive = arch;
            FromBinary(r);
        }

        public override void FromBinary(Reader r)
        {
            BitmapNodeData d = new BitmapNodeData();
            d.Parse(r);
            SetBaseNodeDate(d);
            path = d.path;
            Resource = d.resource;
            relativePath = d.relativePath;
        }

        public override void FromJson(string data, Archive arch = null)
        {
            archive = arch;
            FromJson(data);
        }

        public override void FromJson(string data)
        {
            BitmapNodeData d = JsonConvert.DeserializeObject<BitmapNodeData>(data);
            SetBaseNodeDate(d);
            path = d.path;
            Resource = d.resource;
            relativePath = d.relativePath;
        }

        public override void GetBinary(Writer w)
        {
            BitmapNodeData d = new BitmapNodeData();
            FillBaseNodeData(d);
            d.path = Path;
            d.relativePath = relativePath;
            d.resource = Resource;
            d.Write(w);
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

        public override void Dispose()
        {
            base.Dispose();

            if(brush != null)
            {
                brush = null;
            }
        }
    }
}
