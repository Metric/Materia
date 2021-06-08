using Materia.Rendering.Shaders;
using MLog;

namespace Materia.Rendering.Material
{
    public class PBRLight : PBRMaterial
    {
        public PBRLight() : base() { }

        protected override void LoadShader()
        {
            Shader = GLShaderCache.GetShader("pbrbasic.glsl", "light.glsl");

            if (Shader == null)
            {
                Log.Error("Failed to load PBR light shader");
            }
        }
    }
}
