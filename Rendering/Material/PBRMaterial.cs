using Materia.Rendering.Textures;
using Materia.Rendering.Shaders;
using MLog;
using Materia.Rendering.Interfaces;
using Materia.Rendering.Mathematics;

namespace Materia.Rendering.Material
{
    public class PBRMaterial : IMaterial
    {
        public string Name { get; set; }
        public IGLProgram Shader { get; set; }

        public Vector3 Tint { get; set; } = Vector3.One;

        public IGLTexture Albedo { get; set; }
        public IGLTexture Normal { get; set; }
        public IGLTexture Metallic { get; set; }
        public IGLTexture Roughness { get; set; }
        public IGLTexture Occlusion { get; set; }
        public IGLTexture Height { get; set; }
        public IGLTexture Thickness { get; set; }
        public IGLTexture Emission { get; set; }

        /// <summary>
        /// SSS Related Properties
        /// </summary>
        public float SSSDistortion { get; set; }
        public float SSSAmbient { get; set; }
        public float SSSPower { get; set; }

        public bool UseDisplacement { get; set; }

        public float IOR { get; set; } = 0.04f;
        public float HeightScale { get; set; }

        public bool ClipHeight { get; set; }
        public float ClipHeightBias { get; set; }

        public PBRMaterial()
        {

            SSSDistortion = 0.5f;
            SSSAmbient = 0f;
            SSSPower = 1f;

            LoadShader();
        }

        protected virtual void LoadShader()
        {
            Shader = GLShaderCache.GetShader("pbr.glsl", "pbr.glsl");

            if(Shader == null)
            {
                Log.Error("Failed to load pbr shader");
            }
        }

        public virtual void Dispose()
        {
            
        }
    }
}
