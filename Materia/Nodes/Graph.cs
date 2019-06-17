using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Reflection;
using System.Windows;
using Materia.Nodes.Atomic;
using Materia.Nodes.Attributes;
using OpenTK.Graphics.OpenGL;
using Materia.MathHelpers;
using System.Threading;
using Materia.Nodes.Items;
using NLog;

namespace Materia.Nodes
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

    public class Graph : IDisposable
    {
        private static ILogger Log = LogManager.GetCurrentClassLogger();

        public static int[] GRAPH_SIZES = new int[] { 8, 16, 32, 64, 128, 256, 512, 1024, 2048, 4096 };
        public const int DEFAULT_SIZE = 256;

        public delegate void GraphUpdate(Graph g);
        public event GraphUpdate OnGraphUpdated;
        public event GraphUpdate OnGraphNameChanged;
        public event GraphParameterValue.GraphParameterUpdate OnGraphParameterUpdate;
        public event GraphParameterValue.GraphParameterUpdate OnGraphParameterTypeUpdate;

        [HideProperty]
        public bool ReadOnly { get; set; }
        [HideProperty]
        public string CWD { get; set; }
        public List<Node> Nodes { get; protected set; }
        public Dictionary<string, Node> NodeLookup { get; protected set; }
        public List<string> OutputNodes { get; protected set; }
        public List<string> InputNodes { get; protected set; }

        protected Dictionary<string, object> Variables { get; set; }
        protected Dictionary<string, Point> OriginSizes;

        protected Dictionary<string, Node.NodeData> tempData;

        /// <summary>
        /// Parameters are only available for image graphs and fx graphs
        /// </summary>
        [GraphParameterEditor]
        public Dictionary<string, GraphParameterValue> Parameters { get; protected set; }
        
        [Section(Section = "Custom Parameters")]
        [Title(Title = "")]
        [ParameterEditor]
        public List<GraphParameterValue> CustomParameters { get; protected set; }

        [Section(Section = "Custom Functions")]
        [Title(Title = "")]
        [GraphFunctionEditor]
        public List<FunctionGraph> CustomFunctions { get; protected set; }

        protected string name;
        [TextInput]
        public string Name
        {
            get
            {
                return name;
            }
            set
            {
                name = value;
                if(OnGraphNameChanged != null)
                {
                    OnGraphNameChanged.Invoke(this);
                }
            }
        }

        [HideProperty]
        public double ShiftX { get; set; }

        [HideProperty]
        public double ShiftY { get; set; }

        [HideProperty]
        public float Zoom { get; set; }

        protected GraphPixelType defaultTextureType;

        [Dropdown(null)]
        [Title(Title = "Default Texture Type")]
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

        protected string hdriIndex;

        [HideProperty]
        public string HdriIndex
        {
            get
            {
                return hdriIndex;
            }
            set
            {
                hdriIndex = value;
                Hdri.HdriManager.Selected = value;
            }
        }

        [Dropdown("HdriIndex")]
        [Title(Title = "Hdri Image")]
        public string[] HdriImages { get; set; }

        protected int randomSeed;

        [Title(Title = "Random Seed")]
        public int RandomSeed
        {
            get
            {
                return randomSeed;
            }
            set
            {
                randomSeed = value;
				
				if(this is FunctionGraph)
                {
                    Updated();
                    return;
                }

                Updated();
                TryAndProcess();
            }
        }

        public class GPoint
        {
            public double x;
            public double y;

            public GPoint()
            {

            }

            public GPoint(double x, double y)
            {
                this.x = x;
                this.y = y;
            }

            public GPoint(Point p)
            {
                x = p.X;
                y = p.Y;
            }

            public Point ToPoint()
            {
                return new Point(x, y);
            }
        }

        protected int width;
        protected int height;

        [HideProperty]
        public int Width
        {
            get
            {
                return width;
            }
            set
            {
                if(!ReadOnly)
                    width = value;
            }
        }

        [HideProperty]
        public int Height
        {
            get
            {
                return height;
            }
            set
            {
                if(!ReadOnly)
                    height = value;
            }
        }

        [HideProperty]
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

        public Graph(string name, int w = DEFAULT_SIZE, int h = DEFAULT_SIZE)
        {
            tempData = new Dictionary<string, Node.NodeData>();
            hdriIndex = Hdri.HdriManager.Selected;
            HdriImages = Hdri.HdriManager.Available.ToArray();

            Name = name;
            Zoom = 1;
            ShiftX = ShiftY = 0;
            width = w;
            height = h;

            Variables = new Dictionary<string, object>();
            defaultTextureType = GraphPixelType.RGBA;
            Nodes = new List<Node>();
            NodeLookup = new Dictionary<string, Node>();
            OutputNodes = new List<string>();
            InputNodes = new List<string>();
            OriginSizes = new Dictionary<string, Point>();
            Parameters = new Dictionary<string, GraphParameterValue>();
            CustomParameters = new List<GraphParameterValue>();
            CustomFunctions = new List<FunctionGraph>();
        }

        public virtual object GetVar(string k)
        {
            if (k == null) return null;

            if(Variables.ContainsKey(k))
            {
                return Variables[k];
            }

            return null;
        }

        public Node FindSubNodeById(string id)
        {
            Node n = null;

            //first try this graph
            if(NodeLookup.TryGetValue(id, out n))
            {
                return n;
            }


            var graphinsts = Nodes.FindAll(m => m is GraphInstanceNode);

            //try and retrieve from graph inst
            for(int i = 0; i < graphinsts.Count; i++)
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

            var procnodes = Nodes.FindAll(m => m is PixelProcessorNode);

            //try and find in pixel processors
            for (int i = 0; i < procnodes.Count; i++)
            {
                var proc = procnodes[i] as PixelProcessorNode;

                if(proc.Function != null)
                {
                    if(proc.Function.NodeLookup.TryGetValue(id, out n))
                    {
                        return n;
                    }
                }
            }

            //try and find in parameter functions
            foreach(string k in Parameters.Keys)
            {
                var parameter = Parameters[k];
                if(parameter.IsFunction())
                {
                    var g = parameter.Value as FunctionGraph;

                    if(g.NodeLookup.TryGetValue(id, out n))
                    {
                        return n;
                    }
                }
            }

            //try and find in custom functions
            for(int i = 0; i < CustomFunctions.Count; i++)
            {
                FunctionGraph g = CustomFunctions[i];
                if (g.NodeLookup.TryGetValue(id, out n))
                {
                    return n;
                }
            }

            return null;
        }

        public virtual void RemoveVar(string k)
        {
            if (string.IsNullOrEmpty(k)) return;
            Variables.Remove(k);
        }

        public virtual void SetVar(string k, object v)
        {
            Variables[k] = v;
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

            public string hdriIndex;

            public Dictionary<string, string> parameters;
            public List<string> customParameters;
            public List<string> customFunctions;
        }

        public virtual void TryAndProcess()
        {
            int c = Nodes.Count;
            for(int i = 0; i < c; i++)
            {
                Node n = Nodes[i];

                if(!n.IsRoot())
                {
                    continue;
                }

                n.TryAndProcess();
            }
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

            float wp = (float)width / (float)this.width;
            float hp = (float)height / (float)this.height;

            for (int i = 0; i < c; i++)
            {
                Node n = Nodes[i];

                if (n is OutputNode || n is InputNode)
                {
                    continue;
                }

                 if(!(n is ItemNode) && !(n is BitmapNode))
                {
                    Point osize;

                    if (OriginSizes.TryGetValue(n.Id, out osize))
                    {
                        int fwidth = (int)Math.Min(4096, Math.Max(8, Math.Round(osize.X * wp)));
                        int fheight = (int)Math.Min(4096, Math.Max(8, Math.Round(osize.Y * hp)));

                        //we use assign in order to avoid
                        //triggering update event
                        n.AssignWidth(fwidth);
                        n.AssignHeight(fheight);
                    }
                }
            }
        }

        public virtual string GetJson()
        {
            GraphData d = new GraphData();

            List<string> data = new List<string>();

            for(int i = 0; i < Nodes.Count; i++)
            {
                Node n = Nodes[i];
                data.Add(n.GetJson());
            }

            d.name = Name;
            d.nodes = data;
            d.outputs = OutputNodes;
            d.inputs = InputNodes;
            d.defaultTextureType = defaultTextureType;
            d.shiftX = ShiftX;
            d.shiftY = ShiftY;
            d.zoom = Zoom;
            d.hdriIndex = hdriIndex;
            d.parameters = GetJsonReadyParameters();
            d.width = width;
            d.height = height;
            d.customParameters = GetJsonReadyCustomParameters();
            d.customFunctions = GetJsonReadyCustomFunctions();

            return JsonConvert.SerializeObject(d);
        }

        protected virtual List<string> GetJsonReadyCustomFunctions()
        {
            List<string> funcs = new List<string>();

            for(int i = 0; i < CustomFunctions.Count; i++)
            {
                FunctionGraph f = CustomFunctions[i];
                funcs.Add(f.GetJson());
            }

            return funcs;
        }

        public virtual Dictionary<string, object> GetConstantParameters()
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>();

            foreach(var k in Parameters.Keys)
            {
                if(!Parameters[k].IsFunction())
                {
                    parameters[k] = Parameters[k].Value;
                }
            }

            return parameters;
        }

        protected virtual Dictionary<string, string> GetJsonReadyParameters()
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();

            foreach (var k in Parameters.Keys)
            {
                parameters[k] = Parameters[k].GetJson();
            }

            return parameters;
        }


        protected List<string> GetJsonReadyCustomParameters()
        {
            List<string> parameters = new List<string>();
            for(int i = 0; i < CustomParameters.Count; i++)
            {
                GraphParameterValue g = CustomParameters[i];
                parameters.Add(g.GetJson());
            }
            return parameters;
        }

        public virtual Dictionary<string, object> GetCustomParameters()
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>();

            for(int i = 0; i < CustomParameters.Count; i++)
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
                CustomFunctions = new List<FunctionGraph>();

                for(int i = 0; i < functions.Count; i++)
                {
                    string k = functions[i];
                    FunctionGraph g = new FunctionGraph("temp");
                    CustomFunctions.Add(g);
                    g.FromJson(k);
                }

                for(int i = 0; i < CustomFunctions.Count; i++)
                {
                    FunctionGraph g = CustomFunctions[i];
                    //set parent graph
                    g.ParentGraph = this;
                    //finally set connections
                    g.SetConnections();
                }
            }
        }

        public virtual void AssignParameters(Dictionary<string, object> parameters)
        {
            if(parameters != null)
            {
                foreach(var k in parameters.Keys)
                {
                    GraphParameterValue gparam = null;

                    if(Parameters.TryGetValue(k, out gparam))
                    {
                        if(!gparam.IsFunction())
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

        public virtual void AssignCustomParameters(Dictionary<string, object> parameters)
        {
            if(parameters != null)
            {
                foreach(var k in parameters.Keys)
                {
                    var param = CustomParameters.Find(m => m.Id.Equals(k));

                    if(param != null)
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

        protected virtual void SetJsonReadyParameters(Dictionary<string, string> parameters)
        { 
            if (parameters != null)
            {
                //remove previous listeners
                //for function graphs
                if (Parameters != null && Parameters.Count > 0)
                {
                    foreach (var param in Parameters.Values)
                    {
                        if(param.IsFunction())
                        {
                            var f = param.Value as FunctionGraph;
                            f.OnGraphUpdated -= Graph_OnGraphUpdated;
                        }
                    }
                }

                Parameters = new Dictionary<string, GraphParameterValue>();

                foreach (var k in parameters.Keys)
                {
                    string[] split = k.Split('.');

                    Node n = null;
                    NodeLookup.TryGetValue(split[0], out n);

                    var param  = GraphParameterValue.FromJson(parameters[k], n);
                    Parameters[k] = param;

                    param.ParentGraph = this;
                    param.OnGraphParameterTypeChanged += Graph_OnGraphParameterTypeChanged;
                    param.OnGraphParameterUpdate += Graph_OnGraphParameterUpdate;

                    if (param.IsFunction())
                    { 
                        var f = Parameters[k].Value as FunctionGraph;
                        f.OnGraphUpdated += Graph_OnGraphUpdated;
                    }
                }
            }
        }

        public bool RemoveCustomParameter(GraphParameterValue p)
        {
            if(CustomParameters.Remove(p))
            {
                p.ParentGraph = null;
                p.OnGraphParameterUpdate -= Graph_OnGraphParameterUpdate;
                p.OnGraphParameterTypeChanged -= Graph_OnGraphParameterTypeChanged;
                return true;
            }

            return false;
        }

        public void AddCustomParameter(GraphParameterValue p)
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
        } 

        public bool RemoveCustomFunction(FunctionGraph g)
        {
            if(CustomFunctions.Remove(g))
            {
                g.ParentGraph = null;
                g.OnGraphUpdated -= Graph_OnGraphUpdated;
                return true;
            }

            return false;
        }

        public void AddCustomFunction(FunctionGraph g)
        {
            if (CustomFunctions.Contains(g)) return;
            if (g.ParentGraph != null && g.ParentGraph != this)
            {
                g.ParentGraph.RemoveCustomFunction(g);
            }
            CustomFunctions.Add(g);
            g.ParentGraph = this;
            g.OnGraphUpdated += Graph_OnGraphUpdated;
        }

        protected virtual void SetJsonReadyCustomParameters(List<string> parameters)
        {
            if(parameters != null)
            {
                CustomParameters.Clear();

                for(int i = 0; i < parameters.Count; i++)
                {
                    string k = parameters[i];
                    var param = GraphParameterValue.FromJson(k, null);
                    param.ParentGraph = this;
                    param.OnGraphParameterTypeChanged += Graph_OnGraphParameterTypeChanged;
                    param.OnGraphParameterUpdate += Graph_OnGraphParameterUpdate;
                    CustomParameters.Add(param);
                }
            }
        }

        public virtual bool Add(Node n)
        {
            if (NodeLookup.ContainsKey(n.Id)) return false;

            if(n is OutputNode)
            {
                OutputNodes.Add(n.Id);
            }
            else if(n is InputNode)
            {
                InputNodes.Add(n.Id);
            }

            NodeLookup[n.Id] = n;
            Nodes.Add(n);

            n.OnUpdate += N_OnUpdate;

            return true;
        }

        private void N_OnUpdate(Node n)
        {
            Updated();
        }

        /// <summary>
        /// This is used in GraphInstanceNodes
        /// We only save the final buffers connected to the outputs,
        /// and release all other buffers to save video card memory
        /// since it is all in video memory and shader based
        /// we do not have to transfer data to the video card
        /// so it will be relatively fast still to update
        /// when we have to recreate the textures
        /// </summary>
        public virtual void ReleaseIntermediateBuffers()
        {
            for(int i = 0; i < Nodes.Count; i++)
            {
                Node n = Nodes[i];
                if (n is OutputNode)
                {
                    continue;
                }

                if (n.Buffer != null)
                {
                    n.Buffer.Release();
                }
            }
        }

        public virtual void Remove(Node n)
        {
            if(n is OutputNode)
            {
                OutputNodes.Remove(n.Id);
            }
            else if(n is InputNode)
            {
                InputNodes.Remove(n.Id);
            }

            NodeLookup.Remove(n.Id);
            if(Nodes.Remove(n))
            {
                n.OnUpdate -= N_OnUpdate;
            }
            n.Dispose();
        }

        public virtual Node CreateNode(string type)
        {
            if(ReadOnly)
            {
                return null;
            }

            if(type.Contains(System.IO.Path.DirectorySeparatorChar))
            {
                var n = new GraphInstanceNode(width, height);
                n.ParentGraph = this;
                return n;
            }

            try
            {
                Type t = Type.GetType(type);
                if(t != null)
                {
                    if (t.Equals(typeof(OutputNode)))
                    {
                        var n  = new OutputNode(defaultTextureType);
                        n.ParentGraph = this;
                        return n;
                    }
                    else if(t.Equals(typeof(InputNode)))
                    {
                        var n = new InputNode(defaultTextureType);
                        n.ParentGraph = this;
                        return n;
                    }
                    else if(t.Equals(typeof(CommentNode)) || t.Equals(typeof(PinNode)))
                    {
                        Node n = (Node)Activator.CreateInstance(t);
                        n.ParentGraph = this;
                        return n;
                    }
                    else
                    {
                        Node n = (Node)Activator.CreateInstance(t, width, height, defaultTextureType);
                        n.ParentGraph = this;
                        return n;
                    }
                }
                else
                {
                    var n = new GraphInstanceNode(width, height);
                    n.ParentGraph = this;
                    return n;
                }
            }
            catch (Exception e)
            {
                Log.Error(e);
                return null;
            }
        }

        public virtual void FromJson(string data)
        {
            GraphData d = JsonConvert.DeserializeObject<GraphData>(data);

            if (d != null)
            {
                tempData = new Dictionary<string, Node.NodeData>();
                Dictionary<string, Node> lookup = new Dictionary<string, Node>();

                hdriIndex = d.hdriIndex;
                Name = d.name;
                OutputNodes = d.outputs;
                InputNodes = d.inputs;
                defaultTextureType = d.defaultTextureType;
                ShiftX = d.shiftX;
                ShiftY = d.shiftY;
                Zoom = d.zoom;
                width = d.width;
                height = d.height;

                if (width == 0 || width == int.MaxValue) width = 256;
                if (height == 0 || height == int.MaxValue) height = 256;

                SetJsonReadyCustomParameters(d.customParameters);
                SetJsonReadyCustomFunctions(d.customFunctions);

                //parse node data
                //setup initial object instances
                for(int i = 0; i < d.nodes.Count; i++)
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
                                        n.ParentGraph = this;
                                        n.Id = nd.id;
                                        lookup[nd.id] = n;
                                        Nodes.Add(n);
                                        tempData[nd.id] = nd;
                                        LoadNode(n, s);
                                    }
                                    else if (t.Equals(typeof(InputNode)))
                                    {
                                        InputNode n = new InputNode(defaultTextureType);
                                        n.ParentGraph = this;
                                        n.Id = nd.id;
                                        lookup[nd.id] = n;
                                        Nodes.Add(n);
                                        tempData[nd.id] = nd;
                                        LoadNode(n, s);
                                    }
                                    else if (t.Equals(typeof(CommentNode)) || t.Equals(typeof(PinNode)))
                                    {
                                        Node n = (Node)Activator.CreateInstance(t);
                                        if (n != null)
                                        {
                                            n.ParentGraph = this;
                                            n.Id = nd.id;
                                            lookup[nd.id] = n;
                                            Nodes.Add(n);
                                            tempData[nd.id] = nd;
                                            LoadNode(n, s);
                                        }
                                    }
                                    else
                                    {
                                        Node n = (Node)Activator.CreateInstance(t, nd.width, nd.height, defaultTextureType);
                                        if (n != null)
                                        {
                                            n.ParentGraph = this;
                                            n.Id = nd.id;
                                            lookup[nd.id] = n;
                                            Nodes.Add(n);
                                            tempData[nd.id] = nd;
                                            LoadNode(n, s);
                                        }
                                    }
                                }
                                else
                                {
                                    //log we could not load graph node
                                }
                            }
                            catch
                            {
                                //log we could not load graph node
                            }
                        }
                    }
                }

                NodeLookup = lookup;
                SetJsonReadyParameters(d.parameters);

                if (!(this is FunctionGraph))
                {
                    SetConnections();
                }
            }
        }

        private void LoadNode(Node n, string data)
        {
            n.FromJson(data);

            //this handles a case where
            //the function graph is already created
            //and possible being reloaded
            //after being cleared and then FromJson is called again
            if (n is MathNode && this is FunctionGraph)
            {
                FunctionGraph t = this as FunctionGraph;
                MathNode mn = n as MathNode;

                if (t.ParentNode != null)
                {
                    mn.ParentNode = t.ParentNode;
                }
                else if (t.ParentGraph != null)
                {
                    mn.OnFunctionParentSet();
                }
            }

            //origin sizes are only for graph instances
            //not actually used in the current one being edited
            //it is used in the ResizeWith
            OriginSizes[n.Id] = new Point(n.Width, n.Height);
        }

        public void SetConnections()
        {
            //finally after every node is populated
            //try and connect them all!
            foreach (Node n in Nodes)
            {
                Node.NodeData nd = null;
                //we prevent the input on change event from happening
                if (tempData.TryGetValue(n.Id, out nd))
                {
                    n.SetConnections(NodeLookup, nd.outputs, false);
                    n.OnUpdate += N_OnUpdate;
                }
            }

            //release temp data
            tempData.Clear();

            if (this is FunctionGraph)
            {
                (this as FunctionGraph).UpdateOutputTypes();
            }
            //do not processs graph instances while loading
            else if(ParentNode == null)
            {
                TryAndProcess();
            }
        }

        public void CopyResources(string cwd)
        {
            foreach (Node n in Nodes)
            {
                n.CopyResources(cwd);
            }

            //set last in case we need to copy from current graph cwd to new cwd
            this.CWD = cwd;
        }

        protected virtual void ClearParameters()
        {
            if(CustomParameters != null)
            {
                //just a quick sanity check
                foreach (var param in CustomParameters)
                {
                    if (param.IsFunction())
                    {
                        FunctionGraph fn = param.Value as FunctionGraph;
                        fn.OnGraphUpdated -= Graph_OnGraphUpdated;
                        fn.Dispose();
                    }
                }

                CustomParameters.Clear();
            }

            if(CustomFunctions != null)
            {
                foreach(var f in CustomFunctions)
                {
                    f.Dispose();
                }

                CustomFunctions.Clear();
            }

            if(Parameters != null)
            {
                foreach(var param in Parameters.Values)
                {
                    if(param.IsFunction())
                    {
                        FunctionGraph fn = param.Value as FunctionGraph;
                        fn.OnGraphUpdated -= Graph_OnGraphUpdated;
                        fn.Dispose();
                    }
                }

                Parameters.Clear();
            }
        }

        public virtual void Dispose()
        {
            if (Nodes != null)
            {
                foreach (Node n in Nodes)
                {
                    n.Dispose();
                }

                Nodes.Clear();
            }

            if (NodeLookup != null)
            {
                NodeLookup.Clear();
            }

            if (OutputNodes != null)
            {
                OutputNodes.Clear();
            }

            if (InputNodes != null)
            {
                InputNodes.Clear();
            }

            ClearParameters();
        }

        protected void Updated()
        {
            if(OnGraphUpdated != null)
            {
                OnGraphUpdated.Invoke(this);
            }
        }

        public bool IsParameterValueFunction(string id, string parameter)
        {
            string cid = id + "." + parameter;

            GraphParameterValue v = null;

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

        public GraphParameterValue GetParameterRaw(string id, string parameter)
        {
            GraphParameterValue p = null;

            string cid = id + "." + parameter;

            Parameters.TryGetValue(cid, out p);

            return p;
        }

        public object GetParameterValue(string id, string parameter)
        {
            string cid = id + "." + parameter;

            GraphParameterValue p = null;

            if (Parameters.TryGetValue(cid, out p))
            {
                if (p.IsFunction())
                {
                    FunctionGraph g = p.Value as FunctionGraph;

                    g.TryAndProcess();

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

            GraphParameterValue p = null;

            if(Parameters.TryGetValue(cid, out p))
            {
                if(p.IsFunction())
                {
                    FunctionGraph g = p.Value as FunctionGraph;

                    g.TryAndProcess();

                    if(g.Result == null)
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

        public void RemoveParameterValue(string id, string parameter)
        {
            string cid = id + "." + parameter;

            GraphParameterValue p = null;

            if (Parameters.TryGetValue(cid, out p))
            {
                p.OnGraphParameterUpdate -= Graph_OnGraphParameterUpdate;
                p.OnGraphParameterTypeChanged -= Graph_OnGraphParameterTypeChanged;

                if (p.IsFunction())
                {
                    FunctionGraph g = p.Value as FunctionGraph;
                    g.OnGraphUpdated -= Graph_OnGraphUpdated;
                    g.Dispose();
                }
            }

            Parameters.Remove(cid);

            Updated();
        }

        public void SetParameterValue(string id, string parameter, object v, bool overrideType = false, NodeType toverride = NodeType.Float)
        {
            string cid = id + "." + parameter;

            GraphParameterValue p = null;

            if (Parameters.TryGetValue(cid, out p))
            {
                if (p.IsFunction())
                {
                    FunctionGraph g = p.Value as FunctionGraph;
                    g.OnGraphUpdated -= Graph_OnGraphUpdated;
                    g.Dispose();
                }

                if(v is FunctionGraph)
                {
                    p.Type = (v as FunctionGraph).ExpectedOutput;
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
                if(v is FunctionGraph)
                {
                    t = (v as FunctionGraph).ExpectedOutput;
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

                p = Parameters[cid] = new GraphParameterValue(parameter, v, "", t);
                p.ParentGraph = this;
                p.OnGraphParameterUpdate += Graph_OnGraphParameterUpdate;
                p.OnGraphParameterTypeChanged += Graph_OnGraphParameterTypeChanged;
            }

            if (v is FunctionGraph)
            {
                (v as FunctionGraph).OnGraphUpdated += Graph_OnGraphUpdated;
            }

            Updated();
        }

        private void Graph_OnGraphParameterUpdate(GraphParameterValue param)
        {
            if(OnGraphParameterUpdate != null)
            {
                OnGraphParameterUpdate.Invoke(param);
            }
        }

        private void Graph_OnGraphParameterTypeChanged(GraphParameterValue param)
        {
            if(OnGraphParameterTypeUpdate != null)
            {
                OnGraphParameterTypeUpdate.Invoke(param);
            }
        }

        private void Graph_OnGraphUpdated(Graph g)
        {
            if(g is FunctionGraph)
            {
                FunctionGraph fg = g as FunctionGraph;

                if (fg.ParentNode != null)
                {
                    fg.ParentNode.TryAndProcess();
                }
            }
        }
    }
}
