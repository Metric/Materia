using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Materia.Nodes;
using Materia.Graph;
using MLog;
using Materia.Rendering.Attributes;
using MateriaCore.Localization;
using System.Reflection;
using System;
using System.Linq;
using Materia.Nodes.Atomic;
using Avalonia.Layout;
using MateriaCore.Utils;
using Materia.Graph.Exporters;

namespace MateriaCore.Components
{
    public class ParameterMap : UserControl
    {
        StackPanel stack;

        static Local local = new Local();

        public ParameterMap()
        {
            this.InitializeComponent();
        }

        public ParameterMap(object n, string[] ignore = null, bool showLabel = true, bool inlinePropertyLabels = false) : this()
        {
            Set(n, ignore, showLabel, inlinePropertyLabels);
        }

        public ParameterMap(Node n, List<ParameterValue> values, bool showLabel = true, bool inlinePropertyLabels = false) : this()
        {
            Set(n, values, showLabel, inlinePropertyLabels);
        }

        public ParameterMap(Graph g, Dictionary<string, ParameterValue> values, bool showLabel = true, bool inlinePropertyLabels = false) : this()
        {
            Set(g, values, showLabel, inlinePropertyLabels);
        }

        ParameterInputType GetParamType(ParameterValue v)
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
            for (int i = 0; i < infos.Length; ++i)
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
                        sect = local.Get("Default");
                    }
                    else
                    {
                        //convert the section name to localized string
                        string key = sect.Replace(" ", "");

                        try
                        {
                            string loc = local.Get(key);
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

                    if (!stack.Children.Contains(sect) && sect.Parent == null)
                    {
                        stack.Children.Add(sect);
                    }

                    foreach (var t in items)
                    {
                        string name = t.Item2.Name;
                        if (string.IsNullOrEmpty(name))
                        {
                            name = t.Item1.Name;
                        }


                        string key = name.Replace(" ", "");

                        try
                        {
                            string loc = local.Get(key);
                            if (!string.IsNullOrEmpty(loc))
                            {
                                name = loc;
                            }
                        }
                        catch (Exception e)
                        {

                        }

                        Control ele = BuildParamater(n, t.Item2, t.Item1);
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
                    foreach (var t in items)
                    {
                        string name = t.Item2.Name;
                        if (string.IsNullOrEmpty(name))
                        {
                            name = t.Item1.Name;
                        }

       
                        string key = name.Replace(" ", "");

                        try
                        {
                            string loc = local.Get(key);
                            if (!string.IsNullOrEmpty(loc))
                            {
                                name = loc;
                            }
                        }
                        catch (Exception e)
                        {

                        }

                        Control ele = BuildParamater(n, t.Item2, t.Item1);
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
                                        stack.Children.Add(pl);
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
                                        stack.Children.Add(pl);
                                    }
                                    else
                                    {
                                        inlinePanel.Children.Add(pl);
                                    }
                                }
                            }

                            if (!inlinePropertyLabels)
                            {
                                stack.Children.Add(ele);
                            }
                            else
                            {
                                inlinePanel.Children.Add(ele);
                                stack.Children.Add(inlinePanel);
                            }
                        }
                    }
                }
            }
        }

        public void Set(Node n, List<ParameterValue> values, bool showLabel = true, bool inlinePropertyLabels = false)
        {
            Clear();
            Dictionary<string, List<ParameterValue>> sorter = new Dictionary<string, List<ParameterValue>>();
            //create a copy
            foreach (var p in values)
            {
                if (p.IsFunction()) continue;

                string sec = p.Section;
                if (string.IsNullOrEmpty(sec))
                {
                    sec = local.Get("Default");
                }


                string key = sec.Replace(" ", "");

                try
                {
                    string loc = local.Get(key);
                    if (!string.IsNullOrEmpty(loc))
                    {
                        sec = loc;
                    }
                }
                catch (Exception e)
                {

                }

                List<ParameterValue> items = null;
                if (sorter.TryGetValue(sec, out items))
                {
                    items.Add(p);
                }
                else
                {
                    items = new List<ParameterValue>();
                    items.Add(p);
                    sorter[sec] = items;
                }
            }

            List<string> keys = sorter.Keys.ToList();
            keys.Sort();

            foreach (string k in keys)
            {
                List<ParameterValue> items = sorter[k];
                if (showLabel)
                {
                    PropertySection sect = new PropertySection();
                    sect.Title = k;

                    if (!stack.Children.Contains(sect) && sect.Parent == null)
                    {
                        stack.Children.Add(sect);
                    }

                    foreach (var v in items)
                    {
                        EditableAttribute ed = new EditableAttribute(GetParamType(v), v.Name, v.Section, v.Min, v.Max);
                        Control ele = BuildParamater(v, ed, v.GetType().GetProperty("Value"));

                        if (ele != null)
                        {
                            string name = v.Name;
                            string key = name.Replace(" ", "");

                            try
                            {
                                string loc = local.Get(key);
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
                    foreach (var v in items)
                    {
                        EditableAttribute ed = new EditableAttribute(GetParamType(v), v.Name, v.Section, v.Min, v.Max);
                        Control ele = BuildParamater(v, ed, v.GetType().GetProperty("Value"));

                        if (ele != null)
                        {
                            string name = v.Name;
                            string key = name.Replace(" ", "");

                            try
                            {
                                string loc = local.Get(key);
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
                                stack.Children.Add(lbl);
                                stack.Children.Add(ele);
                            }
                            else
                            {
                                StackPanelAuto inlinePanel = new StackPanelAuto();
                                inlinePanel.Direction = Orientation.Horizontal;
                                inlinePanel.HalfAndHalf = true;
                                inlinePanel.Children.Add(lbl);
                                inlinePanel.Children.Add(ele);
                                stack.Children.Add(inlinePanel);
                            }
                        }
                    }
                }
            }
        }

        public void Set(Graph g, Dictionary<string, ParameterValue> values, bool showLabel = true, bool inlinePropertyLabels = false)
        {
            Clear();
            Dictionary<string, List<Tuple<PropertyLabel, Control>>> sorter = new Dictionary<string, List<Tuple<PropertyLabel, Control>>>();

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
                string key = vname.Replace(" ", "");

                try
                {
                    string loc = local.Get(key);
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
                if (nodeInfo != null)
                {
                    ed = nodeInfo.GetCustomAttribute<EditableAttribute>();
                }
                if (ed == null)
                {
                    ed = new EditableAttribute(GetParamType(v), v.Name, v.Section, v.Min, v.Max);
                }

                Control ele = BuildParamater(v, ed, v.GetType().GetProperty("Value"), nodeInfo);

                if (ele != null)
                {
                    string inherit = "";
                    if (g != null && g.ParentNode is GraphInstanceNode)
                    {
                        inherit = local.Get("Inherited") + " ";
                    }

                    string sec = v.Section;
                    key = sec.Replace(" ", "");

                    try
                    {
                        string loc = local.Get(key);
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
                        sect = inherit + local.Get("Default");
                    }

                    List<Tuple<PropertyLabel, Control>> items = null;
                    if (sorter.TryGetValue(sect, out items))
                    {
                        items.Add(new Tuple<PropertyLabel, Control>(lbl, ele));
                    }
                    else
                    {
                        items = new List<Tuple<PropertyLabel, Control>>();
                        items.Add(new Tuple<PropertyLabel, Control>(lbl, ele));
                        sorter[sect] = items;
                    }
                }
            }

            List<string> keys = sorter.Keys.ToList();
            keys.Sort();

            foreach (string k in keys)
            {
                List<Tuple<PropertyLabel, Control>> items = sorter[k];

                if (showLabel)
                {
                    PropertySection sect = new PropertySection();
                    sect.Title = k;

                    if (!stack.Children.Contains(sect) && sect.Parent == null)
                    {
                        stack.Children.Add(sect);
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
                            stack.Children.Add(t.Item1);
                            stack.Children.Add(t.Item2);
                        }
                        else
                        {
                            StackPanelAuto inlinePanel = new StackPanelAuto();
                            inlinePanel.Direction = Orientation.Horizontal;
                            inlinePanel.HalfAndHalf = true;
                            inlinePanel.Children.Add(t.Item1);
                            inlinePanel.Children.Add(t.Item2);
                            stack.Children.Add(inlinePanel);
                        }
                    }
                }
            }
        }

        protected Control BuildParamater(object owner, EditableAttribute edit, PropertyInfo v, PropertyInfo template = null)
        {
            if (v == null || owner == null || edit == null)
            {
                return null;
            }

            if (template == null)
            {
                template = v;
            }

            switch (edit.Type)
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
                    if (template.PropertyType.IsEnum)
                    {
                        dvalues = Enum.GetNames(template.PropertyType);
                    }
                    else if (v.PropertyType.Equals(typeof(string[])))
                    {
                        dvalues = (string[])v.GetValue(owner);
                    }
                    if (da != null && da.Values != null && da.Values.Length > 0)
                    {
                        dvalues = da.Values;
                    }
                    if (da != null)
                    {
                        return new DropDown(dvalues, owner, v, da.OutputProperty, da.IsEditable);
                    }

                    return new DropDown(dvalues, owner, v);
                case ParameterInputType.Color:
                    return new ColorSelect(v, owner);
                case ParameterInputType.Curves:
                    return new Curves(v, owner);
                case ParameterInputType.Levels:
                    Levels levels = new Levels(null, owner, v);
                    if (owner is ImageNode)
                    {
                        GlobalEvents.Emit(GlobalEvent.ScheduleExport, this, new MemoryExporter(owner as ImageNode, 256, 256, (bits) =>
                        {
                            if (bits == null) return;
                            ImageNode n = owner as ImageNode;
                            Materia.Rendering.Imaging.RawBitmap raw = null;
                            raw = new Materia.Rendering.Imaging.RawBitmap(256, 256, bits);
                            levels.SetBitmap(raw);
                        }));
                    }
                    return levels;
                case ParameterInputType.Map:
                    object mo = v.GetValue(owner);
                    if (mo is Dictionary<string, ParameterValue> && owner is GraphInstanceNode)
                    {
                        return new ParameterMap((owner as GraphInstanceNode).GraphInst, mo as Dictionary<string, ParameterValue>);
                    }
                    else if (mo is List<ParameterValue> && owner is Node)
                    {
                        return new ParameterMap(owner as Node, mo as List<ParameterValue>);
                    }
                    else if (mo is List<ParameterValue>)
                    {
                        return new ParameterMap(null, mo as List<ParameterValue>);
                    }
                    return null;
                case ParameterInputType.MapEdit:
                    object meo = v.GetValue(owner);
                    if (meo is Dictionary<string, ParameterValue> && owner is Graph && !(owner is Function))
                    {
                        return new ParameterEditor(owner as Graph, meo as Dictionary<string, ParameterValue>);
                    }
                    else if (meo is List<ParameterValue> && owner is Graph && !(owner is Function))
                    {
                        return new ParameterEditor(owner as Graph, meo as List<ParameterValue>);
                    }
                    else if (meo is List<Function> && owner is Graph && !(owner is Function))
                    {
                        return new FunctionEditor(owner as Graph, meo as List<Function>);
                    }
                    else if (meo is Dictionary<string, Function> && !(owner is Function))
                    {
                        return new FunctionEditor(owner as Graph, meo as Dictionary<string, Function>);
                    }
                    return null;
                case ParameterInputType.MeshFile:
                    return new FileSelector(v, owner, "Mesh Files|fbx;obj");
                case ParameterInputType.ImageFile:
                    return new FileSelector(v, owner, "Image Files|png;jpg;tif;bmp;jpeg");
                case ParameterInputType.GraphFile:
                    return new FileSelector(v, owner, "Materia Graph|mtg;mtga");
                case ParameterInputType.Text:
                    return new TextInput(v, owner, template.GetCustomAttribute<ReadOnlyAttribute>() != null);
                case ParameterInputType.MultiText:
                    return new TextInput(v, owner, template.GetCustomAttribute<ReadOnlyAttribute>() != null, true);
                case ParameterInputType.Toggle:
                    return new Toggle(edit.Name, v, owner);
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

        public void Clear()
        {
            stack.Children.Clear();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            stack = this.FindControl<StackPanel>("Stack");
        }
    }
}
