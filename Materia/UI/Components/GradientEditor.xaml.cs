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

namespace Materia.UI.Components
{
    /// <summary>
    /// Interaction logic for GradientEditor.xaml
    /// </summary>
    public partial class GradientEditor : UserControl
    {
        List<GradientHandle> handles;

        PropertyInfo property;
        object propertyOwner;
        FloatBitmap fbmp;

        public GradientEditor()
        {
            InitializeComponent();
            handles = new List<GradientHandle>();
        }

        public GradientEditor(PropertyInfo prop, object owner)
        {
            InitializeComponent();
            property = prop;
            propertyOwner = owner;
        }

        bool mouseDown;
        GradientHandle selected;
        Point startPos;

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
                            AddHandle(new Point(g.positions[i] * (HandleHolder.ActualWidth - 4), 0), g.colors[i]);
                        }
                    }
                    else
                    {
                        AddHandle(new Point(0, 0), new MVector(0, 0, 0, 1));
                        AddHandle(new Point(HandleHolder.Width - 4, 0), new MVector(1, 1, 1, 1));
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message + " | " + e.StackTrace);
            }
        }

        void AddHandle(Point p, MVector c, bool updateProperty = false)
        {
            GradientHandle h = new GradientHandle();
            h.Position = (float)Math.Min(1, Math.Max(0, (p.X / (HandleHolder.ActualWidth - 4))));
            h.SetColor(c);
            h.MouseDown += H_MouseDown;
            h.OnColorChanged += H_OnColorChanged;
            handles.Add(h);
            HandleHolder.Children.Add(h);
            Canvas.SetLeft(h, h.Position * (HandleHolder.ActualWidth - 4));

            UpdateGradientPreview(updateProperty);
        }

        private void H_OnColorChanged()
        {
            UpdateGradientPreview(true);
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            base.OnMouseLeave(e);

            if (selected != null)
            {
                Canvas.SetZIndex(selected, 0);
                selected = null;
            }

            mouseDown = false;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            GradientHandle h = selected;

            if (mouseDown && h != null)
            {
                Canvas.SetZIndex(h, 1);
                Point p = e.GetPosition(HandleHolder);
                double xdiff = p.X - startPos.X;
                float npos = (float)Math.Min(1, Math.Max(0, h.Position + (xdiff / (HandleHolder.ActualWidth - 4))));
                startPos = p;
                UpdateHandlePosition(h, npos);
                UpdateGradientPreview(true);
            }
        }

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            base.OnMouseUp(e);

            if(selected != null)
            {
                Canvas.SetZIndex(selected, 0);
                selected = null;
            }

            mouseDown = false;
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

                    }
                }
            }
        }

        void UpdateHandlePosition(GradientHandle h, float p)
        {
            h.Position = p;
            Canvas.SetLeft(h, h.Position * (HandleHolder.ActualWidth - 4));
        }

        void UpdateHandlesOnResize()
        {
            foreach(var h in handles)
            {
                Canvas.SetLeft(h, h.Position * (HandleHolder.ActualWidth - 4));
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
                Point p = e.GetPosition(HandleHolder);
                AddHandle(p, new MVector(0, 0, 0, 1), true);
            }
        }
    }
}
