using Materia.Rendering.Textures;
using Materia.Rendering.Shaders;
using MLog;
using Materia.Rendering.Interfaces;

namespace Materia.Rendering.Material
{
    public class PBRMaterial : IMaterial
    {
        

        public string Name { get; set; }
        public IGLProgram Shader { get; set; }

        public GLTexture2D Albedo { get; set; }
        public GLTexture2D Normal { get; set; }
        public GLTexture2D Metallic { get; set; }
        public GLTexture2D Roughness { get; set; }
        public GLTexture2D Occlusion { get; set; }
        public GLTexture2D Height { get; set; }
        public GLTexture2D Thickness { get; set; }
        public GLTexture2D Emission { get; set; }

        /// <summary>
        /// SSS Related Properties
        /// </summary>
        public float SSSDistortion { get; set; }
        public float SSSAmbient { get; set; }
        public float SSSPower { get; set; }

        public bool UseDisplacement { get; set; }

        public float IOR { get; set; }
        public float HeightScale { get; set; }

        public bool ClipHeight { get; set; }
        public float ClipHeightBias { get; set; }

        public PBRMaterial()
        {

            SSSDistortion = 0.5f;
            SSSAmbient = 0f;
            SSSPower = 1f;

            BRDF.Create();
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
            if (Albedo != null)
            {
                Albedo.Dispose();
                Albedo = null;
            }
            if (Height != null)
            {
                Height.Dispose();
                Height = null;
            }
            if (Normal != null)
            {
                Normal.Dispose();
                Normal = null;
            }
            if (Metallic != null)
            {
                Metallic.Dispose();
                Metallic = null;
            }
            if (Roughness != null)
            {
                Roughness.Dispose();
                Roughness = null;
            }
            if (Occlusion != null)
            {
                Occlusion.Dispose();
                Occlusion = null;
            }
            if(Thickness != null)
            {
                Thickness.Dispose();
                Thickness = null;
            }
            if(Emission != null)
            {
                Emission.Dispose();
                Emission = null;
            }
        }
    }
}
