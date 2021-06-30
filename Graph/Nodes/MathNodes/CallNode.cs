using System;
using System.Collections.Generic;
using Materia.Rendering.Attributes;
using Newtonsoft.Json;
using Materia.Graph;
using Materia.Graph.IO;

namespace Materia.Nodes.MathNodes
{
    public class CallNode : MathNode
    {
        protected int selectedIndex;

        public int SelectedIndex
        {
            get
            {
                return selectedIndex;
            }
            set
            {
                selectedIndex = value;

                Function g = parentGraph as Function;
                Graph.Graph p = g.ParentNode != null ? g.ParentNode.ParentGraph : g.ParentGraph;
                
                if (selectedIndex >= 0 && selectedIndex < p.CustomFunctions.Count)
                {
                    UnsubscribeFromArgs();

                    if(selectedFunction != null)
                    {
                        selectedFunction.OnOutputSet -= SelectedFunction_OnOutputSet;
                        selectedFunction.OnArgAdded -= SelectedFunction_OnArgAdded;
                        selectedFunction.OnArgRemoved -= SelectedFunction_OnArgRemoved;
                    }

                    selectedFunction = p.CustomFunctions[selectedIndex];
                    selectedName = selectedFunction.Name;
                    selectedFunction.OnOutputSet += SelectedFunction_OnOutputSet;
                    selectedFunction.OnArgAdded += SelectedFunction_OnArgAdded;
                    selectedFunction.OnArgRemoved += SelectedFunction_OnArgRemoved;
                        
                    SubscribeToArgs();

                    UpdateInputs();
                    TriggerValueChange();
                }
            }
        }

        private void SelectedFunction_OnArgRemoved(Node n)
        {
            n.OnValueUpdated -= Arg_OnValueUpdated;
            UpdateInputs();
        }

        private void SelectedFunction_OnArgAdded(Node n)
        {
            n.OnValueUpdated += Arg_OnValueUpdated;
            UpdateInputs();
        }

        private void SubscribeToArgs()
        {
            if (selectedFunction != null) {
                List<ArgNode> args = selectedFunction.Args;

                foreach(ArgNode n in args)
                {
                    n.OnValueUpdated += Arg_OnValueUpdated;
                }
            }
        }

        private void Arg_OnValueUpdated(Node n)
        {
            UpdateInputs();
        }

        private void UnsubscribeFromArgs()
        {
            if (selectedFunction != null)
            {
                List<ArgNode> args = selectedFunction.Args;

                foreach (ArgNode n in args)
                {
                    n.OnValueUpdated -= Arg_OnValueUpdated;
                }
            }
        }

        private void SelectedFunction_OnOutputSet(Node n)
        {
            UpdateInputs();
        }

        protected string[] function;

        [Dropdown("SelectedIndex")]
        [Editable(ParameterInputType.Dropdown, "Function")]
        public string[] Function
        {
            get
            {
                return function;
            }
        }

        protected string selectedName;
        public Function selectedFunction { get; protected set; }

        NodeOutput output;

        public CallNode(int w, int h, GraphPixelType p = GraphPixelType.RGBA) : base()
        {
            //we ignore w,h,p

            CanPreview = false;

            defaultName = Name = "Call";
 
            shaderId = "S" + Id.Split('-')[0];

            output = new NodeOutput(NodeType.Float | NodeType.Float2 | NodeType.Float3 | NodeType.Float4 | NodeType.Bool, this);
            Outputs.Add(output);
        }

        public override string GetDescription()
        {
            return selectedName;
        }

        protected void OnParentSet()
        {
            Function g = parentGraph as Function;

            if(g != null)
            {
                Graph.Graph p = g.ParentNode != null ? g.ParentNode.ParentGraph : g.ParentGraph;

                function = new string[p.CustomFunctions.Count];

                int i = 0;
                for (i = 0; i < p.CustomFunctions.Count; ++i)
                {
                    var f = p.CustomFunctions[i];

                    if (f.Name.Equals(selectedName) || string.IsNullOrEmpty(selectedName))
                    {
                        UnsubscribeFromArgs();

                        if(selectedFunction != null)
                        {
                            selectedFunction.OnOutputSet -= SelectedFunction_OnOutputSet;
                            selectedFunction.OnArgAdded -= SelectedFunction_OnArgAdded;
                            selectedFunction.OnArgRemoved -= SelectedFunction_OnArgRemoved;
                        }

                        selectedName = f.Name;
                        selectedIndex = i;
                        selectedFunction = f;

                        SubscribeToArgs();
                    }

                    function[i] = f.Name;
                }

                UpdateInputs();
            }
        }

        protected void UpdateInputs()
        {
            List<NodeOutput> previousOutputs = new List<NodeOutput>();
            List<int> indices = new List<int>();
            List<NodeInput> previous = new List<NodeInput>();

            foreach (var i in Inputs)
            {
                //isntead of directly
                //removing from UI at this point
                //we simply store them
                //for later when
                //we either replace them
                //or completely remove them
                previous.Add(i);

                int index = -1;

                previousOutputs.Add(i.Reference);

                if (i.HasInput)
                {
                    index = i.Reference.To.IndexOf(i);
                    i.Reference.Remove(i);
                }

                indices.Add(index);
            }

            Inputs.Clear();

            if (previousOutputs.Count > 0)
            {
                var prevEx = previousOutputs[0];
                var idx = indices[0];

                if(prevEx != null)
                {
                    if (idx == -1)
                    {
                        prevEx.Add(ExecuteInput);
                    }
                    else
                    {
                        prevEx.InsertAt(idx, ExecuteInput);
                    }
                }
            }

            Inputs.Add(ExecuteInput);

            if (previous.Count > 0)
            {
                AddedInput(ExecuteInput, previous[0]);
            }
            else
            {
                AddedInput(ExecuteInput);
            }

            if(selectedFunction != null)
            {
                var args = selectedFunction.Args;

                int i = 1;
                foreach(ArgNode arg in args)
                {
                    NodeInput input = new NodeInput(arg.InputType, this, arg.InputName);

                    if (previousOutputs.Count > 1 && i < previousOutputs.Count)
                    {
                        var prev = previousOutputs[i];
                        var idx = indices[i];

                        //whoops forgot to verify that the output
                        //parent could indeed accept the new input arg type
                        if (prev != null && (prev.Type & input.Type) != 0)
                        {
                            if (idx < 0)
                            {
                                prev.Add(input);
                            }
                            else
                            {
                                prev.InsertAt(idx, input);
                            }
                        }
                    }

                    Inputs.Add(input);

                    //if we still have previous nodes
                    //then we simply want to try and
                    //replace them on add
                    //so the UI will update the graphical connections 
                    //otherwise we just add a new input node
                    if (i < previous.Count)
                    {
                        AddedInput(input, previous[i]);
                    }
                    else
                    {
                        AddedInput(input);
                    }

                    ++i;
                }

                //remove any left over nodes from ui
                //if they were not replaced
                //this happens in cases where
                //the new function set has less args
                //then the previous function set
                while (i < previous.Count)
                {
                    RemovedInput(previous[i]);
                    ++i;
                }

                UpdateOutputType();
            }
        }

        public override void UpdateOutputType()
        {
            if (selectedFunction != null)
            {
                var op = selectedFunction.GetOutputType();

                if (op != null)
                {
                    output.Type = op.Value;
                }
            }
        }

        public string GetFunctionShaderCode()
        {
            if (selectedFunction == null)
            {
                return "";
            }

            string code = selectedFunction.GetFunctionShaderCode();
            UpdateOutputType();

            return code;
        }

        public override string GetShaderPart(string currentFrag)
        {
            if (selectedFunction == null)
            {
                return "";
            }

            bool canProcess = true;

            foreach (var i in Inputs)
            {
                if (i != ExecuteInput)
                {
                    if (!i.HasInput)
                    {
                        canProcess = false;
                        break;
                    }
                }
            }

            if (!canProcess)
            {
                return "";
            }

            var s = shaderId + "1";

            string prefix = "";

            if(output.Type == NodeType.Float)
            {
                prefix = "float ";
            }
            else if(output.Type == NodeType.Float2)
            {
                prefix = "vec2 ";
            }
            else if(output.Type == NodeType.Float3)
            {
                prefix = "vec3 ";
            }
            else if(output.Type == NodeType.Float4 || output.Type == NodeType.Color || output.Type == NodeType.Gray)
            {
                prefix = "vec4 ";
            }
            else if(output.Type == NodeType.Bool)
            {
                prefix = "float ";
            }
            else if(output.Type == NodeType.Matrix)
            {
                prefix = "mat4 ";
            }

            string frag = prefix + s + " = " + selectedFunction.Name.Replace(" ", "").Replace("-", "_") + "(";

            foreach(var i in Inputs)
            {
                if (i != ExecuteInput)
                {
                    var nid = (i.Reference.Node as MathNode).ShaderId;
                    var index = i.Reference.Node.Outputs.IndexOf(i.Reference);
                    nid += index;

                    frag += nid + ",";
                }
            }

            if(Inputs.Count > 1)
            {
                frag = frag.Substring(0, frag.Length - 1);
            }

            frag += ");\r\n";

            return frag;
        }

        public override void TryAndProcess()
        {
            if (selectedFunction == null) return;

            foreach(NodeInput inp in Inputs)
            {
                if(inp != ExecuteInput && inp.IsValid)
                {
                    selectedFunction.SetVar(inp.Name, inp.Data, inp.Type);
                }
            }

            selectedFunction.TryAndProcess();

            if (selectedFunction.Result != null)
            {
                output.Data = selectedFunction.Result;
                result = output.Data?.ToString();
            }

            UpdateOutputType();
        }

        public class CallNodeData : NodeData
        {
            public int selectedIndex;
            public string selectedName;

            public override void Write(Writer w)
            {
                base.Write(w);
                w.Write(selectedIndex);
                w.Write(selectedName);
            }

            public override void Parse(Reader r)
            {
                base.Parse(r);
                selectedIndex = r.NextInt();
                selectedName = r.NextString();
            }
        }

        public override void GetBinary(Writer w)
        {
            CallNodeData d = new CallNodeData();
            FillBaseNodeData(d);
            d.selectedIndex = selectedIndex;
            d.selectedName = selectedName;
            d.Write(w);
        }

        public override void FromBinary(Reader r)
        {
            CallNodeData d = new CallNodeData();
            d.Parse(r);
            SetBaseNodeDate(d);
            selectedIndex = d.selectedIndex;
            selectedName = d.selectedName;

            OnParentSet();
        }

        public override string GetJson()
        {
            CallNodeData d = new CallNodeData();
            FillBaseNodeData(d);

            d.selectedIndex = selectedIndex;
            d.selectedName = selectedName;

            return JsonConvert.SerializeObject(d);
        }

        public override void AssignParentGraph(Graph.Graph g)
        {
            if (parentGraph != null)
            {
                if (parentGraph is Function)
                {
                    Function fg = parentGraph as Function;

                    fg.OnParentGraphSet -= Fg_OnParentGraphSet;
                    fg.OnParentNodeSet -= Fg_OnParentNodeSet;
                }
            }

            if (g != null)
            {
                if (g is Function)
                {
                    Function fg = g as Function;

                    fg.OnParentGraphSet += Fg_OnParentGraphSet;
                    fg.OnParentNodeSet += Fg_OnParentNodeSet;
                }
            }

            base.AssignParentGraph(g);

            OnParentSet();
        }

        public override void AssignParentNode(Node n)
        {
            base.AssignParentNode(n);

            OnParentSet();
        }

        public override void FromJson(string data)
        {
            CallNodeData d = JsonConvert.DeserializeObject<CallNodeData>(data);
            SetBaseNodeDate(d);
            selectedIndex = d.selectedIndex;
            selectedName = d.selectedName;

            OnParentSet();
        }

        public override void Dispose()
        {
            if(parentGraph != null)
            {
                if(parentGraph is Function)
                {
                    Function fg = parentGraph as Function;

                    fg.OnParentGraphSet -= Fg_OnParentGraphSet;
                    fg.OnParentNodeSet -= Fg_OnParentNodeSet;
                }
            }

            base.Dispose();
        }

        private void Fg_OnParentNodeSet(Function g)
        {
            OnParentSet();   
        }

        private void Fg_OnParentGraphSet(Function g)
        {
            OnParentSet();
        }
    }
}
