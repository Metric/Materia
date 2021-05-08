using Materia.Rendering.Attributes;
using Materia.Rendering.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace MateriaCore.Settings
{
    public class Lighting : Settings
    {
        public event Action<Lighting> Update;

        protected MVector position = new MVector(0,2,2,0);
        [Editable(ParameterInputType.Float3Input, "Position", "Position")]
        public MVector Position
        {
            get => position;
            set
            {
                position = value;
                Update?.Invoke(this);
            }
        }

        protected MVector color = new MVector(1,1,1,1);
        [Editable(ParameterInputType.Color, "Color", "Light")]
        public MVector Color
        {
            get
            {
                return color;
            }
            set
            {
                color = value;
                Update?.Invoke(this);
            }
        }

        protected float power = 1;
        [Editable(ParameterInputType.FloatInput, "Power", "Light")]
        public float Power
        {
            get
            {
                return power;
            }
            set
            {
                power = value;
                Update?.Invoke(this);
            }
        }

        protected float bloomIntensity = 8;
        [Editable(ParameterInputType.FloatInput, "Bloom Intensity", "Effects")]
        public float BloomIntensity
        {
            get
            {
                return bloomIntensity;
            }
            set
            {
                bloomIntensity = value;
                Update?.Invoke(this);
            }
        }

        public Lighting()
        {
            name = "lighting";
            Reset(false);
        }

        public override void Load()
        {
            //do nothing
        }

        protected override string GetContent()
        {
            return "";
        }

        public void DefaultPosition()
        {
            position.X = 0;
            position.Y = 2;
            position.Z = 2;
            position.W = 0;

            Update?.Invoke(this);
        }

        public void Reset(bool triggerUpdate = true)
        {
            position.X = 0;
            position.Y = 2;
            position.Z = 2;
            position.W = 0;

            color.X = 1;
            color.Y = 1;
            color.Z = 1;
            color.W = 1;

            power = 1;

            bloomIntensity = 8;

            if (triggerUpdate)
            {
                Update?.Invoke(this);
            }
        }

        public void SetToOrigin()
        {
            position.X = 0;
            position.Y = 0;
            position.Z = 0;
            position.W = 0;

            Update?.Invoke(this);
        }
    }
}
