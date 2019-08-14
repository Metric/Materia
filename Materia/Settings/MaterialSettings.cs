using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;
using Materia.Nodes.Attributes;
using NLog;

namespace Materia.Settings
{
    public class MaterialSettings : Settings
    {
        private static ILogger Log = LogManager.GetCurrentClassLogger();
        
        public delegate void MaterialUpdate(MaterialSettings sender);
        public event MaterialUpdate OnMaterialUpdated;

        protected float heightScale;
        [Editable(Nodes.ParameterInputType.FloatSlider, "Height Scale", "General")]
        public float HeightScale
        {
            get
            {
                return heightScale;
            }
            set
            {
                heightScale = value;
                if (OnMaterialUpdated != null)
                {
                    OnMaterialUpdated.Invoke(this);
                }
            }
        }

        protected float indexOfRefraction;
        [Editable(Nodes.ParameterInputType.FloatInput, "Index of Refraction", "General")]
        public float IndexOfRefraction
        {
            get
            {
                return indexOfRefraction;
            }
            set
            {
                indexOfRefraction = value;
                if (OnMaterialUpdated != null)
                {
                    OnMaterialUpdated.Invoke(this);
                }
            }
        }

        protected float heightClipBias;
        [Editable(Nodes.ParameterInputType.FloatSlider, "Height Clip Bias", "General")]
        public float HeightClipBias
        {
            get
            {
                return heightClipBias;
            }
            set
            {
                heightClipBias = value;
                if (OnMaterialUpdated != null)
                {
                    OnMaterialUpdated.Invoke(this);
                }
            }
        }

        protected bool clipHeight;
        [Editable(Nodes.ParameterInputType.Toggle, "Clip Height", "General")]
        public bool Clip
        {
            get
            {
                return clipHeight;
            }
            set
            {
                clipHeight = value;
                if (OnMaterialUpdated != null)
                {
                    OnMaterialUpdated.Invoke(this);
                }
            }
        }

        protected bool useDisplacement;
        [Editable(Nodes.ParameterInputType.Toggle, "Use Displacement", "General")]
        public bool Displacement
        {
            get
            {
                return useDisplacement;
            }
            set
            {
                useDisplacement = value;
                if(OnMaterialUpdated != null)
                {
                    OnMaterialUpdated.Invoke(this);
                }
            }
        }

        protected float sssDistortion;
        [Editable(Nodes.ParameterInputType.FloatSlider, "Distortion", "Subsurface Scattering")]
        public float SSSDistortion
        {
            get
            {
                return sssDistortion;
            }
            set
            {
                sssDistortion = value;
                if (OnMaterialUpdated != null)
                {
                    OnMaterialUpdated.Invoke(this);
                }
            }
        }

        protected float sssAmbient;
        [Editable(Nodes.ParameterInputType.FloatSlider, "Ambient", "Subsurface Scattering")]
        public float SSSAmbient
        {
            get
            {
                return sssAmbient;
            }
            set
            {
                sssAmbient = value;
                if (OnMaterialUpdated != null)
                {
                    OnMaterialUpdated.Invoke(this);
                }
            }
        }

        protected float sssPower;
        [Editable(Nodes.ParameterInputType.FloatInput, "Power", "Subsurface Scattering")]
        public float SSSPower
        {
            get
            {
                return sssPower;
            }
            set
            {
                sssPower = value;
                if (OnMaterialUpdated != null)
                {
                    OnMaterialUpdated.Invoke(this);
                }
            }
        }

        public MaterialSettings()
        {
            name = "material";

            sssDistortion = 0.5f;
            sssAmbient = 0;
            sssPower = 1f;

            heightScale = 0.2f;
            heightClipBias = 0;
            clipHeight = false;
            useDisplacement = false;
            indexOfRefraction = 0.04f;
        }

        public void ResetToDefault()
        {
            sssDistortion = 0.5f;
            sssAmbient = 0;
            sssPower = 1f;

            heightScale = 0.2f;
            heightClipBias = 0;
            clipHeight = false;
            indexOfRefraction = 0.04f;
            useDisplacement = false;

            if(OnMaterialUpdated != null)
            {
                OnMaterialUpdated.Invoke(this);
            }
        }

        public override void Load()
        {
            try
            {
                if (File.Exists(FilePath))
                {
                    MaterialSettings mt = JsonConvert.DeserializeObject<MaterialSettings>(File.ReadAllText(FilePath));

                    if(mt != null)
                    {
                        heightScale = mt.heightScale;
                        clipHeight = mt.clipHeight;
                        indexOfRefraction = mt.indexOfRefraction;
                        heightClipBias = mt.heightClipBias;
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }

        protected override string GetContent()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
