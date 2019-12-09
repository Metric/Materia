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
using System.Text.RegularExpressions;
using D = System.Drawing;
using Materia.Imaging;
using Materia.WinApi;
using Materia.UI.Helpers;
using System.Runtime.InteropServices;
using System.Windows.Threading;

namespace Materia.UI.Components
{
    /// <summary>
    /// Interaction logic for ColorPicker.xaml
    /// </summary>
    public partial class ColorPicker : Window
    {
        D.Color current;
        D.Color original;
        HsvColor hsv;
        float alpha;

        public D.Color Selected
        {
            get
            {
                return current;
            }
        }

        bool isInputHText;
        bool isInputSText;
        bool isInputVText;

        bool isInputRText;
        bool isInputGText;
        bool isInputBText;

        bool isInputAText;

        bool isInputHSlide;
        bool isInputSSlide;
        bool isInputVSlide;

        bool isInputRSlide;
        bool isInputGSlide;
        bool isInputBSlide;

        bool isInputASlide;

        bool satMouseDown;
        bool hueMouseDown;

        RawBitmap hueBitmap;
        RawBitmap svBitmap;

        static Regex isFloatNumber = new Regex("\\-?[0-9]*\\.?[0-9]?");
        static Regex isIntNumber = new Regex("\\-?[0-9]*");

        MouseHook msHook;

        double screenScale;

        bool globalPicking = false;

        Magnifier mag;

        DispatcherTimer tColor;

        D.Rectangle lastMagRect;

        public ColorPicker()
        {
            InitializeComponent();
            original = current = D.Color.FromArgb(255, 255, 255, 255);
            hsv = HsvColor.FromColor(current);
            Init();
        }

        public ColorPicker(D.Color p)
        {
            InitializeComponent();
            current = p;
            original = p;
            hsv = HsvColor.FromColor(p);
            Init();
        }

        void Init()
        {
            tColor = new DispatcherTimer(DispatcherPriority.Normal, App.Current.Dispatcher);
            tColor.Interval = new TimeSpan(0,0,0,0,33);
            tColor.Tick += TColor_Tick;
            tColor.Stop();

            mag = new Magnifier();
            mag.Owner = null;
            mag.Hide();

            PrevColor.Background = new SolidColorBrush(Color.FromArgb(current.A, current.R, current.G, current.B));
            SelectedColor.Background = new SolidColorBrush(Color.FromArgb(current.A, current.R, current.G, current.B));
            alpha = current.A / 255.0f;

            msHook = new MouseHook();
            msHook.MouseClickEvent += MsHook_MouseClickEvent;

            EventManager.RegisterClassHandler(typeof(Window), Keyboard.KeyDownEvent, new KeyEventHandler(OnKeyDown), true);
        }

        private void TColor_Tick(object sender, EventArgs e)
        {
            if (globalPicking)
            {
                D.Point p = msHook.Point;

                WpfScreen screen = WpfScreen.GetScreenFrom(new System.Windows.Point(p.X * screenScale, p.Y * screenScale));

                if(p.X >= screen.WorkingArea.X / screenScale && p.Y >= screen.WorkingArea.Y / screenScale)
                {
                    lastMagRect = screen.WorkingArea;
                } 

                UpdatePreview(GetColorAt(p.X, p.Y));

                mag.Update(bmp, p.X, p.Y, lastMagRect, screenScale);
            }
        }

        private void Dropper_Click(object sender, RoutedEventArgs e)
        {
            mag.Show();
            tColor.Start();
            globalPicking = true;
            msHook.SetHook();
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                globalPicking = false;
                msHook.UnHook();
                UpdatePreview();
                tColor.Stop();

                if (mag != null)
                {
                    mag.Hide();
                }
            }
        }

        private void MsHook_MouseClickEvent(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if(globalPicking)
            {
                if (e.Button == System.Windows.Forms.MouseButtons.Left)
                {
                    D.Color c = GetColorAt((int)e.X, (int)e.Y);
                    current = c;
                    hsv = HsvColor.FromColor(c);

                    UpdatePreview();
                    UpdateSliders();
                    UpdateTextFields();
                    RedrawSatVal();

                    globalPicking = false;

                    mag.Hide();

                    msHook.UnHook();
                    tColor.Stop();
                }
            }
        }

        D.Bitmap bmp = new D.Bitmap(16, 16, D.Imaging.PixelFormat.Format32bppArgb);
        D.Color GetColorAt(int x, int y)
        {
            D.Rectangle bounds = new D.Rectangle(x - 8, y - 8, 16, 16);

            using(D.Graphics gdest = D.Graphics.FromImage(bmp))
            {
                gdest.CopyFromScreen(bounds.Location, D.Point.Empty, bounds.Size);
            }
            return bmp.GetPixel(7, 7);
        }

        void UpdatePreview()
        {
            if (SelectedColor.Background is SolidColorBrush)
            {
                SolidColorBrush b = SelectedColor.Background as SolidColorBrush;
                b.Color = Color.FromArgb((byte)(alpha * 255), current.R, current.G, current.B);
            }
            else
            {
                SelectedColor.Background = new SolidColorBrush(Color.FromArgb((byte)(alpha * 255), current.R, current.G, current.B));
            }

            //update HPoint
            UpdateHuePoint();
            UpdateSVPoint();
        }

        void UpdatePreview(D.Color c)
        {
            if (SelectedColor.Background is SolidColorBrush)
            {
                SolidColorBrush b = SelectedColor.Background as SolidColorBrush;
                b.Color = Color.FromArgb((byte)(alpha * 255), c.R, c.G, c.B);
            }
            else
            {
                SelectedColor.Background = new SolidColorBrush(Color.FromArgb((byte)(alpha * 255), c.R, c.G, c.B));
            }
        }

        void UpdateSVPoint()
        {
            float sf = hsv.S;
            float sv = hsv.V;

            //- 5 for centering the cursor thingy
            float x = (svBitmap.Width * sf) - 5;
            float y = svBitmap.Height - (svBitmap.Height * sv) - 5;

            Canvas.SetLeft(SVPoint, x);
            Canvas.SetTop(SVPoint, y);
        }

        void UpdateHuePoint()
        {
            float f = hsv.H / 359.0f;
            float y = hueBitmap.Height * f;
            Canvas.SetTop(HPoint, y);
        }

        void RedrawHue()
        {
            for(int y = 0; y < hueBitmap.Height; ++y)
            {
                float f = y / (float)hueBitmap.Height;
                float h = f * 359;
                HsvColor v = new HsvColor(h, 1, 1);
                D.Color c = v.ToColor();

                for (int x = 0; x < hueBitmap.Width; ++x)
                {
                    hueBitmap.SetPixel(x, y, c.R, c.G, c.B, 255);
                }
            }

            HueSelector.Source = hueBitmap.ToImageSource();
        }

        void RedrawSatVal()
        {
            for(int y = 0; y < svBitmap.Height; ++y)
            {
                for(int x = 0; x < svBitmap.Width; ++x)
                {
                    float sf = x / (float)svBitmap.Width;
                    float sv = 1.0f - y / (float)svBitmap.Height;

                    HsvColor v = new HsvColor(hsv.H, sf, sv);
                    D.Color c = v.ToColor();

                    svBitmap.SetPixel(x, y, c.R, c.G, c.B, 255);
                }
            }

            SaturationValueSelector.Source = svBitmap.ToImageSource();
        }

        void UpdateTextFields()
        {
            isInputHText = true;
            isInputSText = true;
            isInputVText = true;
            isInputRText = true;
            isInputGText = true;
            isInputBText = true;
            isInputAText = true;

            float h = (hsv.H / 359.0f);
            HInput.Text = h >= 0.01 ? String.Format("{0:0.000}", h) : "0";
            SInput.Text = hsv.S >= 0.01 ? String.Format("{0:0.000}", hsv.S) : "0";
            VInput.Text = hsv.V >= 0.01 ? String.Format("{0:0.000}", hsv.V) : "0";

            float r = (current.R / 255.0f);
            float g = (current.G / 255.0f);
            float b = (current.B / 255.0f);

            RInput.Text = r >= 0.01 ? String.Format("{0:0.000}", r) : "0";
            GInput.Text = g >= 0.01 ? String.Format("{0:0.000}", g) : "0";
            BInput.Text = b >= 0.01 ? String.Format("{0:0.000}", b) : "0";

            AInput.Text = alpha >= 0.01 ? String.Format("{0:0.000}", alpha) : "0";
        }

        void UpdateSliders()
        {
            isInputASlide = true;
            isInputBSlide = true;
            isInputGSlide = true;
            isInputRSlide = true;
            isInputVSlide = true;
            isInputSSlide = true;
            isInputHSlide = true;

            HSlideInput.Value = (hsv.H / 359.0f);
            SSlideInput.Value = hsv.S;
            VSlideInput.Value = hsv.V;

            RSlideInput.Value = (current.R / 255.0f);
            GSlideInput.Value = (current.G / 255.0f);
            BSlideInput.Value = (current.B / 255.0f);

            ASlideInput.Value = alpha;
        }

        private void HInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (isInputHText)
            {
                isInputHText = false;
                return;
            }

            float f;
            if (float.TryParse(HInput.Text, out f))
            {
                float rH = Math.Min(359, Math.Max(0, f * 359.0f));
                hsv.H = rH;
                current = hsv.ToColor();
                UpdateSliders();
                UpdateTextFields();
                UpdatePreview();
                RedrawSatVal();
            }
        }

        private void HInput_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !isFloatNumber.IsMatch(e.Text);
        }

        private void SInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            if(isInputSText)
            {
                isInputSText = false;
                return;
            }

            float f;
            if(float.TryParse(SInput.Text, out f))
            {
                float sf = Math.Min(1, Math.Max(0, f));
                hsv.S = sf;
                current = hsv.ToColor();
                UpdateTextFields();
                UpdateSliders();
                UpdatePreview();
            }
        }

        private void SInput_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !isFloatNumber.IsMatch(e.Text);
        }

        private void VInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (isInputVText)
            {
                isInputVText = false;
                return;
            }

            float f;
            if (float.TryParse(VInput.Text, out f))
            {
                float sf = Math.Min(1, Math.Max(0, f));
                hsv.V = sf;
                current = hsv.ToColor();
                UpdateSliders();
                UpdateTextFields();
                UpdatePreview();
            }
        }

        private void VInput_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !isFloatNumber.IsMatch(e.Text);
        }

        private void RInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (isInputRText)
            {
                isInputRText = false;
                return;
            }

            float f;
            if (float.TryParse(RInput.Text, out f))
            {
                byte sf = (byte)Math.Min(255, Math.Max(0, f * 255));

                current = D.Color.FromArgb((int)(alpha * 255), sf, current.G, current.B);
                hsv = HsvColor.FromColor(current);

                UpdateSliders();
                UpdateTextFields();
                UpdatePreview();
                RedrawSatVal();
            }
        }

        private void RInput_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !isFloatNumber.IsMatch(e.Text);
        }

        private void GInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (isInputGText)
            {
                isInputGText = false;
                return;
            }

            float f;
            if (float.TryParse(GInput.Text, out f))
            {
                byte sf = (byte)Math.Min(255, Math.Max(0, f * 255));

                current = D.Color.FromArgb((int)(alpha * 255),current.R, sf, current.B);
                hsv = HsvColor.FromColor(current);

                UpdateSliders();
                UpdateTextFields();
                UpdatePreview();
                RedrawSatVal();
            }
        }

        private void GInput_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !isFloatNumber.IsMatch(e.Text);
        }

        private void BInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (isInputBText)
            {
                isInputBText = false;
                return;
            }

            float f;
            if (float.TryParse(BInput.Text, out f))
            {
                byte sf = (byte)Math.Min(255, Math.Max(0, f * 255));

                current = D.Color.FromArgb((int)(alpha * 255), current.R, current.G, sf);
                hsv = HsvColor.FromColor(current);

                UpdateSliders();
                UpdateTextFields();
                UpdatePreview();
                RedrawSatVal();
            }
        }

        private void BInput_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !isFloatNumber.IsMatch(e.Text);
        }

        private void AInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (isInputAText)
            {
                isInputAText = false;
                return;
            }

            float f;
            if (float.TryParse(AInput.Text, out f))
            {
                alpha = Math.Min(1, Math.Max(0, f));

                current = D.Color.FromArgb((int)(alpha * 255), current.R, current.G, current.B);

                UpdateSliders();
                UpdateTextFields();
                UpdatePreview();
            }
        }

        private void AInput_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !isFloatNumber.IsMatch(e.Text);
        }

        private void HSlideInput_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if(isInputHSlide)
            {
                isInputHSlide = false;
                return;
            }

            float f  = (float)HSlideInput.Value;
            hsv.H = f * 359;
            current = hsv.ToColor();
            UpdatePreview();
            UpdateSliders();
            UpdateTextFields();
            RedrawSatVal();
        }

        private void SSlideInput_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (isInputVSlide)
            {
                isInputVSlide = false;
                return;
            }

            float f = (float)SSlideInput.Value;
            hsv.S = f;
            current = hsv.ToColor();
            UpdatePreview();
            UpdateSliders();
            UpdateTextFields();
        }

        private void VSlideInput_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (isInputSSlide)
            {
                isInputSSlide = false;
                return;
            }

            float f = (float)VSlideInput.Value;
            hsv.V = f;
            current = hsv.ToColor();
            UpdatePreview();
            UpdateSliders();
            UpdateTextFields();
        }

        private void RSlideInput_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (isInputRSlide)
            {
                isInputRSlide = false;
                return;
            }

            float f = (float)RSlideInput.Value;
            byte v = (byte)Math.Min(255, Math.Max(0, f * 255));
            current = D.Color.FromArgb((int)(alpha * 255), v, current.G, current.B);
            hsv = HsvColor.FromColor(current);
            UpdatePreview();
            UpdateTextFields();
            RedrawSatVal();
        }

        private void GSlideInput_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (isInputGSlide)
            {
                isInputGSlide = false;
                return;
            }

            float f = (float)GSlideInput.Value;
            byte v = (byte)Math.Min(255, Math.Max(0, f * 255));
            current = D.Color.FromArgb((int)(alpha * 255), current.R, v, current.B);
            hsv = HsvColor.FromColor(current);
            UpdatePreview();
            UpdateTextFields();
            UpdateSliders();
            RedrawSatVal();
        }

        private void BSlideInput_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (isInputBSlide)
            {
                isInputBSlide = false;
                return;
            }

            float f = (float)BSlideInput.Value;
            byte v = (byte)Math.Min(255, Math.Max(0, f * 255));
            current = D.Color.FromArgb((int)(alpha * 255), current.R, current.G, v);
            hsv = HsvColor.FromColor(current);
            UpdatePreview();
            UpdateTextFields();
            UpdateSliders();
            RedrawSatVal();
        }

        private void ASlideInput_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (isInputASlide)
            {
                isInputASlide = false;
                return;
            }

            float f = (float)ASlideInput.Value;
            alpha = Math.Min(1, Math.Max(0, f));
            current = D.Color.FromArgb((int)(alpha * 255), current.R, current.G, current.B);
            hsv = HsvColor.FromColor(current);
            UpdatePreview();
            UpdateSliders();
            UpdateTextFields();
        }

        private void HueSelector_MouseMove(object sender, MouseEventArgs e)
        {
            if(hueMouseDown)
            {
                Point p = e.GetPosition(HueSelector);
                float y = (float)p.Y / hueBitmap.Height * 359.0f;
                y = Math.Min(359.0f, Math.Max(0, y));
                hsv.H = y;
                current = hsv.ToColor();

                UpdatePreview();
                UpdateTextFields();
                UpdateSliders();
                RedrawSatVal();
            }
        }

        private void SaturationValueSelector_MouseMove(object sender, MouseEventArgs e)
        {
            if (satMouseDown)
            {
                Point p = e.GetPosition(SaturationValueSelector);
                float s = (float)p.X / (float)svBitmap.Width;
                float v = 1.0f - (float)p.Y / (float)svBitmap.Height;

                s = Math.Min(1, Math.Max(0, s));
                v = Math.Min(1, Math.Max(0, v));

                hsv.S = s;
                hsv.V = v;

                current = hsv.ToColor();

                UpdatePreview();
                UpdateTextFields();
                UpdateSliders();
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            screenScale = PresentationSource.FromVisual(this).CompositionTarget.TransformToDevice.M11;
            hueBitmap = new RawBitmap((int)GHue.ActualWidth, (int)GHue.ActualHeight);
            svBitmap = new RawBitmap((int)GSatVal.ActualWidth, (int)GSatVal.ActualHeight);
            RedrawHue();
            RedrawSatVal();
            UpdatePreview();
            UpdateTextFields();
            UpdateSliders();
            UpdateSliders();
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            globalPicking = false;
            msHook.UnHook();
            tColor.Stop();
            mag.Close();
            mag = null;
        }

        private void SelectButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void SaturationValueSelector_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if(e.LeftButton == MouseButtonState.Pressed)
            {
                satMouseDown = true;

                Point p = e.GetPosition(SaturationValueSelector);
                float s = (float)p.X / (float)svBitmap.Width;
                float v = 1.0f - (float)p.Y / (float)svBitmap.Height;

                hsv.S = s;
                hsv.V = v;

                current = hsv.ToColor();

                UpdatePreview();
                UpdateTextFields();
                UpdateSliders();
            }
        }

        private void HueSelector_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if(e.LeftButton == MouseButtonState.Pressed)
            {
                hueMouseDown = true;

                Point p = e.GetPosition(HueSelector);
                float y = (float)p.Y / hueBitmap.Height * 359.0f;
                hsv.H = y;
                current = hsv.ToColor();

                UpdatePreview();
                UpdateTextFields();
                UpdateSliders();
                RedrawSatVal();
            }
        }

        private void Window_MouseUp(object sender, MouseButtonEventArgs e)
        {
            hueMouseDown = false;
            satMouseDown = false;
        }

        private void Window_MouseLeave(object sender, MouseEventArgs e)
        {
            hueMouseDown = false;
            satMouseDown = false;
        }

        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            if(hueMouseDown)
            {
                HueSelector_MouseMove(HueSelector, e);
            }
            else if(satMouseDown)
            {
                SaturationValueSelector_MouseMove(SaturationValueSelector, e);
            }
        }
    }
}
