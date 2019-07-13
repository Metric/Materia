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
        [Section(Section = "General")]
        [Title(Title = "Height Scale")]
        [Slider(IsInt = false, Max = 1.0f, Min = 0f)]
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
        [Section(Section = "General")]
        [Title(Title = "Index of Refraction")]
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
        [Section(Section = "General")]
        [Title(Title = "Height Clip Bias")]
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
        [Section(Section = "General")]
        [Title(Title = "Clip Height")]
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
        [Section(Section = "General")]
        [Title(Title = "Use Displacement")]
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
        [Section(Section = "Subsurface Scattering")]
        [Title(Title = "Distortion")]
        [Slider(IsInt = false, Max = 1.0f, Min = 0.0f)]
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
        [Section(Section = "Subsurface Scattering")]
        [Title(Title = "Ambient")]
        [Slider(IsInt = false, Max = 1.0f, Min = 0.0f)]
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
        [Section(Section = "Subsurface Scattering")]
        [Title(Title = "Power")]
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
