using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Reflection;
using Materia.Nodes;
using Materia.MathHelpers;

namespace Materia
{
    /// <summary>
    /// Interaction logic for UINodeParameters.xaml
    /// </summary>
    public partial class UINodeParameters : UserControl
    {
        public object node { get; protected set; }

        Dictionary<string, PropertyInfo> propertyLookup;
        Dictionary<string, UIElement> elementLookup;
        List<UIElement> labels;

        public delegate void RemoveEvent(UINodeParameters p);
        public event RemoveEvent OnClose;

        public static UINodeParameters Instance { get; protected set; }

        public UINodeParameters()
        {
            InitializeComponent();
            Instance = this;
            labels = new List<UIElement>();
            propertyLookup = new Dictionary<string, PropertyInfo>();
            elementLookup = new Dictionary<string, UIElement>();
        }

        public UINodeParameters(object n)
        {
            InitializeComponent();
            Instance = this;
            node = n;
            labels = new List<UIElement>();
            propertyLookup = new Dictionary<string, PropertyInfo>();
            elementLookup = new Dictionary<string, UIElement>();

            if (node is Node)
            {
                Node nd = (Node)node;
                foreach (NodeInput inp in nd.Inputs)
                {
                    inp.OnInputChanged += Inp_OnInputChanged;
                    inp.OnInputAdded += Inp_OnInputAdded;
                }
            }

            Params.Set(node);
        }

        public void SetActive(object n)
        {
            if (n == null) return;
            if (n == node) return;

            ClearView();

            node = n;

            if (node is Node)
            {
                Node nd = (Node)node;
                Title.Text = nd.Name;


                foreach (NodeInput inp in nd.Inputs)
                {
                    inp.OnInputChanged += Inp_OnInputChanged;
                    inp.OnInputAdded += Inp_OnInputAdded;
                }

                nd.OnInputRemovedFromNode += Nd_OnInputRemovedFromNode;
                nd.OnInputAddedToNode += Nd_OnInputAddedToNode;
            }
            else if(node is Graph)
            {
                Title.Text = "Graph";
            }
            else if(node is Camera)
            {
                Title.Text = "3D Camera";
            }
            else
            {
                Title.Text = "";
            }

            Params.Set(node);
        }

        private void Nd_OnInputAddedToNode(Node n, NodeInput inp)
        {
            inp.OnInputChanged += Inp_OnInputChanged;
            inp.OnInputAdded += Inp_OnInputAdded;
        }

        private void Nd_OnInputRemovedFromNode(Node n, NodeInput inp)
        {
            inp.OnInputChanged -= Inp_OnInputChanged;
            inp.OnInputAdded -= Inp_OnInputAdded;
        }

        private void Inp_OnInputAdded(NodeInput n)
        {
            Node_OnUpdate(n.Node);
        }

        private void Inp_OnInputChanged(NodeInput n)
        {
            Node_OnUpdate(n.Node);
        }

        private void Node_OnUpdate(Node n)
        {
            IEnumerable<UIElement> values = Params.elementLookup.Values;

            UILevels v = (UILevels)values.FirstOrDefault(m => m is UILevels);

            if(v != null)
            { 
                if (n.Inputs.Count > 0 && n.Inputs[0].Input != null)
                {
                    v.OnUpdate(n.Inputs[0].Input.Node);
                }
            }
        }

        public void ClearView()
        {
            if(node is Node)
            {
                Node nd = (Node)node;
                if (nd != null && nd.Inputs != null)
                {
                    foreach (NodeInput inp in nd.Inputs)
                    {
                        inp.OnInputChanged -= Inp_OnInputChanged;
                        inp.OnInputAdded -= Inp_OnInputAdded;
                    }
                }

                nd.OnInputAddedToNode -= Nd_OnInputAddedToNode;
                nd.OnInputRemovedFromNode -= Nd_OnInputRemovedFromNode;
            }

            Params.Clear();

            node = null;
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            if(OnClose != null)
            {
                OnClose.Invoke(this);
            }
        }
    }
}
