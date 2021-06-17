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
        public List<string> nodes;
        public List<string> outputs;
        public List<string> inputs;
        public GraphPixelType defaultTextureType;

        public float shiftX;
        public float shiftY;
        public float zoom;

        public int width;
        public int height;
        public bool absoluteSize;

        public Dictionary<string, string> parameters;
        public List<string> customParameters;
        public List<string> customFunctions;

        public float? version;

        public virtual void Write(Writer w)
        {
            w.Write(version.Value);
            w.Write(name);
            w.Write((int)defaultTextureType);
            w.Write(shiftX);
            w.Write(shiftY);
            w.Write(zoom);
            w.Write(absoluteSize);

            //these are just string ids
            w.WriteStringList(outputs.ToArray());
            w.WriteStringList(inputs.ToArray());
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
            //now write parameters
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
                //v.GetBinary(w);
            }
        }

        public virtual void Parse(Reader r)
        {
            version = r.NextFloat();
            name = r.NextString();
            defaultTextureType = (GraphPixelType)r.NextInt();
            shiftX = r.NextFloat();
            shiftY = r.NextFloat();
            zoom = r.NextFloat();
            absoluteSize = r.NextBool();

            outputs = new List<string>(r.NextStringList());
            inputs = new List<string>(r.NextStringList());
        }
    }
}
