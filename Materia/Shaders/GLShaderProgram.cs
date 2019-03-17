using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL;
using OpenTK;

namespace Materia.Shaders
{
    public class GLShaderProgram
    {
        public int Id { get; protected set; }

        List<GLShader> shaders;
        bool autoReleaseShaders;

        public GLShaderProgram(bool autoReleaseShaders = true)
        {
            this.autoReleaseShaders = autoReleaseShaders;
            shaders = new List<GLShader>();
            Id = GL.CreateProgram();
        }

        public void AttachShader(GLShader shader)
        {
            shaders.Add(shader);
            GL.AttachShader(Id, shader.Id);
        }

        public bool Link(out string log)
        {
            log = null;
            int success = 0;
            GL.LinkProgram(Id);

            GL.GetProgram(Id, GetProgramParameterName.LinkStatus, out success);

            if(success < 1)
            {
                int length = 0;
                GL.GetProgramInfoLog(Id, 512, out length, out log);
            }

            if(success == 1 && autoReleaseShaders)
            {
                //automatically release the shaders as we have succesfully
                //built the shader program for use now
                foreach(GLShader s in shaders)
                {
                    GL.DetachShader(Id, s.Id);
                    s.Release();
                }

                shaders.Clear();
            }

            return success == 1;
        }

        public void Use()
        {
            GL.UseProgram(Id);
        }

        public void SetUniformMatrix4(string name, ref Matrix4 m)
        {
            int location = GL.GetUniformLocation(Id, name);
            GL.UniformMatrix4(location, false, ref m);
        }

        public void SetUniformMatrix3(string name, ref Matrix3 m)
        {
            int location = GL.GetUniformLocation(Id, name);
            GL.UniformMatrix3(location, false, ref m);
        }

        public void UniformBlockBinding(string name, int pos)
        {
            int index = GL.GetUniformBlockIndex(Id, name);
            GL.UniformBlockBinding(Id, index, pos);
        }

        public void SetUniform(string name, int i)
        {
            int location = GL.GetUniformLocation(Id, name);
            GL.Uniform1(location, i);
        }

        public void SetUniform(string name, bool b)
        {
            int location = GL.GetUniformLocation(Id, name);
            GL.Uniform1(location, b ? (int)1 : (int)0);
        }

        public void SetUniform(string name, uint i)
        {
            int location = GL.GetUniformLocation(Id, name);
            GL.Uniform1(location, i);
        }

        public void SetUniform(string name, float f)
        {
            int location = GL.GetUniformLocation(Id, name);
            GL.Uniform1(location, f);
        }

        public void SetUniform3(string name, ref Vector3 v)
        {
            int location = GL.GetUniformLocation(Id, name);
            GL.Uniform3(location, ref v);
        }

        public void SetUniform2(string name, ref Vector2 v)
        {
            int location = GL.GetUniformLocation(Id, name);
            GL.Uniform2(location, ref v);
        }

        public void SetUniform4F(string name, ref Vector4 v)
        {
            int location = GL.GetUniformLocation(Id, name);
            GL.Uniform4(location, ref v);
        }

        public void Release()
        {
            if (Id != 0)
            {
                GL.DeleteProgram(Id);
                Id = 0;
            }
        }
    }
}
