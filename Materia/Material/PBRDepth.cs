using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;

namespace Materia.Material
{
    public class PBRDepth : PBRMaterial
    {
        private static ILogger Log = LogManager.GetCurrentClassLogger();

        public PBRDepth() : base() {}

        protected override void LoadShader()
        {
            Shader = GetShader("pbr.glsl", "depth.glsl");

            if(Shader == null)
            {
                Log.Error("Failed to load PBR depth shader");
            }
        }
    }
}
