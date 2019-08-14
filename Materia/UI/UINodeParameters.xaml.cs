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
        public static UINodeParameters Instance { get; protected set; }

        public UINodeParameters()
        {
            InitializeComponent();
            Instance = this;
        }

        public UINodeParameters(object n)
        {
            InitializeComponent();
            Instance = this;
            node = n;
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
            }
            else if(node is Graph)
            {
                Title.Text = "Graph";
            }
            else if(node is Camera)
            {
                Title.Text = "3D Camera";
            }
            else if (node is Settings.MaterialSettings)
            {
                Title.Text = "Material";
            }
            else if(node is Settings.LightingSettings)
            {
                Title.Text = "Lighting";
            }
            else
            {
                Title.Text = node.GetType().Name.ToString().Split(new char[] { '.' }).LastOrDefault();
            }

            Params.Set(node);
        }


        public void ClearView()
        {
            Params.Clear();
            node = null;
        }
    }
}
