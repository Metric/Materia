using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;

namespace Materia.Material
{
    public class PBRTess : PBRMaterial
    {
        private static ILogger Log = LogManager.GetCurrentClassLogger();

        protected override void LoadShader()
        {
            Shader = GetShader("pbrtes.glsl", "tcs.glsl", "tes.glsl", "pbr.glsl");

            if (Shader == null)
            {
                Log.Error("Failed to load pbr tessallation shader");
            }

            LoadBRDF();
        }
    }
}
