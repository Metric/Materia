using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Materia.Shaders
{
    public interface GLShader
    {
        int Id { get; set; }
        bool Compile(out string log);
        void Release();
    }
}
