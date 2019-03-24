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
using System.Reflection;
using Materia.Nodes;
using Materia.Nodes.Attributes;
using Materia.Nodes.Containers;
using Materia.Imaging;
using Materia.UI.Components;
using OpenTK;
using Materia.Nodes.Atomic;

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

            CreateProperties();

            if (node is Node)
            {
                Node nd = (Node)node;
                foreach (NodeInput inp in nd.Inputs)
                {
                    inp.OnInputChanged += Inp_OnInputChanged;
                    inp.OnInputAdded += Inp_OnInputAdded;
                }
            }
        }

        public void SetActive(object n)
        {
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
            else
            {
                Title.Text = "";
            }

            CreateProperties();
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
            IEnumerable<UIElement> values = elementLookup.Values;

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
            foreach (UIElement v in elementLookup.Values)
            {
                Stack.Children.Remove(v);
            }

            elementLookup.Clear();

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

            node = null;

            Title.Text = "";

            propertyLookup.Clear();

            foreach(UIElement l in labels)
            {
                Stack.Children.Remove(l);
            }

            labels.Clear();
        }

        void CreateProperties()
        {
            var props = node.GetType().GetProperties();

            Dictionary<string, List<PropertyInfo>> sections = new Dictionary<string, List<PropertyInfo>>();

            foreach(var p in props)
            {
                var s = p.GetCustomAttribute<SectionAttribute>();

                if((!p.PropertyType.Equals(typeof(float))
                    && !p.PropertyType.Equals(typeof(int))
                    && !p.PropertyType.Equals(typeof(bool))
                    && !p.PropertyType.IsEnum
                    && !p.PropertyType.Equals(typeof(string[]))
                    && p.CustomAttributes.Count() == 0) 
                    || p.GetCustomAttribute<HidePropertyAttribute>() != null)
                {
                    continue;
                }

                string section = null;
                if (s == null)
                {
                    section = "Default";
                }
                else
                {
                    section = s.Section;
                }

                List<PropertyInfo> sectionProps = null;

                if(sections.TryGetValue(section, out sectionProps))
                {
                    sectionProps.Add(p);
                }
                else
                {
                    sectionProps = new List<PropertyInfo>();
                    sectionProps.Add(p);
                    sections[section] = sectionProps;
                }
            }

            foreach(var s in sections.Keys)
            {
                List<PropertyInfo> list = sections[s];

                PropertySection st = new PropertySection()
                {
                    Title = s
                };

                Stack.Children.Add(st);
                labels.Add(st);

                foreach(var p in list)
                {
                    string name = p.Name;
                    propertyLookup[name] = p;
                    CreateUIElement(p.PropertyType, p, name);
                }
            }
        }

        void CreateUIElement(Type t, PropertyInfo p, string name)
        {
            DropdownAttribute dp = p.GetCustomAttribute<DropdownAttribute>();
            LevelEditorAttribute le = p.GetCustomAttribute<LevelEditorAttribute>();
            CurveEditorAttribute ce = p.GetCustomAttribute<CurveEditorAttribute>();
            SliderAttribute sl = p.GetCustomAttribute<SliderAttribute>();
            FileSelectorAttribute fsl = p.GetCustomAttribute<FileSelectorAttribute>();
            HidePropertyAttribute hp = p.GetCustomAttribute<HidePropertyAttribute>();
            ColorPickerAttribute cp = p.GetCustomAttribute<ColorPickerAttribute>();
            TitleAttribute ti = p.GetCustomAttribute<TitleAttribute>();
            TextInputAttribute tinp = p.GetCustomAttribute<TextInputAttribute>();
            GraphParameterEditorAttribute gpe = p.GetCustomAttribute<GraphParameterEditorAttribute>();
            ParameterMapEditorAttribute pme = p.GetCustomAttribute<ParameterMapEditorAttribute>();
            PromoteAttribute pro = p.GetCustomAttribute<PromoteAttribute>();

            //handle very special stuff
            //exposed constant parameter variable names
            if(gpe != null)
            {
                if(node is Graph)
                {
                    Graph g = node as Graph;

                    GraphParameterEditor inp = new GraphParameterEditor(g, g.Parameters);
                    Stack.Children.Add(inp);
                    elementLookup[name] = inp;
                }
            }
            //for graph instance exposed parameters from underlying graph
            else if(pme != null)
            {
                if(node is GraphInstanceNode)
                {
                    GraphInstanceNode gin = node as GraphInstanceNode;
                    ParameterMap pm = new ParameterMap(gin.GraphInst, gin.Parameters);
                    Stack.Children.Add(pm);
                    elementLookup[name] = pm;
                }
            }

            string title = name;

            if(ti != null)
            {
                title = ti.Title;
            }

            PropertyInfo op = null;

            //we don't create an element for this one
            //as it is hidden
            if(hp != null)
            {
                return;
            }

            try
            {
                if (ce != null)
                {
                    op = node.GetType().GetProperty(ce.OutputProperty);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
            }

            if(t.Equals(typeof(Vector4)))
            {
                if(cp != null)
                {
                    PropertyLabel l = null;
                    if(pro != null && node is Node)
                    {
                        l = new PropertyLabel(title, node as Node, name);
                    }
                    else
                    {
                        l = new PropertyLabel();
                        l.Title = title;
                    }

                    labels.Add(l);
                    Stack.Children.Add(l);

                    ColorSelect cs = new ColorSelect(p, node);
                    Stack.Children.Add(cs);
                    elementLookup[name] = cs;
                }
            }
            else if(t.Equals(typeof(string[])))
            {
                if (dp != null)
                {
                    PropertyLabel l = new PropertyLabel();
                    l.Title = title;
                    labels.Add(l);
                    Stack.Children.Add(l);

                    DropDown inp = new DropDown((string[])p.GetValue(node), node, p, dp.OutputProperty);
                    Stack.Children.Add(inp);
                    elementLookup[name] = inp;
                }
            }
            else if(t.Equals(typeof(bool)))
            {
                PropertyLabel l = null;
                if (pro != null && node is Node)
                {
                    l = new PropertyLabel(title, node as Node, name);
                }
                else
                {
                    l = new PropertyLabel();
                    l.Title = title;
                }

                labels.Add(l);
                Stack.Children.Add(l);

                ToggleControl tg = new ToggleControl(name, p, node);
                Stack.Children.Add(tg);
                elementLookup[name] = tg;
            }
            else if(t.Equals(typeof(string)))
            {
                if(tinp != null)
                {
                    PropertyLabel l = new PropertyLabel();
                    l.Title = title;
                    labels.Add(l);
                    Stack.Children.Add(l);

                    PropertyInput ip = new PropertyInput(p, node);
                    Stack.Children.Add(ip);
                    elementLookup[name] = ip;
                }
                else if(fsl != null)
                {
                    PropertyLabel l = new PropertyLabel();
                    l.Title = title;
                    labels.Add(l);
                    Stack.Children.Add(l);

                    FileSelector inp = new FileSelector(p, node, fsl.Filter);
                    Stack.Children.Add(inp);
                    elementLookup[name] = inp;
                }
                else if(dp != null)
                {
                    PropertyLabel l = new PropertyLabel();
                    l.Title = title;
                    labels.Add(l);
                    Stack.Children.Add(l);

                    object[] names = dp.Values;
                    DropDown inp = new DropDown(names, node, p, dp.OutputProperty);
                    Stack.Children.Add(inp);
                    elementLookup[name] = inp;
                }
            }
            if (t.Equals(typeof(float)))
            {
                if (sl != null)
                {
                    PropertyLabel l = null;
                    if (pro != null && node is Node)
                    {
                        l = new PropertyLabel(title, node as Node, name);
                    }
                    else
                    {
                        l = new PropertyLabel();
                        l.Title = title;
                    }

                    labels.Add(l);
                    Stack.Children.Add(l);

                    NumberSlider inp = new NumberSlider(sl, p, node);
                    Stack.Children.Add(inp);
                    elementLookup[name] = inp;
                }
                else
                {
                    PropertyLabel l = null;
                    if (pro != null && node is Node)
                    {
                        l = new PropertyLabel(title, node as Node, name);
                    }
                    else
                    {
                        l = new PropertyLabel();
                        l.Title = title;
                    }

                    labels.Add(l);
                    Stack.Children.Add(l);

                    NumberInput inp = new NumberInput(NumberInputType.Float, node, p);
                    Stack.Children.Add(inp);
                    elementLookup[name] = inp;
                }
            }
            else if(t.Equals(typeof(int)))
            {
                if (dp != null)
                {
                    PropertyLabel l = new PropertyLabel();
                    l.Title = title;
                    labels.Add(l);
                    Stack.Children.Add(l);

                    //do a dropdown
                    object[] names = dp.Values;
                    DropDown inp = new DropDown(names, node, p, dp.OutputProperty);
                    Stack.Children.Add(inp);
                    elementLookup[name] = inp;
                }
                else if(sl != null)
                {
                    PropertyLabel l = null;
                    if (pro != null && node is Node)
                    {
                        l = new PropertyLabel(title, node as Node, name);
                    }
                    else
                    {
                        l = new PropertyLabel();
                        l.Title = title;
                    }

                    labels.Add(l);
                    Stack.Children.Add(l);

                    NumberSlider inp = new NumberSlider(sl, p, node);
                    Stack.Children.Add(inp);
                    elementLookup[name] = inp;
                }
                else
                {
                    PropertyLabel l = null;
                    if (pro != null && node is Node)
                    {
                        l = new PropertyLabel(title, node as Node, name);
                    }
                    else
                    {
                        l = new PropertyLabel();
                        l.Title = title;
                    }

                    labels.Add(l);
                    Stack.Children.Add(l);

                    NumberInput inp = new NumberInput(NumberInputType.Int, node, p);
                    Stack.Children.Add(inp);
                    elementLookup[name] = inp;
                }
            }
            else if(t.Equals(typeof(MultiRange)))
            {
                if (le != null)
                {
                    UILevels lv = null;
                    if (node is Node)
                    {
                        Node nd = (Node)node;
                        if (nd.Inputs.Count > 0 && nd.Inputs[0].Input != null)
                        {
                            var n = nd.Inputs[0].Input.Node;
                            byte[] result = n.GetPreview(n.Width, n.Height);

                            RawBitmap bit = null;

                            if (result != null)
                            {
                                bit = new RawBitmap(n.Width, n.Height, result);
                            }

                            lv = new UILevels(bit, node, p);
                        }
                        else
                        {
                            lv = new UILevels(null, node, p);
                        }
                        Stack.Children.Add(lv);
                        elementLookup[name] = lv;
                    }
                }
            }
            else if(op != null && ce != null)
            {
                UICurves cv = new UICurves(p, op, node);
                Stack.Children.Add(cv);
                elementLookup[name] = cv;
            }
            else if(t.IsEnum)
            {
                PropertyLabel l = new PropertyLabel();
                l.Title = title;
                labels.Add(l);
                Stack.Children.Add(l);

                string[] names = Enum.GetNames(t);
                DropDown inp = new DropDown(names, node, p);
                Stack.Children.Add(inp);
                elementLookup[name] = inp;
            }
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
