using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Materia.Rendering.Mathematics;
using MateriaCore.Components.Dialogs;
using System.Threading.Tasks;

namespace MateriaCore.Components
{
    public class GradientHandle : UserControl
    {
        public delegate void ColorChanged(GradientHandle handle);
        public event ColorChanged OnColorChanged;

        Button selectedColor;

        public float Position { get; set; }

        protected MVector color;
        public MVector SelectedColor
        {
            get
            {
                return color;
            }
            set
            {
                color = value;
            }
        }

        public GradientHandle()
        {
            this.InitializeComponent();
            selectedColor.DoubleTapped += SelectedColor_DoubleTapped;
        }

        private void SelectedColor_DoubleTapped(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            ColorPicker cp = new ColorPicker(color);
            Task<bool> resultor = cp.ShowDialog<bool>(MainWindow.Instance);
            bool result = false;
            Task.Run(async () =>
            {
                result = await resultor;
            })
            .ContinueWith(t =>
            {
                if (result)
                {
                    SetColor(cp.SelectedVector);
                    OnColorChanged?.Invoke(this);
                }
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        public void SetColor(MVector c)
        {
            color = c;
            selectedColor.Background = new SolidColorBrush(Color.FromArgb((byte)(c.W * 255), (byte)(c.X * 255), (byte)(c.Y * 255), (byte)(c.Z * 255)));
        }
        

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            selectedColor = this.FindControl<Button>("SelectedColor");
        }
    }
}
