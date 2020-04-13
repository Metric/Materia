using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Markup.Xaml;
using Materia.Graph;
using MateriaCore.Components.Dialogs;
using MateriaCore.Localization;
using System.Threading.Tasks;

namespace MateriaCore.Components
{
    public class ParameterListItem : UserControl
    {
        public delegate void Remove(ParameterListItem c);
        public event Remove OnRemove;

        Button removeParam;
        Button collapseButton;
        StackPanel defaultStack;
        TextInput paramName;

        public ParameterValue Param { get; protected set; }

        public string Id { get; protected set; }

        /// <summary>
        /// IsReadOnly applied to the name field
        /// It does not apply to other fields
        /// that may be available
        /// </summary>
        public bool IsReadOnly { get; protected set; }

        static Local local = new Local("ParameterName");

        public ParameterListItem()
        {
            this.InitializeComponent();
            removeParam.Click += RemoveParam_Click;
            collapseButton.Click += CollapseButton_Click;
        }

        public ParameterListItem(string title, string id, ParameterValue v, params string[] ignore) : this()
        {
            Param = v;
            IsReadOnly = true;
            //add setting to Params.Set()
            Id = id;
            paramName.Placeholder = local.Get("ParameterName");
            paramName.Set(title);
            DisplayDefaultParam();
            Collapse();
        }

        public ParameterListItem(ParameterValue v, string id = "", bool isReadOnly = false, params string[] ignore) : this()
        {
            Param = v;
            IsReadOnly = isReadOnly;
            //add setting to Params.Set()
            Id = id;
            InitNameField();
            DisplayDefaultParam();
            Collapse();
        }

        private void InitNameField()
        {
            if (Param == null) return;

            var prop = Param.GetType().GetProperty("Name");
            if (prop == null) return;

            paramName.Placeholder = local.Get("ParameterName");
            paramName.Set(prop, Param, IsReadOnly);
        }

        protected void DisplayDefaultParam()
        {
            defaultStack.Children.Clear();
            //var p = new ParameterMap(null, new List<GraphParameterValue>(new GraphParameterValue[] { Param }), false, true);
            //defaultStack.Children.Add(p);
        }

        private void CollapseButton_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (defaultStack.IsVisible)
            {
                Collapse();
            }
            else
            {
                Expand();
            }
        }

        private void Collapse()
        {
            defaultStack.IsVisible = false;
            //Params.IsVisible = false;
            var transform = collapseButton.RenderTransform as RotateTransform;
            if (transform != null)
            {
                transform.Angle = 0;
            }
        }

        private void Expand()
        {
            defaultStack.IsVisible = true;
            //Params.IsVisible = true;
            var transform = collapseButton.RenderTransform as RotateTransform;
            if (transform != null)
            {
                transform.Angle = 90;
            }
        }

        private void RemoveParam_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            Task<MessageBox.MessageBoxResult> resulter = MessageBox.Show(MainWindow.Instance, local.Get("RemoveParameter") + ": " + Param.Name, "", MessageBox.MessageBoxButtons.OkCancel);
            MessageBox.MessageBoxResult result = MessageBox.MessageBoxResult.No;
            Task.Run(async () =>
            {
                result = await resulter;
            }).ContinueWith(t =>
            {
                if (result == MessageBox.MessageBoxResult.Ok)
                {
                    OnRemove?.Invoke(this);
                }
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnAttachedToVisualTree(e);

            if (Param != null) 
            {
                Param.OnGraphParameterTypeChanged += Param_OnGraphParameterTypeChanged;
            }
        }

        protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnDetachedFromVisualTree(e);

            if (Param != null)
            {
                Param.OnGraphParameterTypeChanged -= Param_OnGraphParameterTypeChanged;
            }
        }

        private void Param_OnGraphParameterTypeChanged(ParameterValue param)
        {
            if (param == Param)
            {
                DisplayDefaultParam();
            }
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            removeParam = this.FindControl<Button>("RemoveParam");
            collapseButton = this.FindControl<Button>("CollapsedButton");
            defaultStack = this.FindControl<StackPanel>("ParamDefaultStack");
            paramName = this.FindControl<TextInput>("ParamName");
        }
    }
}
