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
        public NodeDataType dataType; //for binary not json
        public List<NodeConnection> outputs;
        public float tileX;
        public float tileY;
        public string name;
        public GraphPixelType internalPixelType;
        public int inputCount;
        public int outputCount;
        public float viewOriginX;
        public float viewOriginY;

        //minimum 56 byte size for a node
        //not terrible
        public virtual void Write(Writer w)
        {
            //these are read before Parse()
            w.Write((ushort)dataType); //2 bytes
            w.Write((ushort)width); //2 bytes
            w.Write((ushort)height); //2 bytes
            w.Write(id); //Guid Length

            //these are handled in Parse()
            w.Write(name); //minimum 4 bytes, max 4 + string length
            w.Write(absoluteSize); //1 byte
            w.Write(tileX); //4 bytes
            w.Write(tileY); //4 bytes

            InternalGraphPixelType internalType = Enum.Parse<InternalGraphPixelType>(internalPixelType.ToString());
            w.Write((byte)internalType); //1 byte
            
            w.Write(inputCount); //4 bytes
            w.Write(outputCount); //4 bytes
            w.Write(viewOriginX); //4 bytes
            w.Write(viewOriginY); //4 bytes
            w.WriteObjectList(outputs.ToArray()); //minimum 4 bytes, max 4 + outputs.Count * 4 * 3 
        }

        public virtual void Parse(Reader r)
        {
            name = r.NextString();
            absoluteSize = r.NextBool();
            tileX = r.NextFloat();
            tileY = r.NextFloat();

            InternalGraphPixelType internalType = (InternalGraphPixelType)r.NextByte();
            internalPixelType = Enum.Parse<GraphPixelType>(internalType.ToString());
            
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
