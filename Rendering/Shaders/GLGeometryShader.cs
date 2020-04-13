using Materia.Rendering.Interfaces;

namespace Materia.Rendering.Shaders
{
    public class GLGeometryShader : IGLShader
    {
        public int Id { get; set; }

        public GLGeometryShader(string data)
        {
            Id = 0;
            Id = IGL.Primary.CreateShader((int)ShaderType.GeometryShader);
            IGL.Primary.ShaderSource(Id, data);
        }

        public bool Compile(out string log)
        {
            log = null;
            IGL.Primary.CompileShader(Id);
            int success = 0;
            IGL.Primary.GetShader(Id, (int)ShaderParameter.CompileStatus, out success);
            if(success < 1)
            {
                int length = 0;
                IGL.Primary.GetShaderInfoLog(Id, 512, out length, out log);
            }
            return success == 1;
        }

        public void Dispose()
        {
            if (Id != 0)
            {
                IGL.Primary.DeleteShader(Id);
                Id = 0;
            }
        }
    }
}
