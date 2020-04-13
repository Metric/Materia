using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Markup.Xaml;
using System.Reflection;
using Materia.Rendering.Mathematics;
using MateriaCore.Components.Dialogs;
using MateriaCore.Utils;
using System.Threading.Tasks;

namespace MateriaCore.Components
{
    public class ColorSelect : UserControl
    {
        PropertyInfo property;
        object propertyOwner;

        private Grid selectColor;
        private Button dropper;

        MVector current;

        ScreenPixelGrabber pixelGrabber;

        public ColorSelect()
        {
            InitializeComponent();
            selectColor.Background = new SolidColorBrush(Colors.Black);
            selectColor.PointerReleased += SelectColor_PointerReleased;
            dropper.Click += Dropper_Click;
        }

        private void Dropper_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
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

        private void PixelGrabber_OnGrabbed(ref System.Drawing.Color c)
        {
            current.X = c.R / 255.0f;
            current.Y = c.G / 255.0f;
            current.Z = c.B / 255.0f;
            current = current.Clamp(MVector.Zero, MVector.One);
            property.SetValue(propertyOwner, current);
            UpdateBrush();
        }

        private void SelectColor_PointerReleased(object sender, Avalonia.Input.PointerReleasedEventArgs e)
        {
            ColorPicker cp = new ColorPicker(current);
            Task<bool> resulter = cp.ShowDialog<bool>(MainWindow.Instance);

            bool result = false;
            Task.Run(async () =>
            {
                result = await resulter;
            }).ContinueWith(t =>
            {
                if(result)
                {
                    current = cp.SelectedVector;
                    property.SetValue(propertyOwner, current);
                    UpdateBrush();
                }
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        public ColorSelect(PropertyInfo p, object owner) : this()
        {
            property = p;
            propertyOwner = owner;

            MVector m = (MVector)p.GetValue(owner);
            current = m.Clamp(MVector.Zero, MVector.One);
            UpdateBrush();
        }

        private void UpdateBrush()
        {
            Color c = Color.FromArgb((byte)(current.W * 255), (byte)(current.X * 255), (byte)(current.Y * 255), (byte)(current.Z * 255));
            SolidColorBrush brush = selectColor.Background as SolidColorBrush;
            brush.Color = c;
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            selectColor = this.FindControl<Grid>("SelectColor");
            dropper = this.FindControl<Button>("Dropper");
        }

        protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnDetachedFromVisualTree(e);

            pixelGrabber?.Dispose();
        }
    }
}
