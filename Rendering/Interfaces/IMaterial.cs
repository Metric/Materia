using System;

namespace Materia.Rendering.Interfaces
{
    public interface IMaterial : IDisposable
    {
        string Name { get; set; }
        IGLProgram Shader { get; set; }
    }
}
