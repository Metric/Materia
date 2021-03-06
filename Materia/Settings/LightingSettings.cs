﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;
using Materia.MathHelpers;
using Materia.Nodes.Attributes;
using NLog;

namespace Materia.Settings
{
    public class LightingSettings : Settings
    {
        private static ILogger Log = LogManager.GetCurrentClassLogger();

        public delegate void LightingUpdate(LightingSettings sender);
        public event LightingUpdate OnLightingUpdated;

        protected MVector position;
        [Editable(Nodes.ParameterInputType.Float3Input, "Position", "Position")]
        public MVector Position
        {
            get
            {
                return position;
            }
            set
            {
                position = value;

                if (OnLightingUpdated != null)
                {
                    OnLightingUpdated.Invoke(this);
                }
            }
        }

        protected MVector color;
        [Editable(Nodes.ParameterInputType.Color, "Color", "Light")]
        public MVector Color
        {
            get
            {
                return color;
            }
            set
            {
                color = value;
                if (OnLightingUpdated != null)
                {
                    OnLightingUpdated.Invoke(this);
                }
            }
        }

        protected float power;
        [Editable(Nodes.ParameterInputType.FloatInput, "Power", "Light")]
        public float Power
        {
            get
            {
                return power;
            }
            set
            {
                power = value;

                if (OnLightingUpdated != null)
                {
                    OnLightingUpdated.Invoke(this);
                }
            }
        }

        protected float bloomIntensity;
        [Editable(Nodes.ParameterInputType.FloatInput, "Bloom Intensity", "Effects")]
        public float BloomIntensity
        {
            get
            {
                return bloomIntensity;
            }
            set
            {
                bloomIntensity = value;

                if (OnLightingUpdated != null)
                {
                    OnLightingUpdated.Invoke(this);
                }
            }
        }

        public LightingSettings()
        {
            name = "lighting";
            position = new MVector(0, 2, 2, 0);
            color = new MVector(1, 1, 1, 1);
            power = 1;
            bloomIntensity = 8;
        }

        public override void Load()
        {
            //do nothing
        }

        public void DefaultPosition()
        {
            position.X = 0;
            position.Y = 2;
            position.Z = 2;
            position.W = 0;

            if (OnLightingUpdated != null)
            {
                OnLightingUpdated.Invoke(this);
            }
        }

        public void Reset()
        {
            position.X = 0;
            position.Y = 2;
            position.Z = 2;
            position.W = 0;

            color = new MVector(1, 1, 1, 1);
            power = 1;

            if (OnLightingUpdated != null)
            {
                OnLightingUpdated.Invoke(this);
            }
        }

        public void SetToOrigin()
        {
            position.X = 0;
            position.Y = 0;
            position.Z = 0;
            position.W = 0;

            if (OnLightingUpdated != null)
            {
                OnLightingUpdated.Invoke(this);
            }
        }

        protected override string GetContent()
        {
            return "";
        }
    }
}
