using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Materia.Nodes;
using Materia.Nodes.Atomic;
using Materia.Imaging.GLProcessing;
using Materia.Textures;
using Materia.Archive;
using Materia.Nodes.Attributes;
using Newtonsoft.Json;

namespace Materia.Layering
{
    public enum LayerPasses
    {
        Base = 1,
        Metallic = 2,
        Roughness = 3,
        Occlusion = 4,
        Normal = 5,
        Emission = 6,
        Thickness = 7,
        Height = 8
    }

    public class Layer : IDisposable
    {
        public static LayerPasses LayerMode { get; set; }

        public string Id { get; protected set; }

        public Graph Core { get; protected set; }
        public Node Mask { get; set; }

        protected Dictionary<LayerPasses, BlendType> blendModes;
        protected Dictionary<LayerPasses, float> opacityModes;

        [Editable(ParameterInputType.Dropdown, "Blending")]
        public BlendType Blending
        {
            get
            {
                return blendModes[LayerMode];
            }
            set
            {
                blendModes[LayerMode] = value;
                ParentGraph?.CombineLayers();
            }
        }

        [Editable(ParameterInputType.FloatSlider, "Opacity")]
        public float Opacity
        {
            get
            {
                return opacityModes[LayerMode];
            }
            set
            {
                opacityModes[LayerMode] = value;
                ParentGraph?.CombineLayers();
            }
        }

        protected bool visible;
        public bool Visible
        {
            get
            {
                return visible;
            }
            set
            {
                visible = value;
                ParentGraph?.CombineLayers();
            }
        }

        protected string name;
        public string Name
        {
            get
            {
                return name;
            }
            set
            {
                name = value;
                if (Core != null)
                {
                    Core.Name = name;
                }
            }
        }

        public Graph ParentGraph { get; protected set; }

        protected int width;
        public int Width
        {
            get
            {
                return width;
            }
            set
            {
                width = value;

                Core?.ResizeWith(width, height);
                ParentGraph?.CombineLayers();
            }
        }

        protected int height;
        public int Height
        {
            get
            {
                return height;
            }
            set
            {
                height = value;

                Core?.ResizeWith(width, height);
                ParentGraph?.CombineLayers();
            }
        }

        protected BlendProcessor processor;

        static Layer()
        {
            LayerMode = LayerPasses.Base;
        }

        /// <summary>
        /// Use this one only for when restoring
        /// from json
        /// </summary>
        public Layer(int w, int h, Graph parent)
        {
            opacityModes = new Dictionary<LayerPasses, float>();
            blendModes = new Dictionary<LayerPasses, BlendType>();

            width = w;
            height = h;
            Id = Guid.NewGuid().ToString();
            ParentGraph = parent;
            processor = new BlendProcessor();

            InitDefaultModes();
        }

        public Layer(string name, int w, int h, Graph parent)
        {
            opacityModes = new Dictionary<LayerPasses, float>();
            blendModes = new Dictionary<LayerPasses, BlendType>();

            Id = Guid.NewGuid().ToString();
            Name = name;
            width = w;
            height = h;
            Core = new ImageGraph(name, w, h);
            Core.ParentGraph = parent;

            ParentGraph = parent;

            Blending = BlendType.Copy;
            Opacity = 1.0f;
            processor = new BlendProcessor();

            InitDefaultModes();
        }

        public Layer(Layer other)
        {
            opacityModes = new Dictionary<LayerPasses, float>();
            blendModes = new Dictionary<LayerPasses, BlendType>();

            Id = Guid.NewGuid().ToString();
            width = other.width;
            height = other.height;
            ParentGraph = other.ParentGraph;
            FromJson(other, other.ParentGraph);
            processor = new BlendProcessor();
            name += " copy";

            foreach(LayerPasses pass in other.blendModes.Keys)
            {
                blendModes[pass] = other.blendModes[pass];
                opacityModes[pass] = other.opacityModes[pass];
            }
        }

        protected void InitDefaultModes()
        {
            for(int i = 1; i <= 8; ++i)
            {
                LayerPasses pass = (LayerPasses)i;
                opacityModes[pass] = 1.0f;
                blendModes[pass] = BlendType.Copy;
            }
        }

        public virtual void TryAndProcess()
        {
            if (Core.Modified && Visible)
            {
                Core.TryAndProcess();
                Core.Modified = false;
            }
        }

        public bool Combine(Dictionary<OutputType, Node> render)
        {
            if (render == null) return false;
            //we simply pass on the previous layer
            //to the next instead of using our own layer
            if (!Visible) return false;

            bool combinedAtLeastOne = false;

            if (Core == null)
            {
                return false;
            }

            if (CombineOutputTypes(OutputType.basecolor, opacityModes[LayerPasses.Base], blendModes[LayerPasses.Base], Core, render, Mask, processor))
            {
                combinedAtLeastOne = true;
            }
            
            if (CombineOutputTypes(OutputType.metallic, opacityModes[LayerPasses.Metallic], blendModes[LayerPasses.Metallic], Core, render, Mask, processor))
            {
                combinedAtLeastOne = true;
            }

            if (CombineOutputTypes(OutputType.roughness, opacityModes[LayerPasses.Roughness], blendModes[LayerPasses.Roughness], Core, render, Mask, processor))
            {
                combinedAtLeastOne = true;
            }

            if (CombineOutputTypes(OutputType.normal, opacityModes[LayerPasses.Normal], blendModes[LayerPasses.Normal], Core, render, Mask, processor))
            {
                combinedAtLeastOne = true;
            }

            if (CombineOutputTypes(OutputType.occlusion, opacityModes[LayerPasses.Occlusion], blendModes[LayerPasses.Occlusion], Core, render, Mask, processor))
            {
                combinedAtLeastOne = true;
            }

            if (CombineOutputTypes(OutputType.height, opacityModes[LayerPasses.Height], blendModes[LayerPasses.Height], Core, render, Mask, processor))
            {
                combinedAtLeastOne = true;
            }

            if (CombineOutputTypes(OutputType.emission, opacityModes[LayerPasses.Emission], blendModes[LayerPasses.Emission], Core, render, Mask, processor))
            {
                combinedAtLeastOne = true;
            }

            if (CombineOutputTypes(OutputType.thickness, opacityModes[LayerPasses.Thickness], blendModes[LayerPasses.Thickness], Core, render, Mask, processor))
            {
                combinedAtLeastOne = true;
            }

            return combinedAtLeastOne;
        }

        protected static bool CombineOutputTypes(OutputType type, float opacity, BlendType blending, Graph previous, Dictionary<OutputType, Node> render, Node mask, BlendProcessor processor)
        {
            if (previous == null || render == null || processor == null) return false;

            Node node = null;

            foreach (string k in previous.OutputNodes)
            {
                Node n = null;
                if (previous.NodeLookup.TryGetValue(k, out n))
                {
                    OutputNode output = n as OutputNode;
                    if (output != null && output.OutType == type)
                    {
                        node = n;
                        break;
                    }
                }
            }

            Node renderLayer = null;
            render.TryGetValue(type, out renderLayer);

            if (node != null && renderLayer != null && renderLayer.GetActiveBuffer() != null && renderLayer.GetActiveBuffer().Id != 0 && node.GetActiveBuffer() != null && node.GetActiveBuffer().Id != 0)
            {
                processor.Alpha = opacity;
                processor.AlphaMode = (int)AlphaModeType.Add;
                processor.BlendMode = (int)blending;
                processor.Luminosity = 1.0f;
                processor.Process(node.Width, node.Height, node.GetActiveBuffer(), renderLayer.GetActiveBuffer(), mask?.GetActiveBuffer(), renderLayer.GetActiveBuffer());
                processor.Complete();
                return true;
            }

            return false;
        }

        public class LayerData
        {
            public string id;
            public string core;
            public string mask;
            public bool visible;
            public string name;
            public int width;
            public int height;
            public Dictionary<LayerPasses, BlendType> blendModes;
            public Dictionary<LayerPasses, float> opacityModes;
        }

        public string GetJson()
        {
            LayerData d = new LayerData();
            d.core = Core.GetJson();
            d.mask = Mask == null ? null : Mask.Id;
            d.visible = Visible;
            d.name = Name;
            d.width = width;
            d.height = height;
            d.id = Id;
            d.opacityModes = opacityModes;
            d.blendModes = blendModes;

            return JsonConvert.SerializeObject(d);
        }

        public void FromJson(Layer l, Graph parentGraph)
        {
            LayerData d = JsonConvert.DeserializeObject<LayerData>(l.GetJson());
            if (d == null) return;

            if (d.blendModes != null)
            {
                foreach(LayerPasses pass in d.blendModes.Keys)
                {
                    blendModes[pass] = d.blendModes[pass];
                }
            }
            if (d.opacityModes != null)
            {
                foreach (LayerPasses pass in d.opacityModes.Keys)
                {
                    opacityModes[pass] = d.opacityModes[pass];
                }
            }

            Name = d.name;
            visible = d.visible;
            width = d.width;
            height = d.height;

            Graph temp = new ImageGraph(Name, width, height, parentGraph);
            temp.FromJson(d.core);
            Core = temp;

            if (!string.IsNullOrEmpty(d.mask))
            {
                Node n = null;
                if (Core.NodeLookup.TryGetValue(d.mask, out n))
                {
                    Mask = n;
                }
            }

            if (parentGraph != null)
            {
                parentGraph.LayerLookup[Id] = this;
            }
        }

        public void FromJson(string json, Graph parentGraph, MTGArchive archive = null)
        {
            LayerData d = JsonConvert.DeserializeObject<LayerData>(json);
            if (d == null) return;


            if (d.blendModes != null)
            {
                foreach (LayerPasses pass in d.blendModes.Keys)
                {
                    blendModes[pass] = d.blendModes[pass];
                }
            }
            if (d.opacityModes != null)
            {
                foreach (LayerPasses pass in d.opacityModes.Keys)
                {
                    opacityModes[pass] = d.opacityModes[pass];
                }
            }

            Name = d.name;
            visible = d.visible;
            width = d.width;
            height = d.height;
            Id = d.id;

            Graph temp = new ImageGraph(Name, width, height, parentGraph);
            temp.FromJson(d.core, archive);
            Core = temp;

            if (!string.IsNullOrEmpty(d.mask))
            {
                Node n = null;
                if (Core.NodeLookup.TryGetValue(d.mask, out n))
                {
                    Mask = n;
                }
            }

            if (parentGraph != null)
            {
                parentGraph.LayerLookup[Id] = this;
            }
        }

        public void Dispose()
        {
            if (processor != null)
            {
                processor.Release();
                processor = null;
            }

            if (Mask != null)
            {
                Mask = null;
            }

            if (Core != null)
            {
                Core.Dispose();
                Core = null;
            }
        }
    }
}
