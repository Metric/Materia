﻿using Newtonsoft.Json;
using Materia.Rendering.Attributes;
using Materia.Graph;

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
                content = value;

                if (OnItemContentChanged != null)
                {
                    OnItemContentChanged.Invoke(this);
                }
            }
        }

        public class ItemNodeData : NodeData
        {
            public string content;
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
