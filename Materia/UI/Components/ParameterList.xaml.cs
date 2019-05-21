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
using Materia.MathHelpers;
using Materia.Nodes.Atomic;
using Materia.Imaging;
using Materia.Nodes.Containers;

namespace Materia.UI.Components
{
    /// <summary>
    /// Interaction logic for ParameterList.xaml
    /// </summary>
    public partial class ParameterList : UserControl
    {
        object node;

        Dictionary<string, PropertyInfo> propertyLookup;
        public Dictionary<string, UIElement> elementLookup { get; protected set; }
        List<UIElement> labels;

        bool showSectionLabels;
        string[] ignored;

        public ParameterList()
        {
            InitializeComponent();
            propertyLookup = new Dictionary<string, PropertyInfo>();
            elementLookup = new Dictionary<string, UIElement>();
            labels = new List<UIElement>();
        }

        public void Set(object obj, bool showSectionLabels = true, params string[] ignoreFields)
        {
            Clear();
            node = obj;
            this.showSectionLabels = showSectionLabels;
            ignored = ignoreFields;
            CreateProperties();
        }

        public void Clear()
        {
            foreach (UIElement v in elementLookup.Values)
            {
                Stack.Children.Remove(v);
            }

            elementLookup.Clear();

            propertyLookup.Clear();

            foreach (UIElement l in labels)
            {
                Stack.Children.Remove(l);
            }

            labels.Clear();

            node = null;
        }

        void CreateProperties()
        {
            var props = node.GetType().GetProperties();

            Dictionary<string, List<PropertyInfo>> sections = new Dictionary<string, List<PropertyInfo>>();

            foreach (var p in props)
            {
                var s = p.GetCustomAttribute<SectionAttribute>();

                if(ignored != null && ignored.Length > 0)
                {
                    if(ignored.Contains(p.Name))
                    {
                        continue;
                    }
                }

                if ((!p.PropertyType.Equals(typeof(float))
                    && !p.PropertyType.Equals(typeof(int))
                    && !p.PropertyType.Equals(typeof(bool))
                    && !p.PropertyType.IsEnum
                    && !p.PropertyType.Equals(typeof(string[]))
                    && p.CustomAttributes.Count() == 0
                    && !p.PropertyType.Equals(typeof(Gradient)))
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

                if (sections.TryGetValue(section, out sectionProps))
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

            foreach (var s in sections.Keys)
            {
                List<PropertyInfo> list = sections[s];

                if (showSectionLabels)
                {
                    PropertySection st = new PropertySection()
                    {
                        Title = s
                    };

                    Stack.Children.Add(st);
                    labels.Add(st);
                }

                foreach (var p in list)
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
            ParameterEditorAttribute pe = p.GetCustomAttribute<ParameterEditorAttribute>();
            GraphFunctionEditorAttribute fe = p.GetCustomAttribute<GraphFunctionEditorAttribute>();

            //handle very special stuff
            //exposed constant parameter variable names
            if (gpe != null)
            {
                if (node is Graph && !(node is FunctionGraph))
                {
                    Graph g = node as Graph;

                    GraphParameterEditor inp = new GraphParameterEditor(g, g.Parameters);
                    Stack.Children.Add(inp);
                    elementLookup[name] = inp;
                }
            }
            //handle special custom parameter editing
            else if(pe != null)
            {
                if(node is Graph && !(node is FunctionGraph))
                {
                    Graph g = node as Graph;
                    CustomParameterEditor inp = new CustomParameterEditor(g);
                    Stack.Children.Add(inp);
                    elementLookup[name] = inp;
                }
            }
            else if(fe != null)
            {
                if(node is Graph && !(node is FunctionGraph))
                {
                    Graph g = node as Graph;
                    CustomFunctionEditor inp = new CustomFunctionEditor(g);
                    Stack.Children.Add(inp);
                    elementLookup[name] = inp;
                }
            }
            //for graph instance exposed parameters from underlying graph
            else if (pme != null)
            {
                if (node is GraphInstanceNode)
                {
                    GraphInstanceNode gin = node as GraphInstanceNode;

                    if (p.PropertyType.Equals(typeof(List<GraphParameterValue>)))
                    {
                        ParameterMap pm = new ParameterMap(gin.CustomParameters);
                        Stack.Children.Add(pm);
                        elementLookup[name] = pm;
                    }
                    else
                    {
                        ParameterMap pm = new ParameterMap(gin.Parameters);
                        Stack.Children.Add(pm);
                        elementLookup[name] = pm;
                    }
                }
            }

            string title = name;

            if (ti != null)
            {
                title = ti.Title;
            }

            PropertyInfo op = null;

            //we don't create an element for this one
            //as it is hidden
            if (hp != null)
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

            if(t.Equals(typeof(Gradient)))
            {
                var l = new PropertyLabel();
                l.Title = title;

                labels.Add(l);
                Stack.Children.Add(l);

                GradientEditor inp = new GradientEditor(p, node);
                Stack.Children.Add(inp);
                elementLookup[name] = inp;
            }
            else if (t.Equals(typeof(MVector)))
            {
                if (cp != null)
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

                    ColorSelect cs = new ColorSelect(p, node);
                    Stack.Children.Add(cs);
                    elementLookup[name] = cs;
                }
            }
            else if (t.Equals(typeof(string[])))
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
            else if (t.Equals(typeof(bool)))
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
            else if (t.Equals(typeof(string)))
            {
                if (tinp != null)
                {
                    PropertyLabel l = new PropertyLabel();
                    l.Title = title;
                    labels.Add(l);
                    Stack.Children.Add(l);

                    PropertyInput ip = new PropertyInput(p, node);
                    Stack.Children.Add(ip);
                    elementLookup[name] = ip;
                }
                else if (fsl != null)
                {
                    PropertyLabel l = new PropertyLabel();
                    l.Title = title;
                    labels.Add(l);
                    Stack.Children.Add(l);

                    FileSelector inp = new FileSelector(p, node, fsl.Filter);
                    Stack.Children.Add(inp);
                    elementLookup[name] = inp;
                }
                else if (dp != null)
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
            else if (t.Equals(typeof(float)))
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
            else if (t.Equals(typeof(int)))
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
                else if (sl != null)
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
            else if (t.Equals(typeof(MultiRange)))
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
            else if (op != null && ce != null)
            {
                UICurves cv = new UICurves(p, op, node);
                Stack.Children.Add(cv);
                elementLookup[name] = cv;
            }
            else if (t.IsEnum)
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
    }
}
