using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
using System.Windows.Shapes;
using Materia.MathHelpers;
using Materia.Nodes.Containers;
using Materia.UI.Helpers;
using Materia.Imaging;
using Materia.Nodes.Helpers;
using NLog;

namespace Materia.UI.Components
{
    /// <summary>
    /// Interaction logic for GradientEditor.xaml
    /// </summary>
    public partial class GradientEditor : UserControl
    {
        private static ILogger Log = LogManager.GetCurrentClassLogger();

        const double HANDLE_HALF_WIDTH = 8;

        List<GradientHandle> handles;

        PropertyInfo property;
        object propertyOwner;
        FloatBitmap fbmp;

        bool mouseDown;
        GradientHandle selected;
        System.Windows.Point startPos;

        public GradientEditor()
        {
            InitializeComponent();
            MouseMove += GradientEditor_MouseMove;
            MouseUp += GradientEditor_MouseUp;
            MouseLeave += GradientEditor_MouseLeave;
            handles = new List<GradientHandle>();
        }

        public GradientEditor(PropertyInfo prop, object owner)
        {
            InitializeComponent();
            handles = new List<GradientHandle>();
            MouseMove += GradientEditor_MouseMove;
            MouseUp += GradientEditor_MouseUp;
            MouseLeave += GradientEditor_MouseLeave;
            property = prop;
            propertyOwner = owner;
        }

        private void GradientEditor_MouseLeave(object sender, MouseEventArgs e)
        {
            selected = null;
            mouseDown = false;
        }

        private void GradientEditor_MouseUp(object sender, MouseButtonEventArgs e)
        {
            selected = null;
            mouseDown = false;
        }

        private void GradientEditor_MouseMove(object sender, MouseEventArgs e)
        {
            if(mouseDown && selected != null)
            {
                System.Windows.Point p = e.GetPosition(HandleHolder);
                double delta = p.X - startPos.X;
                double pos = UpdateHandleUIPosition(selected, delta);
                UpdateHandlePosition(selected, (float)Math.Min(1, Math.Max(0, (pos / (HandleHolder.ActualWidth - HANDLE_HALF_WIDTH)))));
                startPos = p;
            }
        }

        void InitHandles()
        {
            try
            {
                fbmp = new FloatBitmap((int)GradientImage.ActualWidth, (int)GradientImage.ActualHeight);

                HandleHolder.Children.Clear();
                handles = new List<GradientHandle>();

                object obj = property.GetValue(propertyOwner);

                if (obj is Gradient)
                {
                    Gradient g = (Gradient)obj;


                    if (g != null && g.positions != null && g.colors != null && g.positions.Length == g.colors.Length)
                    {
                        for (int i = 0; i < g.positions.Length; i++)
                        {
                            AddHandle(new System.Windows.Point(g.positions[i] * HandleHolder.ActualWidth - HANDLE_HALF_WIDTH, 0), g.colors[i]);
                        }
                    }
                    else
                    {
                        AddHandle(new System.Windows.Point(-HANDLE_HALF_WIDTH, 0), new MVector(0, 0, 0, 1));
                        AddHandle(new System.Windows.Point(HandleHolder.ActualWidth - HANDLE_HALF_WIDTH, 0), new MVector(1, 1, 1, 1));
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }

        void AddHandle(System.Windows.Point p, MVector c, bool updateProperty = false)
        {
            GradientHandle h = new GradientHandle();
            h.HorizontalAlignment = HorizontalAlignment.Left;
            h.VerticalAlignment = VerticalAlignment.Top;
            h.Position = (float)Math.Min(1, Math.Max(0, ((p.X + HANDLE_HALF_WIDTH) / HandleHolder.ActualWidth)));
            h.SetColor(c);
            h.MouseDown += H_MouseDown;
            h.OnColorChanged += H_OnColorChanged;
            handles.Add(h);
            HandleHolder.Children.Add(h);
            Canvas.SetLeft(h, h.Position * HandleHolder.ActualWidth - HANDLE_HALF_WIDTH);

            UpdateGradientPreview(updateProperty);
        }

        private void H_OnColorChanged()
        {
            UpdateGradientPreview(true);
        }


        private void H_MouseDown(object sender, MouseButtonEventArgs e)
        {
            GradientHandle h = (GradientHandle)sender;
            if(e.LeftButton == MouseButtonState.Pressed && (Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt)))
            {
                if (handles.Count > 2)
                {
                    HandleHolder.Children.Remove(h);
                    handles.Remove(h);
                    UpdateGradientPreview(true);
                }
            }
            else if(e.LeftButton == MouseButtonState.Pressed)
            {
                startPos = e.GetPosition(HandleHolder);
                selected = h;
                mouseDown = true;
            }
        }

        void UpdateGradientPreview(bool updateProperty = false)
        {
            if(handles.Count >= 2)
            {
                if(fbmp == null || fbmp.Width == 0 || fbmp.Height == 0)
                {
                    fbmp = new FloatBitmap((int)ImageWrapper.ActualWidth, (int)ImageWrapper.ActualHeight);
                }

                if (fbmp.Width > 0 && fbmp.Height > 0)
                {
                    handles.Sort((a, b) =>
                    {
                        if (a.Position < b.Position)
                        {
                            return -1;
                        }
                        else if (a.Position > b.Position)
                        {
                            return 1;
                        }

                        return 0;
                    });

                    try
                    {
                        float[] pos = new float[handles.Count];
                        MVector[] cols = new MVector[handles.Count];

                        for (int i = 0; i < handles.Count; i++)
                        {
                            pos[i] = handles[i].Position;
                            cols[i] = handles[i].SColor;
                        }

                        Utils.CreateGradient(fbmp, pos, cols);

                        if(updateProperty)
                        {
                            Gradient g = new Gradient();
                            g.colors = cols;
                            g.positions = pos;

                            if(propertyOwner != null && property != null)
                            {
                                property.SetValue(propertyOwner, g);
                            }
                        }

                        GradientImage.Source = fbmp.ToImageSource();
                    }
                    catch (Exception e)
                    {
                        Log.Error(e);
                    }
                }
            }
        }

        double UpdateHandleUIPosition(GradientHandle h, double dx)
        {
            double c = Canvas.GetLeft(h);
            c += dx;
            c = Math.Min(HandleHolder.ActualWidth - HANDLE_HALF_WIDTH, Math.Max(-HANDLE_HALF_WIDTH, c));
            Canvas.SetLeft(h, c);
            return c;
        } 

        void UpdateHandlePosition(GradientHandle h, float p)
        {
            h.Position = p;
            UpdateGradientPreview(true);
        }

        void UpdateHandlesOnResize()
        {
            foreach(var h in handles)
            {
                Canvas.SetLeft(h, h.Position * HandleHolder.ActualWidth - HANDLE_HALF_WIDTH);
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            InitHandles();
        }

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if(IsLoaded)
            {
                if (ImageWrapper.ActualWidth > 0 && ImageWrapper.ActualHeight > 0)
                {
                    fbmp = new FloatBitmap((int)ImageWrapper.ActualWidth, (int)ImageWrapper.ActualHeight);
                    UpdateGradientPreview();
                }

                UpdateHandlesOnResize();
            }
        }

        private void UserControl_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount > 1)
            {
                System.Windows.Point p = e.GetPosition(HandleHolder);

                GradientHandle handle = null;
                double min = double.PositiveInfinity;
                for(int i = 0; i < handles.Count; i++)
                {
                    double x = Canvas.GetLeft(handles[i]);
                    double dist = Math.Abs(p.X - x);

                    if(dist < min)
                    {
                        handle = handles[i];
                        min = dist;
                    }
                }

                p.X -= HANDLE_HALF_WIDTH;

                if (handle == null)
                {
                    AddHandle(p, new MVector(0, 0, 0, 1), true);
                }
                else
                {
                    AddHandle(p, handle.SColor, true);
                }
            }
        }
    }
}
