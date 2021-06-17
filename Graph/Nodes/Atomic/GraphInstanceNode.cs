using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Materia.Rendering.Attributes;
using Materia.Rendering.Textures;
using MLog;
using Materia.Graph;
using Materia.Graph.IO;
using Materia.Rendering.Extensions;
using Materia.Rendering.Mathematics;

namespace Materia.Nodes.Atomic
{
    public class GraphInstanceNode : ImageNode
    {
        public Graph.Graph GraphInst { get; protected set; }

        private string relativePath;

        protected string path;
        protected Dictionary<string, object> jsonParameters;
        protected Dictionary<string, object> jsonCustomParameters;
        protected Dictionary<string, ParameterValue> nameMap;
        protected int randomSeed;
        protected bool updatingParams;

        protected bool loading;
        private bool isArchive;

        private Archive archive;
        private Archive child;

        private bool isDirty;

        [ReadOnly]
        [Editable(ParameterInputType.Text, "Materia Graph File", "Content")]
        public string GraphFilePath
        {
            get
            {
                return path;
            }
            ///temporary
            ///until the fix is
            ///implemented for proper
            ///resetting
            ///set
            ///{
            ///    path = value;
            ///    Load(path);
            ///    Updated();
            ///}
        }

        [Editable(ParameterInputType.Map, "Parameters", "Instance Parameters")]
        public Dictionary<string, ParameterValue> Parameters
        {
            get
            {
                if(GraphInst != null)
                {
                    return GraphInst.Parameters;
                }

                return null;
            }
        }

        [Editable(ParameterInputType.Map, "Custom Parameters", "Instance Parameters")]
        public List<ParameterValue> CustomParameters
        {
            get
            {
                if(GraphInst != null)
                {
                    return GraphInst.CustomParameters;
                }

                return null;
            }
        }

        [Editable(ParameterInputType.IntInput, "Random Seed", "Instance Parameters")]
        public int RandomSeed
        {
            get
            {
                return randomSeed;
            }
            set
            {
                AssignSeed(value);
                TriggerValueChange();
            }
        }

        public new float TileX
        {
            get
            {
                return tileX;
            }
            set
            {
                tileX = value;
            }
        }

        public new float TileY
        {
            get
            {
                return tileY;
            }
            set
            {
                tileY = value;
            }
        }

        private Reader graphData;
        private string rawGraphData;

        public GraphInstanceNode(int w, int h, GraphPixelType p = GraphPixelType.RGBA) : base()
        {
            isDirty = true;
            width = w;
            height = h;

            nameMap = new Dictionary<string, ParameterValue>();

            Id = Guid.NewGuid().ToString();

            tileX = tileY = 1;

            internalPixelType = p;

            Name = "Graph Instance";

            path = "";
        }

        public ParameterValue GetCustomParameter(string name)
        {
            ParameterValue v = null;
            nameMap.TryGetValue(name, out v);
            return v;
        }

        public void AssignSeed(int seed)
        {
            randomSeed = seed;
            GraphInst?.AssignSeed(seed);
        }

        public override void SetCWD(string cwd)
        {
            base.SetCWD(cwd);
            if (GraphInst == null) return;
            GraphInst.CurrentWorkingDirectory = cwd;
        }

        public bool Load(string path)
        {
            if (string.IsNullOrEmpty(path)) return false;

            if (GraphInst != null)
            {
                GraphInst.OnParameterUpdate -= GraphInst_OnParameterUpdate;
                GraphInst.Dispose();
                GraphInst = null;
            }

            return TryAndLoadFile(path);
        }

        public void GatherOutputs(HashSet<Node> inStack, Queue<Node> stack)
        {
            if (GraphInst == null) return;
            for (int i = 0; i < GraphInst.OutputNodes.Count; ++i)
            {
                var id = GraphInst.OutputNodes[i];
                if (GraphInst.NodeLookup.TryGetValue(id, out Node n))
                {
                    if (n == null) continue;
                    if (n.IsScheduled) continue;
                    if (n is OutputNode && !inStack.Contains(n))
                    {
                        stack.Enqueue(n);
                        inStack.Add(n);
                    }
                }
            }
        }

        private static List<string> shelfFiles = null;
        private string ResolveToShelf(string shelfPath, string filename)
        {
            if (!Directory.Exists(shelfPath)) return null;
            if (shelfFiles == null) shelfFiles = new List<string>(Directory.GetFiles(shelfPath, "*.*", SearchOption.AllDirectories));

            for (int i = 0; i < shelfFiles.Count; ++i)
            {
                if (shelfFiles[i].EndsWith(filename))
                {
                    return shelfFiles[i];
                }
            }

            return null;
        }

        protected void LoadFromAbsolute(string path)
        {
            loading = true;
            Name = Path.GetFileNameWithoutExtension(path);
            graphData = new Reader(File.ReadAllBytes(path));
            PrepareGraph();
            loading = false;
        }

        protected bool LoadFromArchive(Archive child, Archive parent = null)
        {
            if (child == null) return false;

            child.Open();
            var childFiles = child.GetAvailableFiles();
            if (childFiles != null)
            {
                var mtg = childFiles.Find(f => f.path.ToLower().EndsWith(".mtg"));
                if (mtg != null)
                {
                    loading = true;
                    Name = Path.GetFileNameWithoutExtension(path);
                    
                    graphData = new Reader(mtg.ExtractBinary());
                    
                    child.Close();
                    PrepareGraph();
                    parent?.Close();
                   
                    loading = false;
                    return true;
                }
            }

            child.Close();
            parent?.Close();

            return false;
        }

        protected bool LoadFromArchive(string path, Archive parent = null)
        {
            child = new Archive(path);
            return LoadFromArchive(child, parent);
        }

        protected bool TryAndLoadFile(string path)
        {
            isArchive = path.ToLower().EndsWith(".mtga");
            
            //convert path to a relative resource path
            string resourcePath = Path.Combine("resources", Path.GetFileName(path));
            string absolutePath = path; //always base absolute

            relativePath = resourcePath;

            //only applies if the original file cannot be found
            if (!File.Exists(path))
            {
                //try and resolve to a local shelf file first
                if (!string.IsNullOrEmpty(ApplicationDirectory))
                {
                    string shelfPath = Path.Combine(ApplicationDirectory, "Shelf");
                    string shelfFilePath = ResolveToShelf(shelfPath, Path.GetFileName(path));

                    if (!string.IsNullOrEmpty(shelfFilePath) && File.Exists(shelfFilePath))
                    {
                        Log.Debug("Found Graph Instance File at shelf: " + shelfFilePath);
                        absolutePath = shelfFilePath;
                    }
                }

                //CWD / resources instance will always overwrite shelf instance
                if (!string.IsNullOrEmpty(CurrentWorkingDirectory))
                {
                    string cwdFilePath = Path.Combine(CurrentWorkingDirectory, Path.GetFileName(path));
                    string cwdResourceFilePath = Path.Combine(CurrentWorkingDirectory, resourcePath);

                    if (File.Exists(cwdResourceFilePath))
                    {
                        Log.Debug("Found Graph Instance File at resources: " + cwdResourceFilePath);
                        absolutePath = cwdResourceFilePath;
                    }
                    else if (File.Exists(cwdFilePath))
                    {
                        Log.Debug("Found Graph Instance File at CWD: " + cwdFilePath);
                        absolutePath = cwdFilePath;
                    }
                }
            }

            //ensure propeer point place in the future
            //for copytoresource
            path = absolutePath;

            //handle within active archive
            if (archive != null)
            {
                archive.Open();
                Archive.ArchiveFile mtg = null;
                var files = archive.GetAvailableFiles();
                mtg = files.Find(f => f.path.Equals(resourcePath));

                //try and load from absolute path
                //since it was not found in the archive resource path
                if (mtg == null)
                {
                    if (File.Exists(absolutePath))
                    {
                        if (isArchive)
                        {
                            return LoadFromArchive(absolutePath, archive);
                        }

                        LoadFromAbsolute(absolutePath);
                        
                        archive.Close();
                        return true;
                    }

                    archive.Close();
                    return false;
                }

                if (isArchive)
                {
                    child = new Archive(resourcePath, mtg.ExtractBinary());
                    return LoadFromArchive(child, archive);                   
                }

                loading = true;
                Name = Path.GetFileNameWithoutExtension(path);

                graphData = new Reader(mtg.ExtractBinary());
                PrepareGraph();
                archive.Close();
                loading = false;

                return true;
            }
            //handle path to archive when not in another archive
            else if (File.Exists(absolutePath))
            {
                if (isArchive)
                {
                    return LoadFromArchive(absolutePath);
                }

                LoadFromAbsolute(absolutePath);
                return true;
            }

            return false;
        }

        void PrepareGraph()
        {
            isDirty = true;

            nameMap = new Dictionary<string, ParameterValue>();

            GraphInst = new Image(Name, width, height);

            GraphInst.CurrentWorkingDirectory = CurrentWorkingDirectory;
            GraphInst.AssignParentNode(this);

            if (graphData != null && graphData.Length > 0)
            {
                GraphInst.FromBinary(graphData, child);
            }
            //this will only be triggered in the following case:
            //the file does not exist from the known path
            //in the shelf, cwd resource, or cwd
            //as the graphData will not be created
            //and the graph instance loaded was originally
            //json based
            else if (!string.IsNullOrEmpty(rawGraphData))
            {
                GraphInst.FromJson(rawGraphData, child);
            }

            GraphInst.AssignParameters(jsonParameters);
            GraphInst.AssignCustomParameters(jsonCustomParameters);
            GraphInst.AssignSeed(randomSeed);

            //Whoops was doing this in reverse originally
            //this is the proper way to ensure the graph instance
            //is using the previously saved graph default texture type
            //previously it was assigning the default from graph instance
            //to the loaded graph, which could break depending
            //on the graph it was being inserted into
            AssignPixelType(GraphInst.DefaultTextureType);

            GraphInst.OnParameterUpdate += GraphInst_OnParameterUpdate;

            //now do real initial resize
            GraphInst.ResizeWith(width, height);

            //setup inputs and outputs
            Setup();

            loading = false;
        }

        void Setup()
        {
            var inputsConnections = GetInputConnections();
            var outputConnections = GetOutputConnections();

            List<NodeInput> previousInputs = new List<NodeInput>();
            List<NodeOutput> previousOutputs = new List<NodeOutput>();

            foreach(NodeInput inp in Inputs)
            {
                inp.Reference?.Remove(inp);
                previousInputs.Add(inp);
            }

            foreach(NodeOutput op in Outputs)
            {
                previousOutputs.Add(op);
                RemovedOutput(op);
            }

            Inputs.Clear();
            Outputs.Clear();

            int count = 0;
            if(GraphInst.InputNodes.Count > 0)
            {
                count = GraphInst.InputNodes.Count;
                for(int i = 0; i < count; ++i)
                {
                    string id = GraphInst.InputNodes[i];
                    Node n;
                    if (GraphInst.NodeLookup.TryGetValue(id, out n))
                    {
                        InputNode inp = (InputNode)n;
                        inp.Inputs.Clear();
                        NodeInput np = new NodeInput(NodeType.Color | NodeType.Gray, this, inp.Name);
                        Inputs.Add(np);
                        inp.Inputs.Add(np);
                    }
                }
            }

            if(GraphInst.OutputNodes.Count > 0)
            {
                count = GraphInst.OutputNodes.Count;
                for(int i = 0; i < count; ++i)
                {
                    string id = GraphInst.OutputNodes[i];
                    Node n;
                    if (GraphInst.NodeLookup.TryGetValue(id, out n))
                    {
                        OutputNode op = (OutputNode)n;
                        op.Outputs.Clear();
                        NodeOutput ot = new NodeOutput(NodeType.Color | NodeType.Gray, this, op.Name);
                        Outputs.Add(ot);
                        op.Outputs.Add(ot);
                    }
                }
            }

            //name map used in parameter mapping for quicker lookup
            count = GraphInst.CustomParameters.Count;
            for(int i = 0; i < count; ++i)
            {
                var param = GraphInst.CustomParameters[i];
                nameMap[param.Name] = param;
            }

            SetConnections(parentGraph.NodeLookup, outputConnections, true);

            //set individual input connections from parent node
            foreach(var con in inputsConnections)
            {
                Node n = null;
                if(parentGraph.NodeLookup.TryGetValue(con.node, out n))
                {
                    n.SetConnection(this, con, true);
                }
            }

            count = Inputs.Count;
            for(int i = 0; i < count; ++i)
            {
                if (i < previousInputs.Count)
                {
                    AddedInput(Inputs[i], previousInputs[i]);
                }
                else
                {
                    AddedInput(Inputs[i]);
                }
            }

            if (count < previousInputs.Count)
            {
                for(int i = count; i < previousInputs.Count; ++i)
                {
                    RemovedInput(previousInputs[i]);
                }
            }
            
            count = Outputs.Count;
            for (int i = 0; i < count; ++i)
            {
                if(i < previousOutputs.Count)
                {
                    AddedOutput(Outputs[i], previousOutputs[i]);
                }
                else
                {
                    AddedOutput(Outputs[i]);
                }
            }
        }

        public void Reload()
        {
            if (!Load(path) 
                && ((graphData != null && graphData.Length > 0)
                || !string.IsNullOrEmpty(rawGraphData)))
            {
                if (graphData != null)
                {
                    graphData.Position = 0; //reset position
                }

                loading = true;
                PrepareGraph();
                loading = false;
            }
        }

        public override byte[] Export(int w = 0, int h = 0)
        {
            if (GraphInst == null) return null;

            if (GraphInst.OutputNodes.Count > 0)
            {
                var id = GraphInst.OutputNodes[0];
                if (GraphInst.NodeLookup.TryGetValue(id, out Node n))
                {
                    return n?.Export(w, h);
                }
            }

            return null;
        }

        public override GLTexture2D GetActiveBuffer()
        {
            if (GraphInst == null) return null;

            if(GraphInst.OutputNodes.Count > 0)
            {
                var id = GraphInst.OutputNodes[0];
                if (GraphInst.NodeLookup.TryGetValue(id, out Node n))
                {
                    return n?.GetActiveBuffer();
                }
            }

            return null;
        }

        public override void AssignParentGraph(Graph.Graph g)
        {
            if(parentGraph != null)
            {
                parentGraph.OnParameterUpdate -= ParentGraph_OnParameterUpdate;
            }

            base.AssignParentGraph(g);

            if (g != null)
            {
                g.OnParameterUpdate += ParentGraph_OnParameterUpdate;
            }
        }

        public override void TriggerValueChange()
        {
            isDirty = true;

            if (GraphInst != null)
            {
                List<GraphInstanceNode> nodes = GraphInst.InstanceNodes;
                if (nodes != null)
                {
                    for (int i = 0; i < nodes.Count; ++i)
                    {
                        nodes[i].isDirty = true;
                    }
                }
            }

            base.TriggerValueChange();
        }

        private void GraphInst_OnParameterUpdate(ParameterValue param)
        {
            TriggerValueChange();
        }

        private void ParentGraph_OnParameterUpdate(ParameterValue param)
        {
            TriggerValueChange();
        }

        public void PopulateGraphParams()
        {
            if (!isDirty) return;

            Graph.Graph p = parentGraph;
            foreach (string k in Parameters.Keys)
            {
                ParameterValue realParam = Parameters[k];
                string[] split = k.Split('.');
                if (split.Length < 2) continue;
                if (p.HasParameterValue(split[0], split[1]))
                {
                    realParam.AssignValue(p.GetParameterValue(split[0], split[1]));
                }
            }

            foreach(ParameterValue realParam in CustomParameters)
            {
                if (p.HasParameterValue(Id, realParam.Name))
                {
                    realParam.AssignValue(p.GetParameterValue(Id, realParam.Name));
                }
            }

            isDirty = false;
        }

        protected override void OnPixelFormatChange()
        {
            base.OnPixelFormatChange();
            GraphInst?.AssignPixelType(internalPixelType);
        }

        public override void AssignPixelType(GraphPixelType pix)
        {
            base.AssignPixelType(pix);
            GraphInst?.AssignPixelType(pix);
        }

        public override void SetSize(int w, int h)
        {
            base.SetSize(w, h);
            GraphInst?.ResizeWith(w, h);
        }

        //we actually store the graph raw data
        //so this file can be transported without needing
        //the original graph file
        public class GraphInstanceNodeData : NodeData
        {
            public Dictionary<string, object> parameters = new Dictionary<string, object>();
            public Dictionary<string, object> customParameters = new Dictionary<string, object>();
            public int randomSeed;
            public string path;
            public string rawData;

            public override void Write(Writer w)
            {
                base.Write(w);
                w.Write(randomSeed);
                w.Write(path);

                w.Write(parameters.Count);

                foreach(string k in parameters.Keys)
                {
                    object o = parameters[k];
                    w.Write(k);

                    if (o.IsNumber())
                    {
                        w.Write((int)NodeType.Float);
                        w.Write(o.ToFloat());
                    }
                    else if(o.IsBool())
                    {
                        w.Write((int)NodeType.Bool);
                        w.Write(o.ToBool());
                    }
                    else if(o.IsVector())
                    {
                        w.Write((int)NodeType.Float4);
                        MVector mv = (MVector)o;
                        w.WriteObjectList(mv.ToArray());
                    }
                    else if(o.IsMatrix())
                    {
                        w.Write((int)NodeType.Matrix);
                        Matrix4 m = (Matrix4)o;
                        w.WriteObjectList(m.ToArray());
                    }
                    else
                    {
                        w.Write((int)NodeType.Float);
                        w.Write((float)0);
                    }
                }

                w.Write(customParameters.Count);

                foreach(string k in customParameters.Keys)
                {
                    object o = parameters[k];
                    w.Write(k);

                    if (o.IsNumber())
                    {
                        w.Write((int)NodeType.Float);
                        w.Write(o.ToFloat());
                    }
                    else if (o.IsBool())
                    {
                        w.Write((int)NodeType.Bool);
                        w.Write(o.ToBool());
                    }
                    else if (o.IsVector())
                    {
                        w.Write((int)NodeType.Float4);
                        MVector mv = (MVector)o;
                        w.WriteObjectList(mv.ToArray());
                    }
                    else if (o.IsMatrix())
                    {
                        w.Write((int)NodeType.Matrix);
                        Matrix4 m = (Matrix4)o;
                        w.WriteObjectList(m.ToArray());
                    }
                    else
                    {
                        w.Write((int)NodeType.Float);
                        w.Write((float)0);
                    }
                }
            }

            public override void Parse(Reader r)
            {
                base.Parse(r);
                randomSeed = r.NextInt();
                path = r.NextString();

                parameters = new Dictionary<string, object>();
                customParameters = new Dictionary<string, object>();

                int pcount = r.NextInt();
                for (int i = 0; i < pcount; ++i)
                {
                    string k = r.NextString();
                    NodeType type = (NodeType)r.NextInt();

                    switch (type)
                    {
                        case NodeType.Bool:
                            parameters[k] = r.NextBool();
                            break;
                        case NodeType.Float:
                            parameters[k] = r.NextFloat();
                            break;
                        case NodeType.Float2:
                        case NodeType.Float3:
                        case NodeType.Float4:
                        case NodeType.Gray:
                        case NodeType.Color:
                            parameters[k] = MVector.FromArray(r.NextList<float>());
                            break;
                        case NodeType.Matrix:
                            Matrix4 mv = Matrix4.Identity;
                            float[] values = r.NextList<float>();
                            mv.FromArray(values);
                            parameters[k] = mv;
                            break;
                    }
                }

                pcount = r.NextInt();
                for (int i = 0; i < pcount; ++i)
                {
                    string k = r.NextString();
                    NodeType type = (NodeType)r.NextInt();

                    switch (type)
                    {
                        case NodeType.Bool:
                            customParameters[k] = r.NextBool();
                            break;
                        case NodeType.Float:
                            customParameters[k] = r.NextFloat();
                            break;
                        case NodeType.Float2:
                        case NodeType.Float3:
                        case NodeType.Float4:
                        case NodeType.Gray:
                        case NodeType.Color:
                            customParameters[k] = MVector.FromArray(r.NextList<float>());
                            break;
                        case NodeType.Matrix:
                            Matrix4 mv = Matrix4.Identity;
                            float[] values = r.NextList<float>();
                            mv.FromArray(values);
                            customParameters[k] = mv;
                            break;
                    }
                }
            }
        }

        public override void CopyResources(string CWD)
        {
            if (!string.IsNullOrEmpty(path) 
                && !string.IsNullOrEmpty(relativePath) && File.Exists(path))
            {
                CopyResourceTo(CWD, relativePath, path);
            }
        }

        public override void FromJson(string data, Archive arch = null)
        {
            archive = arch;
            FromJson(data);
        }

        //helper function for older graphs
        protected void ValidatePixelType()
        {
            if(internalPixelType != GraphPixelType.Luminance32F 
                && internalPixelType != GraphPixelType.Luminance16F
                && internalPixelType != GraphPixelType.RGB
                && internalPixelType != GraphPixelType.RGB16F
                && internalPixelType != GraphPixelType.RGB32F
                && internalPixelType != GraphPixelType.RGBA
                && internalPixelType != GraphPixelType.RGBA16F
                && internalPixelType != GraphPixelType.RGBA32F)
            {
                internalPixelType = GraphPixelType.RGBA;
            }
        }

        public override void GetBinary(Writer w)
        {
            GraphInstanceNodeData d = new GraphInstanceNodeData();
            FillBaseNodeData(d);
            d.path = path;
            d.rawData = string.IsNullOrEmpty(rawGraphData) ? GraphInst?.GetJson() : rawGraphData;
            
            if (GraphInst != null)
            {
                d.parameters = GraphInst.GetConstantParameters();
                d.customParameters = GraphInst.GetCustomParameters();
            }

            d.randomSeed = randomSeed;
            d.Write(w);
        }

        public override void FromBinary(Reader r, Archive arch = null)
        {
            archive = arch;
            FromBinary(r);
        }

        public override void FromBinary(Reader r)
        {
            GraphInstanceNodeData d = new GraphInstanceNodeData();
            d.Parse(r);
            SetBaseNodeDate(d);
            path = d.path;
            randomSeed = d.randomSeed;
            jsonParameters = d.parameters;
            jsonCustomParameters = d.customParameters;

            ValidatePixelType();

            Load(path);
        }

        public override void FromJson(string data)
        {
            GraphInstanceNodeData d = JsonConvert.DeserializeObject<GraphInstanceNodeData>(data);
            SetBaseNodeDate(d);
            rawGraphData = d.rawData;
            path = d.path;
            jsonParameters = d.parameters;
            jsonCustomParameters = d.customParameters;
            randomSeed = d.randomSeed;

            ValidatePixelType();

            //we do this incase 
            //the original graph was updated
            //and thus we should pull it in
            //if it exists
            //otherwise we fall back on
            //last saved graph data
            //also we do this
            //to try and load from 
            //archive first
            //this only applies to the JSON format
            //otherwise the binary expects the actual graph
            //file is included in resources folder or is a shelf file
            bool didLoad = Load(path);

            //if path not found or could not load
            //fall back to last instance data saved
            if (!didLoad && !string.IsNullOrEmpty(rawGraphData))
            {
                loading = true;
                PrepareGraph();
                loading = false;
            }
        }

        public override string GetJson()
        {
            GraphInstanceNodeData d = new GraphInstanceNodeData();
            FillBaseNodeData(d);
            d.path = path;
            
            if (GraphInst != null)
            {
                d.parameters = GraphInst.GetConstantParameters();
                d.customParameters = GraphInst.GetCustomParameters();
            }
           
            d.randomSeed = RandomSeed;

            return JsonConvert.SerializeObject(d);
        }

        public override void Dispose()
        {
            if (parentGraph != null)
            {
                parentGraph.OnParameterUpdate -= ParentGraph_OnParameterUpdate;
            }

            if (child != null)
            {
                child.Dispose();
                child = null;
            }

            base.Dispose();

            if(GraphInst != null)
            {
                GraphInst.OnParameterUpdate -= GraphInst_OnParameterUpdate;
                GraphInst.Dispose();
                GraphInst = null;
            }
        }
    }
}
