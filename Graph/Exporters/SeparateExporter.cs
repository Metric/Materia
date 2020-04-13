﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Materia.Nodes;
using Materia.Nodes.Atomic;
using Materia.Rendering.Imaging;
using System.IO;

namespace Materia.Graph.Exporters
{
    public class SeparateExporter : Exporter
    {
        Graph graph;
        public SeparateExporter(Graph g)
        {
            graph = g;
        }

        public override void ExportSync(string path)
        {
            string name = graph.Name;

            foreach (var s in graph.OutputNodes)
            {
                Node n = null;

                if (graph.NodeLookup.TryGetValue(s, out n))
                {
                    if (n is OutputNode)
                    {
                        OutputNode on = n as OutputNode;

                        if (on.OutType == OutputType.basecolor)
                        {
                            RawBitmap bmp = null;
                            byte[] bits = on.GetPreview(on.Width, on.Height);

                            if (bits != null)
                            {
                                bmp = new RawBitmap(on.Width, on.Height, bits);
                                var src = bmp.ToBitmap();

                                using (FileStream fs = new FileStream(System.IO.Path.Combine(path, name + "_color.png"), FileMode.OpenOrCreate))
                                {
                                    src.Save(fs, System.Drawing.Imaging.ImageFormat.Png);
                                }

                                src.Dispose();
                            }
                        }
                        else if (on.OutType == OutputType.normal)
                        {
                            RawBitmap bmp = null;
                            byte[] bits = on.GetPreview(on.Width, on.Height);

                            if (bits != null)
                            {
                                bmp = new RawBitmap(on.Width, on.Height, bits);
                                var src = bmp.ToBitmap();

                                using (FileStream fs = new FileStream(System.IO.Path.Combine(path, name + "_normal.png"), FileMode.OpenOrCreate))
                                {
                                    src.Save(fs, System.Drawing.Imaging.ImageFormat.Png);
                                }

                                src.Dispose();
                            }
                        }
                        else if (on.OutType == OutputType.metallic)
                        {
                            RawBitmap bmp = null;
                            byte[] bits = on.GetPreview(on.Width, on.Height);

                            if (bits != null)
                            {
                                bmp = new RawBitmap(on.Width, on.Height, bits);
                                var src = bmp.ToBitmap();

                                using (FileStream fs = new FileStream(System.IO.Path.Combine(path, name + "_metallic.png"), FileMode.OpenOrCreate))
                                {
                                    src.Save(fs, System.Drawing.Imaging.ImageFormat.Png);
                                }

                                src.Dispose();
                            }
                        }
                        else if (on.OutType == OutputType.roughness)
                        {
                            RawBitmap bmp = null;
                            byte[] bits = on.GetPreview(on.Width, on.Height);

                            if (bits != null)
                            {
                                bmp = new RawBitmap(on.Width, on.Height, bits);
                                var src = bmp.ToBitmap();

                                using (FileStream fs = new FileStream(System.IO.Path.Combine(path, name + "_roughness.png"), FileMode.OpenOrCreate))
                                {
                                    src.Save(fs, System.Drawing.Imaging.ImageFormat.Png);
                                }

                                src.Dispose();
                            }
                        }
                        else if (on.OutType == OutputType.occlusion)
                        {
                            RawBitmap bmp = null;
                            byte[] bits = on.GetPreview(on.Width, on.Height);

                            if (bits != null)
                            {
                                bmp = new RawBitmap(on.Width, on.Height, bits);
                                var src = bmp.ToBitmap();

                                using (FileStream fs = new FileStream(System.IO.Path.Combine(path, name + "_occlusion.png"), FileMode.OpenOrCreate))
                                {
                                    src.Save(fs, System.Drawing.Imaging.ImageFormat.Png);
                                }

                                src.Dispose();
                            }
                        }
                        else if (on.OutType == OutputType.height)
                        {
                            RawBitmap bmp = null;
                            byte[] bits = on.GetPreview(on.Width, on.Height);

                            if (bits != null)
                            {
                                bmp = new RawBitmap(on.Width, on.Height, bits);
                                var src = bmp.ToBitmap();

                                using (FileStream fs = new FileStream(System.IO.Path.Combine(path, name + "_height.png"), FileMode.OpenOrCreate))
                                {
                                    src.Save(fs, System.Drawing.Imaging.ImageFormat.Png);
                                }

                                src.Dispose();
                            }
                        }
                        else if (on.OutType == OutputType.thickness)
                        {
                            RawBitmap bmp = null;
                            byte[] bits = on.GetPreview(on.Width, on.Height);

                            if (bits != null)
                            {
                                bmp = new RawBitmap(on.Width, on.Height, bits);
                                var src = bmp.ToBitmap();

                                using (FileStream fs = new FileStream(System.IO.Path.Combine(path, name + "_thickness.png"), FileMode.OpenOrCreate))
                                {
                                    src.Save(fs, System.Drawing.Imaging.ImageFormat.Png);
                                }

                                src.Dispose();
                            }
                        }
                        else if (on.OutType == OutputType.emission)
                        {
                            RawBitmap bmp = null;
                            byte[] bits = on.GetPreview(on.Width, on.Height);

                            if (bits != null)
                            {
                                bmp = new RawBitmap(on.Width, on.Height, bits);
                                var src = bmp.ToBitmap();

                                using (FileStream fs = new FileStream(System.IO.Path.Combine(path, name + "_emission.png"), FileMode.OpenOrCreate))
                                {
                                    src.Save(fs, System.Drawing.Imaging.ImageFormat.Png);
                                }

                                src.Dispose();
                            }
                        }
                    }
                }
            }
        }

        public override Task Export(string path)
        {
            int i = 0;

            string name = graph.Name;

            Queue<Task> runningTasks = new Queue<Task>();

            foreach (var s in graph.OutputNodes)
            {
                Node n = null;

                if (graph.NodeLookup.TryGetValue(s, out n))
                {
                    if (n is OutputNode)
                    {
                        OutputNode on = n as OutputNode;

                        if (on.OutType == OutputType.basecolor)
                        {
                            RawBitmap bmp = null;
                            byte[] bits = on.GetPreview(on.Width, on.Height);

                            if (bits != null)
                            {
                                var t = Task.Run(() =>
                                {
                                    bmp = new RawBitmap(on.Width, on.Height, bits);
                                    var src = bmp.ToBitmap();

                                    using (FileStream fs = new FileStream(System.IO.Path.Combine(path, name + "_color.png"), FileMode.OpenOrCreate))
                                    {
                                        src.Save(fs, System.Drawing.Imaging.ImageFormat.Png);
                                    }

                                    src.Dispose();
                                });
                                runningTasks.Enqueue(t);
                            }
                        }
                        else if (on.OutType == OutputType.normal)
                        {
                            RawBitmap bmp = null;
                            byte[] bits = on.GetPreview(on.Width, on.Height);

                            if (bits != null)
                            {
                                var t = Task.Run(() =>
                                {
                                    bmp = new RawBitmap(on.Width, on.Height, bits);
                                    var src = bmp.ToBitmap();

                                    using (FileStream fs = new FileStream(System.IO.Path.Combine(path, name + "_normal.png"), FileMode.OpenOrCreate))
                                    {
                                        src.Save(fs, System.Drawing.Imaging.ImageFormat.Png);
                                    }

                                    src.Dispose();
                                });
                                runningTasks.Enqueue(t);
                            }
                        }
                        else if (on.OutType == OutputType.metallic)
                        {
                            RawBitmap bmp = null;
                            byte[] bits = on.GetPreview(on.Width, on.Height);

                            if (bits != null)
                            {
                                var t = Task.Run(() =>
                                {
                                    bmp = new RawBitmap(on.Width, on.Height, bits);
                                    var src = bmp.ToBitmap();

                                    using (FileStream fs = new FileStream(System.IO.Path.Combine(path, name + "_metallic.png"), FileMode.OpenOrCreate))
                                    {
                                        src.Save(fs, System.Drawing.Imaging.ImageFormat.Png);
                                    }

                                    src.Dispose();
                                });
                                runningTasks.Enqueue(t);
                            }
                        }
                        else if (on.OutType == OutputType.roughness)
                        {
                            RawBitmap bmp = null;
                            byte[] bits = on.GetPreview(on.Width, on.Height);

                            if (bits != null)
                            {
                                var t = Task.Run(() =>
                                {
                                    bmp = new RawBitmap(on.Width, on.Height, bits);
                                    var src = bmp.ToBitmap();

                                    using (FileStream fs = new FileStream(System.IO.Path.Combine(path, name + "_roughness.png"), FileMode.OpenOrCreate))
                                    {
                                        src.Save(fs, System.Drawing.Imaging.ImageFormat.Png);
                                    }

                                    src.Dispose();
                                });
                                runningTasks.Enqueue(t);
                            }
                        }
                        else if (on.OutType == OutputType.occlusion)
                        {
                            RawBitmap bmp = null;
                            byte[] bits = on.GetPreview(on.Width, on.Height);

                            if (bits != null)
                            {
                                var t = Task.Run(() =>
                                {
                                    bmp = new RawBitmap(on.Width, on.Height, bits);
                                    var src = bmp.ToBitmap();

                                    using (FileStream fs = new FileStream(System.IO.Path.Combine(path, name + "_occlusion.png"), FileMode.OpenOrCreate))
                                    {
                                        src.Save(fs, System.Drawing.Imaging.ImageFormat.Png);
                                    }

                                    src.Dispose();
                                });
                                runningTasks.Enqueue(t);
                            }
                        }
                        else if (on.OutType == OutputType.height)
                        {
                            RawBitmap bmp = null;
                            byte[] bits = on.GetPreview(on.Width, on.Height);

                            if (bits != null)
                            {
                                var t = Task.Run(() =>
                                {
                                    bmp = new RawBitmap(on.Width, on.Height, bits);
                                    var src = bmp.ToBitmap();

                                    using (FileStream fs = new FileStream(System.IO.Path.Combine(path, name + "_height.png"), FileMode.OpenOrCreate))
                                    {
                                        src.Save(fs, System.Drawing.Imaging.ImageFormat.Png);
                                    }

                                    src.Dispose();
                                });
                                runningTasks.Enqueue(t);
                            }
                        }
                        else if (on.OutType == OutputType.thickness)
                        {
                            RawBitmap bmp = null;
                            byte[] bits = on.GetPreview(on.Width, on.Height);

                            if (bits != null)
                            {
                                var t = Task.Run(() =>
                                {
                                    bmp = new RawBitmap(on.Width, on.Height, bits);
                                    var src = bmp.ToBitmap();

                                    using (FileStream fs = new FileStream(System.IO.Path.Combine(path, name + "_thickness.png"), FileMode.OpenOrCreate))
                                    {
                                        src.Save(fs, System.Drawing.Imaging.ImageFormat.Png);
                                    }

                                    src.Dispose();
                                });
                                runningTasks.Enqueue(t);
                            }
                        }
                    }
                }
            }

            int totalTasks = runningTasks.Count;

            ProgressChanged(0, totalTasks, 0);

            return Task.Run(async () =>
            {
                while (runningTasks.Count > 0)
                {
                    i = totalTasks - runningTasks.Count + 1;

                    Task t = runningTasks.Dequeue();

                    ProgressChanged(i, totalTasks, (float)i / (float)totalTasks);

                    if (!t.IsCompleted && !t.IsCanceled)
                    {
                        await t;
                    }
                }
            });
        }
    }
}
