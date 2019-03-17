using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL;

namespace Materia.Shaders
{
    public class GLFragmentShader : GLShader
    {
        public int Id { get; set; }

        public GLFragmentShader(string data)
        {
            Id = 0;
            Id = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(Id, data);
        }

        public bool Compile(out string log)
        {
            log = null;
            GL.CompileShader(Id);
            int success = 0;
            GL.GetShader(Id, ShaderParameter.CompileStatus, out success);
            if(success < 1)
            {
                int length = 0;
                GL.GetShaderInfoLog(Id, 512, out length, out log);
            }
            return success == 1;
        }

        public void Release()
        {
            if (Id != 0)
            {
                GL.DeleteShader(Id);
                Id = 0;
            }
        }
    }
}
