using Materia.Rendering.Attributes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MateriaCore.Settings
{
    public class Material : Settings
    {
        public event Action<Material> Update;

        protected float heightScale = 0.2f;
        [Editable(ParameterInputType.FloatSlider, "Height Scale", "General")]
        public float HeightScale
        {
            get
            {
                return heightScale;
            }
            set
            {
                heightScale = value;
                Update?.Invoke(this);
            }
        }

        protected float indexOfRefraction = 0.04f;
        [Editable(ParameterInputType.FloatInput, "Index of Refraction", "General")]
        public float IndexOfRefraction
        {
            get
            {
                return indexOfRefraction;
            }
            set
            {
                indexOfRefraction = value;
                Update?.Invoke(this);
            }
        }

        protected float heightClipBias = 0;
        [Editable(ParameterInputType.FloatSlider, "Height Clip Bias", "General")]
        public float HeightClipBias
        {
            get
            {
                return heightClipBias;
            }
            set
            {
                heightClipBias = value;
                Update?.Invoke(this);
            }
        }

        protected bool clipHeight = false;
        [Editable(ParameterInputType.Toggle, "Clip Height", "General")]
        public bool Clip
        {
            get
            {
                return clipHeight;
            }
            set
            {
                clipHeight = value;
                Update?.Invoke(this);
            }
        }

        protected bool useDisplacement = false;
        [Editable(ParameterInputType.Toggle, "Use Displacement", "General")]
        public bool Displacement
        {
            get
            {
                return useDisplacement;
            }
            set
            {
                useDisplacement = value;
                Update?.Invoke(this);
            }
        }

        protected float sssDistortion = 0.5f;
        [Editable(ParameterInputType.FloatSlider, "Distortion", "Subsurface Scattering")]
        public float SSSDistortion
        {
            get
            {
                return sssDistortion;
            }
            set
            {
                sssDistortion = value;
                Update?.Invoke(this);
            }
        }

        protected float sssAmbient = 0;
        [Editable(ParameterInputType.FloatSlider, "Ambient", "Subsurface Scattering")]
        public float SSSAmbient
        {
            get
            {
                return sssAmbient;
            }
            set
            {
                sssAmbient = value;
                Update?.Invoke(this);
            }
        }

        protected float sssPower = 1;
        [Editable(ParameterInputType.FloatInput, "Power", "Subsurface Scattering")]
        public float SSSPower
        {
            get
            {
                return sssPower;
            }
            set
            {
                sssPower = value;
                Update?.Invoke(this);
            }
        }

        public Material()
        {
            name = "material";
            Reset(false);
            Load();
        }

        public void Reset(bool triggerUpdate = true)
        {
            sssDistortion = 0.5f;
            sssAmbient = 0;
            sssPower = 1f;

            heightScale = 0.2f;
            heightClipBias = 0;
            clipHeight = false;
            indexOfRefraction = 0.04f;
            useDisplacement = false;

            if (triggerUpdate)
            {
                Update?.Invoke(this);
            }
        }

        public override void Load()
        {
            try
            {
                if (File.Exists(FilePath))
                {
                    Material mt = JsonConvert.DeserializeObject<Material>(File.ReadAllText(FilePath));

                    if (mt != null)
                    {
                        heightScale = mt.heightScale;
                        clipHeight = mt.clipHeight;
                        indexOfRefraction = mt.indexOfRefraction;
                        heightClipBias = mt.heightClipBias;
                        useDisplacement = mt.useDisplacement;
                        sssDistortion = mt.sssDistortion;
                        sssAmbient = mt.sssAmbient;
                        sssPower = mt.sssPower;
                    }
                }
            }
            catch (Exception e)
            {
                MLog.Log.Error(e);
            }
        }

        protected override string GetContent()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
