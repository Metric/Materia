using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Materia.Material
{
    public class PBRDepth : PBRMaterial
    {
        public PBRDepth() : base() {}

        protected override void LoadShader()
        {
            Shader = GetShader("pbr.glsl", "depth.glsl");

            if(Shader == null)
            {
                Console.WriteLine("Failed to load PBR depth shader");
            }
        }
    }
}
