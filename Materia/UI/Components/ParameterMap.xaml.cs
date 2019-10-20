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
using System.Reflection;
using Materia.Nodes.Attributes;
using Materia.Nodes.Atomic;
using OpenTK;
using Materia.MathHelpers;
using NLog;
using System.Resources;

namespace Materia.UI.Components
{
    /// <summary>
    /// Interaction logic for ParameterMap.xaml
    /// </summary>
    public partial class ParameterMap : UserControl
    {
        private static ILogger Log = LogManager.GetCurrentClassLogger();

        public ParameterMap()
        {
            InitializeComponent();
        }

        public void Clear()
        {
            Stack.Children.Clear();
        }

        public ParameterMap(object n, string[] ignore = null,  bool showLabel = true, bool inlinePropertyLabels = false)
        {
            InitializeComponent();
            Set(n, ignore, showLabel, inlinePropertyLabels);
        }

        public ParameterMap(Node n, List<GraphParameterValue> values, bool showLabel = true, bool inlinePropertyLabels = false)
        {
            InitializeComponent();
            Set(n, values, showLabel, inlinePropertyLabels);
        }

        public ParameterMap(Graph g, Dictionary<string, GraphParameterValue> values, bool showLabel = true, bool inlinePropertyLabels = false)
        {
            InitializeComponent();
            Set(g, values, showLabel, inlinePropertyLabels);
        }

        ParameterInputType GetParamInputType(GraphParameterValue v)
        {
            ParameterInputType ptype = v.InputType;

            if (ptype == ParameterInputType.FloatInput && v.Type == NodeType.Float2)
            {
                ptype = ParameterInputType.Float2Input;
            }
            else if (ptype == ParameterInputType.FloatSlider && v.Type == NodeType.Float2)
            {
                ptype = ParameterInputType.Float2Slider;
            }
            else if (ptype == ParameterInputType.FloatInput && v.Type == NodeType.Float3)
            {
                ptype = ParameterInputType.Float3Input;
            }
            else if (ptype == ParameterInputType.FloatSlider && v.Type == NodeType.Float3)
            {
                ptype = ParameterInputType.Float3Slider;
            }
            else if (ptype == ParameterInputType.FloatInput && v.Type == NodeType.Float4)
            {
                ptype = ParameterInputType.Float4Input;
            }
            else if (ptype == ParameterInputType.FloatSlider && v.Type == NodeType.Float4)
            {
                ptype = ParameterInputType.Float4Slider;
            }
            else if (ptype == ParameterInputType.Color && v.Type == NodeType.Float)
            {
                ptype = ParameterInputType.FloatInput;
            }
            else if (ptype == ParameterInputType.Color && v.Type == NodeType.Bool)
            {
                ptype = ParameterInputType.Toggle;
            }
            else if (ptype == ParameterInputType.IntInput && v.Type == NodeType.Float2)
            {
                ptype = ParameterInputType.Int2Input;
            }
            else if (ptype == ParameterInputType.IntSlider && v.Type == NodeType.Float2)
            {
                ptype = ParameterInputType.Int2Slider;
            }
            else if (ptype == ParameterInputType.IntInput && v.Type == NodeType.Float3)
            {
                ptype = ParameterInputType.Int3Input;
            }
            else if (ptype == ParameterInputType.IntSlider && v.Type == NodeType.Float3)
            {
                ptype = ParameterInputType.Int3Slider;
            }
            else if (ptype == ParameterInputType.IntInput && v.Type == NodeType.Float4)
            {
                ptype = ParameterInputType.Int4Input;
            }
            else if (ptype == ParameterInputType.IntSlider && v.Type == NodeType.Float4)
            {
                ptype = ParameterInputType.Int4Slider;
            }

            return ptype;
        }

        public void Set(object n, string[] ignore = null, bool showLabel = true, bool inlinePropertyLabels = false)
        {
            Clear();

            PropertyInfo[] infos = n.GetType().GetProperties();

            Dictionary<string, List<Tuple<PropertyInfo, EditableAttribute>>> sorter = new Dictionary<string, List<Tuple<PropertyInfo, EditableAttribute>>>();
            for (int i = 0; i < infos.Length; i++)
            {
                EditableAttribute ed = infos[i].GetCustomAttribute<EditableAttribute>();
                if (ed != null)
                {
                    if (ignore != null && Array.IndexOf(ignore, infos[i].Name) > -1)
                    {
                        continue;
                    }

                    List<Tuple<PropertyInfo, EditableAttribute>> items = null;
                    string sect = ed.Section;

                    if (string.IsNullOrEmpty(sect))
                    {
                        sect = Properties.Resources.GRAPH_Default;
                    }
                    else
                    {
                        //convert the section name to localized string
                        var rm = Properties.Resources.ResourceManager;
                        string key = "GRAPH_" + sect.Replace(" ", "_");

                        try
                        {
                            string loc = rm.GetString(key);
                            if (!string.IsNullOrEmpty(loc))
                            {
                                sect = loc;
                            }
                        }
                        catch (Exception e)
                        {

                        }
                    }

                    if (sorter.TryGetValue(sect, out items))
                    {
                        items.Add(new Tuple<PropertyInfo, EditableAttribute>(infos[i], ed));
                    }
                    else
                    {
                        items = new List<Tuple<PropertyInfo, EditableAttribute>>();
                        items.Add(new Tuple<PropertyInfo, EditableAttribute>(infos[i], ed));
                        sorter[sect] = items;
                    }
                }
            }

            List<string> keys = sorter.Keys.ToList();
            keys.Sort();

            foreach (string k in keys)
            {
                List<Tuple<PropertyInfo, EditableAttribute>> items = sorter[k];

                if (showLabel)
                {
                    PropertySection sect = new PropertySection();
                    sect.Title = k;

                    if (!Stack.Children.Contains(sect) && sect.Parent == null)
                    {
                        Stack.Children.Add(sect);
                    }

                    foreach (var t in items)
                    {
                        string name = t.Item2.Name;
                        if (string.IsNullOrEmpty(name))
                        {
                            name = t.Item1.Name;
                        }

                        //try and convert name to localized string
                        //convert the section name to localized string
                        var rm = Properties.Resources.ResourceManager;
                        string key = "GRAPH_" + name.Replace(" ", "_");

                        try
                        {
                            string loc = rm.GetString(key);
                            if (!string.IsNullOrEmpty(loc))
                            {
                                name = loc;
                            }
                        }
                        catch (Exception e)
                        {

                        }

                        UIElement ele = BuildParamater(n, t.Item2, t.Item1);
                        if (ele != null)
                        {
                            StackPanelAuto inlinePanel = new StackPanelAuto();
                            inlinePanel.Direction = Orientation.Horizontal;
                            inlinePanel.HalfAndHalf = true;

                            //special case to eliminate the labels for 
                            //Parameter, CustomParameters, and CustomFunctions
                            if (!(n is GraphInstanceNode && t.Item1.Name.Equals("Parameters"))
                               && !(n is GraphInstanceNode && t.Item1.Name.Equals("CustomParameters"))
                               && !(n is Graph && t.Item1.Name.Equals("CustomParameters"))
                               && !(n is Graph && t.Item1.Name.Equals("Parameters"))
                               && !(n is Graph && t.Item1.Name.Equals("CustomFunctions"))
                               && !(n is Graph && t.Item1.Name.Equals("ParameterFunctions")))
                            {
                                if (n is Node && t.Item1.GetCustomAttribute<PromoteAttribute>() != null)
                                {
                                    PropertyLabel pl = new PropertyLabel(name, n as Node, t.Item1.Name);
                                    if (!inlinePropertyLabels)
                                    {
                                        sect.Add(pl);
                                    }
                                    else
                                    {
                                        inlinePanel.Children.Add(pl);
                                    }
                                }
                                else
                                {
                                    PropertyLabel pl = new PropertyLabel(name, null, t.Item1.Name);
                                    if (!inlinePropertyLabels)
                                    {
                                        sect.Add(pl);
                                    }
                                    else
                                    {
                                        inlinePanel.Children.Add(pl);
                                    }
                                }
                            }

                            if (!inlinePropertyLabels)
                            {
                                sect.Add(ele);
                            }
                            else
                            {
                                inlinePanel.Children.Add(ele);
                                sect.Add(inlinePanel);
                            }
                        }
                    }
                }
                else
                {
                    foreach(var t in items)
                    {
                        string name = t.Item2.Name;
                        if (string.IsNullOrEmpty(name))
                        {
                            name = t.Item1.Name;
                        }

                        //try and convert name to localized string
                        //convert the section name to localized string
                        var rm = Properties.Resources.ResourceManager;
                        string key = "GRAPH_" + name.Replace(" ", "_");

                        try
                        {
                            string loc = rm.GetString(key);
                            if (!string.IsNullOrEmpty(loc))
                            {
                                name = loc;
                            }
                        }
                        catch (Exception e)
                        {

                        }

                        UIElement ele = BuildParamater(n, t.Item2, t.Item1);
                        if (ele != null)
                        {
                            StackPanelAuto inlinePanel = new StackPanelAuto();
                            inlinePanel.Direction = Orientation.Horizontal;
                            inlinePanel.HalfAndHalf = true;

                            //special case to eliminate the labels for 
                            //Parameter, CustomParameters, and CustomFunctions
                            if (!(n is GraphInstanceNode && t.Item1.Name.Equals("Parameters"))
                                && !(n is GraphInstanceNode && t.Item1.Name.Equals("CustomParameters"))
                                && !(n is Graph && t.Item1.Name.Equals("CustomParameters"))
                                && !(n is Graph && t.Item1.Name.Equals("Parameters"))
                                && !(n is Graph && t.Item1.Name.Equals("CustomFunctions"))
                                && !(n is Graph && t.Item1.Name.Equals("ParameterFunctions")))
                            { 
                                if (n is Node)
                                {
                                    PropertyLabel pl = new PropertyLabel(name, n as Node, t.Item1.Name);
                                    if (!inlinePropertyLabels)
                                    {
                                        Stack.Children.Add(pl);
                                    }
                                    else
                                    {
                                        inlinePanel.Children.Add(pl);
                                    }
                                }
                                else
                                {
                                    PropertyLabel pl = new PropertyLabel(name, null, t.Item1.Name);
                                    if (!inlinePropertyLabels)
                                    {
                                        Stack.Children.Add(pl);
                                    }
                                    else
                                    {
                                        inlinePanel.Children.Add(pl);
                                    }
                                }
                            }

                            if (!inlinePropertyLabels)
                            {
                                Stack.Children.Add(ele);
                            }
                            else
                            {
                                inlinePanel.Children.Add(ele);
                                Stack.Children.Add(inlinePanel);
                            }
                        }
                    }
                }
            }
        }

        public void Set(Node n, List<GraphParameterValue> values, bool showLabel = true, bool inlinePropertyLabels = false)
        {
            Clear();
            Dictionary<string, List<GraphParameterValue>> sorter = new Dictionary<string, List<GraphParameterValue>>();
            //create a copy
            foreach (var p in values)
            {
                if (p.IsFunction()) continue;

                string sec = p.Section;
                if (string.IsNullOrEmpty(sec))
                {
                    sec = Properties.Resources.GRAPH_Default;
                }

                //try and convert name to localized string
                //convert the section name to localized string
                var rm = Properties.Resources.ResourceManager;
                string key = "GRAPH_" + sec.Replace(" ", "_");

                try
                {
                    string loc = rm.GetString(key);
                    if (!string.IsNullOrEmpty(loc))
                    {
                        sec = loc;
                    }
                }
                catch (Exception e)
                {

                }

                List<GraphParameterValue> items = null;
                if (sorter.TryGetValue(sec, out items))
                {
                    items.Add(p);
                }
                else
                {
                    items = new List<GraphParameterValue>();
                    items.Add(p);
                    sorter[sec] = items;
                }
            }

            List<string> keys = sorter.Keys.ToList();
            keys.Sort();

            foreach (string k in keys)
            {
                List<GraphParameterValue> items = sorter[k];
                if (showLabel)
                {
                    PropertySection sect = new PropertySection();
                    sect.Title = k;

                    if (!Stack.Children.Contains(sect) && sect.Parent == null)
                    {
                        Stack.Children.Add(sect);
                    }

                    foreach (var v in items)
                    {
                        EditableAttribute ed = new EditableAttribute(GetParamInputType(v), v.Name, v.Section, v.Min, v.Max);
                        UIElement ele = BuildParamater(v, ed, v.GetType().GetProperty("Value"));

                        if (ele != null)
                        {
                            string name = v.Name;
                            //try and convert name to localized string
                            //convert the section name to localized string
                            var rm = Properties.Resources.ResourceManager;
                            string key = "GRAPH_" + name.Replace(" ", "_");

                            try
                            {
                                string loc = rm.GetString(key);
                                if (!string.IsNullOrEmpty(loc))
                                {
                                    name = loc;
                                }
                            }
                            catch (Exception e)
                            {

                            }


                            PropertyLabel lbl = new PropertyLabel(name, n, "$Custom." + v.Name);

                            if (!inlinePropertyLabels)
                            {
                                sect.Add(lbl);
                                sect.Add(ele);
                            }
                            else
                            {
                                StackPanelAuto inlinePanel = new StackPanelAuto();
                                inlinePanel.Direction = Orientation.Horizontal;
                                inlinePanel.HalfAndHalf = true;
                                inlinePanel.Children.Add(lbl);
                                inlinePanel.Children.Add(ele);
                                sect.Add(inlinePanel);
                            }
                        }
                    }
                }
                else
                {
                    foreach(var v in items)
                    {
                        EditableAttribute ed = new EditableAttribute(GetParamInputType(v), v.Name, v.Section, v.Min, v.Max);
                        UIElement ele = BuildParamater(v, ed, v.GetType().GetProperty("Value"));

                        if (ele != null)
                        {
                            string name = v.Name;

                            //try and convert name to localized string
                            //convert the section name to localized string
                            var rm = Properties.Resources.ResourceManager;
                            string key = "GRAPH_" + name.Replace(" ", "_");

                            try
                            {
                                string loc = rm.GetString(key);
                                if (!string.IsNullOrEmpty(loc))
                                {
                                    name = loc;
                                }
                            }
                            catch (Exception e)
                            {

                            }

                            PropertyLabel lbl = new PropertyLabel(name, n, "$Custom." + v.Name);

                            if (!inlinePropertyLabels)
                            {
                                Stack.Children.Add(lbl);
                                Stack.Children.Add(ele);
                            }
                            else
                            {
                                StackPanelAuto inlinePanel = new StackPanelAuto();
                                inlinePanel.Direction = Orientation.Horizontal;
                                inlinePanel.HalfAndHalf = true;
                                inlinePanel.Children.Add(lbl);
                                inlinePanel.Children.Add(ele);
                                Stack.Children.Add(inlinePanel);
                            }
                        }
                    }
                }
            }
        }

        public void Set(Graph g, Dictionary<string, GraphParameterValue> values, bool showLabel = true, bool inlinePropertyLabels = false)
        {
            Clear();
            Dictionary<string, List<Tuple<PropertyLabel, UIElement>>> sorter = new Dictionary<string, List<Tuple<PropertyLabel, UIElement>>>();

            foreach (var k in values.Keys)
            {
                var v = values[k];

                if (v.IsFunction()) continue;

                string[] split = k.Split('.');

                var n = g.FindSubNodeById(split[0]);

                PropertyInfo nodeInfo = null;

                string customHeader = "";

                if (n != null)
                {
                    nodeInfo = n.GetType().GetProperty(split[1]);

                    if (nodeInfo == null && n is GraphInstanceNode)
                    {
                        //then it might be an underling custom parameter on the node
                        GraphInstanceNode inst = n as GraphInstanceNode;

                        var realParam = inst.GetCustomParameter(split[1]);

                        if (realParam != null)
                        {
                            //initiate custom header
                            //for proper underlying processing
                            //on the label
                            customHeader = "$Custom.";
                            //just set the parameter inputtype the same
                            //also ensure min and max are the same
                            v.InputType = realParam.InputType;
                            v.Max = realParam.Max;
                            v.Min = realParam.Min;
                            v.Section = realParam.Section;
                        }
                    }
                }

                //try and convert name to localized string
                //convert the section name to localized string
                string vname = v.Name;
                var rm = Properties.Resources.ResourceManager;
                string key = "GRAPH_" + vname.Replace(" ", "_");

                try
                {
                    string loc = rm.GetString(key);
                    if (!string.IsNullOrEmpty(loc))
                    {
                        vname = loc;
                    }
                }
                catch (Exception e)
                {

                }

                PropertyLabel lbl = new PropertyLabel(vname, n, customHeader + split[1]);
                EditableAttribute ed = null;
                if(nodeInfo != null)
                {
                    ed = nodeInfo.GetCustomAttribute<EditableAttribute>();
                }
                if (ed == null)
                {
                    ed = new EditableAttribute(GetParamInputType(v), v.Name, v.Section, v.Min, v.Max);
                }

                UIElement ele = BuildParamater(v, ed, v.GetType().GetProperty("Value"), nodeInfo);

                if (ele != null)
                {
                    string inherit = "";
                    if(g != null && g.ParentNode is GraphInstanceNode)
                    {
                        inherit = Properties.Resources.GRAPH_Inherited + " ";
                    }

                    string sec = v.Section;
                    key = "GRAPH_" + sec.Replace(" ", "_");

                    try
                    {
                        string loc = rm.GetString(key);
                        if (!string.IsNullOrEmpty(loc))
                        {
                            sec = loc;
                        }
                    }
                    catch (Exception e)
                    {

                    }

                    string sect = inherit + sec;
                    if (string.IsNullOrEmpty(v.Section))
                    {
                        sect = inherit + Properties.Resources.GRAPH_Default;
                    }

                    List<Tuple<PropertyLabel, UIElement>> items = null;
                    if (sorter.TryGetValue(sect, out items))
                    {
                        items.Add(new Tuple<PropertyLabel, UIElement>(lbl, ele));
                    }
                    else
                    {
                        items = new List<Tuple<PropertyLabel, UIElement>>();
                        items.Add(new Tuple<PropertyLabel, UIElement>(lbl, ele));
                        sorter[sect] = items;
                    }
                }
            }

            List<string> keys = sorter.Keys.ToList();
            keys.Sort();

            foreach (string k in keys)
            {
                List<Tuple<PropertyLabel, UIElement>> items = sorter[k];

                if (showLabel)
                {
                    PropertySection sect = new PropertySection();
                    sect.Title = k;

                    if (!Stack.Children.Contains(sect) && sect.Parent == null)
                    {
                        Stack.Children.Add(sect);
                    }

                    foreach (var t in items)
                    {
                        if (!inlinePropertyLabels)
                        {
                            sect.Add(t.Item1);
                            sect.Add(t.Item2);
                        }
                        else
                        {
                            StackPanelAuto inlinePanel = new StackPanelAuto();
                            inlinePanel.Direction = Orientation.Horizontal;
                            inlinePanel.HalfAndHalf = true;
                            inlinePanel.Children.Add(t.Item1);
                            inlinePanel.Children.Add(t.Item2);
                            sect.Add(inlinePanel);
                        }
                    }
                }
                else
                {
                    foreach (var t in items)
                    {
                        if (!inlinePropertyLabels)
                        {
                            Stack.Children.Add(t.Item1);
                            Stack.Children.Add(t.Item2);
                        }
                        else
                        {
                            StackPanelAuto inlinePanel = new StackPanelAuto();
                            inlinePanel.Direction = Orientation.Horizontal;
                            inlinePanel.HalfAndHalf = true;
                            inlinePanel.Children.Add(t.Item1);
                            inlinePanel.Children.Add(t.Item2);
                            Stack.Children.Add(inlinePanel);
                        }
                    }
                }
            }
        }

        protected UIElement BuildParamater(object owner, EditableAttribute edit, PropertyInfo v, PropertyInfo template = null)
        {
            if(v == null || owner == null || edit == null)
            {
                return null;
            }

            if(template == null)
            {
                template = v;
            }

            switch(edit.Type)
            {
                case ParameterInputType.FloatInput:
                    NumberInput nfp = new NumberInput();
                    nfp.Set(NumberInputType.Float, owner, v);
                    return nfp;
                case ParameterInputType.FloatSlider:
                    NumberSlider nfs = new NumberSlider();
                    nfs.Ticks = edit.Ticks;
                    nfs.Set(edit.Min, edit.Max, v, owner);
                    return nfs;
                case ParameterInputType.IntInput:
                    NumberInput nip = new NumberInput();
                    nip.Set(NumberInputType.Int, owner, v);
                    return nip;
                case ParameterInputType.IntSlider:
                    NumberSlider nis = new NumberSlider();
                    nis.Ticks = edit.Ticks;
                    nis.IsInt = true;
                    nis.Set(edit.Min, edit.Max, v, owner);
                    return nis;
                case ParameterInputType.Dropdown:
                    DropdownAttribute da = template.GetCustomAttribute<DropdownAttribute>();
                    object[] dvalues = null;
                    if(template.PropertyType.IsEnum)
                    {
                        dvalues = Enum.GetNames(template.PropertyType);
                    }
                    else if(v.PropertyType.Equals(typeof(string[])))
                    {
                        dvalues = (string[])v.GetValue(owner);
                    }
                    if (da != null && da.Values != null && da.Values.Length > 0)
                    {
                        dvalues = da.Values;
                    }
                    if(da != null)
                    {
                        return new DropDown(dvalues, owner, v, da.OutputProperty, da.IsEditable);
                    }

                    return new DropDown(dvalues, owner, v);
                case ParameterInputType.Color:
                    return new ColorSelect(v, owner);
                case ParameterInputType.Curves:
                    return new UICurves(v, owner);
                case ParameterInputType.Levels:
                    Imaging.RawBitmap raw = null;
                    if (owner is ImageNode)
                    {
                        byte[] bits = (owner as ImageNode).GetPreview(256, 256);
                        if (bits != null)
                        {
                            raw = new Imaging.RawBitmap(256, 256, bits);
                        }
                    }
                    return new UILevels(raw, owner, v);
                case ParameterInputType.Map:
                    object mo = v.GetValue(owner);
                    if(mo is Dictionary<string, GraphParameterValue> && owner is GraphInstanceNode)
                    {
                        return new ParameterMap((owner as GraphInstanceNode).GraphInst, mo as Dictionary<string, GraphParameterValue>);
                    }
                    else if(mo is List<GraphParameterValue> && owner is Node)
                    {
                        return new ParameterMap(owner as Node, mo as List<GraphParameterValue>);
                    }
                    else if(mo is List<GraphParameterValue>)
                    {
                        return new ParameterMap(null, mo as List<GraphParameterValue>);
                    }
                    return null;
                case ParameterInputType.MapEdit:
                    object meo = v.GetValue(owner);
                    if(meo is Dictionary<string, GraphParameterValue> && owner is Graph && !(owner is FunctionGraph))
                    {
                        return new GraphParameterEditor(owner as Graph, meo as Dictionary<string, GraphParameterValue>);
                    }
                    else if(meo is List<GraphParameterValue> && owner is Graph && !(owner is FunctionGraph))
                    {
                        return new CustomParameterEditor(owner as Graph);
                    }
                    else if(meo is List<FunctionGraph> && owner is Graph && !(owner is FunctionGraph))
                    {
                        return new CustomFunctionEditor(owner as Graph);
                    }
                    else if(meo is  Dictionary<string, FunctionGraph> && !(owner is FunctionGraph))
                    {
                        return new GraphFunctionEditor(owner as Graph);
                    }
                    return null;
                case ParameterInputType.MeshFile:
                    return new FileSelector(v, owner, "Mesh Files|*.fbx;*.obj");
                case ParameterInputType.ImageFile:
                    return new FileSelector(v, owner, "Image Files|*.png;*.jpg;*.tif;*.bmp;*.jpeg");
                case ParameterInputType.GraphFile:
                    return new FileSelector(v, owner, "Materia Graph|*.mtg");
                case ParameterInputType.Text:
                    return new PropertyInput(v, owner, template.GetCustomAttribute<ReadOnlyAttribute>() != null);
                case ParameterInputType.MultiText:
                    return new PropertyInput(v, owner, template.GetCustomAttribute<ReadOnlyAttribute>() != null, true);
                case ParameterInputType.Toggle:
                    return new ToggleControl(edit.Name, v, owner);
                case ParameterInputType.Gradient:
                    return new GradientEditor(v, owner);
                case ParameterInputType.Float2Input:
                    return new VectorInput(v, owner, NodeType.Float2);
                case ParameterInputType.Float2Slider:
                    return new VectorSlider(v, owner, edit.Min, edit.Max, NodeType.Float2);
                case ParameterInputType.Float3Input:
                    return new VectorInput(v, owner, NodeType.Float3);
                case ParameterInputType.Float3Slider:
                    return new VectorSlider(v, owner, edit.Min, edit.Max, NodeType.Float3);
                case ParameterInputType.Float4Input:
                    return new VectorInput(v, owner, NodeType.Float4);
                case ParameterInputType.Float4Slider:
                    return new VectorSlider(v, owner, edit.Min, edit.Max, NodeType.Float4);
                case ParameterInputType.Int2Input:
                    return new VectorInput(v, owner, NodeType.Float2, NumberInputType.Int);
                case ParameterInputType.Int2Slider:
                    return new VectorSlider(v, owner, edit.Min, edit.Max, NodeType.Float2, true);
                case ParameterInputType.Int3Input:
                    return new VectorInput(v, owner, NodeType.Float3, NumberInputType.Int);
                case ParameterInputType.Int3Slider:
                    return new VectorSlider(v, owner, edit.Min, edit.Max, NodeType.Float3, true);
                case ParameterInputType.Int4Input:
                    return new VectorInput(v, owner, NodeType.Float4, NumberInputType.Int);
                case ParameterInputType.Int4Slider:
                    return new VectorSlider(v, owner, edit.Min, edit.Max, NodeType.Float4, true);
            }

            return null;
        }
    }
}
