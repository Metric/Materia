using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.IO;
using Newtonsoft.Json;
using Materia.Nodes.Attributes;
using Materia.Textures;
using System.Threading;
using NLog;
using Materia.Archive;
using Materia.Layering;

namespace Materia.Nodes.Atomic
{
    public class GraphInstanceNode : ImageNode
    {
        private static ILogger Log = LogManager.GetCurrentClassLogger();

        public Graph GraphInst { get; protected set; }

        protected string path;
        protected Dictionary<string, object> jsonParameters;
        protected Dictionary<string, object> jsonCustomParameters;
        protected Dictionary<string, GraphParameterValue> nameMap;
        protected int randomSeed;
        protected bool updatingParams;

        protected bool loading;
        private bool isArchive;

        private MTGArchive archive;
        private MTGArchive child;

        private bool isLayer;
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
        public Dictionary<string, GraphParameterValue> Parameters
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
        public List<GraphParameterValue> CustomParameters
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

        protected string GraphData { get; set; }

        public GraphInstanceNode(int w, int h, GraphPixelType p = GraphPixelType.RGBA) : base()
        {
            isDirty = true;
            width = w;
            height = h;

            nameMap = new Dictionary<string, GraphParameterValue>();

            Id = Guid.NewGuid().ToString();

            tileX = tileY = 1;

            internalPixelType = p;

            Name = "Graph Instance";

            path = "";
        }

        public GraphParameterValue GetCustomParameter(string name)
        {
            GraphParameterValue v = null;
            nameMap.TryGetValue(name, out v);
            return v;
        }

        public void AssignSeed(int seed)
        {
            randomSeed = seed;
            GraphInst?.AssignSeed(seed);
        }

        public bool Load(string path)
        {
            if (string.IsNullOrEmpty(path)) return false;

            if (GraphInst != null)
            {
                GraphInst.OnGraphParameterUpdate -= GraphInst_OnGraphParameterUpdate;
                GraphInst.Dispose();
                GraphInst = null;
            }

            nameMap = new Dictionary<string, GraphParameterValue>();

            if (path.Contains("Materia::Layer::"))
            {
                isLayer = true;
                return TryAndLoadLayer(path);
            }

            isLayer = false;
            return TryAndLoadFile(path);
        }

        protected bool TryAndLoadLayer(string path)
        {
            string[] split = path.Split(new string[] { "::" }, StringSplitOptions.RemoveEmptyEntries);
            if (split.Length < 3) return false;
            string layerId = split[split.Length - 1];

            if (string.IsNullOrEmpty(layerId)) return false;

            Layer l = null;

            if (parentGraph == null) return false;

            Graph p = parentGraph;

            //this is only possible if this graph instance
            //actually belongs to a layer graph
            while(p.ParentGraph != null)
            {
                p = p.ParentGraph;
            }
            
            if (p.LayerLookup.TryGetValue(layerId, out l))
            {
                loading = true;
                this.path = path;
                Name = l.Name;
                GraphData = l.Core.GetJson();
                PrepareGraph();
                return true;
            }

            return false;
        }

        protected bool TryAndLoadFile(string path)
        {
            isArchive = path.ToLower().EndsWith(".mtga");
            //convert path to a relative resource path
            string relative = Path.Combine("resources", Path.GetFileName(path));

            //handle archives within archives
            if (isArchive && archive != null)
            {
                archive.Open();
                var files = archive.GetAvailableFiles();
                var m = files.Find(f => f.path.Equals(relative));
                if (m != null)
                {
                    loading = true;
                    child = new MTGArchive(relative, m.ExtractBinary());
                    child.Open();
                    var childFiles = child.GetAvailableFiles();
                    if (childFiles != null)
                    {
                        var mtg = childFiles.Find(f => f.path.ToLower().EndsWith(".mtg"));
                        if (mtg != null)
                        {
                            loading = true;
                            this.path = path;
                            string nm = Path.GetFileNameWithoutExtension(path);
                            Name = nm;
                            GraphData = mtg.ExtractText();
                            child.Close();
                            PrepareGraph();
                            archive.Close();
                            loading = false;
                            return true;
                        }
                    }
                }

                archive.Close();
            }
            //handle absolute path to archive when not in another archive
            else if (File.Exists(path) && isArchive && archive == null)
            {
                loading = true;
                child = new MTGArchive(path);
                child.Open();
                var childFiles = child.GetAvailableFiles();
                if (childFiles != null)
                {
                    var mtg = childFiles.Find(f => f.path.ToLower().EndsWith(".mtg"));
                    if (mtg != null)
                    {
                        loading = true;
                        this.path = path;
                        string nm = Path.GetFileNameWithoutExtension(path);
                        Name = nm;
                        GraphData = mtg.ExtractText();
                        child.Close();
                        PrepareGraph();
                        loading = false;
                        return true;
                    }
                }
            }
            //otherwise try relative storage for the archive when not in another archive
            else if (isArchive && archive == null && ParentGraph != null && !string.IsNullOrEmpty(ParentGraph.CWD) && File.Exists(Path.Combine(ParentGraph.CWD, relative)))
            {
                string realPath = Path.Combine(ParentGraph.CWD, relative);
                child = new MTGArchive(realPath);
                child.Open();
                var childFiles = child.GetAvailableFiles();
                if (childFiles != null)
                {
                    var mtg = childFiles.Find(f => f.path.ToLower().EndsWith(".mtg"));
                    if (mtg != null)
                    {
                        loading = true;
                        this.path = path;
                        string nm = Path.GetFileNameWithoutExtension(path);
                        Name = nm;
                        GraphData = mtg.ExtractText();
                        child.Close();
                        PrepareGraph();
                        loading = false;
                        return true;
                    }
                }
            }
            else if (!isArchive && File.Exists(path) && Path.GetExtension(path).ToLower().EndsWith("mtg"))
            {
                loading = true;
                this.path = path;
                string nm = Path.GetFileNameWithoutExtension(path);
                Name = nm;
                GraphData = File.ReadAllText(path);
                PrepareGraph();
                loading = false;
                return true;
            }

            return false;
        }

        void PrepareGraph()
        {
            isDirty = true;
            GraphInst = new ImageGraph(Name, width, height);
            GraphInst.AssignParentNode(this);
            GraphInst.FromJson(GraphData, child);

            GraphInst.AssignParameters(jsonParameters);
            GraphInst.AssignCustomParameters(jsonCustomParameters);
            GraphInst.AssignSeed(randomSeed);
            GraphInst.AssignPixelType(internalPixelType);
            GraphInst.OnGraphParameterUpdate += GraphInst_OnGraphParameterUpdate;
            //now do real initial resize
            GraphInst.ResizeWith(width, height);

            GraphInst.InitializeRenderTextures();
            //mark as readonly
            GraphInst.ReadOnly = true;

            //setup inputs and outputs
            Setup();
            loading = false;
        }

        private void GraphInst_OnGraphParameterUpdate(GraphParameterValue param)
        {
            TriggerValueChange();
        }

        void Setup()
        {
            var inputsConnections = GetParentConnections();
            var outputConnections = GetConnections();

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
                        NodeInput np = new NodeInput(NodeType.Color | NodeType.Gray, this, inp, inp.Name);
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
                        NodeOutput ot = new NodeOutput(NodeType.Color | NodeType.Gray, n, this, op.Name);
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
                if(parentGraph.NodeLookup.TryGetValue(con.parent, out n))
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
            if (!this.Load(path) && !string.IsNullOrEmpty(GraphData))
            { 
                nameMap = new Dictionary<string, GraphParameterValue>();
                loading = true;
                PrepareGraph();
                loading = false;
            }
        }

        public override byte[] GetPreview(int width, int height)
        {
            //we only show the first output as preview
            if(Outputs.Count > 0)
            {
                return Outputs[0].Node.GetPreview(width, height);
            }

            return null;
        }

        public override GLTextuer2D GetActiveBuffer()
        {
            if(Outputs.Count > 0)
            {
                return Outputs[0].Node.GetActiveBuffer();
            }

            return null;
        }

        public override void AssignParentGraph(Graph g)
        {
            if(parentGraph != null)
            {
                parentGraph.OnGraphParameterUpdate -= ParentGraph_OnGraphParameterUpdate;
            }

            base.AssignParentGraph(g);

            if (g != null)
            {
                g.OnGraphParameterUpdate += ParentGraph_OnGraphParameterUpdate;
            }
        }

        private void ParentGraph_OnGraphParameterUpdate(GraphParameterValue param)
        {
            isDirty = true;
            PopulateGraphParams();
            List<GraphInstanceNode> nodes = GraphInst.InstanceNodes;

            if(nodes != null)
            {
                foreach(GraphInstanceNode n  in nodes)
                {
                    n.isDirty = true;
                    n.PopulateGraphParams();
                }
            }

            TriggerValueChange();
        }

        public void PopulateGraphParams()
        {
            if (!isDirty) return;

            Graph p = parentGraph;
            foreach (string k in Parameters.Keys)
            {
                GraphParameterValue realParam = Parameters[k];
                string[] split = k.Split('.');
                if (split.Length < 2) continue;
                if (p.HasParameterValue(split[0], split[1]))
                {
                    realParam.AssignValue(p.GetParameterValue(split[0], split[1]));
                }
            }

            foreach(GraphParameterValue realParam in CustomParameters)
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
            public List<string> inputIds;
            public Dictionary<string, object> parameters;
            public Dictionary<string, object> customParameters;
            public int randomSeed;
            public string rawData;
            public string path;
            public bool isLayer;
        }

        public override void CopyResources(string CWD)
        {
            if(isArchive && archive == null && !string.IsNullOrEmpty(path))
            {
                string relative = Path.Combine("resources", Path.GetFileName(path));
                CopyResourceTo(CWD, relative, path);
            }
        }

        public override void FromJson(string data, MTGArchive arch = null)
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

        public override void FromJson(string data)
        {
            GraphInstanceNodeData d = JsonConvert.DeserializeObject<GraphInstanceNodeData>(data);
            SetBaseNodeDate(d);
            GraphData = d.rawData;
            path = d.path;
            jsonParameters = d.parameters;
            jsonCustomParameters = d.customParameters;
            randomSeed = d.randomSeed;
            isLayer = d.isLayer;

            ValidatePixelType();

            bool didLoad = false;

            //we do this incase 
            //the original graph was updated
            //and thus we should pull it in
            //if it exists
            //otherwise we fall back on
            //last saved graph data
            //also we do this
            //to try and load from 
            //archive first
            didLoad = Load(path);

            //if path not found or could not load
            //fall back to last instance data saved
            if (!didLoad && !string.IsNullOrEmpty(GraphData))
            {
                nameMap = new Dictionary<string, GraphParameterValue>();
                loading = true;
                PrepareGraph();
                loading = false;
            }
        }

        public override string GetJson()
        {
            GraphInstanceNodeData d = new GraphInstanceNodeData();
            FillBaseNodeData(d);
            d.rawData = GraphData;
            d.path = path;
            
            if (GraphInst != null)
            {
                d.parameters = GraphInst.GetConstantParameters();
                d.customParameters = GraphInst.GetCustomParameters();
            }
           
            d.randomSeed = RandomSeed;
            d.isLayer = isLayer;

            return JsonConvert.SerializeObject(d);
        }

        public override void Dispose()
        {
            if (parentGraph != null)
            {
                parentGraph.OnGraphParameterUpdate -= ParentGraph_OnGraphParameterUpdate;
            }

            if (child != null)
            {
                child.Dispose();
                child = null;
            }

            base.Dispose();

            if(GraphInst != null)
            {
                GraphInst.OnGraphParameterUpdate -= GraphInst_OnGraphParameterUpdate;
                GraphInst.Dispose();
                GraphInst = null;
            }
        }
    }
}
