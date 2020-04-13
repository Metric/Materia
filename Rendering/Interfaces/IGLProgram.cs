using System;
using Materia.Rendering.Mathematics;

namespace Materia.Rendering.Interfaces
{
    public interface IGLProgram : IDisposable
    {
        int Id { get; }
        void AttachShader(IGLShader shader);
        bool Link(out string log);
        void Use();
        void Unbind();
        void SetUniformMatrix4(string name, ref Matrix4 m);
        void SetUniformMatrix3(string name, ref Matrix3 m);
        void UniformBlockBinding(string name, int pos);
        void SetUniform(string name, int i);
        void SetUniform(string name, bool b);
        void SetUniform(string name, uint i);
        void SetUniform(string name, float f);
        void SetUniform3(string name, ref Vector3 v);
        void SetUniform2(string name, ref Vector2 v);
        void SetUniform4F(string name, ref Vector4 v);
    }
}
