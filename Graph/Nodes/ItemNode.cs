using Newtonsoft.Json;
using Materia.Rendering.Attributes;
using Materia.Graph;
using Materia.Graph.IO;

namespace Materia.Nodes
{
    public class ItemNode : Node
    {
        public delegate void ItemContentChange(ItemNode n);
        public event ItemContentChange OnItemContentChanged;

        public new string Name
        {
            get
            {
                return name;
            }
            set
            {
                name = value;
            }
        }

        protected string content;
        [Editable(ParameterInputType.Text, "Content")]
        public string Content
        {
            get
            {
                return content;
            }
            set
            {
                if (content != value)
                {
                    content = value;
                    OnItemContentChanged?.Invoke(this);
                }
            }
        }

        public class ItemNodeData : NodeData
        {
            public string content;
            public override void Write(Writer w)
            {
                base.Write(w);
                w.Write(content);
            }

            public override void Parse(Reader r)
            {
                base.Parse(r);
                content = r.NextString();
            }
        }

        /// <summary>
        /// Use this to set content
        /// and not trigger the OnItemContentChanged event
        /// </summary>
        /// <param name="d"></param>
        public void SetContent(string d)
        {
            content = d;
        }

        public override void GetBinary(Writer w)
        {
            ItemNodeData d = new ItemNodeData();
            FillBaseNodeData(d);
            d.content = content;
            d.Write(w);
        }

        public override void FromBinary(Reader r, Archive archive = null)
        {
            FromBinary(r);
        }

        public virtual void FromBinary(Reader r)
        {
            ItemNodeData d = new ItemNodeData();
            d.Parse(r);
            SetBaseNodeDate(d);
            content = d.content;
        }

        public override void FromJson(string data, Archive archive = null)
        {
            FromJson(data);
        }

        public virtual void FromJson(string data)
        {
            ItemNodeData d = JsonConvert.DeserializeObject<ItemNodeData>(data);
            SetBaseNodeDate(d);
            content = d.content;
        }

        public override string GetJson()
        {
            ItemNodeData d = new ItemNodeData();
            FillBaseNodeData(d);
            d.content = content;
            return JsonConvert.SerializeObject(d);
        }
    }
}
