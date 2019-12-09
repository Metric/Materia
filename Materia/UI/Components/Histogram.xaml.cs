using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using Materia.Imaging;
using static Materia.UILevels;
using OpenTK;
using Materia.UI.Helpers;
using System.Threading;

namespace Materia.UI.Components
{
    /// <summary>
    /// Interaction logic for Histogram.xaml
    /// </summary>
    public partial class Histogram : UserControl
    {
        CancellationTokenSource ctk;

        LevelMode mode;

        bool isLoaded;

        public LevelMode Mode
        {
            get
            {
                return mode;
            }
            set
            {
                mode = value;
                BuildHistogramImage();
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
                    BuildHistogramImage();
                }
            }
        }

        public Histogram()
        {
            InitializeComponent();
        }

        public void GenerateHistograph(RawBitmap fromBitmap)
        {
            if (fromBitmap == null) return;

            int[,] hist = new int[3, 256];

            if(ctk != null)
            {
                ctk.Cancel();
            }

            ctk = new CancellationTokenSource();

            Task.Run(() =>
            {
                for (int y = 0; y < fromBitmap.Height; ++y)
                {
                    byte r = 0, g = 0, b = 0, a = 0;
                    for (int x = 0; x < fromBitmap.Width; ++x)
                    {
                        fromBitmap.GetPixel(x, y, out r, out g, out b, out a);

                        hist[0, r]++;
                        hist[1, g]++;
                        hist[2, b]++;
                    }
                }

            }, ctk.Token).ContinueWith(t =>
            {
                if (t.IsCanceled) return;

                App.Current.Dispatcher.Invoke(() =>
                {
                    Histograph = hist;
                });
            });
        }

        public void GenerateHistograph(FloatBitmap fromBitmap)
        {
            if (fromBitmap == null) return;
            int[,] hist = new int[3, 256];

            if(ctk != null)
            {
                ctk.Cancel();
            }

            ctk = new CancellationTokenSource();

            Task.Run(() =>
            {
                for (int y = 0; y < fromBitmap.Height; ++y)
                {
                    float r = 0, g = 0, b = 0, a = 0;
                    for (int x = 0; x < fromBitmap.Width; ++x)
                    {
                        fromBitmap.GetPixel(x, y, out r, out g, out b, out a);

                        int rb = (int)Math.Min(255, Math.Max(0, r * 255));
                        int gb = (int)Math.Min(255, Math.Max(0, g * 255));
                        int bb = (int)Math.Min(255, Math.Max(0, b * 255));

                        hist[0, rb]++;
                        hist[1, gb]++;
                        hist[2, bb]++;
                    }
                }
            }, ctk.Token).ContinueWith(t =>
            {
                if (t.IsCanceled) return;

                App.Current.Dispatcher.Invoke(() =>
                {
                    Histograph = hist;
                });
            });
        }

        public void BuildHistogramImage()
        {
            if (!isLoaded) return;

            if (ActualHeight == 0 || ActualWidth == 0) return;

            RawBitmap bmp = new RawBitmap((int)ActualWidth, (int)ActualHeight);

            if(ctk != null)
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
                                    int ax = (int)Math.Floor(w * ActualWidth);

                                    float p = (float)t / (float)maxValue[g];
                                    int may = (int)Math.Min((int)ActualHeight, Math.Floor(p * (int)ActualHeight));

                                    points.Add(new Vector2(ax, may));
                                }

                                List<Vector2> spline = CatmullRomSpline.GetSpline(points, 8);

                                Parallel.For(0, spline.Count, i =>
                                {
                                    Vector2 p = spline[i];

                                    for (int k = 0; k < p.Y; ++k)
                                    {
                                        bmp.SetPixel((int)p.X, bmp.Height - 1 - k, 255, 255, 255, 220);
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
                                int ax = (int)Math.Floor(w * ActualWidth);

                                float p = (float)t / (float)maxValue[g];
                                int may = (int)Math.Min(ActualHeight, Math.Floor(p * ActualHeight));

                                points.Add(new Vector2(ax, may));
                            }

                            List<Vector2> spline = CatmullRomSpline.GetSpline(points, 8);

                            Parallel.For(0, spline.Count, i =>
                            {
                                Vector2 p = spline[i];

                                for (int k = 0; k < p.Y; ++k)
                                {
                                    bmp.SetPixel((int)p.X, bmp.Height - 1 - k, 255, 255, 255, 220);
                                }
                            });
                        }
                    }
                }
            }, ctk.Token).ContinueWith(t =>
            {
                if (t.IsCanceled) return;

                if (bmp == null) return;

                Application.Current.Dispatcher.Invoke(() =>
                {
                    PreviewView.Source = bmp.ToImageSource();
                });
            }); 
        }

        int[] MaxHistographValue()
        {
            int[] max = new int[3];

            for (int g = 0; g < 3; ++g)
            {
                for (int i = 0; i < 256; ++i)
                {
                    if(histograph[g,i] > max[g])
                    {
                        max[g] = histograph[g, i];
                    }
                }
            }

            return max;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            isLoaded = true;
        }

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            BuildHistogramImage();
        }
    }
}
