using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Materia.Nodes.Atomic;
using Materia.Rendering.Attributes;
using Materia.Nodes;
using Materia.Rendering.Interfaces;
using Materia.Rendering.Mathematics;
using Materia.Nodes.Items;
using MLog;
using Materia.Nodes.MathNodes;
using System.Collections.Concurrent;
using VCDiff.Encoders;
using VCDiff.Decoders;
using VCDiff.Shared;
using VCDiff.Includes;
using System.Text;
using System.IO;
using System.Linq;
using Materia.Graph.IO;

namespace Materia.Graph
{ 
    public struct VariableDefinition
    {
        public NodeType Type;
        public object Value;

        public VariableDefinition(object v, NodeType type)
        {
            Type = type;
            Value = v;
        }
    }

    public struct UndoRedoData
    {
        public List<byte[]> undos;
        public List<byte[]> redos;
        public byte[] previousState;

        public UndoRedoData(List<byte[]> undo, List<byte[]> redo, byte[] previous)
        {
            undos = undo;
            redos = redo;
            previousState = previous;
        }
    }


    public class Graph : IDisposable
    {
        public const float GRAPH_VERSION = 2.0f;

        public static bool ShaderLogging { get; set; } = true; //for debugging right now

        public static ushort[] GRAPH_SIZES = new ushort[] { 8, 16, 32, 64, 128, 256, 512, 1024, 2048, 4096, 8192 };
        public const ushort DEFAULT_SIZE = 512;

        public delegate void GraphUpdate(Graph g);
        public delegate void ParameterUpdate(ParameterValue p);
        public event GraphUpdate OnUpdate;
        public event GraphUpdate OnNameChange;
        public event ParameterValue.ParameterUpdate OnParameterUpdate;
        public event ParameterValue.ParameterUpdate OnParameterTypeUpdate;
        public event GraphUpdate OnLoad;
        public event GraphUpdate OnUndo;
        public event GraphUpdate OnRedo;

        public string Id { get; protected set; } = Guid.NewGuid().ToString();

        public GraphState State { get; protected set; } = GraphState.Ready;

        public bool ReadOnly { get; set; } = false;

        public float Version { get; protected set; } = GRAPH_VERSION;

        public bool Modified { get; set; } = false;

        private string currentWorkingDirectory = "";
        public string CurrentWorkingDirectory
        {
            get => currentWorkingDirectory;
            set
            {
                currentWorkingDirectory = value;
                for (int i = 0; i < Nodes.Count; ++i)
                {
                    Nodes[i]?.SetCWD(currentWorkingDirectory);
                }
            }
        }

        public List<Node> Nodes { get; protected set; } = new List<Node>();
        public Dictionary<string, Node> NodeLookup { get; protected set; } = new Dictionary<string, Node>();
        public List<string> OutputNodes { get; protected set; } = new List<string>();
        public List<string> InputNodes { get; protected set; } = new List<string>();

        private Dictionary<string, VariableDefinition> variables = new Dictionary<string, VariableDefinition>();
        protected Dictionary<string, VariableDefinition> Variables
        {
            get => variables;
            set => variables = value;
        }

        private Dictionary<string, PointF> originSizes = new Dictionary<string, PointF>();
        protected Dictionary<string, PointF> OriginSizes
        {
            get => originSizes;
            set => originSizes = value;
        }

        public Archive Archive { get; protected set; }

        /// <summary>
        /// Stores the new vcdiff values of the graph for undo / redo
        /// </summary>
        protected ConcurrentStack<byte[]> undo = new ConcurrentStack<byte[]>();
        protected ConcurrentStack<byte[]> redo = new ConcurrentStack<byte[]>();

        public int UndoCount { get => undo.Count; }
        public int RedoCount { get => redo.Count; }

        /// <summary>
        /// Gets or sets the undo stack. Getting is not thread safe.
        /// </summary>
        /// <value>
        /// The undo stack.
        /// </value>
        public List<byte[]> UndoStack
        {
            get
            {
                return undo.ToList();
            }
            set
            {
                var stack = value;
                if (stack == null) return;
                stack.Reverse();
                undo.Clear();
                for (int i = 0; i < stack.Count; ++i)
                {
                    undo.Push(stack[i]);
                }
            }
        }

        /// <summary>
        /// Gets or sets the redo stack. Getting is not thread safe.
        /// </summary>
        /// <value>
        /// The redo stack.
        /// </value>
        public List<byte[]> RedoStack
        {
            get
            {
                return redo.ToList();
            }
            set
            {
                var stack = value;
                if (stack == null) return;
                stack.Reverse();
                redo.Clear();
                for (int i = 0; i < stack.Count; ++i)
                {
                    redo.Push(stack[i]);
                }
            }
        }

        /// <summary>
        /// The previous byte data for undo / redo
        /// </summary>
        protected byte[] previousByteData = null;

        /// <summary>
        /// Parameters are only available for image graphs
        /// </summary>
        [Editable(ParameterInputType.MapEdit, "Promoted Parameters", "Promoted Parameters")]
        public Dictionary<string, ParameterValue> Parameters { get; protected set; } = new Dictionary<string, ParameterValue>();

        [Editable(ParameterInputType.MapEdit, "Custom Parameters", "Custom Parameters")]
        public List<ParameterValue> CustomParameters { get; protected set; } = new List<ParameterValue>();

        [Editable(ParameterInputType.MapEdit, "Custom Functions", "Custom Fuctions")]
        public List<Function> CustomFunctions { get; protected set; } = new List<Function>();

        //this is a container
        //that is filled when a parameter is promoted to a function graph
        //it is also filled when we load a graph
        //its primary use is for the editor
        //so it can list all available promoted parameters to functions
        //even if the parameter has been renamed or changed in the underlying graph
        //that way the graph can be cleaned up / properly updated to handle the changes
        //by the user
        [Editable(ParameterInputType.MapEdit, "Promoted Functions", "Promoted Functions")]
        public Dictionary<string, Function> ParameterFunctions { get; protected set; } = new Dictionary<string, Function>();


        /// <summary>
        /// Helper array
        /// to quickly go through graph instance nodes
        /// </summary>
        public List<GraphInstanceNode> InstanceNodes { get; protected set; } = new List<GraphInstanceNode>();

        /// <summary>
        /// Helper to go through pixel processor nodes
        /// </summary>
        public List<PixelProcessorNode> PixelNodes { get; protected set; } = new List<PixelProcessorNode>();

        public List<Node> RootNodes
        {
            get
            {
                return Nodes.FindAll(m => m.IsRoot());
            }
        }

        public List<Node> EndNodes
        {
            get
            {
                return Nodes.FindAll(m => m.IsEnd());
            }
        }

        protected string name;
        [Editable(ParameterInputType.Text, "Name")]
        public string Name
        {
            get
            {
                return name;
            }
            set
            {
                if (name != value)
                {
                    name = value;
                    OnNameChange?.Invoke(this);
                    Modified = true;
                }
            }
        }

        public float ShiftX { get; set; } = 0;

        public float ShiftY { get; set; } = 0;

        public float Zoom { get; set; } = 1;

        protected GraphPixelType defaultTextureType = GraphPixelType.RGBA;

        [Editable(ParameterInputType.Dropdown, "Default Texture Format")]
        public GraphPixelType DefaultTextureType
        {
            get
            {
                return defaultTextureType;
            }
            set
            {
                if (!ReadOnly)
                {
                    defaultTextureType = value;
                }
            }
        }

        protected int randomSeed = 0;

        [Editable(ParameterInputType.IntInput, "Random Seed")]
        public int RandomSeed
        {
            get
            {
                return randomSeed;
            }
            set
            {
                if (randomSeed != value)
                {
                    Modified = true;
                    AssignSeed(value);

                    //no need to try and process
                    //on a function graph
                    //as the parent image graph
                    //should be one to trigger
                    //the function graph
                    if (this is Function)
                    {
                        return;
                    }

                    if (parentNode == null)
                    {
                        Updated();
                        Schedule();
                    }
                }
            }
        }

        protected ushort width = DEFAULT_SIZE;
        protected ushort height = DEFAULT_SIZE;

        public ushort Width
        {
            get
            {
                return width;
            }
            set
            {
                if (!ReadOnly && width != value)
                {
                    width = value;
                    Modified = true;
                }
            }
        }

        public ushort Height
        {
            get
            {
                return height;
            }
            set
            {
                if (!ReadOnly && height != value)
                {
                    height = value;
                    Modified = true;
                }
            }
        }

        [JsonIgnore]
        protected Node parentNode = null;
        public virtual Node ParentNode
        {
            get
            {
                return parentNode;
            }
            set
            {
                parentNode = value;
            }
        }

        /// <summary>
        /// Note this is used in function graphs
        /// and not for image graphs
        /// as a function graph if it does not
        /// have a parent node
        /// then it references the parent graph
        /// as a custom function
        /// </summary>
        protected Graph parentGraph = null;
        public Graph ParentGraph
        {
            get
            {
                return parentGraph;
            }
            set
            {
                parentGraph = value;
            }
        }

        protected bool absoluteSize = false;
        [Editable(ParameterInputType.Toggle, "Absolute Size", "Basic")]
        public bool AbsoluteSize
        {
            get => absoluteSize;
            set
            {
                if (absoluteSize != value)
                {
                    absoluteSize = value;
                    Modified = true;
                }
            }
        }

        protected ConcurrentQueue<Node> scheduledNodes = new ConcurrentQueue<Node>();
        public bool IsProcessing { get; protected set; } = false;

        /// <summary>
        /// A graph must be instantiated on the UI / Main Thread
        /// So it can acquire the proper TaskScheduler for nodes
        /// </summary>
        /// <param name="name"></param>
        /// <param name="w"></param>
        /// <param name="h"></param>
        /// <param name="async"></param>
        public Graph(string graphName, ushort w = DEFAULT_SIZE, ushort h = DEFAULT_SIZE)
        {
            if (Node.Context == null)
            {
                Node.Context = TaskScheduler.FromCurrentSynchronizationContext();
            }

            name = graphName;
            width = w;
            height = h;
        }

        public void CopyResources(string cwd, bool setCWD = false)
        {
            int count = Nodes.Count;
            for (int i = 0; i < count; ++i)
            {
                var n = Nodes[i];
                n?.CopyResources(cwd);
            }

            if (setCWD)
            {
                //set last in case we need to copy from current graph cwd to new cwd
                CurrentWorkingDirectory = cwd;
            }
        }

        protected static List<Node> Backtrack(NodeInput n, Node endNode, HashSet<Node> inStack = null)
        {
            List<Node> items = new List<Node>();
            Queue<Node> stack = new Queue<Node>();

            if (inStack == null) {
                inStack = new HashSet<Node>();
            }

            if (n.HasInput)
            {
                if (inStack.Contains(n.Reference.Node))
                {
                    return items;
                }

                inStack.Add(n.Reference.Node);
                stack.Enqueue(n.Reference.Node);

                while (stack.Count > 0)
                {
                    Node previous = stack.Dequeue();

                    items.Add(previous);

                    if (previous == endNode) break;

                    if (previous.Inputs.Count > 1)
                    {
                        List<List<Node>> backtracks = new List<List<Node>>();
                        for (int i = 0; i < previous.Inputs.Count; ++i)
                        {
                            NodeInput inp = previous.Inputs[i];
                            List<Node> back = Backtrack(inp, endNode, inStack);
                            if (back.Count > 0)
                            { 
                                backtracks.Add(back);
                            }
                        }

                        //this handles a case where input may come
                        //after another node in length
                        //but inputs needs to be last
                        if (backtracks.Count > 0 && backtracks[0].Count > 0)
                        {
                            if (backtracks[0][0] is InputNode)
                            {
                                backtracks.Reverse();
                            }
                        }

                        //this handles a case where
                        //a shorter path is actually the last path
                        //in some cases
                        //and in others it is the first path
                        if (backtracks.Count >= 2)
                        {
                            if(backtracks[0].Count >= backtracks[1].Count)
                            {
                                backtracks.Sort((a, b) =>
                                {
                                    return a.Count - b.Count;
                                });
                            }
                            else
                            {
                                backtracks.Sort((a, b) =>
                                {
                                    return b.Count - a.Count;
                                });
                            }
                        }

                        for(int i = 0; i < backtracks.Count; ++i)
                        {
                            items.AddRange(backtracks[i]);
                        }
                    }
                    else if(previous.Inputs.Count == 1)
                    {
                        NodeInput inp = previous.Inputs[0];

                        if(inp.HasInput)
                        {
                            if (inStack.Contains(inp.Reference.Node) && inp.Reference.Node != endNode) continue;
                            else if(inStack.Contains(inp.Reference.Node) && inp.Reference.Node == endNode)
                            {
                                items.Add(inp.Reference.Node);
                                continue;
                            }

                            inStack.Add(inp.Reference.Node);
                            stack.Enqueue(inp.Reference.Node);
                        }
                    }
                }
            }

            return items;
        }

        public static void GatherNodes(List<Node> startingNodes, Queue<Node> queue, Node endNode = null, HashSet<Node> inStack = null)
        {
            inStack ??= new HashSet<Node>();

            Queue<Node> stack = new Queue<Node>();

            for(int i = 0; i < startingNodes.Count; ++i)
            {
                stack.Enqueue(startingNodes[i]);
                inStack.Add(startingNodes[i]);
            }

            while(stack.Count > 0)
            {
                Node n = stack.Dequeue();

                if (n.IsScheduled)
                {
                    continue;
                }

                if (n.Inputs.Count == 0 && endNode != null
                    && n != endNode && startingNodes.Contains(n))
                {
                    //we ignore this node
                    //as it is a starting node but has no inputs
                    //and is not the expected endNode
                    continue;
                }

                List<List<Node>> backtracks = new List<List<Node>>();

                //only backtrack if the node is not the expected endNode
                if (n != endNode)
                { 
                    for (int i = 0; i < n.Inputs.Count; ++i)
                    {
                        List<Node> backs = Backtrack(n.Inputs[i], endNode, inStack);

                        //ignore branchs that do not connect to our endNode
                        if (endNode != null 
                            && startingNodes.Count > 1
                            && !backs.Contains(endNode))
                        {
                            continue;
                        }

                        //only add if we actually have nodes to process
                        if (backs.Count > 0)
                        {
                            backtracks.Add(backs);
                        }
                    }
                }

                if (backtracks.Count == 0 && n.Inputs.Count > 0 
                    && endNode != null && endNode != n)
                {
                    //meaning we are not actually connected to the expected endNode
                    //do not queue and ignore this node
                    continue;
                }   

                for (int i = 0; i < backtracks.Count; ++i)
                {
                    List<Node> backs = backtracks[i];

                    //backs is reversed since it was a stack
                    //so we loop through in reverse to order properly
                    for (int j = backs.Count - 1; j >= 0; --j)
                    {
                        Node next = backs[j];
                        if (next.IsScheduled) continue;

                        next.IsScheduled = true;
                        queue.Enqueue(next);

                        if (next is GraphInstanceNode)
                        {
                            //this is needed in order to properly handle graph instances within graph instances
                            //otherwise they will not be scheduled properly
                            GraphInstanceNode gn = next as GraphInstanceNode;
                            Queue<Node> internalStack = new Queue<Node>();
                            gn.GatherOutputs(inStack, internalStack);

                            if (internalStack.Count > 0)
                            {
                                GatherNodes(internalStack.ToList(), queue, null, inStack);
                            }
                        }
                    }
                }

                n.IsScheduled = true;
                queue.Enqueue(n);
                if (n is GraphInstanceNode)
                {
                    GraphInstanceNode gn = n as GraphInstanceNode;
                    gn.GatherOutputs(inStack, stack);
                }
            }
        }


        public virtual bool HasVar(string k)
        {
            return variables.ContainsKey(k);
        }

        public virtual object GetVar(string k)
        {
            if (string.IsNullOrEmpty(k)) return null;

            if(variables.ContainsKey(k))
            {
                return variables[k].Value;
            }

            return null;
        }

        public virtual string[] GetAvailableVariables(NodeType type)
        {
            List<string> available = new List<string>();

            try
            {
                foreach (string k in variables.Keys)
                {
                    VariableDefinition o = variables[k];

                    if (o.Type == type || (o.Type & type) != 0)
                    {
                        available.Add(k);
                    }
                }
            }
            catch (Exception e)
            {

            }

            //sort alphabetically
            available.Sort();

            return available.ToArray();
        }

        public Node FindSubNodeById(string id)
        {
            Node n = null;

            //first try this graph
            if(NodeLookup.TryGetValue(id, out n))
            {
                return n;
            }

            var graphinsts = InstanceNodes;

            //try and retrieve from graph inst
            for(int i = 0; i < graphinsts.Count; ++i)
            {
                var proc = graphinsts[i];

                if (proc.GraphInst != null)
                {
                    n = proc.GraphInst.FindSubNodeById(id);

                    if(n != null)
                    {
                        return n;
                    }
                }
            }

            return null;
        }

        public virtual void RemoveVar(string k)
        {
            if (string.IsNullOrEmpty(k)) return;
            variables.Remove(k);
        }

        public virtual void SetVar(string k, object v, NodeType type)
        {
            if (string.IsNullOrEmpty(k)) return;
            variables[k] = new VariableDefinition(v, type);
        }

        public virtual Graph Top()
        {
            Graph g = this;

            if(g.parentGraph != null)
            {
                g = g.parentGraph;
            }

            while(g.parentNode != null)
            {
                g = g.parentNode.ParentGraph;
            }

            return g;
        }

        public void Poll()
        {
            if (scheduledNodes.Count > 0)
            {
                IsProcessing = true;
                scheduledNodes.TryDequeue(out Node n);

                if (n == null) return;

                if (!(n is GraphInstanceNode))
                {
                    n.TryAndProcess();
                    n.IsScheduled = false;
                }
                else
                {
                    GraphInstanceNode gn = n as GraphInstanceNode;
                    gn?.PopulateGraphParams();
                    n.IsScheduled = false;
                }
            }
            else if (IsProcessing)
            {
                IsProcessing = false;
            }
        }

        public virtual void Schedule(Node n)
        {
            if (n is MathNode || n is ItemNode) return;
            if (n.IsScheduled) return;
            if (n.ParentGraph == null) return;

            IsProcessing = true;

            Node realNode = n;
            Queue<Node> queue = new Queue<Node>();
            List<Node> starting = realNode.ParentGraph.EndNodes;

            Node endNode = realNode;

            if (starting.Contains(realNode))
            {
                //if this node is an end node itself
                //we can ignore all other end nodes
                //as a point of rebuilding
                starting.Clear();
                starting.Add(realNode);
            }

            GatherNodes(starting, queue, endNode);

            Node[] nodesToSchedule = queue.ToArray();
            for (int i = 0; i < nodesToSchedule.Length; ++i)
            {
                scheduledNodes.Enqueue(nodesToSchedule[i]);
            }
        }

        public virtual void Schedule()
        {
            List<Node> root = EndNodes;
            Queue<Node> nodes = new Queue<Node>();
            GatherNodes(root, nodes, null);
            Node[] nodesToSchedule = nodes.ToArray();
            for (int i = 0; i < nodesToSchedule.Length; ++i)
            {
                scheduledNodes.Enqueue(nodesToSchedule[i]);
            }
            IsProcessing = true;
        }

        public virtual void AssignPixelType(GraphPixelType pixel)
        {
            defaultTextureType = pixel;

            //no need to loop through nodes
            //on a function graph
            if (this is Function)
            {
                return;
            }

            int c = Nodes.Count;
            for(int i = 0; i < c; ++i)
            {
                Node n = Nodes[i];
                n.AssignPixelType(pixel);
            }
        }

        public virtual void AssignSeed(int seed)
        {
            randomSeed = seed;

            //no need to loop through nodes
            //on a function graph
            //when we assign seed
            if (this is Function)
            {
                return;
            }

            //need to assign to function graphs
            //and graph instances + pixel procs
            int c = PixelNodes.Count;
            for(int i = 0; i < c; ++i)
            {
                PixelProcessorNode proc = PixelNodes[i];
                proc.Function?.AssignSeed(seed);
            }

            c = InstanceNodes.Count;
            for (int i = 0; i < c; ++i)
            {
                GraphInstanceNode inst = InstanceNodes[i];
                inst.AssignSeed(seed);
            }

            c = CustomFunctions.Count;
            for (int i = 0; i < c; ++i)
            {
                Function g = CustomFunctions[i];
                g.AssignSeed(seed);
            }

            try
            {
                foreach (Function g in ParameterFunctions.Values)
                {
                    g.AssignSeed(seed);
                }
            }
            catch (Exception e)
            {

            }

        }

        public virtual void AssignParentNode(Node n)
        {
            parentNode = n;
        }

        /// <summary>
        /// this is used in GraphInstances
        /// To resize proportionate to new size
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public virtual void ResizeWith(int width, int height)
        {
            int c = Nodes.Count;

            //do not resize if the graph is not relative
            //or if the graph is a function graph
            if (AbsoluteSize || this is Function) return;

            float wp = (float)width / (float)this.width;
            float hp = (float)height / (float)this.height;

            for (int i = 0; i < c; ++i)
            {
                Node n = Nodes[i];

                if(!(n is ItemNode) && !(n is BitmapNode))
                {   
                    PointF osize;

                    if (OriginSizes.TryGetValue(n.Id, out osize))
                    {
                        int fwidth = (int)Math.Min(4096, Math.Max(8, Math.Round(osize.x * wp)));
                        int fheight = (int)Math.Min(4096, Math.Max(8, Math.Round(osize.y * hp)));

                        //if not relative skip
                        if (n.AbsoluteSize) continue;

                        n.SetSize(fwidth, fheight);
                    }
                }
            }

            Modified = true;
        }

        public virtual void GetBinary(Writer w)
        {
            if (w == null) return;

            //write binary header marker
            w.Write((byte)'M');

            GraphData d = new GraphData();
            d.name = Name;
            d.version = GRAPH_VERSION;
            d.id = Id;
            
            //d.outputs = OutputNodes ?? new List<string>();
            //d.inputs = InputNodes ?? new List<string>();
            
            d.defaultTextureType = defaultTextureType;
            
            d.shiftX = ShiftX;
            d.shiftY = ShiftY;
            d.zoom = Zoom;

            d.absoluteSize = absoluteSize;
            
            d.width = width;
            d.height = height;

            //write header info
            d.Write(w);

            //write custom params / functions
            d.WriteCustomParameters(w, CustomParameters);
            d.WriteCustomFunctions(w, CustomFunctions);

            //followed by individual nodes
            d.WriteNodes(w, Nodes);

            //write parameters
            d.WriteParameters(w, Parameters);

            //this is for the different graph types
            //that may need to write extra data
            //not associated with the base graph
            WriteExtended(w);
        }

        protected virtual void ReadExtended(Reader r)
        {
            //do nothing in this one
        }

        protected virtual void WriteExtended(Writer w)
        {
            //do nothing on this one
        }

        public virtual string GetJson()
        {
            GraphData d = new GraphData();

            List<string> data = new List<string>();

            int count = Nodes.Count;
            for(int i = 0; i < count; ++i)
            {
                Node n = Nodes[i];
                data.Add(n.GetJson());
            }

            d.name = Name;
            d.nodes = data;
            d.id = Id;
            
            //d.outputs = OutputNodes;
            //d.inputs = InputNodes;
            
            d.defaultTextureType = defaultTextureType;
            
            d.shiftX = ShiftX;
            d.shiftY = ShiftY;
            d.zoom = Zoom;

            d.parameters = GetJsonReadyParameters();
            
            d.width = width;
            d.height = height;
            
            d.absoluteSize = AbsoluteSize;
            d.customParameters = GetJsonReadyCustomParameters();
            d.customFunctions = GetJsonReadyCustomFunctions();
            d.version = GRAPH_VERSION;

            return JsonConvert.SerializeObject(d);
        }

        #region Custom Functions & Params
        protected virtual List<string> GetJsonReadyCustomFunctions()
        {
            List<string> funcs = new List<string>();

            int count = CustomFunctions.Count;
            for(int i = 0; i < count; ++i)
            {
                Function f = CustomFunctions[i];
                funcs.Add(f.GetJson());
            }

            return funcs;
        }

        public virtual Dictionary<string, object> GetConstantParameters()
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>();

            try
            {
                foreach (var k in Parameters.Keys)
                {
                    if (Parameters[k].IsFunction()) continue;
                    parameters[k] = Parameters[k].Value;
                }
            }
            catch (Exception e)
            {

            }

            return parameters;
        }

        protected virtual Dictionary<string, string> GetJsonReadyParameters()
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();

            try
            {
                foreach (var k in Parameters.Keys)
                {
                    parameters[k] = Parameters[k].GetJson();
                }
            }
            catch (Exception e)
            {

            }

            return parameters;
        }


        protected List<string> GetJsonReadyCustomParameters()
        {
            List<string> parameters = new List<string>();
            int count = CustomParameters.Count;
            for(int i = 0; i < count; ++i)
            {
                ParameterValue g = CustomParameters[i];
                parameters.Add(g.GetJson());
            }
            return parameters;
        }

        public virtual Dictionary<string, object> GetCustomParameters()
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>();

            int count = CustomParameters.Count;
            for(int i = 0; i < count; ++i)
            {
                var param = CustomParameters[i];
                if (param.IsFunction()) continue;
                parameters[param.Id] = param.Value;
            }

            return parameters;
        }

        protected virtual void SetJsonReadyCustomFunctions(List<string> functions)
        {
            if(functions != null)
            {
                CustomFunctions = new List<Function>();

                int fcount = functions.Count;
                for(int i = 0; i < fcount; ++i)
                {
                    string k = functions[i];
                    Function g = new Function("temp");
                    g.AssignParentGraph(this);
                    CustomFunctions.Add(g);
                    g.FromJson(k);
                }

                for(int i = 0; i < fcount; ++i)
                {
                    Function g = CustomFunctions[i];
                    //set parent graph via this
                    //method to trigger an event
                    //that is necessary for call nodes
                    g.ParentGraph = this;
                    //set connections
                    g.SetConnections();
                }
            }
        }

        protected virtual void RestoreBinaryCustomFunctions(Reader r)
        {
            try
            {
                int count = r.NextInt();
                for (int i = 0; i < count; ++i)
                {
                    if (i >= CustomFunctions.Count)
                    {
                        Function g = new Function("temp");
                        CustomFunctions.Add(g);
                        g.AssignParentGraph(this);
                        g.FromBinary(r);
                        continue;
                    }

                    Function existing = CustomFunctions[i];
                    existing?.Restore(r);
                }

                int diff = CustomFunctions.Count - count;
                for (int i = 0; i < diff; ++i)
                {
                    CustomFunctions[count]?.Dispose();
                    CustomFunctions.RemoveAt(count);
                }

                for (int i = 0; i < count; ++i)
                {
                    Function g = CustomFunctions[i];
                    g.ParentGraph = this;
                    g.SetConnections();
                }
            }
            catch
            {
                Log.Error("Failed to restore custom functions");
            }
        }

        protected virtual void SetBinaryCustomFunctions(Reader r)
        {
            try
            {
                CustomFunctions = new List<Function>();

                int count = r.NextInt();
                for (int i = 0; i < count; ++i)
                {
                    Function g = new Function("temp");
                    CustomFunctions.Add(g);
                    g.AssignParentGraph(this);
                    g.FromBinary(r);
                }

                for (int i = 0; i < count; ++i)
                {
                    Function g = CustomFunctions[i];
                    //set parent graph via this
                    //method to trigger an event
                    //that is necessary for call nodes
                    g.ParentGraph = this;
                    //set connections
                    g.SetConnections();
                }
            }
            catch
            {
                Log.Error("Failed to read custom functions");
            }
        }

        public virtual void AssignParameters(Dictionary<string, object> parameters)
        {
            try
            {
                if (parameters != null)
                {
                    foreach (var k in parameters.Keys)
                    {
                        ParameterValue gparam = null;

                        if (Parameters.TryGetValue(k, out gparam))
                        {
                            if (!gparam.IsFunction())
                            {
                                //we use assignvalue as not to trigger the 
                                //update event
                                //since this is ever only called from
                                //a loading graph instance
                                gparam.AssignValue(parameters[k]);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {

            }
        }

        public virtual void AssignCustomParameters(Dictionary<string, object> parameters)
        {
            try
            {
                if (parameters != null)
                {
                    foreach (var k in parameters.Keys)
                    {
                        var param = CustomParameters.Find(m => m.Id.Equals(k));

                        if (param != null)
                        {
                            if (!param.IsFunction())
                            {
                                //we use assignvalue as not to trigger the 
                                //update event
                                //since this is ever only called from
                                //a loading graph instance
                                param.AssignValue(parameters[k]);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {

            }
        }

        protected virtual void SetJsonReadyParameters(Dictionary<string, string> parameters)
        {
            try
            {
                if (parameters != null)
                {
                    ParameterFunctions = new Dictionary<string, Function>();
                    Parameters = new Dictionary<string, ParameterValue>();

                    foreach (var k in parameters.Keys)
                    {
                        string[] split = k.Split('.');

                        Node n = null;
                        NodeLookup.TryGetValue(split[0], out n);

                        var param = ParameterValue.FromJson(parameters[k], n);
                        param.Key = k;
                        Parameters[k] = param;

                        param.ParentGraph = this;
                        param.OnParameterTypeChanged += Graph_OnGraphParameterTypeChanged;
                        param.OnParameterUpdate += Graph_OnGraphParameterUpdate;

                        if (param.IsFunction())
                        {
                            var f = Parameters[k].Value as Function;
                            f.AssignParentGraph(this);
                            ParameterFunctions[k] = f;
                        }
                    }
                }
            }
            catch (Exception e)
            {

            }
        }

        protected virtual void RestoreBinaryParameters(Reader r) 
        {
            try
            {
                HashSet<string> restored = new HashSet<string>();
                int count = r.NextInt();
                for (int i = 0; i < count; ++i)
                {
                    string k = r.NextString();
                    restored.Add(k);
                    string[] split = k.Split('.');
                    NodeLookup.TryGetValue(split[0], out Node n);
                    if (!Parameters.TryGetValue(k, out ParameterValue param)) 
                    {
                        param = ParameterValue.FromBinary(r, n);
                        Parameters[k] = param;
                        param.Key = k;
                        param.ParentGraph = this;
                        param.OnParameterTypeChanged += Graph_OnGraphParameterTypeChanged;
                        param.OnParameterUpdate += Graph_OnGraphParameterUpdate;

                        if (param.IsFunction())
                        {
                            var f = param.Value as Function;
                            f.AssignParentGraph(this);
                            ParameterFunctions[k] = f;
                        }

                        continue;
                    }

                    ParameterFunctions.Remove(k); //reset

                    if (param == null) continue;
                    param.RestoreBinary(r, n);
                    if (param.IsFunction())
                    {
                        var f = param.Value as Function;
                        f.AssignParentGraph(this);
                        ParameterFunctions[k] = f;
                    }
                }

                string[] keys = Parameters.Keys.ToArray();
                for (int i = 0; i < keys.Length; ++i)
                {
                    var k = keys[i];
                    if (!restored.Contains(k))
                    {
                        Parameters.Remove(k);
                        ParameterFunctions.Remove(k);
                    }
                }
            }
            catch
            {
                Log.Error("Failed to restore binary parameters");
            }
        }

        protected virtual void SetBinaryParameters(Reader r)
        {
            try
            {
                ParameterFunctions = new Dictionary<string, Function>();
                Parameters = new Dictionary<string, ParameterValue>();

                int count = r.NextInt();
                for (int i = 0; i < count; ++i)
                {
                    string k = r.NextString();
                    string[] split = k.Split('.');

                    Node n = null;
                    NodeLookup.TryGetValue(split[0], out n);
                    var param = ParameterValue.FromBinary(r, n);
                    Parameters[k] = param;
                    param.Key = k;
                    param.ParentGraph = this;
                    param.OnParameterTypeChanged += Graph_OnGraphParameterTypeChanged;
                    param.OnParameterUpdate += Graph_OnGraphParameterUpdate;

                    if (param.IsFunction())
                    {
                        var f = param.Value as Function;
                        f.AssignParentGraph(this);
                        ParameterFunctions[k] = f;
                    }
                }
            }
            catch
            {
                Log.Error("Failed to read binary parameters");
            }
        }

        public bool RemoveCustomParameter(ParameterValue p)
        {
            if(CustomParameters.Remove(p))
            {
                p.ParentGraph = null;
                p.OnParameterUpdate -= Graph_OnGraphParameterUpdate;
                p.OnParameterTypeChanged -= Graph_OnGraphParameterTypeChanged;
                Modified = true;
                return true;
            }

            return false;
        }

        public void AddCustomParameter(ParameterValue p)
        {
            if (CustomParameters.Contains(p)) return;
            if (p.ParentGraph != null && p.ParentGraph != this)
            {
                p.ParentGraph.RemoveCustomParameter(p);
            }
            CustomParameters.Add(p);
            p.ParentGraph = this;
            p.OnParameterTypeChanged += Graph_OnGraphParameterTypeChanged;
            p.OnParameterUpdate += Graph_OnGraphParameterUpdate;
            Modified = true;
        } 

        public bool RemoveCustomFunction(Function g)
        {
            if(CustomFunctions.Remove(g))
            {
                g.AssignParentGraph(null);
                Modified = true;
                return true;
            }

            return false;
        }

        public void AddCustomFunction(Function g)
        {
            if (CustomFunctions.Contains(g)) return;
            if (g.ParentGraph != null && g.ParentGraph != this)
            {
                g.ParentGraph.RemoveCustomFunction(g);
            }
            CustomFunctions.Add(g);
            g.AssignParentGraph(this);
            Modified = true;
        }

        protected virtual void SetJsonReadyCustomParameters(List<string> parameters)
        {
            if(parameters != null)
            {
                CustomParameters.Clear();

                int count = parameters.Count;
                for(int i = 0; i < count; ++i)
                {
                    string k = parameters[i];
                    var param = ParameterValue.FromJson(k, null);
                    param.ParentGraph = this;
                    param.OnParameterTypeChanged += Graph_OnGraphParameterTypeChanged;
                    param.OnParameterUpdate += Graph_OnGraphParameterUpdate;
                    CustomParameters.Add(param);
                }
            }
        }

        protected virtual void RestoreBinaryCustomParameters(Reader r)
        {
            try
            {
                int count = r.NextInt();
                for (int i = 0; i < count; ++i)
                {
                    if (i >= CustomParameters.Count)
                    {
                        var param = ParameterValue.FromBinary(r, null);
                        param.ParentGraph = this;
                        param.OnParameterTypeChanged += Graph_OnGraphParameterTypeChanged;
                        param.OnParameterUpdate += Graph_OnGraphParameterUpdate;
                        CustomParameters.Add(param);
                        continue;
                    }

                    var existingParam = CustomParameters[i];
                    existingParam?.RestoreBinary(r, null);
                }

                //remove unneeded params
                int diff = CustomParameters.Count - count;
                for (int i = 0; i < diff; ++i)
                {
                    CustomParameters.RemoveAt(count);
                }
            }
            catch
            {
                Log.Error("Failed to restore binary custom parameters");
            }
        }

        protected virtual void SetBinaryCustomParameters(Reader r)
        {
            try
            {
                int count = r.NextInt();
                for (int i = 0; i < count; ++i)
                {
                    var param = ParameterValue.FromBinary(r, null);
                    param.ParentGraph = this;
                    param.OnParameterTypeChanged += Graph_OnGraphParameterTypeChanged;
                    param.OnParameterUpdate += Graph_OnGraphParameterUpdate;
                    CustomParameters.Add(param);
                }
            }
            catch
            {
                Log.Error("Failed to read binary custom parameters");
            }
        }
        #endregion

        #region Node Creation
        public virtual bool Add(Node n)
        {
            if (NodeLookup.ContainsKey(n.Id)) return false;

            n.ParentGraph?.Remove(n, false);

            n.AssignParentGraph(this);

            if (n is OutputNode)
            {
                OutputNodes.Add(n.Id);
            }
            else if (n is InputNode)
            {
                InputNodes.Add(n.Id);
            }

            if (n is GraphInstanceNode)
            {
                InstanceNodes.Add(n as GraphInstanceNode);
            }
            else if (n is PixelProcessorNode)
            {
                PixelNodes.Add(n as PixelProcessorNode);
            }

            NodeLookup[n.Id] = n;
            Nodes.Add(n);

            Modified = true;

            return true;
        }

        public virtual void Remove(Node n, bool dispose = true)
        {
            if (n is OutputNode)
            {
                OutputNodes.Remove(n.Id);
            }
            else if (n is InputNode)
            {
                InputNodes.Remove(n.Id);
            }

            if (n is GraphInstanceNode)
            {
                InstanceNodes.Remove(n as GraphInstanceNode);
            }
            else if (n is PixelProcessorNode)
            {
                PixelNodes.Remove(n as PixelProcessorNode);
            }

            n.AssignParentGraph(null);
            NodeLookup.Remove(n.Id);
            Nodes.Remove(n);

            if (dispose)
            {
                n.Dispose();
            }

            Modified = true;
        }

        public virtual Node CreateNode(Type t)
        {
            try
            {
                if (t != null)
                {
                    return CreateNodeFromType(t.Name, Width, Height, defaultTextureType);
                }
            }
            catch (Exception e)
            {
                Log.Error(e);
            }

            return null;
        }

        public virtual Node CreateNode(string type)
        {
            if(ReadOnly)
            {
                return null;
            }

            return CreateNodeFromType(type, Width, Height, defaultTextureType);
        }

        protected static Node CreateNodeFromType(NodeDataType ntype, int w, int h, GraphPixelType pixel)
        {
            switch (ntype)
            {
                case NodeDataType.AtomicAONode:
                    return new AONode(w, h, pixel);
                case NodeDataType.AtomicBitmapNode:
                    return new BitmapNode(w, h, pixel);
                case NodeDataType.AtomicBlendNode:
                    return new BlendNode(w, h, pixel);
                case NodeDataType.AtomicBlurNode:
                    return new BlurNode(w, h, pixel);
                case NodeDataType.AtomicChannelSwitchNode:
                    return new ChannelSwitchNode(w, h, pixel);
                case NodeDataType.AtomicCircleNode:
                    return new CircleNode(w, h, pixel);
                case NodeDataType.AtomicCurvesNode:
                    return new CurvesNode(w, h, pixel);
                case NodeDataType.AtomicDirectionalWarpNode:
                    return new DirectionalWarpNode(w, h, pixel);
                case NodeDataType.AtomicDistanceNode:
                    return new Nodes.Atomic.DistanceNode(w, h, pixel);
                case NodeDataType.AtomicEmbossNode:
                    return new EmbossNode(w, h, pixel);
                case NodeDataType.AtomicFXNode:
                    return new FXNode(w, h, pixel);
                case NodeDataType.AtomicGammaNode:
                    return new GammaNode(w, h, pixel);
                case NodeDataType.AtomicGradientDynamicNode:
                    return new GradientDynamicNode(w, h, pixel);
                case NodeDataType.AtomicGradientMapNode:
                    return new GradientMapNode(w, h, pixel);
                case NodeDataType.AtomicGraphInstanceNode:
                    return new GraphInstanceNode(w, h, pixel);
                case NodeDataType.AtomicGrayscaleConversionNode:
                    return new GrayscaleConversionNode(w, h, pixel);
                case NodeDataType.AtomicHSLNode:
                    return new HSLNode(w, h, pixel);
                case NodeDataType.AtomicInputNode:
                    return new InputNode(pixel);
                case NodeDataType.AtomicInvertNode:
                    return new InvertNode(w, h, pixel);
                case NodeDataType.AtomicLevelsNode:
                    return new LevelsNode(w, h, pixel);
                case NodeDataType.AtomicMeshDepthNode:
                    return new MeshDepthNode(w, h, pixel);
                case NodeDataType.AtomicMeshNode:
                    return new MeshNode(w, h, pixel);
                case NodeDataType.AtomicMotionBlurNode:
                    return new MotionBlurNode(w, h, pixel);
                case NodeDataType.AtomicNormalNode:
                    return new NormalNode(w, h, pixel);
                case NodeDataType.AtomicOutputNode:
                    return new OutputNode(pixel);
                case NodeDataType.AtomicPixelProcessorNode:
                    return new PixelProcessorNode(w, h, pixel);
                case NodeDataType.AtomicSequenceNode:
                    return new SequenceNode(w, h, pixel);
                case NodeDataType.AtomicSharpenNode:
                    return new SharpenNode(w, h, pixel);
                case NodeDataType.AtomicSwitchNode:
                    return new SwitchNode(w, h, pixel);
                case NodeDataType.AtomicTextNode:
                    return new TextNode(w, h, pixel);
                case NodeDataType.AtomicTransformNode:
                    return new TransformNode(w, h, pixel);
                case NodeDataType.AtomicUniformColorNode:
                    return new UniformColorNode(w, h, pixel);
                case NodeDataType.AtomicWarpNode:
                    return new WarpNode(w, h, pixel);
                case NodeDataType.ItemsCommentNode:
                    return new CommentNode();
                case NodeDataType.ItemsPinNode:
                    return new PinNode();
                case NodeDataType.MathNodesAbsoluteNode:
                    return new AbsoluteNode(w, h, pixel);
                case NodeDataType.MathNodesAddNode:
                    return new AddNode(w, h, pixel);
                case NodeDataType.MathNodesAndNode:
                    return new AndNode(w, h, pixel);
                case NodeDataType.MathNodesArcTangentNode:
                    return new ArcTangentNode(w, h, pixel);
                case NodeDataType.MathNodesArgNode:
                    return new ArgNode(w, h, pixel);
                case NodeDataType.MathNodesBooleanConstantNode:
                    return new BooleanConstantNode(w, h, pixel);
                case NodeDataType.MathNodesBreakFloat2Node:
                    return new BreakFloat2Node(w, h, pixel);
                case NodeDataType.MathNodesBreakFloat3Node:
                    return new BreakFloat3Node(w, h, pixel);
                case NodeDataType.MathNodesBreakFloat4Node:
                    return new BreakFloat4Node(w, h, pixel);
                case NodeDataType.MathNodesCallNode:
                    return new CallNode(w, h, pixel);
                case NodeDataType.MathNodesCartesianNode:
                    return new CartesianNode(w, h, pixel);
                case NodeDataType.MathNodesCeilNode:
                    return new CeilNode(w, h, pixel);
                case NodeDataType.MathNodesClampNode:
                    return new ClampNode(w, h, pixel);
                case NodeDataType.MathNodesCosineNode:
                    return new CosineNode(w, h, pixel);
                case NodeDataType.MathNodesDistanceNode:
                    return new Nodes.MathNodes.DistanceNode(w, h, pixel);
                case NodeDataType.MathNodesDivideNode:
                    return new DivideNode(w, h, pixel);
                case NodeDataType.MathNodesDotProductNode:
                    return new DotProductNode(w, h, pixel);
                case NodeDataType.MathNodesEqualNode:
                    return new EqualNode(w, h, pixel);
                case NodeDataType.MathNodesExecuteNode:
                    return new ExecuteNode(w, h, pixel);
                case NodeDataType.MathNodesExponentialNode:
                    return new ExponentialNode(w, h, pixel);
                case NodeDataType.MathNodesFloat2ConstantNode:
                    return new Float2ConstantNode(w, h, pixel);
                case NodeDataType.MathNodesFloat3ConstantNode:
                    return new Float3ConstantNode(w, h, pixel);
                case NodeDataType.MathNodesFloat4ConstantNode:
                    return new Float4ConstantNode(w, h, pixel);
                case NodeDataType.MathNodesFloatConstantNode:
                    return new FloatConstantNode(w, h, pixel);
                case NodeDataType.MathNodesFloorNode:
                    return new FloorNode(w, h, pixel);
                case NodeDataType.MathNodesForLoopNode:
                    return new ForLoopNode(w, h, pixel);
                case NodeDataType.MathNodesFractNode:
                    return new FractNode(w, h, pixel);
                case NodeDataType.MathNodesGetBoolVarNode:
                    return new GetBoolVarNode(w, h, pixel);
                case NodeDataType.MathNodesGetFloat2VarNode:
                    return new GetFloat2VarNode(w, h, pixel);
                case NodeDataType.MathNodesGetFloat3VarNode:
                    return new GetFloat3VarNode(w, h, pixel);
                case NodeDataType.MathNodesGetFloat4VarNode:
                    return new GetFloat4VarNode(w, h, pixel);
                case NodeDataType.MathNodesGetFloatVarNode:
                    return new GetFloatVarNode(w, h, pixel);
                case NodeDataType.MathNodesGreaterThanEqualNode:
                    return new GreaterThanEqualNode(w, h, pixel);
                case NodeDataType.MathNodesGreaterThanNode:
                    return new GreaterThanNode(w, h, pixel);
                case NodeDataType.MathNodesIfElseNode:
                    return new IfElseNode(w, h, pixel);
                case NodeDataType.MathNodesLengthNode:
                    return new LengthNode(w, h, pixel);
                case NodeDataType.MathNodesLerpNode:
                    return new LerpNode(w, h, pixel);
                case NodeDataType.MathNodesLessThanEqualNode:
                    return new LessThanEqualNode(w, h, pixel);
                case NodeDataType.MathNodesLessThanNode:
                    return new LessThanNode(w, h, pixel);
                case NodeDataType.MathNodesLog2Node:
                    return new Log2Node(w, h, pixel);
                case NodeDataType.MathNodesLogNode:
                    return new LogNode(w, h, pixel);
                case NodeDataType.MathNodesMakeFloat2Node:
                    return new MakeFloat2Node(w, h, pixel);
                case NodeDataType.MathNodesMakeFloat3Node:
                    return new MakeFloat3Node(w, h, pixel);
                case NodeDataType.MathNodesMakeFloat4Node:
                    return new MakeFloat4Node(w, h, pixel);
                case NodeDataType.MathNodesMatrixNode:
                    return new MatrixNode(w, h, pixel);
                case NodeDataType.MathNodesMaxNode:
                    return new MaxNode(w, h, pixel);
                case NodeDataType.MathNodesMinNode:
                    return new MinNode(w, h, pixel);
                case NodeDataType.MathNodesModuloNode:
                    return new ModuloNode(w, h, pixel);
                case NodeDataType.MathNodesMultiplyNode:
                    return new MultiplyNode(w, h, pixel);
                case NodeDataType.MathNodesNegateNode:
                    return new NegateNode(w, h, pixel);
                case NodeDataType.MathNodesNormalizeNode:
                    return new NormalizeNode(w, h, pixel);
                case NodeDataType.MathNodesNotEqualNode:
                    return new NotEqualNode(w, h, pixel);
                case NodeDataType.MathNodesNotNode:
                    return new NotNode(w, h, pixel);
                case NodeDataType.MathNodesOrNode:
                    return new OrNode(w, h, pixel);
                case NodeDataType.MathNodesPolarNode:
                    return new PolarNode(w, h, pixel);
                case NodeDataType.MathNodesPow2Node:
                    return new Pow2Node(w, h, pixel);
                case NodeDataType.MathNodesPowNode:
                    return new PowNode(w, h, pixel);
                case NodeDataType.MathNodesRandom2Node:
                    return new Random2Node(w, h, pixel);
                case NodeDataType.MathNodesRandomNode:
                    return new RandomNode(w, h, pixel);
                case NodeDataType.MathNodesRotateMatrixNode:
                    return new RotateMatrixNode(w, h, pixel);
                case NodeDataType.MathNodesRoundNode:
                    return new RoundNode(w, h, pixel);
                case NodeDataType.MathNodesSamplerNode:
                    return new SamplerNode(w, h, pixel);
                case NodeDataType.MathNodesScaleMatrixNode:
                    return new ScaleMatrixNode(w, h, pixel);
                case NodeDataType.MathNodesSetVarNode:
                    return new SetVarNode(w, h, pixel);
                case NodeDataType.MathNodesShearMatrixNode:
                    return new ShearMatrixNode(w, h, pixel);
                case NodeDataType.MathNodesSineNode:
                    return new SineNode(w, h, pixel);
                case NodeDataType.MathNodesSqrtNode:
                    return new SqrtNode(w, h, pixel);
                case NodeDataType.MathNodesSubtractNode:
                    return new SubtractNode(w, h, pixel);
                case NodeDataType.MathNodesTangentNode:
                    return new TangentNode(w, h, pixel);
                case NodeDataType.MathNodesTranslateMatrixNode:
                    return new TranslateMatrixNode(w, h, pixel);
            }

            return null;
        }

        //todo: Create a Unit Test Function for the return types
        //to verify these all return what we expect for the expected string
        protected static Node CreateNodeFromType(string type, int w, int h, GraphPixelType pixel)
        {
            if (string.IsNullOrEmpty(type)) return null;

            if (type.Contains("/") || type.Contains("\\")) return new GraphInstanceNode(w, h, pixel);

            string[] typeSplit = type.Split(".");

            if (typeSplit.Length < 4) return null;

            Enum.TryParse(typeSplit[2] + typeSplit[3], out NodeDataType ntype);

            return CreateNodeFromType(ntype, w, h, pixel);
        }
        #endregion

        #region Binary Loading
        
        public virtual void FromBinary(Reader r, Archive archive = null)
        {
            List<Archive.ArchiveFile> archiveFiles = null;
            if (r == null && archive != null)
            {
                archive.Open();
                archiveFiles = archive.GetAvailableFiles();
                //there is only ever one .mtg in an archive
                var mtgFile = archiveFiles.Find(m => m.path.EndsWith(".mtg"));
                if (mtgFile == null)
                {
                    return;
                }

                r = new Reader(mtgFile.ExtractBinary());
                archive.Close();
            }

            if (r == null || r.Length == 0) return;

            byte b = r.NextByte();
            char c = (char)b;
            if (c == '{') //this is a json file
            {
                r.Position = 0;
                var buff = r.Buffer;
                string data = Encoding.UTF8.GetString(buff.Array, buff.Offset, buff.Count);
                FromJson(data, archive);
                return;
            }
            else if(c == 'M') //materia binary files always start with a capital M
            {
                //parse general header info
                //and string lists
                try
                {
                    GraphData d = new GraphData();
                    d.Parse(r);
                    FromBinary(d, r, archive);
                    return;
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }

            //we are not a properly defined file
            Log.Warn("Invalid MTG File");
        }

        protected virtual void Restore(Reader r)
        {
            GraphData d = new GraphData();
            d.Parse(r); //parse header info

            //keeps track of restored nodes
            //if the node does not exist in this
            //then that means we must dispose of it
            HashSet<string> restored = new HashSet<string>();

            State = GraphState.Loading;

            Name = d.name;
            Id = d.id;

            defaultTextureType = d.defaultTextureType;
            ShiftX = d.shiftX;
            ShiftY = d.shiftY;
            Zoom = d.zoom;

            width = d.width;
            height = d.height;

            AbsoluteSize = d.absoluteSize;

            Version = d.version ?? GRAPH_VERSION;

            if (width <= 0 || width == ushort.MaxValue) width = DEFAULT_SIZE;
            if (height <= 0 || height == ushort.MaxValue) height = DEFAULT_SIZE;

            RestoreBinaryCustomParameters(r);
            RestoreBinaryCustomFunctions(r);

            int count = r.NextInt();

            try
            {
                for (int i = 0; i < count; ++i)
                {
                    NodeDataType type = (NodeDataType)r.NextUShort();

                    int w = r.NextUShort();
                    int h = r.NextUShort();

                    string id = r.NextString();

                    restored.Add(id);

                    if (NodeLookup.TryGetValue(id, out Node n))
                    {
                        if (n != null)
                        {
                            n.SetSize(w, h);
                            RestoreNode(n, r, Archive);
                            continue;
                        }
                    }

                    n = CreateNodeFromType(type, w, h, defaultTextureType);
                    if (n == null)
                    {
                        Log.Debug("Node type does not exist: " + type);
                        continue;
                    }

                    n.AssignParentGraph(this);
                    n.Id = id;
                    NodeLookup[id] = n;
                    Nodes.Add(n);

                    if (n is GraphInstanceNode)
                    {
                        InstanceNodes.Add(n as GraphInstanceNode);
                    }
                    else if (n is PixelProcessorNode)
                    {
                        PixelNodes.Add(n as PixelProcessorNode);
                    }
                    else if (n is InputNode)
                    {
                        InputNodes.Add(n.Id);
                    }
                    else if (n is OutputNode)
                    {
                        OutputNodes.Add(n.Id);
                    }

                    LoadNode(n, r, Archive);
                }

                string[] keys = NodeLookup.Keys.ToArray();
                for (int i = 0; i < keys.Length; ++i)
                {
                    var k = keys[i];
                    if (!restored.Contains(k))
                    {
                        //node no longer exists
                        var n = NodeLookup[k];
                        Remove(n);
                    }
                }
            }
            catch
            {
                Log.Error("Failed to restore all nodes");
            }

            RestoreBinaryParameters(r);

            //for different graph types
            //that may have written extra needed data
            ReadExtended(r);

            if (!(this is Function))
            {
                SetConnections();
            }

            State = GraphState.Ready;
        }


        /// <summary>
        /// Only use this method if you have already parsed
        /// the header info without resetting the provided reader
        /// </summary>
        /// <param name="d">The d.</param>
        /// <param name="r">The r.</param>
        /// <param name="archive">The archive.</param>
        public virtual void FromBinary(GraphData d, Reader r, Archive archive = null)
        {
            if (d == null || r == null || r.Length == 0) return;

            Dictionary<string, Node> lookup = new Dictionary<string, Node>();
            State = GraphState.Loading;

            Dispose(); //ensure all previous data released

            Archive = archive;

            Name = d.name;
            Id = d.id;
            
            //OutputNodes = d.outputs;
            //InputNodes = d.inputs;
            
            defaultTextureType = d.defaultTextureType;

            ShiftX = d.shiftX;
            ShiftY = d.shiftY;
            Zoom = d.zoom;
            
            width = d.width;
            height = d.height;
            
            AbsoluteSize = d.absoluteSize;

            Version = d.version ?? GRAPH_VERSION;

            if (width <= 0 || width == ushort.MaxValue) width = DEFAULT_SIZE;
            if (height <= 0 || height == ushort.MaxValue) height = DEFAULT_SIZE;

            SetBinaryCustomParameters(r);
            SetBinaryCustomFunctions(r);

            int count = r.NextInt();

            try
            {
                for (int i = 0; i < count; ++i)
                {
                    NodeDataType type = (NodeDataType)r.NextUShort();
                    
                    int w = r.NextUShort();
                    int h = r.NextUShort();

                    string id = r.NextString();

                    Node n = CreateNodeFromType(type, w, h, defaultTextureType);
                    if (n == null)
                    {
                        Log.Debug("Node type does not exist: " + type);
                        continue;
                    }

                    n.AssignParentGraph(this);
                    n.Id = id;
                    lookup[id] = n;
                    Nodes.Add(n);

                    if (n is GraphInstanceNode)
                    {
                        InstanceNodes.Add(n as GraphInstanceNode);
                    }
                    else if (n is PixelProcessorNode)
                    {
                        PixelNodes.Add(n as PixelProcessorNode);
                    }
                    else if(n is InputNode)
                    {
                        InputNodes.Add(n.Id);
                    }
                    else if(n is OutputNode)
                    {
                        OutputNodes.Add(n.Id);
                    }

                    LoadNode(n, r, archive);
                }
            }
            catch
            {
                Log.Error("Failed to read all nodes");
            }

            //set node lookup here
            //for params / ReadExtended
            NodeLookup = lookup;

            SetBinaryParameters(r);

            //for different graph types
            //that may have written extra needed data
            ReadExtended(r);

            if (!(this is Function))
            {
                SetConnections();
            }

            State = GraphState.Ready;
        }

        #endregion

        #region Json Loading

        public virtual void FromJson(GraphData d, Archive archive = null)
        {
            if (d == null) return;

            Dictionary<string, Node> lookup = new Dictionary<string, Node>();

            State = GraphState.Loading;

            Dispose(); //ensure all previous data is released

            Archive = archive;

            Name = d.name;
            Id = string.IsNullOrEmpty(d.id) ? Id : d.id;

            //still trying to figure out why we stored these?
            //OutputNodes = d.outputs;
            //InputNodes = d.inputs;

            defaultTextureType = d.defaultTextureType;
            
            ShiftX = d.shiftX;
            ShiftY = d.shiftY;
            Zoom = d.zoom;
            
            width = d.width;
            height = d.height;

            AbsoluteSize = d.absoluteSize;
            Version = d.version ?? GRAPH_VERSION;

            if (width <= 0 || width == ushort.MaxValue) width = DEFAULT_SIZE;
            if (height <= 0 || height == ushort.MaxValue) height = DEFAULT_SIZE;

            SetJsonReadyCustomParameters(d.customParameters);
            SetJsonReadyCustomFunctions(d.customFunctions);

            int count = d.nodes.Count;

            //parse node data
            //setup initial object instances
            for (int i = 0; i < count; ++i)
            {
                string s = d.nodes[i];

                //this is killing us, because we are having to double process shit
                //deserialize once here
                NodeData nd = JsonConvert.DeserializeObject<NodeData>(s);

                if (nd != null)
                {
                    string type = nd.type;
                    Node n = CreateNodeFromType(type, nd.width, nd.height, defaultTextureType);
                    if (n == null)
                    {
                        Log.Debug("Node type does not exist: " + type);
                        continue;
                    }

                    n.AssignParentGraph(this);
                    n.Id = nd.id;
                    lookup[nd.id] = n;
                    Nodes.Add(n);

                    if (n is GraphInstanceNode)
                    {
                        InstanceNodes.Add(n as GraphInstanceNode);
                    }
                    else if (n is PixelProcessorNode)
                    {
                        PixelNodes.Add(n as PixelProcessorNode);
                    }
                    else if (n is InputNode)
                    {
                        InputNodes.Add(n.Id);
                    }
                    else if (n is OutputNode)
                    {
                        OutputNodes.Add(n.Id);
                    }

                    //deserialize again in the node load
                    LoadNode(n, s, archive);
                }
            }

            NodeLookup = lookup;
            SetJsonReadyParameters(d.parameters);

            if (!(this is Function))
            {
                SetConnections();
            }

            State = GraphState.Ready;
        }

        public virtual void FromJson(string data, Archive archive = null)
        {
            List<Archive.ArchiveFile> archiveFiles = null;
            if (string.IsNullOrEmpty(data) && archive != null)
            {
                archive.Open();
                archiveFiles = archive.GetAvailableFiles();
                //there is only ever one .mtg in an archive
                var mtgFile = archiveFiles.Find(m => m.path.EndsWith(".mtg"));
                if (mtgFile == null)
                {
                    return;
                }

                data = mtgFile.ExtractText();
                archive.Close();
            }
            else if (string.IsNullOrEmpty(data) && archive == null)
            {
                return;
            }


            //depending on graph data size, this can take upwards of 16+ milliseconds which is bad
            GraphData d = JsonConvert.DeserializeObject<GraphData>(data);

            if (d != null)
            {
                FromJson(d, archive);
            }
        }

        private void RegisterFunctionNode(Node n)
        {
            //slight optimization for function graphs
            MathNode mn = n as MathNode;
            Function fg = this as Function;
            mn.AssignParentNode(fg.parentNode);

            if (n is ExecuteNode && fg.Execute == null)
            {
                fg.Execute = n as ExecuteNode;
            }
            if (n is ArgNode)
            {
                if (!fg.Args.Contains(n as ArgNode))
                {
                    fg.Args.Add(n as ArgNode);
                }
            }
            else if (n is CallNode)
            {
                if (!fg.Calls.Contains(n as CallNode))
                {
                    fg.Calls.Add(n as CallNode);
                }
            }
            else if (n is SamplerNode)
            {
                if (!fg.Samplers.Contains(n))
                {
                    fg.Samplers.Add(n as SamplerNode);
                }
            }
            else if (n is ForLoopNode)
            {
                if (!fg.ForLoops.Contains(n))
                {
                    fg.ForLoops.Add(n as ForLoopNode);
                }
            }
        }

        private void RestoreNode(Node n, Reader r, Archive arch = null)
        {
            if (n is MathNode && this is Function)
            {
                RegisterFunctionNode(n);
            }

            if (n is GraphInstanceNode)
            {
                var gi = n as GraphInstanceNode;
                gi.Restore(r, arch);
            }
            else
            {
                n.FromBinary(r, arch);
            }

            originSizes[n.Id] = new PointF(n.Width, n.Height);
        }

        private void LoadNode(Node n, Reader r, Archive archive = null)
        {
            if (n is MathNode && this is Function)
            {
                RegisterFunctionNode(n);
            }

            n.FromBinary(r, archive);

            originSizes[n.Id] = new PointF(n.Width, n.Height);
        }

        private void LoadNode(Node n, string data, Archive archive = null)
        {
            if (n is MathNode && this is Function)
            {
                RegisterFunctionNode(n);
            }

            n.FromJson(data, archive);

            //origin sizes are only for graph instances
            //not actually used in the current one being edited
            //it is used in the ResizeWith
            originSizes[n.Id] = new PointF(n.Width, n.Height);
        }

        public void SetConnections()
        {
            //finally after every node is populated
            //try and connect them all!
            int count = Nodes.Count;
            for (int i = 0; i < count; ++i)
            {
                Node n = Nodes[i];
                n.RestoreConnections(NodeLookup, true);
            }

            OnLoad?.Invoke(this);
        }
        #endregion

        #region Copy & Paste Param Support
        public void PasteParameters(Dictionary<string, string> cparams, NodeData from, Node to)
        {
            var t = to.GetType();
            var props = t.GetProperties();

            foreach(var prop in props)
            {
                string pdata = null;
                string cid = from.id + "." + prop.Name;
                string nid = to.Id + "." + prop.Name;
                if(cparams.TryGetValue(cid, out pdata))
                {
                    ParameterValue gv = ParameterValue.FromJson(pdata, to);
                    Parameters[nid] = gv;
                }
            }
        }

        public virtual Dictionary<string, string> CopyParameters(Node n)
        {
            Dictionary<string, string> cparams = new Dictionary<string, string>();
            var t = n.GetType();
            var props = t.GetProperties();

            foreach(var prop in props)
            {
                if(HasParameterValue(n.Id, prop.Name))
                {
                    string cid = n.Id + "." + prop.Name;
                    cparams[cid] = GetParameterRaw(n.Id, prop.Name).GetJson();
                }
            }
            return cparams;
        }
        #endregion

        #region Parameters Add / Remove / Access / Clear
        public bool IsParameterValueFunction(string id, string parameter)
        {
            string cid = id + "." + parameter;

            ParameterValue v = null;

            if (Parameters.TryGetValue(cid, out v))
            {
                return v.IsFunction();
            }

            return false;
        }

        public bool HasParameterValue(string id, string parameter)
        {
            string cid = id + "." + parameter;

            return Parameters.ContainsKey(cid);
        }

        public ParameterValue GetParameterRaw(string id, string parameter)
        {
            ParameterValue p = null;

            string cid = id + "." + parameter;

            Parameters.TryGetValue(cid, out p);

            return p;
        }

        public object GetParameterValue(string id, string parameter)
        {
            string cid = id + "." + parameter;

            ParameterValue p = null;

            if (Parameters.TryGetValue(cid, out p))
            {
                if (p.IsFunction())
                {
                    Function g = p.Value as Function;

                    g.PrepareUniforms();

                    if(g.BuildAsShader)
                    {
                        g.ComputeResult();
                    }
                    else
                    {
                        g.TryAndProcess();
                    }

                    return g.Result;
                }
                else
                {
                    return p.Value;
                }
            }

            return null;
        } 

        public T GetParameterValue<T>(string id, string parameter)
        {
            string cid = id + "." + parameter;

            ParameterValue p = null;

            if(Parameters.TryGetValue(cid, out p))
            {
                if(p.IsFunction())
                {
                    Function g = p.Value as Function;

                    g.PrepareUniforms();

                    if (g.BuildAsShader)
                    {
                        g.ComputeResult();
                    }
                    else
                    {
                        g.TryAndProcess();
                    }

                    if (g.Result == null)
                    {
                        return default(T);
                    }

                    return (T)g.Result;
                }
                else
                {
                    return (T)p.Value;
                }
            }

            return default(T);
        }

        public void RemoveParameterValueNoDispose(string key)
        {
            ParameterValue p = null;
            if (Parameters.TryGetValue(key, out p))
            {
                p.OnParameterUpdate -= Graph_OnGraphParameterUpdate;
                p.OnParameterTypeChanged -= Graph_OnGraphParameterTypeChanged;
                if (p.IsFunction())
                {
                    ParameterFunctions.Remove(key);
                }
            }

            Parameters.Remove(key);

            Modified = true;

            Updated();
        }

        public void RemoveParameterValue(string id, string parameter)
        {
            string cid = id + "." + parameter;

            ParameterValue p = null;

            if (Parameters.TryGetValue(cid, out p))
            {
                p.OnParameterUpdate -= Graph_OnGraphParameterUpdate;
                p.OnParameterTypeChanged -= Graph_OnGraphParameterTypeChanged;

                if (p.IsFunction())
                {
                    Function g = p.Value as Function;
                    g.AssignParentGraph(null);
                    g.AssignParentNode(null);
                    g.Dispose();

                    ParameterFunctions.Remove(cid);
                }
            }

            Parameters.Remove(cid);

            Modified = true;

            Updated();
        }

        public void SetParameterValue(string id, string parameter, object v, bool overrideType = false, NodeType toverride = NodeType.Float)
        {
            string cid = id + "." + parameter;

            ParameterValue p = null;

            if (Parameters.TryGetValue(cid, out p))
            {
                if (p.IsFunction() && v != p.Value)
                {
                    Function g = p.Value as Function;
                    g.AssignParentGraph(null);
                    g.AssignParentNode(null);
                    g.Dispose();
                    ParameterFunctions.Remove(cid);
                }

                if(v is Function)
                {
                    p.Type = (v as Function).ExpectedOutput;
                }
                else if (v is float || v is int || v is double || v is long)
                {
                    p.Type = NodeType.Float;
                }
                else if (v is bool)
                {
                    p.Type = NodeType.Bool;
                }
                else if (v is MVector)
                {
                    p.Type = NodeType.Float4;
                }

                if(overrideType)
                {
                    p.Type = toverride;
                }

                p.Value = v;
            }
            else
            {
                NodeType t = NodeType.Float;
                if(v is Function)
                {
                    t = (v as Function).ExpectedOutput;
                }
                else if (v is float || v is int || v is double || v is long)
                {
                    t = NodeType.Float;
                }
                else if (v is bool)
                {
                    t = NodeType.Bool;
                }
                else if (v is MVector)
                {
                    t = NodeType.Float4;
                }

                if(overrideType)
                {
                    t = toverride;
                }

                p = Parameters[cid] = new ParameterValue(parameter, v, "", t);
                p.Key = cid;
                p.ParentGraph = this;
                p.OnParameterUpdate += Graph_OnGraphParameterUpdate;
                p.OnParameterTypeChanged += Graph_OnGraphParameterTypeChanged;
            }

            if (v is Function)
            {
                Function vg = v as Function;
                vg.AssignParentGraph(this);
                ParameterFunctions[cid] = vg;
            }

            Modified = true;

            Updated();
        }

        protected virtual void ClearParameters()
        {
            if (CustomParameters != null)
            {
                int count = CustomParameters.Count;
                for (int i = 0; i < count; ++i)
                {
                    var param = CustomParameters[i];
                    if (param.IsFunction())
                    {
                        Function fn = param.Value as Function;
                        fn.Dispose();
                    }
                }

                CustomParameters.Clear();
            }

            if (CustomFunctions != null)
            {
                int count = CustomFunctions.Count;
                for (int i = 0; i < count; ++i)
                {
                    var f = CustomFunctions[i];
                    f.Dispose();
                }

                CustomFunctions.Clear();
            }

            if (Parameters != null)
            {
                try
                {
                    foreach (var param in Parameters.Values)
                    {
                        if (param.IsFunction())
                        {
                            Function fn = param.Value as Function;
                            fn.Dispose();
                        }
                    }
                }
                catch (Exception e)
                {

                }

                Parameters.Clear();
            }
        }
        #endregion

        #region Undo / Redo
        public void Undo()
        {
            byte[] binaryData = null;
            Task.Run(() =>
            {
                if (previousByteData == null || previousByteData.Length == 0)
                {
                    return;
                }

                if (undo.Count == 0)
                {
                    return;
                }

                byte[] diff = null;

                if (undo.TryPop(out diff))
                {
                    //we only ever store the last 100 redos
                    if (redo.Count >= 100)
                    {
                        List<byte[]> queue = redo.ToList();
                        queue.Reverse();
                        redo.Clear();

                        //delete the first 10 redos
                        for (int i = 10; i < queue.Count; ++i)
                        {
                            redo.Push(queue[i]);
                        }
                    }

                    var redoPrevious = previousByteData;

                    //undo the diff
                    using (MemoryStream output = new MemoryStream())
                    using (ByteBuffer dict = new ByteBuffer(previousByteData))
                    using (ByteBuffer delta = new ByteBuffer(diff))
                    {
                        long bytesWritten = 0;
                        VCDecoder decoder = new VCDecoder(dict, delta, output);
                        var result = decoder.Start();
                        while (result == VCDiffResult.SUCCESS)
                        {
                            result = decoder.Decode(out bytesWritten);
                        }

                        previousByteData = output.ToArray();
                        binaryData = previousByteData;
                    }

                    //create the proper redo diff
                    using (MemoryStream output = new MemoryStream())
                    using (ByteBuffer target = new ByteBuffer(redoPrevious))
                    using (ByteBuffer dict = new ByteBuffer(previousByteData))
                    {
                        VCCoder coder = new VCCoder(dict, target, output);
                        if (coder.Encode(false, true) == VCDiffResult.SUCCESS)
                        {
                            redo.Push(output.ToArray());
                        }
                    }
                }
            }).ContinueWith(t =>
            {
                if (binaryData == null || binaryData.Length == 0) return;
                //do not dispose instead do a restore
                //restore is differiental for the graph
                //it will update / add / remove as needed
                //it will also prevent reloading entire graph
                //instances and instead just transfer the data
                //back to the graph instance
                Restore(new Reader(binaryData));
                OnUndo?.Invoke(this);
            }, Node.Context);
        }

        public void Redo()
        {
            byte[] binaryData = null;
            Task.Run(() =>
            {
                if (previousByteData == null || previousByteData.Length == 0)
                {
                    return;
                }

                if (redo.Count == 0)
                {
                    return;
                }

                byte[] diff = null;

                if (redo.TryPop(out diff))
                {
                    var undoPrevious = previousByteData;

                    //redo the diff
                    using (MemoryStream output = new MemoryStream())
                    using (ByteBuffer dict = new ByteBuffer(previousByteData))
                    using (ByteBuffer delta = new ByteBuffer(diff))
                    {
                        long bytesWritten = 0;
                        VCDecoder decoder = new VCDecoder(dict, delta, output);
                        var result = decoder.Start();
                        while (result == VCDiffResult.SUCCESS)
                        {
                            result = decoder.Decode(out bytesWritten);
                        }

                        previousByteData = output.ToArray();
                        binaryData = previousByteData;
                    }

                    //create the proper undo diff
                    using (MemoryStream output = new MemoryStream())
                    using (ByteBuffer target = new ByteBuffer(undoPrevious))
                    using (ByteBuffer dict = new ByteBuffer(previousByteData))
                    {
                        VCCoder coder = new VCCoder(dict, target, output);
                        if (coder.Encode(false, true) == VCDiffResult.SUCCESS)
                        {
                            undo.Push(output.ToArray());
                        }
                    }
                }
            }).ContinueWith(t =>
            {
                if (binaryData == null || binaryData.Length == 0) return;
                //do not dispose instead do a restore
                //restore is differiental for the graph
                //it will update / add / remove as needed
                //it will also prevent reloading entire graph
                //instances and instead just transfer the data
                //back to the graph instance
                Restore(new Reader(binaryData));
                OnRedo?.Invoke(this);
            }, Node.Context);
        }

        public void Snapshot()
        {
            Task.Run(() =>
            {
                using (Writer w = new Writer())
                {
                    GetBinary(w);
                    var segment = w.Buffer;
                    byte[] newData = new byte[segment.Count];
                    Array.Copy(segment.Array, segment.Offset, newData, 0, segment.Count);

                    if (previousByteData == null || previousByteData.Length == 0)
                    {
                        previousByteData = newData;
                        return;
                    }

                    //otherwise take a snapshot of current graph
                    //compared to previous graph using vcdiff

                    //verify max storage
                    if (undo.Count >= 100)
                    {
                        List<byte[]> queue = undo.ToList();
                        queue.Reverse();
                        undo.Clear();

                        //delete the first 10 undos
                        for (int i = 10; i < queue.Count; ++i)
                        {
                            undo.Push(queue[i]);
                        }
                    }

                    using (MemoryStream output = new MemoryStream())
                    using (ByteBuffer target = new ByteBuffer(previousByteData))
                    using (ByteBuffer dict = new ByteBuffer(newData))
                    {
                        VCCoder coder = new VCCoder(dict, target, output);
                        if (coder.Encode(false, true) == VCDiffResult.SUCCESS)
                        {
                            undo.Push(output.ToArray());
                        }
                    }

                    previousByteData = newData;
                }
            });
        }

        /// <summary>
        /// exports all undo redo stacks for the entire graph, params, pixel nodes etc
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, UndoRedoData> ExportUndoRedo()
        {
            Dictionary<string, UndoRedoData> data = new Dictionary<string, UndoRedoData>();
            data[Id] = new UndoRedoData(UndoStack, RedoStack, previousByteData);

            var cfuncs = CustomFunctions;
            for (int i = 0; i < cfuncs.Count; ++i)
            {
                var f = cfuncs[i];
                data[f.Id] = new UndoRedoData(f.UndoStack, f.RedoStack, f.previousByteData);
            }

            var pfuncs = PixelNodes;

            for (int i = 0; i < pfuncs.Count; ++i)
            {
                var n = pfuncs[i];
                var f = n.Function;
                data[f.Id] = new UndoRedoData(f.UndoStack, f.RedoStack, f.previousByteData);
            }

            try
            {
                foreach (var f in ParameterFunctions.Values)
                {
                    data[f.Id] = new UndoRedoData(f.UndoStack, f.RedoStack, f.previousByteData);
                }
            }
            catch { }

            return data;
        }

        /// <summary>
        /// imports alll undo / redo stacks
        /// </summary>
        /// <param name="data"></param>
        public void ImportUndoRedo(Dictionary<string, UndoRedoData> data)
        {
            try
            {
                if (data.TryGetValue(Id, out UndoRedoData rootData))
                {
                    UndoStack = rootData.undos;
                    RedoStack = rootData.redos;
                    previousByteData = rootData.previousState;
                }

                //lookup table
                Dictionary<string, Function> lookup = new Dictionary<string, Function>();

                //we do this so we are not continually accessing the getter method
                //of the CustomFunction property
                List<Function> cfuncs = CustomFunctions;

                for (int i = 0; i < cfuncs.Count; ++i)
                {
                    var f = cfuncs[i];
                    lookup[f.Id] = f;
                }

                var pfuncs = PixelNodes;
                for (int i = 0; i < pfuncs.Count; ++i)
                {
                    var n = pfuncs[i];
                    var f = n.Function;
                    lookup[f.Id] = f;
                }

                try
                {
                    foreach (var f in ParameterFunctions.Values)
                    {
                        lookup[f.Id] = f;
                    }
                }
                catch { }

                foreach (string k in data.Keys)
                {
                    //we have already taken care of this one above
                    if (k == Id) continue;
                    var stackData = data[k];
                    if (lookup.TryGetValue(k, out var f))
                    {
                        f.UndoStack = stackData.undos;
                        f.RedoStack = stackData.redos;
                        f.previousByteData = stackData.previousState;
                    }
                }
            }
            catch { }
        }


        #endregion

        #region Sub Events
        private void Graph_OnGraphParameterUpdate(ParameterValue param)
        {
            OnParameterUpdate?.Invoke(param);
        }

        private void Graph_OnGraphParameterTypeChanged(ParameterValue param)
        {
            OnParameterTypeUpdate?.Invoke(param);
        }

        protected virtual void Updated()
        {
            OnUpdate?.Invoke(this);
        }
        #endregion

        #region Save Helper Accessible Without UI
        public static bool WriteToFile(Graph g, string cwd, string path, bool saveAs = false)
        {
            bool isArchive = Path.GetExtension(path).EndsWith("mtga");
            string archivePath = path;
            path = path.Replace(".mtga", ".mtg");

            if (g == null) return false;

            g.CopyResources(cwd, saveAs);

            using (Writer w = new Writer())
            {
                g.GetBinary(w);
                var buffer = w.Buffer;
                using (var stream = File.Open(path,
                                    FileMode.OpenOrCreate | FileMode.Truncate,
                                    FileAccess.Write))
                {
                    stream.Write(buffer.Array, buffer.Offset, buffer.Count);
                    stream.Flush();
                }
            }

            if (isArchive)
            {
                var arch = new Archive(archivePath);
                if (!arch.Create(path))
                {
                    Log.Error("Failed to create materia graph archive file");
                    return false;
                }
            }

            g.Name = saveAs ? Path.GetFileNameWithoutExtension(path) : g.name;

            return true;
        }
        #endregion

        #region Disposal
        public virtual void Dispose()
        {
            if (Nodes != null)
            {
                int count = Nodes.Count;
                for (int i = 0; i < count; ++i)
                {
                    var n = Nodes[i];
                    n.Dispose();
                }

                Nodes.Clear();
            }

            NodeLookup?.Clear();
            OutputNodes?.Clear();
            InputNodes?.Clear();

            PixelNodes?.Clear();
            InstanceNodes?.Clear();

            ClearParameters();
        }
        #endregion
    }
}
