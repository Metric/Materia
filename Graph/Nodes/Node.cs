using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Materia.Rendering.Imaging;
using Materia.Rendering.Imaging.Processing;
using Materia.Rendering.Textures;
using Materia.Rendering.Interfaces;
using Materia.Rendering.Attributes;
using System.Reflection;
using Materia.Graph;
using MLog;

namespace Materia.Nodes
{
    public abstract class Node : IDisposable, ISchedulable
    {
        

        public static TaskScheduler Context { get; set; }
        public static SynchronizationContext SyncContext { get; set; }

        public delegate void NodeChanged(Node n);
        public delegate void InputChanged(Node n, NodeInput inp, NodeInput previous = null);
        public delegate void OutputChanged(Node n, NodeOutput inp, NodeOutput previosu = null);

        public event InputChanged OnInputRemovedFromNode;
        public event InputChanged OnInputAddedToNode;

        public event NodeChanged OnTextureChanged;
        public event NodeChanged OnTextureRebuilt;
        public event NodeChanged OnValueUpdated;
        public event NodeChanged OnSizeChanged;
        public event NodeChanged OnNameChanged;

        public event OutputChanged OnOutputRemovedFromNode;
        public event OutputChanged OnOutputAddedToNode;

        public bool CanPreview = true;

        public double ViewOriginX = 0;
        public double ViewOriginY = 0;

        protected BasicImageRenderer previewProcessor;

        protected GLTexture2D buffer;
        public GLTexture2D Buffer
        {
            get
            {
                return buffer;
            }
        }

        protected Graph.Graph parentGraph;
        public Graph.Graph ParentGraph
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
                OnNameChanged?.Invoke(this);
            }
        }

        public bool IsScheduled
        {
            get; set;
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
                OnSizeChanged?.Invoke(this);
                ReleaseBuffer();
                TriggerValueChange();
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
                OnSizeChanged?.Invoke(this);
                ReleaseBuffer();
                TriggerValueChange();
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
                TriggerValueChange();
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
                TriggerValueChange();
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
                TriggerValueChange();
            }
        }

        protected bool HasEmpytOutput
        {
            get
            {
                return false;
            }
        }

        protected bool HasEmptyInput
        {
            get
            {
                return false;
            }
        }

        protected static uint Count = 0;
        public List<NodeInput> Inputs { get; protected set; }
        public List<NodeOutput> Outputs { get; protected set; }

        protected List<NodeConnection> rawNodeConnections = null;

        public Node()
        {
            Outputs = new List<NodeOutput>();
            Inputs = new List<NodeInput>();
        }

        public virtual string GetDescription()
        {
            return "";
        }

        protected virtual void RemoveParameters()
        {
            var p = ParentGraph;

            if(p != null && p is Function)
            {
                Function fg = p as Function;
                p = fg.ParentNode != null ? fg.ParentNode.ParentGraph : fg.ParentGraph;
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

        public virtual void TryAndProcess() 
        {
            
        }

        public virtual void TriggerValueChange()
        {
            OnValueUpdated?.Invoke(this);

            if(parentGraph != null)
            {
                if ((parentGraph.ParentNode == null || parentGraph is Function) 
                    && parentGraph.State == GraphState.Ready)
                {
                    parentGraph.Schedule(this);
                }
                else if(parentGraph.ParentNode != null)
                {
                    //go up heirachy to trigger real parent node
                    Node n = parentGraph.ParentNode;
                    while(n.parentGraph.ParentNode != null)
                    {
                        Node tmp = n.parentGraph.ParentNode;
                        if (tmp == null)
                        {
                            break;
                        }

                        n = tmp;
                    }

                    if (n.parentGraph.State == GraphState.Ready)
                    {
                        n.TriggerValueChange();
                    }
                }
            }
        }

        public virtual void TriggerTextureChange()
        {
            OnTextureChanged?.Invoke(this);
        }

        public virtual void Dispose()
        {
            RemoveParameters();

            ParentGraph = null;

            if(buffer != null)
            {
                buffer.Dispose();
                buffer = null;
            }

            if(previewProcessor != null)
            {
                previewProcessor.Dispose();
                previewProcessor = null;
            }
        }
        public abstract string GetJson();
        public abstract void FromJson(string data, Archive archive = null);

        public virtual void AssignParentGraph(Graph.Graph g)
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

            ReleaseBuffer();
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

        public virtual GLTexture2D GetActiveBuffer()
        {
            return buffer;
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
            rawNodeConnections = d.outputs;

            if(Inputs.Count < d.inputCount)
            {
                int diff = d.inputCount - Inputs.Count;

                for(int i = 0; i < diff; ++i)
                {
                    AddPlaceholderInput();
                }
            }

            if(Outputs.Count < d.outputCount)
            {
                int diff = d.outputCount - Outputs.Count;
                for(int i = 0; i < diff; ++i)
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

        protected void AddedInput(NodeInput inp, NodeInput previous = null)
        {
            if(OnInputAddedToNode != null)
            {
                OnInputAddedToNode.Invoke(this, inp, previous);
            }
        }

        protected void RemovedOutput(NodeOutput inp)
        {
            if (OnOutputRemovedFromNode != null)
            {
                OnOutputRemovedFromNode.Invoke(this, inp);
            }
        }

        protected void AddedOutput(NodeOutput inp, NodeOutput previous = null)
        {
            if (OnOutputAddedToNode != null)
            {
                OnOutputAddedToNode.Invoke(this, inp, previous);
            }
        }

        protected virtual void OnPixelFormatChange()
        {
            ReleaseBuffer();
        }

        protected virtual void CreateBufferIfNeeded()
        {
            if (buffer == null || buffer.Id == 0)
            {
                buffer = new GLTexture2D((PixelInternalFormat)((int)internalPixelType));
                buffer.Bind();
                buffer.SetData(IntPtr.Zero, PixelFormat.Rgba, width, height);
                buffer.Linear();
                buffer.Repeat();
                
                if(internalPixelType == GraphPixelType.Luminance16F || internalPixelType == GraphPixelType.Luminance32F)
                {
                    buffer.SetSwizzleLuminance();
                }
                else if(buffer.IsRGBBased)
                {
                    buffer.SetSwizzleRGB();
                }

                GLTexture2D.Unbind();
                OnTextureRebuilt?.Invoke(this);
            }
        }

        public virtual void ReleaseBuffer()
        {
            if(buffer != null)
            {
                buffer.Dispose();
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
                    ++k;
                }
                ++i;
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
                    int idx = n.Reference.Node.Outputs.IndexOf(n.Reference);
                    if (idx > -1)
                    {
                        NodeOutput no = n.Reference.Node.Outputs[idx];
                        int ord = no.To.IndexOf(n);
                        NodeConnection nc = new NodeConnection(n.Reference.Node.Id, Id, idx, i, ord);
                        connections.Add(nc);
                    }
                }
                ++i;
            }

            return connections;
        }

        /// <summary>
        /// This is used in the Undo / Redo System
        /// </summary>
        /// <param name="n"></param>
        /// <param name="connection"></param>
        public void SetConnection(Node n, NodeConnection connection, bool assign)
        {
            if(connection.index < n.Inputs.Count)
            {
                var inp = n.Inputs[connection.index];
                if(connection.outIndex >= 0 && connection.outIndex < Outputs.Count)
                {
                    Outputs[connection.outIndex].InsertAt(connection.order, inp, assign);
                }
            }
            else
            {
                //log to console the fact that we could not connect the node
                Log.Warn("Could not restore a node connections on: " + n.name);
            }
        }

        /// <summary>
        /// Restores the connections from the incoming rawJsonConnections
        /// that were loaded with fromJson
        /// </summary>
        /// <param name="nodes">The nodes.</param>
        /// <param name="assign">if set to <c>true</c> [assign].</param>
        public void RestoreConnections(Dictionary<string, Node> nodes, bool assign)
        {
            if (rawNodeConnections == null || rawNodeConnections.Count == 0) return;
            SetConnections(nodes, rawNodeConnections, assign);
            rawNodeConnections = null;
        }

        /// <summary>
        /// This is used in the Graph.FromJson
        /// And thus the NodeOutput.InsertAt is not used
        /// as there should be no inputs added already
        /// </summary>
        /// <param name="nodes"></param>
        /// <param name="connections"></param>
        /// <param name="triggerAddEvent"></param>
        public void SetConnections(Dictionary<string,Node> nodes, List<NodeConnection> connections, bool assign)
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
                                Outputs[nc.outIndex].Add(inp, assign);
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

        public virtual bool IsRoot()
        {
            return Inputs == null || Inputs.Count == 0 || Inputs.Find(m => m.Reference != null) == null;
        }

        public virtual bool IsEnd()
        {
            return Outputs == null || Outputs.Count == 0 || Outputs.Find(m => m.To.Count != 0) == null;
        }
    }
}
