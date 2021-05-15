using Materia.Rendering.Shaders;
using MLog;
using System;
using System.Collections.Generic;
using System.Text;

namespace Materia.Rendering.Material
{
    public class PBRSkybox : PBRMaterial
    {
        public PBRSkybox() : base() { }

        protected override void LoadShader()
        {
            Shader = GLShaderCache.GetShader("skybox.glsl", "skybox.glsl");

            if (Shader == null)
            {
                Log.Error("Failed to load PBR skybox shader");
            }
        }
    }
}
