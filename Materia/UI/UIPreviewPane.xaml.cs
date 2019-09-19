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
using System.Windows.Shapes;
using static Materia.UILevels;
using Materia.Imaging;
using RSMI.Containers;
using Materia.Geometry;
using Materia.Imaging.GLProcessing;
using OpenTK.Graphics.OpenGL;
using Materia.Math3D;
using Materia.Textures;

namespace Materia
{
    /// <summary>
    /// Interaction logic for UIPreviewPane.xaml
    /// </summary>
    public partial class UIPreviewPane : UserControl
    {
        float scale;

        UINode current;
        Point pan;

        System.Drawing.Point start;

        GLTextuer2D blankTexture;

        double vw;
        double vh;

        public static UIPreviewPane Instance { get; protected set; }

        bool showUV;

        UVRenderer uvs;

        OpenTK.GLControl glview;
        PreviewProcessor processor;

        FullScreenQuad quad;

        protected float updateTime = 0;
        protected float lastUpdate = 0;
        protected const float maxUpdateTime = 1.0f / 60.0f;

        public UIPreviewPane()
        {
            InitializeComponent();
            pan = new Point(0, 0);
            Instance = this;
            scale = 1f;
            vw = 512;
            vh = 512;

            glview = new OpenTK.GLControl();

            glview.Load += Glview_Load;
            glview.MouseDown += Glview_MouseDown;
            glview.MouseMove += Glview_MouseMove;
            glview.MouseUp += Glview_MouseUp;
            glview.MouseWheel += Glview_MouseWheel;
            glview.Paint += Glview_Paint;

            FHost.Child = glview;
        }

        private void Glview_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
        {
            if (glview == null) return;

            ViewContext.VerifyContext(glview);
            ViewContext.Context.MakeCurrent(glview.WindowInfo);

            GL.Disable(EnableCap.CullFace);

            GL.Viewport(0, 0, glview.Width, glview.Height);
            GL.ClearColor(0.1f, 0.1f, 0.1f, 1);
            GL.Clear(ClearBufferMask.ColorBufferBit);
            GL.Clear(ClearBufferMask.DepthBufferBit);

            Matrix4 proj = Matrix4.CreateOrthographic(glview.Width, glview.Height, 0.03f, 1000f);
            Matrix4 translation = Matrix4.CreateTranslation((float)pan.X, (float)-pan.Y, 0);
            //half width/height for scale as it is centered based
            Matrix4 sm = Matrix4.CreateScale(scale * (float)(vw * 0.5f), scale * (float)(vh * 0.5f), 1);
            Matrix4 model = sm * translation;
            Matrix4 view = Matrix4.LookAt(new Vector3(0, 0, 1), Vector3.Zero, Vector3.UnitY);

            processor.FlipY = true;
            processor.Model = model;
            processor.View = view;
            processor.Projection = proj;

            if(current != null && current.Node != null && current.Node.GetActiveBuffer() != null && current.Node.GetActiveBuffer().Id != 0)
            {
                processor.Bind(current.Node.GetActiveBuffer());
            }
            else if(blankTexture != null)
            {
                processor.Bind(blankTexture);
            }

            if(quad != null)
            {
                quad.Draw();
            }

            processor.Unbind();

            if(uvs != null && showUV)
            {
                uvs.View = view;
                uvs.Projection = proj;
                uvs.Model = model;
                uvs.Draw();
            }

            GL.Enable(EnableCap.CullFace);

            glview.SwapBuffers();
        }

        private void Glview_MouseWheel(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Delta < 0)
            {
                scale -= 0.02f;

                if (scale <= 0.1)
                {
                    scale = 0.1f;
                }
            }
            else if (e.Delta > 0)
            {
                scale += 0.02f;

                if (scale > 3)
                {
                    scale = 3.0f;
                }
            }

            UpdateZoomText();
            Invalidate();
        }

        private void Glview_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
        {
           
        }

        private void Glview_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            //times 1000 to convert to MS
            //since updateTime cannot possible be a fraction
            //as is a solid int/float of MS
            if (updateTime >= maxUpdateTime * 1000)
            {
                updateTime = 0;
                if (e.Button == System.Windows.Forms.MouseButtons.Middle)
                {
                    glview.Cursor = System.Windows.Forms.Cursors.SizeAll;

                    var p = e.Location;
                    double dx = p.X - start.X;
                    double dy = p.Y - start.Y;

                    start = p;

                    pan.X += dx;
                    pan.Y += dy;

                    Invalidate();
                }
                else
                {
                    glview.Cursor = System.Windows.Forms.Cursors.Arrow;
                }
            }

            updateTime += Environment.TickCount - lastUpdate;
            lastUpdate = Environment.TickCount;
        }

        private void Glview_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Middle)
            {
                start = e.Location;
            }
        }

        private void Glview_Load(object sender, EventArgs e)
        {
            ViewContext.VerifyContext(glview);
            ViewContext.Context.MakeCurrent(glview.WindowInfo);

            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            if (quad == null)
            {
                quad = new FullScreenQuad();
            }

            if (processor == null)
            {
                processor = new PreviewProcessor();
            }

            blankTexture = new GLTextuer2D(GLInterfaces.PixelInternalFormat.Rgb);
            blankTexture.Bind();
            blankTexture.SetData(IntPtr.Zero, GLInterfaces.PixelFormat.Rgb, 16, 16);
            GLTextuer2D.Unbind();

            Invalidate();
        }

        public void Invalidate()
        {
            if(glview != null)
            {
                glview.Invalidate();
            }
        }

        public void SetMesh(MeshRenderer m)
        {
            if(uvs != null)
            {
                uvs.Release();
            }

            uvs = new UVRenderer(m);

            Invalidate();
        }

        //this is to make sure references are cleared
        //and released for memory purposes
        public void TryAndRemovePreviewNode(UINode n)
        {
            if(current == n)
            {
                current.Node.OnUpdate -= Node_OnUpdate;
                current = null;
            }

            Invalidate();
        }

        public void SetPreviewNode(UINode n)
        {
            if(current != null)
            {
                current.Node.OnUpdate -= Node_OnUpdate;
            }

            current = n;
            current.Node.OnUpdate += Node_OnUpdate;

            Node_OnUpdate(n.Node);
        }

        private void Node_OnUpdate(Nodes.Node n)
        {
            /*byte[] src = n.GetPreview(512, 512);

            if (src != null)
            {
                RawBitmap bitmap = new RawBitmap(512, 512, src);
 
                Histogram.GenerateHistograph(bitmap);
            }*/

            vw = n.Width;
            vh = n.Height;

            Invalidate();
        }

        void UpdateZoomText()
        {
            float p = scale * 100;
            ZoomLevel.Text = String.Format("{0:0}", p) + "%";
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //HistMode.SelectedIndex = 0;

            UpdateZoomText();
        }

        private void FitIntoView_Click(object sender, RoutedEventArgs e)
        {
            pan.X = 0;
            pan.Y = 0;

            float minViewArea = (float)Math.Min(glview.Width, glview.Height);

            if(vw >= vh)
            {
                scale = minViewArea / (float)vw;
            }
            else
            {                      
                scale = minViewArea / (float)vh;
            }

            UpdateZoomText();
            Invalidate();
        }

        private void ZoomOut_Click(object sender, RoutedEventArgs e)
        {
            scale -= 0.02f;

            if (scale <= 0.1)
            {
                scale = 0.1f;
            }

            UpdateZoomText();
            Invalidate();
        }

        private void ZoomIn_Click(object sender, RoutedEventArgs e)
        {
            scale += 0.02f;

            if (scale > 3)
            {
                scale = 3.0f;
            }

            UpdateZoomText();
            Invalidate();
        }

        private void Ratio1_Click(object sender, RoutedEventArgs e)
        {
            pan.X = 0;
            pan.Y = 0;

            scale = 1;

            UpdateZoomText();
            Invalidate();
        }

        private void ToggleHistogram_Click(object sender, RoutedEventArgs e)
        {
            /*if (HistogramArea.Visibility != Visibility.Visible)
            {
                HistogramArea.Visibility = Visibility.Visible;
            }
            else
            {
                HistogramArea.Visibility = Visibility.Collapsed;
            }*/
        }

        private void HistMode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            /*ComboBoxItem item = (ComboBoxItem)HistMode.SelectedItem;
            string c = (string)item.Content;
            var mode = (LevelMode)Enum.Parse(typeof(LevelMode), c);

            Histogram.Mode = mode;*/
        }

        private void ToggleUV_Click(object sender, RoutedEventArgs e)
        {
            showUV = !showUV;

            Invalidate();
        }

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            
        }

        public void Release()
        {
            if(blankTexture != null)
            {
                blankTexture.Release();
                blankTexture = null;
            }

            if(quad != null)
            {
                quad.Release();
                quad = null;
            }

            if(glview != null)
            {
                glview.Dispose();
                glview = null;
                FHost.Child = null;
            }
        }
    }
}
