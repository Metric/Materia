using Materia.Graph.IO;
using Materia.Nodes;
using System;
using System.Collections.Generic;
using System.Text;

//todo: move to Materia.Graph.IO namespace
namespace Materia.Graph
{
    public class GraphData
    {
        public string name;
        public string id;

        public List<string> nodes;
        public GraphPixelType defaultTextureType;

        public float shiftX;
        public float shiftY;
        public float zoom;

        public ushort width;
        public ushort height;
        public bool absoluteSize;

        public Dictionary<string, string> parameters;
        public List<string> customParameters;
        public List<string> customFunctions;

        public float? version;

        public virtual void Write(Writer w)
        {
            w.Write(version.Value);
            w.Write(name);
            w.Write(id);

            InternalGraphPixelType internalType = Enum.Parse<InternalGraphPixelType>(defaultTextureType.ToString());
            w.Write((byte)internalType); //1 byte
            
            w.Write(shiftX);
            w.Write(shiftY);

            w.Write(zoom);
            w.Write(absoluteSize);

            w.Write(width);
            w.Write(height);

            //these are just string ids
            //why are we storing these?
            //we can rebuild on load
            //w.WriteStringList(outputs.ToArray());
            //w.WriteStringList(inputs.ToArray());
        }

        public virtual void WriteNodes(Writer w, List<Node> nodes)
        {
            w.Write(nodes.Count);

            for (int i = 0; i < nodes.Count; ++i)
            {
                var v = nodes[i];
                v.GetBinary(w);
            }
        }

        public virtual void WriteParameters(Writer w, Dictionary<string, ParameterValue> parameters)
        {
            w.Write(parameters.Count);
      
            foreach (string k in parameters.Keys)
            {
                w.Write(k);
                var v = parameters[k];
                v.GetBinary(w);
            }
        }

        public virtual void WriteCustomParameters(Writer w, List<ParameterValue> customParameters)
        {
            w.Write(customParameters.Count);

            for (int i = 0; i < customParameters.Count; ++i)
            {
                var v = customParameters[i];
                v.GetBinary(w);
            }
        }

        public virtual void WriteCustomFunctions(Writer w, List<Function> customFunctions)
        {
            w.Write(customFunctions.Count);

            for (int i = 0; i < customFunctions.Count; ++i)
            {
                var v = customFunctions[i];
                v.GetBinary(w);
            }
        }

        public virtual void Parse(Reader r)
        {
            version = r.NextFloat();
            name = r.NextString();
            id = r.NextString();

            InternalGraphPixelType internalType = (InternalGraphPixelType)r.NextByte();
            defaultTextureType = Enum.Parse<GraphPixelType>(internalType.ToString());
            
            shiftX = r.NextFloat();
            shiftY = r.NextFloat();

            zoom = r.NextFloat();
            absoluteSize = r.NextBool();

            width = r.NextUShort();
            height = r.NextUShort();

            //outputs = new List<string>(r.NextStringList());
            //inputs = new List<string>(r.NextStringList());
        }
    }
}
