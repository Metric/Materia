using Materia.Rendering.Shaders;
using MLog;

namespace Materia.Rendering.Material
{
    public class PBRDepth : PBRMaterial
    {
        

        public PBRDepth() : base() {}

        protected override void LoadShader()
        {
            Shader = GLShaderCache.GetShader("pbr.glsl", "depth.glsl");

            if(Shader == null)
            {
                Log.Error("Failed to load PBR depth shader");
            }
        }
    }
}
