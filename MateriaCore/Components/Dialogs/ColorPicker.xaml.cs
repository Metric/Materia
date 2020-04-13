using Assimp;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Materia.Rendering.Imaging;
using Materia.Rendering.Mathematics;
using System;
using D = System.Drawing;
using System.Reflection;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using MateriaCore.Utils;

namespace MateriaCore.Components.Dialogs
{
    public class ColorPicker : Window
    {
        bool satMouseDown;
        bool hueMouseDown;

        RawBitmap hueBitmap;
        RawBitmap svBitmap;

        NumberSlider hueSlider;
        NumberSlider saturationSlider;
        NumberSlider valueSlider;

        NumberSlider redSlider;
        NumberSlider greenSlider;
        NumberSlider blueSlider;
        NumberSlider alphaSlider;

        Button selectButton;
        Button cancelButton;
        Button dropperButton;

        Grid selectedColor;
        Grid previousColor;

        Grid svPoint;
        Rectangle hPoint;

        Image hueImage;
        Image svImage;

        ScreenPixelGrabber pixelGrabber;

        protected Color current;
        protected MVector currentVector;

        protected Color original;

        protected HsvColor hsv;

        protected float red;
        public float Red
        {
            get
            {
                return red;
            }
            set
            {
                red = value;
                RebuildRGB();
                RedrawSatVal();
            }
        }

        protected float green;
        public float Green
        {
            get
            {
                return green;
            }
            set
            {
                green = value;
                RebuildRGB();
                RedrawSatVal();
            }
        }

        protected float blue;
        public float Blue
        {
            get
            {
                return blue;
            }
            set
            {
                blue = value;
                RebuildRGB();
                RedrawSatVal();
            }
        }

        protected float alpha;
        public float Alpha
        {
            get
            {
                return alpha;
            }
            set
            {
                alpha = value;
                RebuildRGB();
            }
        }

        protected float hue;
        public float Hue
        {
            get
            {
                return hue;
            }
            set
            {
                hue = value;
                RebuildHsv();
                RedrawSatVal();
            }
        }

        protected float sat;
        public float Saturation
        {
            get
            {
                return sat;
            }
            set
            {
                sat = value;
                RebuildHsv();
            }
        }

        protected float value;
        public float Value
        {
            get
            {
                return value;
            }
            set
            {
                this.value = value;
                RebuildHsv();
            }
        }

        public Color Selected
        {
            get
            {
                return current;
            }
        }

        public MVector SelectedVector
        {
            get
            {
                return currentVector;
            }
        }

        public ColorPicker()
        {
            this.InitializeComponent();
            current = original = new Color(255, 0, 0, 0);
            currentVector = new MVector(0, 0, 0, 1);
            hsv = HsvColor.FromMVector(ref currentVector);
            InitBitmaps();
            InitEvents();
            InitSliders();
#if DEBUG
            this.AttachDevTools();
#endif
        }

        public ColorPicker(MVector p)
        {
            this.InitializeComponent();
            current = original = new Color((byte)(p.W * 255), (byte)(p.X * 255), (byte)(p.Y * 255), (byte)(p.Z * 255));
            currentVector = p.Clamp(MVector.Zero, MVector.One);
            hsv = HsvColor.FromMVector(ref currentVector);
            InitBitmaps();
            InitEvents();
            InitSliders();
        }

        public ColorPicker(Color p)
        {
            this.InitializeComponent();
            current = original = p;
            currentVector = new MVector(p.R / 255.0f, p.G / 255.0f, p.B / 255.0f, p.A / 255.0f).Clamp(MVector.Zero, MVector.One);
            hsv = HsvColor.FromMVector(ref currentVector);
            InitBitmaps();
            InitEvents();
            InitSliders();
        }

        void InitSliders()
        {
            previousColor.Background = new SolidColorBrush(current);
            selectedColor.Background = new SolidColorBrush(current);

            red = currentVector.X;
            green = currentVector.Y;
            blue = currentVector.Z;
            alpha = currentVector.W;

            hue = MathF.Min(360, MathF.Max(0, hsv.H / 359.0f));
            sat = hsv.S;
            value = hsv.V;

            //get property infos
            //these are primarily used to update the ui
            //when the sliders or number input changes
            //it is not meant to update the sliders with these values
            PropertyInfo rInfo = GetType().GetProperty("Red");
            PropertyInfo gInfo = GetType().GetProperty("Green");
            PropertyInfo bInfo = GetType().GetProperty("Blue");
            PropertyInfo aInfo = GetType().GetProperty("Alpha");
            PropertyInfo hInfo = GetType().GetProperty("Hue");
            PropertyInfo sInfo = GetType().GetProperty("Saturation");
            PropertyInfo vInfo = GetType().GetProperty("Value");

            hueSlider?.Set(0, 1, hInfo, this);
            saturationSlider?.Set(0, 1, sInfo, this);
            valueSlider?.Set(0, 1, vInfo, this);
            redSlider?.Set(0, 1, rInfo, this);
            greenSlider?.Set(0, 1, gInfo, this);
            blueSlider?.Set(0, 1, bInfo, this);
            alphaSlider?.Set(0, 1, aInfo, this);
        }

        void InitBitmaps()
        {
            svBitmap = new RawBitmap((int)svImage.Width, (int)svImage.Height);
            hueBitmap = new RawBitmap((int)hueImage.Width, (int)hueImage.Height);

            RedrawHue();
            RedrawSatVal();
            UpdatePreview();
        }

        void InitEvents()
        {
            selectButton.Click += SelectButton_Click;
            cancelButton.Click += CancelButton_Click;
            dropperButton.Click += DropperButton_Click;
            hueImage.PointerPressed += OnHuePressed;
            svImage.PointerPressed += OnSatValPressed;
            Opened += ColorPicker_Opened;
            Closing += ColorPicker_Closing;
        }

        private void ColorPicker_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (hueMouseDown)
            {
                hueMouseDown = false;
                UnsubscribeFromWindowHuePointer();
            }
            if (satMouseDown)
            {
                satMouseDown = false;
                UnsubscribeFromWindowSatValPointer();
            }

            pixelGrabber?.Dispose();
        }

        private void ColorPicker_Opened(object sender, EventArgs e)
        {
            UpdateSliders();
        }

        /// <summary>
        /// Picking is only supported on windows for right now
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DropperButton_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                if (pixelGrabber == null || !pixelGrabber.IsGrabbing)
                {
                    if (pixelGrabber == null)
                    {
                        pixelGrabber = new ScreenPixelGrabber();
                        pixelGrabber.OnGrabbed += PixelGrabber_OnGrabbed;
                    }

                    pixelGrabber.Start();
                }
            }
        }

        private void PixelGrabber_OnGrabbed(ref D.Color c)
        {
            current = new Color((byte)(currentVector.W * 255), c.R, c.G, c.B);
            currentVector = new MVector(c.R / 255.0f, c.G / 255.0f, c.B / 255.0f, currentVector.W);
            hsv = HsvColor.FromMVector(ref currentVector);

            UpdatePreview();
            UpdateSliders();
        }

        private void CancelButton_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            Close(false);
        }

        private void SelectButton_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            Close(true);
        }

        protected void RebuildHsv()
        {
            hsv.H = MathF.Min(360, MathF.Max(0, hue * 359));
            hsv.S = MathF.Min(1, MathF.Max(0, sat));
            hsv.V = MathF.Min(1, MathF.Max(0, value));

            currentVector = hsv.ToMVector(currentVector.W);
            current = new Color((byte)(currentVector.W * 255), (byte)(currentVector.X * 255), (byte)(currentVector.Y * 255), (byte)(currentVector.Z * 255));

            UpdatePreview();
        }

        protected void RebuildRGB()
        {
            currentVector.X = red;
            currentVector.Y = green;
            currentVector.Z = blue;
            currentVector.W = alpha;
            currentVector = currentVector.Clamp(MVector.Zero, MVector.One);

            current = new Color((byte)(currentVector.W * 255), (byte)(currentVector.X * 255), (byte)(currentVector.Y * 255), (byte)(currentVector.Z * 255));
            hsv = HsvColor.FromMVector(ref currentVector);

            UpdatePreview();
        }

        protected void UpdateSliders()
        {
            hue = hsv.H / 359.0f;
            sat = hsv.S;
            value = hsv.V;

            red = currentVector.X;
            green = currentVector.Y;
            blue = currentVector.Z;

            alpha = currentVector.W;

            hueSlider.UpdateValue(hue);
            saturationSlider.UpdateValue(sat);
            valueSlider.UpdateValue(value);

            redSlider.UpdateValue(red);
            greenSlider.UpdateValue(green);
            blueSlider.UpdateValue(blue);

            alphaSlider.UpdateValue(alpha);
        }

        protected void UpdatePreview()
        {
            if (selectedColor.Background is SolidColorBrush)
            {
                SolidColorBrush b = selectedColor.Background as SolidColorBrush;
                b.Color = current;
            }
            else
            {
                selectedColor.Background = new SolidColorBrush(current);
            }

            UpdateHuePoint();
            UpdateSVPoint();
        }

        protected void UpdateHuePoint()
        {
            float f = hsv.H / 359.0f;
            double y = Math.Min(hueImage.Height, Math.Max(0, hueImage.Height * f));
            Canvas.SetTop(hPoint, y);
        }

        protected void UpdateSVPoint()
        {
            float sf = MathF.Min(1, MathF.Max(0, hsv.S));
            float sv = MathF.Min(1, MathF.Max(0, 1.0f - hsv.V));

            double x = (svImage.Width * sf) - 5;
            double y = (svImage.Height * sv) - 5;

            Canvas.SetLeft(svPoint, x);
            Canvas.SetTop(svPoint, y);
        }

        protected void RedrawHue()
        {
            Parallel.For(0, hueBitmap.Height, y =>
            {
                float f = y / (float)hueBitmap.Height;
                float h = f * 359.0f;
                HsvColor v = new HsvColor(h, 1, 1);
                GLPixel pix = v.ToGLPixel(1);
                for(int x = 0; x < hueBitmap.Width; ++x)
                {
                    hueBitmap.SetPixel(x, y, ref pix);
                }
            });

            hueImage.Source = hueBitmap.ToAvBitmap();
        }

        protected void RedrawSatVal()
        {
            Parallel.For(0, svBitmap.Height, y =>
            {
                for(int x = 0; x < svBitmap.Width; ++x)
                {
                    float sf = x / (float)svBitmap.Width;
                    float sv = 1.0f - y / (float)svBitmap.Height;

                    HsvColor v = new HsvColor(hsv.H, sf, sv);
                    GLPixel c = v.ToGLPixel(1);
                    svBitmap.SetPixel(x, y, ref c);
                }
            });

            svImage.Source = svBitmap.ToAvBitmap();
        }

        protected void OnSatValMoved(object sender, PointerEventArgs e)
        {
            if (satMouseDown)
            {
                Point p = e.GetPosition(svImage);
                PickSatVal(ref p);
            }
        }

        protected void OnSatValReleased(object sender, PointerReleasedEventArgs e)
        {
            if (satMouseDown)
            {
                UnsubscribeFromWindowSatValPointer();
            }
            satMouseDown = false;
        }

        protected void OnSatValPressed(object sender, PointerPressedEventArgs e)
        {
            satMouseDown = true;
            Point p = e.GetPosition(svImage);
            PickSatVal(ref p);
            SubscribeToWindowSatValPointer();
        }

        protected void OnHueMoved(object sender, PointerEventArgs e)
        {
            if (hueMouseDown)
            {
                Point p = e.GetPosition(hueImage);
                PickHue(ref p);
            }
        }

        protected void OnHueReleased(object sender, PointerReleasedEventArgs e)
        {
            if (hueMouseDown)
            {
                UnsubscribeFromWindowHuePointer();
            }
            hueMouseDown = false;
        }

        protected void OnHuePressed(object sender, PointerPressedEventArgs e)
        {
            hueMouseDown = true;
            Point p = e.GetPosition(hueImage);
            PickHue(ref p);
            SubscribeToWindowHuePointer();
        }

        protected void PickHue(ref Point p)
        {
            float y = (float)p.Y / hueBitmap.Height * 359.0f;
            hsv.H = MathF.Min(360, MathF.Max(0, y));
            currentVector = hsv.ToMVector(currentVector.W);
            current = new Color((byte)(currentVector.W * 255), (byte)(currentVector.X * 255), (byte)(currentVector.Y * 255), (byte)(currentVector.Z * 255));
            UpdatePreview();
            UpdateSliders();
            RedrawSatVal();
        }

        protected void PickSatVal(ref Point p)
        {
            float s = (float)p.X / (float)svBitmap.Width;
            float v = 1.0f - (float)p.Y / (float)svBitmap.Height;

            hsv.S = MathF.Min(1, MathF.Max(s, 0));
            hsv.V = MathF.Min(1, MathF.Max(v, 0));

            currentVector = hsv.ToMVector(currentVector.W);
            current = new Color((byte)(currentVector.W * 255), (byte)(currentVector.X * 255), (byte)(currentVector.Y * 255), (byte)(currentVector.Z * 255));
            UpdatePreview();
            UpdateSliders();
        }

        private void SubscribeToWindowHuePointer()
        {
            PointerMoved += OnHueMoved;
            PointerReleased += OnHueReleased;
        }

        private void UnsubscribeFromWindowHuePointer()
        {
            PointerMoved -= OnHueMoved;
            PointerReleased -= OnHueReleased;
        }

        private void SubscribeToWindowSatValPointer()
        {
            PointerMoved += OnSatValMoved;
            PointerReleased += OnSatValReleased;
        }

        private void UnsubscribeFromWindowSatValPointer()
        {
            PointerMoved -= OnSatValMoved;
            PointerReleased -= OnSatValReleased;
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            hueSlider = this.FindControl<NumberSlider>("HueSlider");
            saturationSlider = this.FindControl<NumberSlider>("SaturationSlider");
            valueSlider = this.FindControl<NumberSlider>("ValueSlider");
            redSlider = this.FindControl<NumberSlider>("RedSlider");
            greenSlider = this.FindControl<NumberSlider>("GreenSlider");
            blueSlider = this.FindControl<NumberSlider>("BlueSlider");
            alphaSlider = this.FindControl<NumberSlider>("AlphaSlider");

            selectedColor = this.FindControl<Grid>("SelectedColor");
            previousColor = this.FindControl<Grid>("PreviousColor");

            hPoint = this.FindControl<Rectangle>("HPoint");
            svPoint = this.FindControl<Grid>("SVPoint");

            hueImage = this.FindControl<Image>("HueSelector");
            svImage = this.FindControl<Image>("SaturationValueSelector");

            selectButton = this.FindControl<Button>("SelectButton");
            cancelButton = this.FindControl<Button>("CancelButton");

            dropperButton = this.FindControl<Button>("Dropper");
        }
    }
}
