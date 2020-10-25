using Materia.Rendering.Shaders;
using MLog;

namespace Materia.Rendering.Material
{
    public class PBRTess : PBRMaterial
    {
        protected override void LoadShader()
        {
            Shader = GLShaderCache.GetShader("pbrtes.glsl", "tcs.glsl", "tes.glsl", "pbr.glsl");

            if (Shader == null)
            {
                Log.Error("Failed to load pbr tessallation shader");
            }
        }
    }
}
