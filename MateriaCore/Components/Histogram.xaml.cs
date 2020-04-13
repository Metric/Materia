using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Materia.Rendering.Imaging;
using Materia.Rendering.Mathematics;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MateriaCore.Components
{
    public class Histogram : UserControl
    {
        Image preview;

        CancellationTokenSource ctk;

        LevelMode mode;

        public LevelMode Mode
        {
            get
            {
                return mode;
            }
            set
            {
                mode = value;
                //build image here
            }
        }

        int[,] histograph;
        int[] maxValue;

        public int[,] Histograph
        {
            get
            {
                return histograph;
            }
            set
            {
                histograph = value;
                if (histograph != null)
                {
                    maxValue = MaxHistographValue();
                    Paint();
                }
            }
        }

        public Histogram()
        {
            this.InitializeComponent();
            PropertyChanged += Histogram_PropertyChanged;
        }

        private void Histogram_PropertyChanged(object sender, AvaloniaPropertyChangedEventArgs e)
        {
            if (e.Property.Name == "Bounds")
            {
                Paint();
            }
        }

        public void Create(GLBitmap bmp)
        {
            if (bmp == null) return;

            if (bmp.Width == 0 || bmp.Height == 0) return;

            int[,] hist = new int[3, 256];

            if (ctk != null)
            {
                ctk.Cancel();
            }

            ctk = new CancellationTokenSource();

            Task.Run(() =>
            {
                for (int y = 0; y < bmp.Height; ++y)
                {
                    for (int x = 0; x < bmp.Width; ++x)
                    {
                        GLPixel pix = bmp.GetPixel(x, y);
                        hist[0, pix.r]++;
                        hist[1, pix.g]++;
                        hist[2, pix.b]++;
                    }
                }
            }, ctk.Token)
            .ContinueWith(t =>
            {
                if (t.IsCanceled)
                {
                    return;
                }

                Histograph = hist;
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        void Paint()
        {
            if (Bounds.Width == 0 || Bounds.Height == 0 || histograph == null || maxValue == null) return;

            RawBitmap bmp = new RawBitmap((int)Bounds.Width, (int)Bounds.Height);

            if (ctk != null)
            {
                ctk.Cancel();
            }

            ctk = new CancellationTokenSource();

            Task.Run(() =>
            {
                if (histograph != null && maxValue != null)
                {
                    if (mode == LevelMode.RGB)
                    {
                        for (int g = 0; g < 3; ++g)
                        {
                            if (maxValue[g] != 0)
                            {
                                //gather initial points
                                List<Vector2> points = new List<Vector2>();

                                for (int x = 0; x < 256; ++x)
                                {
                                    int t = histograph[g, x];
                                    float w = (float)x / 255.0f;
                                    int ax = (int)Math.Floor(w * Bounds.Width);

                                    float p = (float)t / (float)maxValue[g];
                                    int may = (int)Math.Min((int)Bounds.Height, Math.Floor(p * (int)Bounds.Height));

                                    points.Add(new Vector2(ax, may));
                                }

                                List<Vector2> spline = Catmull.Spline(points, 8);

                                GLPixel pixel = GLPixel.FromRGBA(255, 255, 255, 220);

                                Parallel.For(0, spline.Count, i =>
                                {
                                    Vector2 p = spline[i];

                                    for (int k = 0; k < p.Y; ++k)
                                    {
                                        bmp.SetPixel((int)p.X, bmp.Height - 1 - k, ref pixel);
                                    }
                                });
                            }
                        }
                    }
                    else
                    {
                        int g = (int)mode;

                        if (maxValue[g] != 0)
                        {
                            List<Vector2> points = new List<Vector2>();

                            //gather initial points
                            for (int x = 0; x < 256; ++x)
                            {
                                int t = histograph[g, x];
                                float w = (float)x / 255.0f;
                                int ax = (int)Math.Floor(w * Bounds.Width);

                                float p = (float)t / (float)maxValue[g];
                                int may = (int)Math.Min(Bounds.Height, Math.Floor(p * Bounds.Height));

                                points.Add(new Vector2(ax, may));
                            }

                            List<Vector2> spline = Catmull.Spline(points, 8);

                            GLPixel pixel = GLPixel.FromRGBA(255, 255, 255, 220);

                            Parallel.For(0, spline.Count, i =>
                            {
                                Vector2 p = spline[i];

                                for (int k = 0; k < p.Y; ++k)
                                {
                                    bmp.SetPixel((int)p.X, bmp.Height - 1 - k, ref pixel);
                                }
                            });
                        }
                    }
                }
            }, ctk.Token)
            .ContinueWith(t =>
            {
                if (t.IsCanceled)
                {
                    return;
                }

                if (bmp == null)
                {
                    return;
                }

                preview.Source = bmp.ToAvBitmap();
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        int[] MaxHistographValue()
        {
            int[] max = new int[3];

            for(int g = 0; g < 3; ++g)
            {
                for (int i = 0; i < 256; ++i)
                {
                    if (histograph[g,i] > max[g])
                    {
                        max[g] = histograph[g, i];
                    }
                }
            }

            return max;
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            preview = this.FindControl<Image>("PreviewView");
        }
    }
}
