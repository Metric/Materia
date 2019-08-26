using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Materia.Imaging;
using Materia.Nodes.Attributes;
using Materia.Imaging.GLProcessing;
using Materia.Textures;
using System.Reflection;
using Newtonsoft.Json;
using NLog;
using Materia.Nodes.Atomic;
using Materia.Archive;

namespace Materia.Nodes
{
    public abstract class Node : IDisposable
    {
        private static ILogger Log = LogManager.GetCurrentClassLogger();

        public static TaskScheduler Context { get; set; }
        public bool Async { get; set; }
        public bool IsScheduled { get; set; }

        public delegate void UpdateEvent(Node n);
        public delegate void InputChanged(Node n, NodeInput inp);
        public delegate void OutputChanged(Node n, NodeOutput inp);
        public delegate void DescriptionChange(Node n, string desc);

        protected delegate void GraphParentSet();
        protected event GraphParentSet OnGraphParentSet;

        public event UpdateEvent OnUpdate;
        public event UpdateEvent OnNameUpdate;

        public event InputChanged OnInputRemovedFromNode;
        public event InputChanged OnInputAddedToNode;

        public event OutputChanged OnOutputRemovedFromNode;
        public event OutputChanged OnOutputAddedToNode;

        public event DescriptionChange OnDescriptionChanged;

        public bool CanPreview = true;

        public double ViewOriginX = 0;
        public double ViewOriginY = 0;

        protected BasicImageRenderer previewProcessor;

        protected long lastUpdated = 0;

        protected GLTextuer2D buffer;
        public GLTextuer2D Buffer
        {
            get
            {
                return buffer;
            }
        }

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

                if(OnGraphParentSet != null)
                {
                    OnGraphParentSet.Invoke();
                }
            }
        }

        public string Id { get; set; }

        protected string name;

        [Editable(ParameterInputType.Text, "Name", "Basic")]
        public string Name
        {
            get
            {
                return name;
            }
            set
            {
                name = value;
                if(OnNameUpdate != null)
                {
                    OnNameUpdate.Invoke(this);
                }
            }
        }

        public class NodeData
        {
            public string id;
            public int width;
            public int height;
            public bool absoluteSize;
            public string type;
            public List<NodeConnection> outputs;
            public float tileX;
            public float tileY;
            public string name;
            public GraphPixelType internalPixelType;
            public int inputCount;
            public int outputCount;
            public double viewOriginX;
            public double viewOriginY;
        }

        protected FloatBitmap brush;
        public FloatBitmap Brush
        {
            get
            {
                return brush;
            }
        }

        protected int width;

        [Editable(ParameterInputType.IntSlider, "Width", "Basic", 8, 8192, new float[] { 8, 16, 32, 64, 128, 256, 512, 1024, 2048, 4096 })]
        public int Width
        {
            get
            {
                return width;
            }
            set
            {
                width = value;
                OnWidthHeightSet();
            }
        }

        protected int height;
        [Editable(ParameterInputType.IntSlider, "Height", "Basic", 8, 8192, new float[] { 8, 16, 32, 64, 128, 256, 512, 1024, 2048, 4096 })]
        public int Height
        {
            get
            {
                return height;
            }
            set
            {
                height = value;
                OnWidthHeightSet();
            }
        }

        [Editable(ParameterInputType.Toggle, "Absolute Size", "Basic")]
        public bool AbsoluteSize { get; set; }

        protected float tileX;
        protected float tileY;

        [Editable(ParameterInputType.FloatInput, "Tile X", "Basic")]
        public float TileX
        {
            get
            {
                return tileX;
            }
            set
            {
                tileX = value;
                TryAndProcess();
            }
        }

        [Editable(ParameterInputType.FloatInput, "Tile Y", "Basic")]
        public float TileY
        {
            get
            {
                return tileY;
            }
            set
            {
                tileY = value;
                TryAndProcess();
            }
        }

        protected GraphPixelType internalPixelType;
      
        [Editable(ParameterInputType.Dropdown, "Texture Format", "Basic")]
        public GraphPixelType InternalPixelFormat
        {
            get
            {
                return internalPixelType;
            }
            set
            {
                internalPixelType = value;
                OnPixelFormatChange();
            }
        }

        protected bool HasEmpytOutput
        {
            get
            {
                if (Outputs == null) return false;

                return Outputs.FindIndex(m => m.To.Count == 0) > -1;
            }
        }

        protected bool HasEmptyInput
        {
            get
            {
                if (Inputs == null) { return false; }

                return Inputs.FindIndex(m => !m.HasInput) > -1;
            }
        }

        protected static uint Count = 0;
        public List<NodeInput> Inputs { get; protected set; }
        public List<NodeOutput> Outputs { get; protected set; }

        protected abstract void OnWidthHeightSet();
        protected virtual void OnDescription(string desc)
        {
            if(OnDescriptionChanged != null)
            {
                OnDescriptionChanged.Invoke(this, desc);
            }
        }

        public virtual string GetDescription()
        {
            return "";
        }

        protected void Updated()
        {
            if(OnUpdate != null)
            {
                OnUpdate.Invoke(this);
            }
        }

        protected virtual void RemoveParameters()
        {
            var p = ParentGraph;

            if(p != null && p is FunctionGraph)
            {
                p = (p as FunctionGraph).TopGraph();
            }

            if(p != null)
            {
                PropertyInfo[] infos = GetType().GetProperties();

                foreach(var info in infos)
                {
                    p.RemoveParameterValue(Id, info.Name);
                }
            }
        }

        public virtual void Dispose()
        {
            RemoveParameters();

            ParentGraph = null;

            if(buffer != null)
            {
                buffer.Release();
                buffer = null;
            }

            if(previewProcessor != null)
            {
                previewProcessor.Release();
                previewProcessor = null;
            }
        }
        public abstract string GetJson();
        public abstract void FromJson(string data, MTGArchive archive = null);

        public virtual void AssignWidth(int w)
        {
            width = w;
        }

        public virtual void AssignHeight(int h)
        {
            height = h;
        }

        public virtual void AssignParentGraph(Graph g)
        {
            parentGraph = g;
        }

        public virtual void AssignParentNode(Node n)
        {
           
        }

        public virtual void AssignPixelType(GraphPixelType pix)
        {
            internalPixelType = pix;
            ReleaseBuffer();
        }

        public virtual void SetSize(int w, int h)
        {
            width = w;
            height = h;
            OnWidthHeightSet();
        }

        public virtual void CopyResources(string CWD) { }

        protected virtual void CopyResourceTo(string CWD, string relative, string from)
        {
            if (string.IsNullOrEmpty(CWD) || string.IsNullOrEmpty(relative) 
                || string.IsNullOrEmpty(from))
            {
                return;
            }

            string cpath = System.IO.Path.Combine(CWD, relative);
            string cdir = System.IO.Path.GetDirectoryName(cpath);
            if (!System.IO.Directory.Exists(cdir))
            {
                System.IO.Directory.CreateDirectory(cdir);
            }

            //if the paths are the same do nothing!
            if(from.Equals(cpath))
            {
                return;
            }

            if (System.IO.File.Exists(from) && !System.IO.File.Exists(cpath))
            {
                System.IO.File.Copy(from, cpath);
            }
            else if (!string.IsNullOrEmpty(ParentGraph.CWD))
            {
                string opath = System.IO.Path.Combine(ParentGraph.CWD, relative);

                //if the paths are the same do nothing!
                if(opath.Equals(cpath))
                {
                    return;
                }

                if (System.IO.File.Exists(opath) && !System.IO.File.Exists(cpath))
                {
                    System.IO.File.Copy(opath, cpath);
                }
            }
        }

        public virtual byte[] GetPreview(int width, int height)
        {
            return null;
        }

        public virtual GLTextuer2D GetActiveBuffer()
        {
            return buffer;
        }

        public virtual void TryAndProcess()
        {

        }

        public virtual Node TopNode()
        {
            Node p = this;

            while(p != null && p is MathNode)
            {
                var tmp = (p as MathNode).ParentNode;
                if(tmp == null)
                {
                    return p;
                }
                p = tmp;
            }

            return p;
        }

        protected virtual void SetBaseNodeDate(NodeData d)
        {
            width = d.width;
            height = d.height;
            AbsoluteSize = d.absoluteSize;
            internalPixelType = d.internalPixelType;
            name = d.name;
            tileX = d.tileX;
            tileY = d.tileY;
            ViewOriginX = d.viewOriginX;
            ViewOriginY = d.viewOriginY;

            if(Inputs == null && d.inputCount > 0)
            {
                Inputs = new List<NodeInput>();

                for(int i = 0; i < d.inputCount; i++)
                {
                    AddPlaceholderInput();
                }
            }
            else if(Inputs != null && Inputs.Count < d.inputCount)
            {
                int diff = d.inputCount - Inputs.Count;

                for(int i = 0; i < diff; i++)
                {
                    AddPlaceholderInput();
                }
            }

            if(Outputs == null && d.outputCount > 0)
            {
                Outputs = new List<NodeOutput>();

                for(int i = 0; i < d.outputCount; i++)
                {
                    AddPlaceholderOutput();
                }
            }
            else if(Outputs != null && Outputs.Count < d.outputCount)
            {
                int diff = d.outputCount - Outputs.Count;
                for(int i = 0; i < diff; i++)
                {
                    AddPlaceholderOutput();
                }
            }
        }

        protected virtual void FillBaseNodeData(NodeData d)
        {
            d.width = width;
            d.height = height;
            d.absoluteSize = AbsoluteSize;
            d.internalPixelType = internalPixelType;
            d.name = name;
            d.outputs = GetConnections();
            d.tileX = tileX;
            d.tileY = tileY;
            d.type = GetType().ToString();
            d.id = Id;
            d.inputCount = Inputs == null ? 0 : Inputs.Count;
            d.outputCount = Outputs == null ? 0 : Outputs.Count;
            d.viewOriginX = ViewOriginX;
            d.viewOriginY = ViewOriginY;
        }

        protected virtual void AddPlaceholderOutput()
        {

        }

        protected virtual void AddPlaceholderInput()
        {

        }

        protected void RemovedInput(NodeInput inp)
        {
            if(OnInputRemovedFromNode != null)
            {
                OnInputRemovedFromNode.Invoke(this, inp);
            }
        }

        protected void AddedInput(NodeInput inp)
        {
            if(OnInputAddedToNode != null)
            {
                OnInputAddedToNode.Invoke(this, inp);
            }
        }

        protected void RemovedOutput(NodeOutput inp)
        {
            if (OnOutputRemovedFromNode != null)
            {
                OnOutputRemovedFromNode.Invoke(this, inp);
            }
        }

        protected void AddedOutput(NodeOutput inp)
        {
            if (OnOutputAddedToNode != null)
            {
                OnOutputAddedToNode.Invoke(this, inp);
            }
        }

        protected virtual void OnPixelFormatChange()
        {
            if(buffer != null)
            {
                buffer.Release();
                buffer = null;
            }

            TryAndProcess();
        }

        protected virtual void CreateBufferIfNeeded()
        {
            if (buffer == null || buffer.Id == 0)
            {
                buffer = new GLTextuer2D((GLInterfaces.PixelInternalFormat)((int)internalPixelType));
                buffer.Bind();
                buffer.SetData(IntPtr.Zero, GLInterfaces.PixelFormat.Rgba, width, height);
                buffer.Linear();
                buffer.Repeat();
                if(internalPixelType == GraphPixelType.Luminance16F || internalPixelType == GraphPixelType.Luminance32F)
                {
                    buffer.SetSwizzleLuminance();
                }
                GLTextuer2D.Unbind();
            }
        }

        public virtual void ReleaseBuffer()
        {
            if(buffer != null)
            {
                buffer.Release();
                buffer = null;
            }
        }

        public List<NodeConnection> GetConnections()
        {
            List<NodeConnection> outputs = new List<NodeConnection>();

            int i = 0;
            foreach (NodeOutput Output in Outputs)
            {
                int k = 0;
                foreach (NodeInput n in Output.To)
                {
                    int index = n.Node.Inputs.IndexOf(n);

                    if (index > -1)
                    {
                        NodeConnection nc = new NodeConnection(Id, n.Node.Id, i, index, k);
                        outputs.Add(nc);
                    }
                    k++;
                }
                i++;
            }

            return outputs;
        }


        /// <summary>
        /// This is used in the Undo / Redo System
        /// </summary>
        /// <returns></returns>
        public List<NodeConnection> GetParentConnections()
        {
            List<NodeConnection> connections = new List<NodeConnection>();
            int i = 0;
            foreach (NodeInput n in Inputs)
            {
                if (n.HasInput)
                {
                    int idx = n.Input.Node.Outputs.IndexOf(n.Input);
                    if (idx > -1)
                    {
                        NodeOutput no = n.Input.Node.Outputs[idx];
                        int ord = no.To.IndexOf(n);
                        NodeConnection nc = new NodeConnection(n.Input.Node.Id, Id, idx, i, ord);
                        connections.Add(nc);
                    }
                }
                i++;
            }

            return connections;
        }

        /// <summary>
        /// This is used in the Undo / Redo System
        /// </summary>
        /// <param name="n"></param>
        /// <param name="connection"></param>
        public void SetConnection(Node n, NodeConnection connection)
        {
            if(connection.index < n.Inputs.Count)
            {
                var inp = n.Inputs[connection.index];
                if(connection.outIndex >= 0 && connection.outIndex < Outputs.Count)
                {
                    Outputs[connection.outIndex].InsertAt(connection.order, inp, false);
                }
            }
            else
            {
                //log to console the fact that we could not connect the node
                Log.Warn("Could not restore a node connections on: " + n.name);
            }
        }

        /// <summary>
        /// This is used in the Graph.FromJson
        /// And thus the NodeOutput.InsertAt is not used
        /// as there should be no inputs added already
        /// </summary>
        /// <param name="nodes"></param>
        /// <param name="connections"></param>
        /// <param name="triggerAddEvent"></param>
        public void SetConnections(Dictionary<string,Node> nodes, List<NodeConnection> connections, bool triggerAddEvent = true)
        {
            if (connections != null)
            {
                foreach (NodeConnection nc in connections)
                {
                    Node n = null;
                    if (nodes.TryGetValue(nc.node, out n))
                    {
                        if (nc.index < n.Inputs.Count)
                        {
                            var inp = n.Inputs[nc.index];
                            if (nc.outIndex >= 0 && nc.outIndex < Outputs.Count)
                            {
                                Outputs[nc.outIndex].Add(inp, triggerAddEvent);
                            }
                        }
                        else
                        {
                            //log to console the fact that we could not connect the node
                            Log.Warn("Could not restore a node connections on: " + n.name);
                        }
                    }
                    else
                    {
                        //log to console the fact that we could not connect the node
                        Log.Warn("Could not restore a node connection");
                    }
                }
            }
        }

        public abstract Task GetTask();

        public virtual bool IsRoot()
        {
            bool realputs = Inputs == null || Inputs.Count == 0;

            if (realputs) return true;

            var inp = Inputs.Find(m => m.HasInput);
            if (inp == null) return true;

            if (inp.Node is GraphInstanceNode) return true;
            return false;
        }
    }
}
