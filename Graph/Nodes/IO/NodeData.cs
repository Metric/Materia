using Materia.Graph;
using Materia.Graph.IO;
using System;
using System.Collections.Generic;
using System.Text;

//we use the base Materia.Nodes namespace to make life easier on us
//for the moment
//todo: actually use the name space Materia.Nodes.IO
namespace Materia.Nodes
{
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
        public float viewOriginX;
        public float viewOriginY;

        public virtual void Write(Writer w)
        {
            //these are read before Parse()
            w.Write(type);
            w.Write(width);
            w.Write(height);
            w.Write(id);

            //these are handled in Parse()
            w.Write(name);
            w.Write(absoluteSize);
            w.Write(tileX);
            w.Write(tileY);
            w.Write((int)internalPixelType);
            w.Write(inputCount);
            w.Write(outputCount);
            w.Write(viewOriginX);
            w.Write(viewOriginY);
            w.WriteObjectList(outputs.ToArray());
        }

        public virtual void Parse(Reader r)
        {
            name = r.NextString();
            absoluteSize = r.NextBool();
            tileX = r.NextFloat();
            tileY = r.NextFloat();
            internalPixelType = (GraphPixelType)r.NextInt();
            inputCount = r.NextInt();
            outputCount = r.NextInt();
            viewOriginX = r.NextFloat();
            viewOriginY = r.NextFloat();
            outputs = new List<NodeConnection>(r.NextList<NodeConnection>());
        }
    }

    public class VarData : NodeData
    {
        public string varName;

        public override void Write(Writer w)
        {
            base.Write(w);
            w.Write(varName);
        }

        public override void Parse(Reader r)
        {
            base.Parse(r);
            varName = r.NextString();
        }
    }
}
