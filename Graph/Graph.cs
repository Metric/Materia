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

namespace Materia.Graph
{ 
    public enum GraphPixelType
    {
        RGBA = PixelInternalFormat.Rgba8,
        RGBA16F = PixelInternalFormat.Rgba16f,
        RGBA32F = PixelInternalFormat.Rgba32f,
        RGB = PixelInternalFormat.Rgb8,
        RGB16F = PixelInternalFormat.Rgb16f,
        RGB32F = PixelInternalFormat.Rgb32f,
        Luminance16F = PixelInternalFormat.R16f,
        Luminance32F = PixelInternalFormat.R32f,
    }

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

    public enum GraphState
    {
        Loading,
        Ready
    }

    public class Graph : IDisposable
    {
        public const float GRAPH_VERSION = 1.2f;

        public static bool ShaderLogging { get; set; }

        public static int[] GRAPH_SIZES = new int[] { 8, 16, 32, 64, 128, 256, 512, 1024, 2048, 4096, 8192 };
        public const int DEFAULT_SIZE = 512;

        public delegate void GraphUpdate(Graph g);
        public delegate void ParameterUpdate(ParameterValue p);
        public event GraphUpdate OnUpdate;
        public event GraphUpdate OnNameChange;
        public event ParameterValue.GraphParameterUpdate OnParameterUpdate;
        public event ParameterValue.GraphParameterUpdate OnParameterTypeUpdate;
        public event GraphUpdate OnLoad;
        public event GraphUpdate OnUndo;
        public event GraphUpdate OnRedo;

        public GraphState State { get; protected set; }

        public bool ReadOnly { get; set; }

        public float Version { get; protected set; }

        public bool Modified { get; set; }

        public string CWD { get; set; }
        public List<Node> Nodes { get; protected set; }
        public Dictionary<string, Node> NodeLookup { get; protected set; }
        public List<string> OutputNodes { get; protected set; }
        public List<string> InputNodes { get; protected set; }

        protected Dictionary<string, VariableDefinition> Variables { get; set; }
        protected Dictionary<string, PointD> OriginSizes;

        public Archive Archive { get; protected set; }

        /// <summary>
        /// Stores the new vcdiff values of the graph for undo / redo
        /// </summary>
        protected ConcurrentQueue<byte[]> undo;
        protected ConcurrentQueue<byte[]> redo;

        /// <summary>
        /// The previous byte data for undo / redo
        /// </summary>
        protected byte[] previousByteData;

        /// <summary>
        /// Parameters are only available for image graphs
        /// </summary>
        [Editable(ParameterInputType.MapEdit, "Promoted Parameters", "Promoted Parameters")]
        public Dictionary<string, ParameterValue> Parameters { get; protected set; }
             
        [Editable(ParameterInputType.MapEdit, "Custom Parameters", "Custom Parameters")]
        public List<ParameterValue> CustomParameters { get; protected set; }

        [Editable(ParameterInputType.MapEdit, "Custom Functions", "Custom Fuctions")]
        public List<Function> CustomFunctions { get; protected set; }

        //this is a container
        //that is filled when a parameter is promoted to a function graph
        //it is also filled when we load a graph
        //its primary use is for the editor
        //so it can list all available promoted parameters to functions
        //even if the parameter has been renamed or changed in the underlying graph
        //that way the graph can be cleaned up / properly updated to handle the changes
        //by the user
        [Editable(ParameterInputType.MapEdit, "Promoted Functions", "Promoted Functions")]
        public Dictionary<string, Function> ParameterFunctions { get; protected set; }

        /// <summary>
        /// this allows for a quick reference look up
        /// of layers
        /// </summary>
        public Dictionary<string, Layer> LayerLookup { get; protected set; }

        /// <summary>
        /// These layers are layers that are not children
        /// and are root layers
        /// </summary>
        public List<Layer> Layers { get; protected set; }

        /// <summary>
        /// This takes into account all possible
        /// render outputs for layered rendering
        /// </summary>
        public Dictionary<OutputType, Node> Render { get; protected set; }

        
        /// <summary>
        /// Helper array
        /// to quickly go through graph instance nodes
        /// </summary>
        public List<GraphInstanceNode> InstanceNodes { get; set; }

        /// <summary>
        /// Helper to go through pixel processor nodes
        /// </summary>
        protected List<PixelProcessorNode> PixelNodes { get; set; }

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

        public double ShiftX { get; set; }

        public double ShiftY { get; set; }

        public float Zoom { get; set; }

        protected GraphPixelType defaultTextureType;

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

        protected int randomSeed;

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
                        TryAndProcess();
                    }
                }
            }
        }

        protected int width;
        protected int height;

        public int Width
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

        public int Height
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
        protected Node parentNode;
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
        protected Graph parentGraph;
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

        public class GraphData
        {
            public string name;
            public List<string> nodes;
            public List<string> outputs;
            public List<string> inputs;
            public GraphPixelType defaultTextureType;

            public double shiftX;
            public double shiftY;
            public float zoom;

            public int width;
            public int height;
            public bool absoluteSize;

            public Dictionary<string, string> parameters;
            public List<string> customParameters;
            public List<string> customFunctions;
            public List<string> layers;

            public float? version;
        }

        protected ConcurrentQueue<Node> scheduledNodes;
        public bool IsProcessing { get; protected set; }

        /// <summary>
        /// A graph must be instantiated on the UI / Main Thread
        /// So it can acquire the proper TaskScheduler for nodes
        /// </summary>
        /// <param name="name"></param>
        /// <param name="w"></param>
        /// <param name="h"></param>
        /// <param name="async"></param>
        public Graph(string graphName, int w = DEFAULT_SIZE, int h = DEFAULT_SIZE)
        {
            Version = GRAPH_VERSION;

            if (Node.Context == null)
            {
                Node.Context = TaskScheduler.FromCurrentSynchronizationContext();
            }

            undo = new ConcurrentQueue<byte[]>();
            redo = new ConcurrentQueue<byte[]>();

            State = GraphState.Ready;

            name = graphName;
            Zoom = 1;
            ShiftX = ShiftY = 0;
            width = w;
            height = h;

            scheduledNodes = new ConcurrentQueue<Node>();

            PixelNodes = new List<PixelProcessorNode>();
            InstanceNodes = new List<GraphInstanceNode>();

            Layers = new List<Layer>();
            LayerLookup = new Dictionary<string, Layer>();
            Render = new Dictionary<OutputType, Node>();
            Variables = new Dictionary<string, VariableDefinition>();
            defaultTextureType = GraphPixelType.RGBA;
            Nodes = new List<Node>();
            NodeLookup = new Dictionary<string, Node>();
            OutputNodes = new List<string>();
            InputNodes = new List<string>();
            OriginSizes = new Dictionary<string, PointD>();
            Parameters = new Dictionary<string, ParameterValue>();
            CustomParameters = new List<ParameterValue>();
            CustomFunctions = new List<Function>();
            ParameterFunctions = new Dictionary<string, Function>();
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
                CWD = cwd;
            }
        }

        public void PollScheduled()
        {
            if (scheduledNodes.Count > 0)
            {
                IsProcessing = true;
                Node n = null;
                scheduledNodes.TryDequeue(out n);

                if (n == null) return;

                if (!(n is GraphInstanceNode))
                {
                    n.TryAndProcess();
                }
                else
                {
                    //todo: reimplement layer support later
                    //(n as GraphInstanceNode).GraphInst?.CombineLayers();
                    n.TriggerTextureChange();
                }

                n.IsScheduled = false;
            }
            else if (IsProcessing)
            {
                IsProcessing = false;
                CombineLayers();
            }
        }

        public virtual void Schedule(Node n)
        {
            if (n.IsScheduled) return;
            if (n.ParentGraph == null) return;
            if (IsProcessing) return;
            IsProcessing = true;
            Task.Run(() =>
            {
                Node realNode = n; 
                Queue<Node> queue = new Queue<Node>();
                List<Node> starting = realNode.ParentGraph.EndNodes;

                Node endNode = realNode;

                if(starting.Contains(realNode))
                {
                    //if this node is an end node itself
                    //we can ignore all other end nodes
                    //as a point of rebuilding
                    starting.Clear();
                    starting.Add(realNode);
                }

                if (realNode is GraphInstanceNode)
                {
                    GraphInstanceNode gn = realNode as GraphInstanceNode;
                    NodeInput inp = gn.Inputs.Find(m => m.HasInput);

                    if (inp != null)
                    {
                        endNode = inp.ParentNode;
                    }
                    else
                    {
                        endNode = null;
                    }
                }
                else
                {
                    endNode = realNode;
                }

                if (starting.Count > 1 || (starting.Count == 1 && starting[0] != endNode))
                {
                    GatherNodes(starting, queue, endNode);
                }
                else
                {
                    queue.Enqueue(endNode);
                }

                Node[] nodesToSchedule = queue.ToArray();
                for(int i = 0; i < nodesToSchedule.Length; ++i)
                {
                    scheduledNodes.Enqueue(nodesToSchedule[i]);
                }
            });
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

                if(n.Reference.ParentNode is GraphInstanceNode)
                {
                    GraphInstanceNode gn = n.Reference.ParentNode as GraphInstanceNode;
                    if (!inStack.Contains(gn))
                    { 
                        items.Add(gn);
                    }
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
                        if (backtracks.Count > 0)
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

                            if (inp.Reference.ParentNode is GraphInstanceNode)
                            {
                                GraphInstanceNode gr = inp.Reference.ParentNode as GraphInstanceNode;
                                if (!inStack.Contains(gr))
                                {
                                    items.Add(gr);
                                }
                            }

                            inStack.Add(inp.Reference.Node);
                            stack.Enqueue(inp.Reference.Node);
                        }
                    }
                }
            }

            return items;
        }

        public static void GatherNodes(List<Node> startingNodes, Queue<Node> queue, Node endNode)
        {
            HashSet<Node> inStack = new HashSet<Node>();
            Queue<Node> stack = new Queue<Node>();

            for(int i = 0; i < startingNodes.Count; ++i)
            {
                if (startingNodes[i] is GraphInstanceNode)
                {
                    if (startingNodes[i].Outputs.Count > 0)
                    {
                        stack.Enqueue(startingNodes[i].Outputs[0].Node);
                        inStack.Add(startingNodes[i].Outputs[0].Node);
                    }
                }
                else
                {
                    stack.Enqueue(startingNodes[i]);
                    inStack.Add(startingNodes[i]);
                }
            }

            while(stack.Count > 0)
            {
                Node n = stack.Dequeue();

                if (n.IsScheduled)
                {
                    continue;
                }

                List<List<Node>> backtracks = new List<List<Node>>();
                for(int i = 0; i < n.Inputs.Count; ++i)
                {
                    List<Node> backs = Backtrack(n.Inputs[i], endNode, inStack);

                    if(endNode != null)
                    {
                        if (!backs.Contains(endNode)) continue;
                    }

                    if(backs.Count > 0)
                    {
                        backtracks.Add(backs);
                    }
                }

                for(int i = 0; i < backtracks.Count; ++i)
                {
                    List<Node> backs = backtracks[i];
                    for(int j = backs.Count - 1; j >= 0; --j)
                    {
                        Node next = backs[j];

                        if (next.IsScheduled) continue;
                        next.IsScheduled = true;

                        //go ahead and populate params if needed
                        if (next is GraphInstanceNode)
                        {
                            GraphInstanceNode gn = next as GraphInstanceNode;
                            gn.PopulateGraphParams();
                        }

                        queue.Enqueue(next);
                    }
                }

                n.IsScheduled = true;
                    
                //go ahead and populate params if needed
                if (n is GraphInstanceNode)
                {
                    GraphInstanceNode gn = n as GraphInstanceNode;
                    gn.PopulateGraphParams();
                }
                else if (n is OutputNode)
                {
                    OutputNode op = n as OutputNode;
                    if (op.Outputs.Count > 0)
                    {
                        if (op.Outputs[0].ParentNode is GraphInstanceNode)
                        {
                            GraphInstanceNode gn = op.Outputs[0].ParentNode as GraphInstanceNode;
                            gn.IsScheduled = true;
                            gn.PopulateGraphParams();
                            queue.Enqueue(gn);
                        }
                    }
                }

                queue.Enqueue(n);
            }
        }

        public virtual void CombineLayers()
        {
            //todo: reimplement layer support later
            //make sure render textures are available
            //InitializeRenderTextures();

            //int count = Layers.Count;
            //for(int i = count - 1; i >= 0; --i)
            //{
            //    Layer l = Layers[i];
            //    l.Combine(Render);
            //}

            //foreach(OutputNode n in Render.Values)
            //{
            //    n.TriggerTextureChange();
            //}
        }

        public virtual void InitializeRenderTextures()
        {
            //todo: reimplement layer support later
            //foreach(string id in OutputNodes)
            //{
            //    Node n = null;
            //    if (NodeLookup.TryGetValue(id, out n))
            //    {
            //        OutputNode output = n as OutputNode;

            //        if (output == null) continue;

            //we reprocess output node here
            //to enusre we have latest 
            //data from prior node
            //        output.TryAndProcess();
            //        Render[output.OutType] = output;
            //    }
            //}
        }

        public virtual bool HasVar(string k)
        {
            return Variables.ContainsKey(k);
        }

        public virtual object GetVar(string k)
        {
            if (k == null) return null;

            if(Variables.ContainsKey(k))
            {
                return Variables[k].Value;
            }

            return null;
        }

        public virtual string[] GetAvailableVariables(NodeType type)
        {
            List<string> available = new List<string>();

            try
            {
                foreach (string k in Variables.Keys)
                {
                    VariableDefinition o = Variables[k];

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
                var proc = graphinsts[i] as GraphInstanceNode;

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
            Variables.Remove(k);
        }

        public virtual void SetVar(string k, object v, NodeType type)
        {
            Variables[k] = new VariableDefinition(v, type);
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

        public virtual void TryAndProcess()
        {    
            //todo: reimplement layer support later
            //int c = Layers.Count;
            //for(int i = 0; i < c; ++i)
            //{
                //no need to process layers
                //that are invisible
            //    if (Layers[i].Visible)
            //    {
            //        Layers[i].TryAndProcess();
            //    }
            //}

            
            Task.Run(() =>
            {
                List<Node> root = EndNodes;
                Queue<Node> nodes = new Queue<Node>();
                GatherNodes(root, nodes, null);
                Node[] nodesToSchedule = nodes.ToArray();
                Graph g = Top();
                ConcurrentQueue<Node> scheduled = g.scheduledNodes;
                for (int i = 0; i < nodesToSchedule.Length; ++i)
                {
                   scheduled.Enqueue(nodesToSchedule[i]);
                }
            });
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
                    PointD osize;

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

            //resize graph layers as well
            //c = Layers.Count;
            //for (int i = 0; i < c; ++i)
            //{
            //    Layer l = Layers[i];

            //    l.Core?.ResizeWith(width, height);
            //}

            Modified = true;
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

            //todo: reimplement layers
            //List<string> layerData = new List<string>();
            //count = Layers.Count;
            //for (int i = 0; i < count; ++i)
            //{
            //    Layer l = Layers[i];
            //    layerData.Add(l.GetJson());
            //}

            d.name = Name;
            d.nodes = data;
            d.outputs = OutputNodes;
            d.inputs = InputNodes;
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
            d.layers = new List<string>(); //todo: reimplement layers
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
                    if (!Parameters[k].IsFunction())
                    {
                        parameters[k] = Parameters[k].Value;
                    }
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
                        param.OnGraphParameterTypeChanged += Graph_OnGraphParameterTypeChanged;
                        param.OnGraphParameterUpdate += Graph_OnGraphParameterUpdate;

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

        public bool RemoveCustomParameter(ParameterValue p)
        {
            if(CustomParameters.Remove(p))
            {
                p.ParentGraph = null;
                p.OnGraphParameterUpdate -= Graph_OnGraphParameterUpdate;
                p.OnGraphParameterTypeChanged -= Graph_OnGraphParameterTypeChanged;
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
            p.OnGraphParameterTypeChanged += Graph_OnGraphParameterTypeChanged;
            p.OnGraphParameterUpdate += Graph_OnGraphParameterUpdate;
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
                    param.OnGraphParameterTypeChanged += Graph_OnGraphParameterTypeChanged;
                    param.OnGraphParameterUpdate += Graph_OnGraphParameterUpdate;
                    CustomParameters.Add(param);
                }
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
                    if (t.Equals(typeof(OutputNode)))
                    {
                        var n = new OutputNode(defaultTextureType);
                        return n;
                    }
                    else if (t.Equals(typeof(InputNode)))
                    {
                        var n = new InputNode(defaultTextureType);
                        return n;
                    }
                    else if (t.Equals(typeof(CommentNode)) || t.Equals(typeof(PinNode)))
                    {
                        Node n = (Node)Activator.CreateInstance(t);
                        return n;
                    }
                    else
                    {
                        Node n = (Node)Activator.CreateInstance(t, width, height, defaultTextureType);
                        return n;
                    }
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

            if(type.Contains("/") || type.Contains("\\") || type.Contains("Materia::Layer::"))
            {
                var n = new GraphInstanceNode(width, height);
                return n;
            }

            try
            {
                Type t = Type.GetType(type);
                return CreateNode(t);
            }
            catch (Exception e)
            {
                Log.Error(e);
            }

            return null;
        }
        #endregion

        #region Json Loading
        protected virtual void LayersFromJson(GraphData d, Archive archive = null)
        {
            if (d == null) return;
            LayerLookup = new Dictionary<string, Layer>();
            Layers = new List<Layer>();

            if (d.layers != null)
            {
                foreach(string l in d.layers)
                {
                    Layer layer = new Layer(width, height, this);
                    layer.FromJson(l, this, archive);

                    //we do not assign layerlookup
                    //here because layer.FromJson
                    //handles that part
                    //we simply need to add these
                    //root layers to the layers array
                    Layers.Add(layer);
                }
            }
        }

        public virtual void FromJson(GraphData d, Archive archive = null)
        {
            if (d == null) return;

            Archive = archive;
            Modified = true;

            Dictionary<string, Node> lookup = new Dictionary<string, Node>();

            State = GraphState.Loading;

            PixelNodes.Clear();
            InstanceNodes.Clear();

            Name = d.name;
            OutputNodes = d.outputs;
            InputNodes = d.inputs;
            defaultTextureType = d.defaultTextureType;
            ShiftX = d.shiftX;
            ShiftY = d.shiftY;
            Zoom = d.zoom;
            width = d.width;
            height = d.height;
            AbsoluteSize = d.absoluteSize;
            Version = d.version ?? 1.0f;

            if (width <= 0 || width == int.MaxValue) width = 256;
            if (height <= 0 || height == int.MaxValue) height = 256;

            SetJsonReadyCustomParameters(d.customParameters);
            SetJsonReadyCustomFunctions(d.customFunctions);

            //todo: reimplement layer support
            //we load layers first
            //since the graph may
            //have graph instances
            //that rely on a layer
            //LayersFromJson(d, archive);

            int count = d.nodes.Count;
            //parse node data
            //setup initial object instances
            for (int i = 0; i < count; ++i)
            {
                string s = d.nodes[i];
                Node.NodeData nd = JsonConvert.DeserializeObject<Node.NodeData>(s);

                if (nd != null)
                {
                    string type = nd.type;
                    if (!string.IsNullOrEmpty(type))
                    {
                        try
                        {
                            Type t = Type.GetType(type);
                            if (t != null)
                            {
                                //special case to handle output nodes
                                if (t.Equals(typeof(OutputNode)))
                                {
                                    OutputNode n = new OutputNode(defaultTextureType);
                                    n.AssignParentGraph(this);
                                    n.Id = nd.id;
                                    lookup[nd.id] = n;
                                    Nodes.Add(n);
                                    LoadNode(n, s, archive);
                                }
                                else if (t.Equals(typeof(InputNode)))
                                {
                                    InputNode n = new InputNode(defaultTextureType);
                                    n.AssignParentGraph(this);
                                    n.Id = nd.id;
                                    lookup[nd.id] = n;
                                    Nodes.Add(n);
                                    LoadNode(n, s, archive);
                                }
                                else if (t.Equals(typeof(CommentNode)) || t.Equals(typeof(PinNode)))
                                {
                                    Node n = (Node)Activator.CreateInstance(t);
                                    if (n != null)
                                    {
                                        n.AssignParentGraph(this);
                                        n.Id = nd.id;
                                        lookup[nd.id] = n;
                                        Nodes.Add(n);
                                        LoadNode(n, s, archive);
                                    }
                                }
                                else
                                {
                                    Node n = (Node)Activator.CreateInstance(t, nd.width, nd.height, defaultTextureType);
                                    if (n != null)
                                    {
                                        n.AssignParentGraph(this);
                                        n.Id = nd.id;
                                        lookup[nd.id] = n;
                                        Nodes.Add(n);
                                        LoadNode(n, s, archive);

                                        if (n is GraphInstanceNode)
                                        {
                                            InstanceNodes.Add(n as GraphInstanceNode);
                                        }
                                        else if(n is PixelProcessorNode)
                                        {
                                            PixelNodes.Add(n as PixelProcessorNode);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                //log we could not load graph node
                                Log.Debug("Node type does not exist: " + type);
                            }
                        }
                        catch (Exception e)
                        {
                            //log we could not load graph node
                            Log.Error(e);
                        }
                    }
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

            GraphData d = JsonConvert.DeserializeObject<GraphData>(data);

            if (d != null)
            {
                FromJson(d, archive);
            }
        }

        private void LoadNode(Node n, string data, Archive archive = null)
        {
            //slight optimization for function graphs
            if (n is MathNode && this is Function)
            {
                MathNode mn = n as MathNode;
                Function fg = this as Function;
                mn.AssignParentNode(fg.parentNode);

                if (n is ExecuteNode && fg.Execute == null)
                {
                    fg.Execute = n as ExecuteNode;
                }
                if (n is ArgNode)
                {
                    fg.Args.Add(n as ArgNode);
                }
                else if (n is CallNode)
                {
                    fg.Calls.Add(n as CallNode);
                }
                else if (n is SamplerNode)
                {
                    fg.Samplers.Add(n as SamplerNode);
                }
                else if (n is ForLoopNode)
                {
                    fg.ForLoops.Add(n as ForLoopNode);
                }
            }

            n.FromJson(data, archive);

            //origin sizes are only for graph instances
            //not actually used in the current one being edited
            //it is used in the ResizeWith
            OriginSizes[n.Id] = new PointD(n.Width, n.Height);
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
        public void PasteParameters(Dictionary<string, string> cparams, Node.NodeData from, Node to)
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
                p.OnGraphParameterUpdate -= Graph_OnGraphParameterUpdate;
                p.OnGraphParameterTypeChanged -= Graph_OnGraphParameterTypeChanged;
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
                p.OnGraphParameterUpdate -= Graph_OnGraphParameterUpdate;
                p.OnGraphParameterTypeChanged -= Graph_OnGraphParameterTypeChanged;

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
                if (p.IsFunction())
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
                p.OnGraphParameterUpdate += Graph_OnGraphParameterUpdate;
                p.OnGraphParameterTypeChanged += Graph_OnGraphParameterTypeChanged;
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
                //just a quick sanity check
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
            string jsonData = null;
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

                if (undo.TryDequeue(out diff))
                {
                    //undo the diff

                    using (MemoryStream output = new MemoryStream())
                    using (ByteBuffer dict = new ByteBuffer(previousByteData))
                    using (ByteBuffer delta = new ByteBuffer(diff))
                    {
                        long bytesWritten = 0;
                        VCDecoder decoder = new VCDecoder(dict, delta, output);
                        if (decoder.Decode(out bytesWritten) == VCDiffResult.SUCCESS)
                        {
                            previousByteData = output.ToArray();
                            jsonData = Encoding.UTF8.GetString(previousByteData);
                        }
                    }

                    //we only ever store the last 100 redos
                    if (redo.Count >= 100)
                    {
                        byte[][] queue = redo.ToArray();
                        redo.Clear();

                        //delete the first 10 redos
                        for (int i = 10; i < queue.Length; ++i)
                        {
                            redo.Enqueue(queue[i]);
                        }
                    }

                    redo.Enqueue(diff);
                }
            }).ContinueWith(t =>
            {
                if (!string.IsNullOrEmpty(jsonData))
                {
                    Dispose();
                    FromJson(jsonData, Archive);
                    OnUndo?.Invoke(this);
                }
            }, Node.Context);
        }

        public void Redo()
        {
            string jsonData = null;
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

                if (redo.TryDequeue(out diff))
                {
                    //redo the diff
                    using (MemoryStream output = new MemoryStream())
                    using (ByteBuffer dict = new ByteBuffer(previousByteData))
                    using (ByteBuffer delta = new ByteBuffer(diff))
                    {
                        long bytesWritten = 0;
                        VCDecoder decoder = new VCDecoder(dict, delta, output);
                        if (decoder.Decode(out bytesWritten) == VCDiffResult.SUCCESS)
                        {
                            previousByteData = output.ToArray();
                            jsonData = Encoding.UTF8.GetString(previousByteData);
                        }
                    }

                    //store the diff in undo
                    undo.Enqueue(diff);
                }
            }).ContinueWith(t =>
            {
                if (!string.IsNullOrEmpty(jsonData))
                {
                    Dispose();
                    FromJson(jsonData, Archive);
                    OnRedo?.Invoke(this);
                }
            }, Node.Context);
        }

        public void Snapshot(int maxStore = 100)
        {
            Task.Run(() =>
            {
                byte[] newData = Encoding.UTF8.GetBytes(GetJson());

                if (previousByteData == null || previousByteData.Length == 0)
                {
                    previousByteData = newData;
                    return;
                }

                //otherwise take a snapshot of current graph
                //compared to previous graph using vcdiff

                //verify max storage
                if (undo.Count >= maxStore)
                {
                    byte[][] queue = undo.ToArray();
                    undo.Clear();

                    //delete the first 10 undos
                    for (int i = 10; i < queue.Length; ++i)
                    {
                        undo.Enqueue(queue[i]);
                    }
                }

                using (MemoryStream output = new MemoryStream())
                using (ByteBuffer target = new ByteBuffer(previousByteData))
                using (ByteBuffer dict = new ByteBuffer(newData))
                {
                    VCCoder coder = new VCCoder(dict, target, output);
                    if (coder.Encode(false, true) == VCDiffResult.SUCCESS)
                    {
                        undo.Enqueue(output.ToArray());
                    }
                }

                previousByteData = newData;
            });
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

        #region Disposal
        protected void DisposeLayers()
        {
            try
            {
                for (int i = 0; i < Layers.Count; ++i)
                {
                    var l = Layers[i];
                    l?.Dispose();
                }
            }
            catch (Exception e)
            {

            }

            Layers.Clear();
        }

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

            DisposeLayers();

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
