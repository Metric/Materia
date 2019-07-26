using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Materia.Nodes.Attributes;
using Materia.MathHelpers;
using Newtonsoft.Json;

namespace Materia.Nodes
{
    public enum ParameterInputType
    {
        Slider,
        Input
    }

    public class GraphParameterValue
    {
        public delegate void GraphParameterUpdate(GraphParameterValue param);
        public event GraphParameterUpdate OnGraphParameterUpdate;
        public event GraphParameterUpdate OnGraphParameterTypeChanged;

        [JsonIgnore]
        public Graph ParentGraph
        {
            get; set;
        }

        [TextInput]
        public string Name { get; set; }

        [TextInput]
        public string Section { get; set; }

        public string Id { get; set; }

        protected NodeType type;
        [Dropdown(null, "Bool", "Float", "Float2", "Float3", "Float4")]
        public NodeType Type
        {
            get
            {
                return type;
            }
            set
            {
                type = value;
                if (!(v is float) && type == NodeType.Float)
                {
                    v = 0;
                }
                else if (!(v is bool) && type == NodeType.Bool)
                {
                    v = false;
                }
                else if (!(v is MVector) && (type == NodeType.Float2 || type == NodeType.Float3 || type == NodeType.Float4 || type == NodeType.Gray || type == NodeType.Color))
                {
                    v = new MVector();
                }

                if (OnGraphParameterTypeChanged != null)
                {
                    OnGraphParameterTypeChanged.Invoke(this);
                }
            }
        }

        protected ParameterInputType inputType;
        public ParameterInputType InputType
        {
            get
            {
                return inputType;
            }
            set
            {
                inputType = value;

                if (OnGraphParameterTypeChanged != null)
                {
                    OnGraphParameterTypeChanged.Invoke(this);
                }
            }
        }

        [TextInput]
        public string Description { get; set; }

        protected float min;
        public float Min
        {
            get
            {
                return min;
            }
            set
            {
                min = value;
                if (OnGraphParameterTypeChanged != null)
                {
                    OnGraphParameterTypeChanged.Invoke(this);
                }
            }
        }

        protected float max;
        public float Max
        {
            get
            {
                return max;
            }
            set
            {
                max = value;
                if (OnGraphParameterTypeChanged != null)
                {
                    OnGraphParameterTypeChanged.Invoke(this);
                }
            }
        }

        protected object v;
        [HideProperty]
        public object Value
        {
            get
            {
                return v;
            }
            set
            {
                v = value;

                ValidateValue();

                if (OnGraphParameterUpdate != null)
                {
                    OnGraphParameterUpdate.Invoke(this);
                }
            }
        }

        [HideProperty]
        public float FloatValue
        {
            get
            {
                float m = Convert.ToSingle(v);
                if (m < Min) m = Min;
                if (m > Max) m = Max;
                return m;
            }
        }

        [HideProperty]
        public int IntValue
        {
            get
            {
                return Convert.ToInt32(v);
            }
        }

        [HideProperty]
        public bool BoolValue
        {
            get
            {
                return Convert.ToBoolean(v);
            }
        }

        [HideProperty]
        public MVector VectorValue
        {
            get
            {
                if (v is MVector)
                {
                    MVector m = (MVector)v;

                    if (m.X < Min) m.X = Min;
                    if (m.Y < Min) m.Y = Min;
                    if (m.Z < Min) m.Z = Min;
                    if (m.W < Min) m.W = Min;

                    if (m.X > Max) m.X = Max;
                    if (m.Y > Max) m.Y = Max;
                    if (m.Z > Max) m.Z = Max;
                    if (m.W > Max) m.W = Max;

                    return m;
                }
                else
                {
                    return new MVector();
                }
            }
        }

        public class GraphParameterValueData
        {
            public string name;
            public object value;
            public bool isFunction;
            public string description;
            public int type;
            public float min;
            public float max;
            public int inputType;
            public string id;
            public string section;
        }

        public GraphParameterValue(string name, object value,
            string desc = "", NodeType type = NodeType.Float, float min = 0, float max = 1, ParameterInputType itype = ParameterInputType.Input, string id = null)
        {
            Name = name;
            inputType = itype;
            v = value;
            Id = id;
            Description = desc;
            Section = "Default";
            this.type = type;

            if (string.IsNullOrEmpty(Id))
            {
                Id = Guid.NewGuid().ToString();
            }

            ValidateValue();

            this.min = min;
            this.max = max;
        }

        private void ValidateValue()
        {
            if (v is FunctionGraph)
            {
                type = (v as FunctionGraph).ExpectedOutput;
                return;
            }

            if(v != null && v.GetType().IsEnum)
            {
                v = (float)(int)v;
            }
            else if (!(v is float) && !(v is double) && !(v is int) && !(v is long) && type == NodeType.Float)
            {
                v = 0;
            }
            else if (!(v is bool) && type == NodeType.Bool)
            {
                v = false;
            }
            else if (!(v is MVector) && (type == NodeType.Float2 || type == NodeType.Float3 || type == NodeType.Float4 || type == NodeType.Gray || type == NodeType.Color))
            {
                //for right now the only possible JObject is a MVector
                if (v is Newtonsoft.Json.Linq.JObject)
                {
                    Newtonsoft.Json.Linq.JObject jp = v as Newtonsoft.Json.Linq.JObject;

                    try
                    {
                        string sjp = jp.ToString();
                        v = JsonConvert.DeserializeObject<MVector>(sjp);
                    }
                    catch
                    {
                        v = null;
                    }

                    if (v == null || v is Newtonsoft.Json.Linq.JObject)
                    {
                        v = new MVector();
                    }
                }
                else
                {
                    v = new MVector();
                }
            }
        }

        public void AssignValue(object val)
        {
            v = val;
            ValidateValue();
        }

        public bool IsFunction()
        {
            if (v == null) return false;
            return v is FunctionGraph;
        }

        public virtual void SetJson(GraphParameterValueData d, Node n)
        {
            Name = d.name;
            if(d.isFunction)
            {
                FunctionGraph t = new FunctionGraph("temp");
                t.FromJson((string)d.value);
                t.ExpectedOutput = (NodeType)d.type;
                t.ParentNode = n;
                t.SetConnections();
                v = t;
            }
            else
            {
                v = d.value;
            }

            Description = d.description;
            Section = d.section;
            type = (NodeType)d.type;
            inputType = (ParameterInputType)d.inputType;
            Id = d.id;
            min = d.min;
            max = d.max;

            ValidateValue();
        }

        public string GetJson()
        {
            GraphParameterValueData d = new GraphParameterValueData();
            d.name = Name;
            d.isFunction = IsFunction();
            d.description = Description;
            d.type = (int)Type;
            d.min = Min;
            d.max = Max;
            d.inputType = (int)inputType;
            d.id = Id;
            d.section = Section;

            if (d.isFunction)
            {
                FunctionGraph g = Value as FunctionGraph;

                d.value = g.GetJson();
            }
            else
            {
                d.value = Value;
            }

            return JsonConvert.SerializeObject(d);
        }

        public static GraphParameterValue FromJson(string data, Node n)
        {
            GraphParameterValueData d = JsonConvert.DeserializeObject<GraphParameterValueData>(data);
            var g = new GraphParameterValue(d.name, d.value);
            g.SetJson(d, n);
            return g;
        }
    }
}
