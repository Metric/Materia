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

namespace Materia.Nodes.Atomic
{
    public class GraphInstanceNode : ImageNode
    {
        public Graph GraphInst { get; protected set; }

        CancellationTokenSource ctk;

        protected string path;
        protected Dictionary<string, string> jsonParameters;

        [FileSelector]
        [Section(Section = "Content")]
        [Title(Title = "Materia Graph File")]
        public string GraphFilePath
        {
            get
            {
                return path;
            }
            set
            {
                path = value;
                Load(path);
                Updated();
            }
        }

        [ParameterMapEditor]
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

        [HideProperty]
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

        [HideProperty]
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

        [HideProperty]
        public new GraphPixelType InternalPixelFormat
        {
            get
            {
                return internalPixelType;
            }
            set
            {
                internalPixelType = value;
            }
        }

        protected string GraphData { get; set; }

        public GraphInstanceNode(int w, int h, GraphPixelType p = GraphPixelType.RGBA)
        {
            width = w;
            height = h;

            Id = Guid.NewGuid().ToString();

            tileX = tileY = 1;

            Name = "Graph Instance";

            this.path = "";

            //we do not initialize the inputs and outputs here
            //instead they are loaded after the graph is loaded
            Inputs = new List<NodeInput>();
            Outputs = new List<NodeOutput>();

            GraphParameterValue.OnGraphParameterUpdate += GraphParameterValue_OnGraphParameterUpdate;
        }

        private void GraphParameterValue_OnGraphParameterUpdate(GraphParameterValue param)
        {
            if(GraphInst != null)
            {
                if(GraphInst.Parameters.Values.Contains(param))
                {
                    GraphInst.TryAndProcess();
                }
            }
        }

        //used for initial loading
        //not used for restoring from .materia
        public bool Load(string path)
        {
            if(GraphInst != null)
            {
                GraphData = null;

                GraphInst.Dispose();
                GraphInst = null;
            }

            if (File.Exists(path) && Path.GetExtension(path).ToLower().Contains("mtg"))
            {
                this.path = path;

                string nm = Path.GetFileNameWithoutExtension(path);

                Name = nm;

                //the width and height here don't matter
                GraphInst = new Graph(nm);

                GraphData = File.ReadAllText(path);

                GraphInst.FromJson(GraphData);
                GraphInst.SetJsonReadyParameters(jsonParameters);

                //now do real initial resize
                GraphInst.ResizeWith(width, height);

                //mark as readonly
                GraphInst.ReadOnly = true;

                //setup inputs and outputs

                Setup();

                return true;
            }
            else
            {
                this.path = null;
            }

            return false;
        }

        void Setup()
        {
            if(GraphInst.InputNodes.Count > 0)
            {
                foreach(string id in GraphInst.InputNodes)
                {
                    Node n;
                    if(GraphInst.NodeLookup.TryGetValue(id, out n))
                    {
                        InputNode inp = (InputNode)n;
                        NodeInput np = new NodeInput(NodeType.Color | NodeType.Gray, this, inp.Name);
                       
                        inp.SetInput(np);
                        Inputs.Add(np);
                    }
                }
            }

            if(GraphInst.OutputNodes.Count > 0)
            {
                foreach(string id in GraphInst.OutputNodes)
                {
                    Node n;
                    if(GraphInst.NodeLookup.TryGetValue(id, out n))
                    {
                        OutputNode op = (OutputNode)n;

                        NodeOutput ot;

                        ot = new NodeOutput(NodeType.Color | NodeType.Gray, n, op.Name);
                        //we add to our graph instance outputs so things can actually connect 
                        //to the output
                        Outputs.Add(ot);
                        op.SetOutput(ot);

                        n.OnUpdate += N_OnUpdate;
                    }
                }
            }

            //TODO:
            //setup paramater links later
        }

        private void N_OnUpdate(Node n)
        {
            Updated();
            TryAndReleaseBuffers();
        }

        void TryAndReleaseBuffers()
        {
            if(ctk != null)
            {
                try
                {
                    ctk.Cancel();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }

            ctk = new CancellationTokenSource();

            Task.Delay(250, ctk.Token).ContinueWith(t =>
            {
                if (t.IsCanceled) return;

                App.Current.Dispatcher.Invoke(() =>
                {
                    ctk = null;
                    if (GraphInst != null)
                    {
                        GraphInst.ReleaseIntermediateBuffers();
                    }
                });
            });
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

        //we actually store the graph raw data
        //so this file can be transported without needing
        //the original graph file
        public class GraphInstanceNodeData : NodeData
        {
            public List<string> inputIds;
            public Dictionary<string, string> parameters;
            public string rawData;
            public string path;
        }

        public override void FromJson(Dictionary<string, Node> nodes, string data)
        {
            GraphInstanceNodeData d = JsonConvert.DeserializeObject<GraphInstanceNodeData>(data);
            SetBaseNodeDate(d);
            GraphData = d.rawData;
            path = d.path;
            jsonParameters = d.parameters;

            bool didLoad = false;

            //we do this incase 
            //the original graph was updated
            //and thus we should pull it in
            //if it exists
            //otherwise we fall back on
            //last saved graph data

            if(File.Exists(path))
            {
                didLoad = Load(path);
            }

            //if path not found or could not load
            //fall back to last instance data saved
            if (!didLoad)
            {
                GraphInst = new Graph(Name);
                GraphInst.FromJson(GraphData);
                GraphInst.SetJsonReadyParameters(jsonParameters);
                GraphInst.ResizeWith(width, height);

                Setup();
            }
        }

        public override string GetJson()
        {
            GraphInstanceNodeData d = new GraphInstanceNodeData();
            FillBaseNodeData(d);
            d.rawData = GraphData;
            d.path = path;
            d.parameters = GraphInst.GetJsonReadyParameters();

            return JsonConvert.SerializeObject(d);
        }

        protected override void OnWidthHeightSet()
        {
            if(GraphInst != null)
            {
                GraphInst.ResizeWith(width, height);
            }

            Updated();
        }

        public override void Dispose()
        {
            base.Dispose();

            GraphParameterValue.OnGraphParameterUpdate -= GraphParameterValue_OnGraphParameterUpdate;

            if(GraphInst != null)
            {
                GraphInst.Dispose();
                GraphInst = null;
            }
        }
    }
}
