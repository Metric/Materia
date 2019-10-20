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
using Materia.Nodes;

namespace Materia.UI.Components
{
    /// <summary>
    /// Interaction logic for CustomParameter.xaml
    /// </summary>
    public partial class ParameterView : UserControl
    {
        public delegate void Remove(ParameterView c);
        public event Remove OnRemove;

        public GraphParameterValue Param { get; protected set; }

        public string Id { get; protected set; }

        /// <summary>
        /// IsReadOnly applied to the name field
        /// It does not apply to other fields
        /// that may be available
        /// </summary>
        public bool IsReadOnly { get; protected set; }

        public ParameterView()
        {
            InitializeComponent();
        }

        /// <summary>
        /// This is for listing Promoted Graph Parameters
        /// And is used in GraphParameterEditor
        /// </summary>
        /// <param name="title"></param>
        /// <param name="id"></param>
        /// <param name="v"></param>
        /// <param name="ignore"></param>
        public ParameterView(string title, string id, GraphParameterValue v, params string[] ignore)
        {
            InitializeComponent();
            Param = v;
            IsReadOnly = true;
            Params.Set(v, ignore, false, true);
            Id = id;
            ParamName.Placeholder = Properties.Resources.TITLE_PARAMETER_NAME;
            ParamName.Set(title);
            DisplayDefaultParam();
            Collapse();
        }

        /// <summary>
        /// This is used for Custom Parameters
        /// And is used in CustomParameterEditor
        /// </summary>
        /// <param name="v"></param>
        /// <param name="id"></param>
        /// <param name="isReadOnly"></param>
        /// <param name="ignore"></param>
        public ParameterView(GraphParameterValue v, string id = "", bool isReadOnly = false, params string[] ignore)
        {
            InitializeComponent();
            Param = v;
            IsReadOnly = isReadOnly;
            Params.Set(v, ignore, false, true);
            Id = id;
            InitNameField();
            DisplayDefaultParam();
            Collapse();
        }

        private void GraphParameterValue_OnGraphParameterTypeChanged(GraphParameterValue param)
        {
            if(param == Param)
            {
                DisplayDefaultParam();
            }
        }

        private void InitNameField()
        {
            if (Param == null) return;

            var prop = Param.GetType().GetProperty("Name");
            if (prop == null) return;

            ParamName.Placeholder = Properties.Resources.TITLE_PARAMETER_NAME;
            ParamName.Set(prop, Param, IsReadOnly);
        }

        protected void DisplayDefaultParam()
        {
            ParamDefaultStack.Children.Clear();
            var p = new ParameterMap(null, new List<GraphParameterValue>(new GraphParameterValue[] { Param }), false, true);
            ParamDefaultStack.Children.Add(p);
        }

        private void RemoveParam_Click(object sender, RoutedEventArgs e)
        {
            if(MessageBox.Show(Properties.Resources.TITLE_REMOVE_PARAMETER + ": " + Param.Name,"",MessageBoxButton.OKCancel) == MessageBoxResult.OK)
            {
                if(OnRemove != null)
                {
                    OnRemove.Invoke(this);
                }
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if(Param != null)
            {
                Param.OnGraphParameterTypeChanged += GraphParameterValue_OnGraphParameterTypeChanged;
            }
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            if(Param != null)
            {
                Param.OnGraphParameterTypeChanged -= GraphParameterValue_OnGraphParameterTypeChanged;
            }
        }

        private void Collapse()
        {
            ParamDefaultStack.Visibility = Visibility.Collapsed;
            Params.Visibility = Visibility.Collapsed;
            CollapseButtonRotation.Angle = 0;
        }

        private void Expand()
        {
            ParamDefaultStack.Visibility = Visibility.Visible;
            Params.Visibility = Visibility.Visible;
            CollapseButtonRotation.Angle = 90;
        }

        private void CollapsedButton_Click(object sender, RoutedEventArgs e)
        {
            if(ParamDefaultStack.Visibility == Visibility.Visible)
            {
                Collapse();
            }
            else
            {
                Expand();
            }
        }
    }
}
