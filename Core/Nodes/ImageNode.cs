using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Materia.Archive;
using Newtonsoft.Json;

namespace Materia.Nodes
{
    public abstract class ImageNode : Node
    {
        public override void FromJson(string data, MTGArchive archive = null)
        {
            FromJson(data);
        }

        public virtual void FromJson(string data)
        {
            NodeData d = JsonConvert.DeserializeObject<NodeData>(data);
            SetBaseNodeDate(d);
        }

        public override string GetJson()
        {
            NodeData d = new NodeData();
            FillBaseNodeData(d);

            return JsonConvert.SerializeObject(d);
        }

        //the only problem with this is that it is really
        //intensive time wise
        public override byte[] GetPreview(int width, int height)
        {
            if (previewProcessor != null && buffer != null)
            {
                previewProcessor.Process(width, height, buffer);
                byte[] data = previewProcessor.ReadByte(width, height);
                previewProcessor.Complete();

                return data;
            }

            return null;
        }
    }
}
