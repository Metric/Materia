﻿using System;
using System.Linq;
using Materia.Rendering.Attributes;
using Materia.Rendering.Mathematics;
using Newtonsoft.Json;
using Materia.Nodes;
using Materia.Rendering.Extensions;

namespace Materia.Graph
{
    public class ParameterValue
    {
        public const string CODE_PREFIX = "p_";
        public const string CUSTOM_CODE_PREFIX = "c_";

        public delegate void GraphParameterUpdate(ParameterValue param);
        public event GraphParameterUpdate OnGraphParameterUpdate;
        public event GraphParameterUpdate OnGraphParameterTypeChanged;

        [JsonIgnore]
        public Graph ParentGraph
        {
            get; set;
        }

        [Editable(ParameterInputType.Text, "Name")]
        public string Name { get; set; }

        [Editable(ParameterInputType.Text, "Section")]
        public string Section { get; set; }

        public string CodeName 
        { 
            get
            {
                return CODE_PREFIX + Name.Replace(" ", "").Replace("-", "");
            } 
        }

        public string CustomCodeName
        {
            get
            {
                return CUSTOM_CODE_PREFIX + Name.Replace(" ", "").Replace("-", "");
            }
        }

        public string Id { get; set; }

        public string Key { get; set; }

        protected NodeType type;
        [Dropdown(null, false, "Bool", "Float", "Float2", "Float3", "Float4")]
        [Editable(ParameterInputType.Dropdown, "Type")]
        public NodeType Type
        {
            get
            {
                return type;
            }
            set
            {
                type = value;
                ValidateValue();
                if (OnGraphParameterTypeChanged != null)
                {
                    OnGraphParameterTypeChanged.Invoke(this);
                }
            }
        }

        protected ParameterInputType inputType;

        [Dropdown(null, false, "FloatSlider", "FloatInput", "IntSlider", "IntInput", "Color", "Toggle")]
        [Editable(ParameterInputType.Dropdown, "Input Type")]
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

        [Editable(ParameterInputType.Text, "Description")]
        public string Description { get; set; }

        protected float min;
        [Editable(ParameterInputType.FloatInput, "Min")]
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
        [Editable(ParameterInputType.FloatInput, "Max")]
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

        public float FloatValue
        {
            get
            {
                float m = v.ToFloat();
                if (m < Min) m = Min;
                if (m > Max) m = Max;
                return m;
            }
        }

        public int IntValue
        {
            get
            {
                return v.ToInt();
            }
        }

        public bool BoolValue
        {
            get
            {
                return v.ToBool();
            }
        }

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

        public Matrix2 Matrix2Value
        {
            get
            {
                if(v is Matrix2)
                {
                    return (Matrix2)v;
                }

                return Matrix2.Identity;
            }
        }

        public Matrix3 Matrix3Value
        {
            get
            {
                if(v is Matrix3)
                {
                    return (Matrix3)v;
                }

                return Matrix3.Identity;
            }
        }

        public Matrix4 Matrix4Value
        {
            get
            {
                if(v is Matrix4)
                {
                    return (Matrix4)v;
                }

                return Matrix4.Identity;
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

        public ParameterValue(string name, object value,
            string desc = "", NodeType type = NodeType.Float, float min = 0, 
            float max = 1, ParameterInputType itype = ParameterInputType.FloatInput, 
            string id = null)
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

        private T DeserializeValueArray<T>(object v)
        {
            if(v is Newtonsoft.Json.Linq.JArray)
            {
                var jp = v as Newtonsoft.Json.Linq.JArray;

                try
                {
                    string sjp = jp.ToString();
                    T m = JsonConvert.DeserializeObject<T>(sjp);
                    return m;
                }
                catch
                {
    
                }
            }

            return default(T);
        }

        private void ValidateValue()
        {
            if (v is Function)
            {
                type = (v as Function).ExpectedOutput;
                return;
            }

            if(v != null && v.GetType().IsEnum)
            {
                try
                {
                    v = Convert.ToSingle(v);
                }
                catch
                {
                    v = 0;
                }
            }
            else if (!v.IsNumber() && type == NodeType.Float)
            {
                try
                {
                    v = Convert.ToSingle(v);
                }
                catch
                {
                    v = 0;
                }
            }
            else if (!(v is bool) && type == NodeType.Bool)
            {
                try
                {
                    v = Convert.ToBoolean(v);
                }
                catch
                {
                    v = false;
                }
            }
            else if(!(v is Matrix4) && type == NodeType.Matrix)
            {
                v = Matrix4.Identity;
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
            return v is Function;
        }

        public virtual void SetJson(GraphParameterValueData d, Node n)
        {
            Name = d.name;

            type = (NodeType)d.type;
            inputType = (ParameterInputType)d.inputType;
            Id = d.id;
            min = d.min;
            max = d.max;

            if (d.isFunction)
            {
                Function t = new Function("temp");
                t.AssignParentNode(n);
                t.FromJson((string)d.value);
                t.ExpectedOutput = (NodeType)d.type;
                t.SetConnections();
                v = t;
            }
            else
            {
                v = d.value;
            }

            //then it is a matrix
            if(v is Newtonsoft.Json.Linq.JArray && type == NodeType.Matrix)
            {
                float[] m = DeserializeValueArray<float[]>(v);
                //if this fails then the ValidateValue
                //will simply fill it in with the corresponding
                //matrix identity
                if(m != null)
                {
                    //4x4 matrix
                    if(m.Length == 16)
                    {
                        Matrix4 m4 = new Matrix4();
                        m4.FromArray(m);
                        v = m4;
                    }
                }
            }
            //handle this in case parser actually returns it as
            //a float[] instead of a JArray
            //not sure which it will return at the moment
            //when it encodes it from the value field
            //which is classified as a generic object
            else if(v is float[] && type == NodeType.Matrix)
            {
                float[] m = (float[])v;

                if(m != null && m.Length == 16)
                {
                    Matrix4 m4 = new Matrix4();
                    m4.FromArray(m);
                    v = m4;
                }
            }

            Description = d.description;
            Section = d.section;

            //whoops forgot to check for this!
            //otherwise d.section from a file
            //without d.section is null!
            if(string.IsNullOrEmpty(Section))
            {
                Section = "Default";
            }

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
                Function g = Value as Function;

                d.value = g.GetJson();
            }
            else
            {
                if (v is Matrix4)
                {
                    d.value = ((Matrix4)v).ToArray();
                }
                else
                {
                    d.value = v;
                }
            }

            return JsonConvert.SerializeObject(d);
        }

        public static ParameterValue FromJson(string data, Node n)
        {
            GraphParameterValueData d = JsonConvert.DeserializeObject<GraphParameterValueData>(data);
            //create a dummy graph parameter
            var g = new ParameterValue(d.name, 0);
            //load real data
            g.SetJson(d, n);
            return g;
        }
    }
}