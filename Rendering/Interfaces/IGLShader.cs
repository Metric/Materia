using System;

namespace Materia.Rendering.Interfaces
{
    public interface IGLShader : IDisposable
    {
        int Id { get; set; }
        bool Compile(out string log);
    }
}
