using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;

namespace Materia.Material
{
    public class PBRLight : PBRMaterial
    {
        private static ILogger Log = LogManager.GetCurrentClassLogger();

        public PBRLight() : base() { }

        protected override void LoadShader()
        {
            Shader = GetShader("pbr.glsl", "uniformcolor.glsl");

            if (Shader == null)
            {
                Log.Error("Failed to load PBR depth shader");
            }
        }
    }
}
