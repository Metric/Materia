using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Materia.Imaging;
using Materia.Nodes.Attributes;
using Materia.Imaging.GLProcessing;
using Materia.Textures;

namespace Materia.Nodes
{
    public abstract class Node : IDisposable
    {
        public delegate void UpdateEvent(Node n);
        public delegate void InputChanged(Node n, NodeInput inp);
        public delegate void OutputChanged(Node n, NodeOutput inp);
        public event UpdateEvent OnUpdate;
        public event UpdateEvent OnNameUpdate;

        public event InputChanged OnInputRemovedFromNode;
        public event InputChanged OnInputAddedToNode;

        public event OutputChanged OnOutputRemovedFromNode;
        public event OutputChanged OnOutputAddedToNode;

        [HideProperty]
        public bool CanPreview = true;

        public double ViewOriginX = 0;
        public double ViewOriginY = 0;

        protected BasicImageRenderer previewProcessor;

        protected GLTextuer2D buffer;
        public GLTextuer2D Buffer
        {
            get
            {
                return buffer;
            }
        }

        public Graph ParentGraph { get; set; }

        public string Id { get; set; }

        protected string name;
        [TextInput]
        [Section(Section = "Basic")]
        [Title(Title = "Label")]
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
            public string type;
            public List<NodeOutputConnection> outputs;
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

        [Slider(IsInt = true, Max = 4096, Min = 16, Snap = true, Ticks = new float[] { 16, 32, 64, 128, 256, 512, 1024, 2048, 4096 })]
        [Section(Section = "Standard")]
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

        [Slider(IsInt = true, Max = 4096, Min = 16, Snap = true, Ticks = new float[] { 16, 32, 64, 128, 256, 512, 1024, 2048, 4096 })]
        [Section(Section = "Standard")]
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

        protected float tileX;
        protected float tileY;

        [Title(Title = "Tile X")]
        [Section(Section = "Standard")]
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

        [Title(Title = "Tile Y")]
        [Section(Section = "Standard")]
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
        [Dropdown(null)]
        [Title(Title = "Texture Format")]
        [Section(Section = "Standard")]
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

        protected void Updated()
        {
            if(OnUpdate != null)
            {
                OnUpdate.Invoke(this);
            }
        }

        public virtual void Dispose()
        {
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
        public abstract void FromJson(Dictionary<string, Node> nodes, string data);
        public List<NodeOutputConnection> GetConnections()
        {
            List<NodeOutputConnection> outputs = new List<NodeOutputConnection>();

            int i = 0;
            foreach (NodeOutput Output in Outputs)
            {
                foreach (NodeInput n in Output.To)
                {
                    int index = n.Node.Inputs.IndexOf(n);

                    if (index > -1)
                    {
                        NodeOutputConnection nc = new NodeOutputConnection(n.Node.Id, i, index);
                        outputs.Add(nc);
                    }
                }
                i++;
            }

            return outputs;
        }

        public List<Tuple<string, List<NodeOutputConnection>>> GetParentsConnections()
        {
            List<Tuple<string, List<NodeOutputConnection>>> items = new List<Tuple<string, List<NodeOutputConnection>>>();

            foreach(NodeInput n in Inputs)
            {
                if(n.HasInput)
                {
                    var cons = n.Input.Node.GetConnections();
                    items.Add(new Tuple<string, List<NodeOutputConnection>>(n.Input.Node.Id, cons));
                }
            }

            return items;
        }

        public virtual void CopyResources(string CWD) { }

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

        protected virtual void SetBaseNodeDate(NodeData d)
        {
            width = d.width;
            height = d.height;
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

        protected void CreateBufferIfNeeded()
        {
            if (buffer == null || buffer.Id == 0)
            {
                buffer = new GLTextuer2D((OpenTK.Graphics.OpenGL.PixelInternalFormat)((int)internalPixelType));
                buffer.Bind();
                buffer.SetFilter((int)OpenTK.Graphics.OpenGL.TextureMinFilter.Linear, (int)OpenTK.Graphics.OpenGL.TextureMagFilter.Linear);
                buffer.SetWrap((int)OpenTK.Graphics.OpenGL.TextureWrapMode.Repeat);
                if(internalPixelType == GraphPixelType.Luminance16F || internalPixelType == GraphPixelType.Luminance32F)
                {
                    buffer.SetSwizzleLuminance();
                }
                GLTextuer2D.Unbind();
            }
        }

        /// <summary>
        /// Used to set individual node connections filtered by id
        /// </summary>
        /// <param name="nodes"></param>
        /// <param name="connections"></param>
        /// <param name="id"></param>
        public void SetConnection(Dictionary<string, Node> nodes, List<NodeOutputConnection> connections, string id)
        {
            if(connections != null)
            {
                foreach(NodeOutputConnection nc in connections)
                {
                    if(nc.node.Equals(id))
                    {
                        Node n = null;

                        if(nodes.TryGetValue(nc.node, out n))
                        {
                            var inp = n.Inputs[nc.index];
                            if (nc.outIndex >= 0 && nc.outIndex < Outputs.Count)
                            {
                                Outputs[nc.outIndex].Add(inp);
                            }
                        }
                    }
                }
            }
        }

        protected void SetConnections(Dictionary<string,Node> nodes, List<NodeOutputConnection> connections)
        {
            if (connections != null)
            {
                foreach (NodeOutputConnection nc in connections)
                {
                    Node n = null;
                    if (nodes.TryGetValue(nc.node, out n))
                    {
                        var inp = n.Inputs[nc.index];
                        if (nc.outIndex >= 0 && nc.outIndex < Outputs.Count)
                        {
                            Outputs[nc.outIndex].Add(inp);
                        }
                    }
                    else
                    {
                        //log to console the fact that we could not connect the node
                    }
                }
            }
        }
    }
}
