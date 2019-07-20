using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Materia.Math3D;
using Materia.GLInterfaces;
using NLog;

namespace Materia.Shaders
{
    public class GLShaderProgram : IGLProgram
    {
        private static ILogger Log = LogManager.GetCurrentClassLogger();
        public int Id { get; protected set; }

        List<IGLShader> shaders;
        bool autoReleaseShaders;

        public GLShaderProgram(bool autoReleaseShaders = true)
        {
            this.autoReleaseShaders = autoReleaseShaders;
            shaders = new List<IGLShader>();
            Id = IGL.Primary.CreateProgram();
        }

        public void AttachShader(IGLShader shader)
        {
            shaders.Add(shader);
            IGL.Primary.AttachShader(Id, shader.Id);
        }

        public bool Link(out string log)
        {
            log = null;
            int success = 0;
            IGL.Primary.LinkProgram(Id);

            IGL.Primary.GetProgram(Id, (int)GetProgramParameterName.LinkStatus, out success);

            if(success < 1)
            {
                int length = 0;
                IGL.Primary.GetProgramInfoLog(Id, 512, out length, out log);
                Log.Error(log);
            }

            if(success == 1 && autoReleaseShaders)
            {
                //automatically release the shaders as we have succesfully
                //built the shader program for use now
                foreach(IGLShader s in shaders)
                {
                    IGL.Primary.DetachShader(Id, s.Id);
                    s.Release();
                }

                shaders.Clear();
            }

            return success == 1;
        }

        public void Use()
        {
            IGL.Primary.UseProgram(Id);
        }

        public void SetUniformMatrix4(string name, ref Matrix4 m)
        {
            int location = IGL.Primary.GetUniformLocation(Id, name);
            IGL.Primary.UniformMatrix4(location, ref m);
        }

        public void SetUniformMatrix3(string name, ref Matrix3 m)
        {
            int location = IGL.Primary.GetUniformLocation(Id, name);
            IGL.Primary.UniformMatrix3(location, ref m);
        }

        public void UniformBlockBinding(string name, int pos)
        {
            int index = IGL.Primary.GetUniformBlockIndex(Id, name);
            IGL.Primary.UniformBlockBinding(Id, index, pos);
        }

        public void SetUniform(string name, int i)
        {
            int location = IGL.Primary.GetUniformLocation(Id, name);
            IGL.Primary.Uniform1(location, i);
        }

        public void SetUniform(string name, bool b)
        {
            int location = IGL.Primary.GetUniformLocation(Id, name);
            IGL.Primary.Uniform1(location, b ? (int)1 : (int)0);
        }

        public void SetUniform(string name, uint i)
        {
            int location = IGL.Primary.GetUniformLocation(Id, name);
            IGL.Primary.Uniform1(location, i);
        }

        public void SetUniform(string name, float f)
        {
            int location = IGL.Primary.GetUniformLocation(Id, name);
            IGL.Primary.Uniform1(location, f);
        }

        public void SetUniform3(string name, ref Vector3 v)
        {
            int location = IGL.Primary.GetUniformLocation(Id, name);
            IGL.Primary.Uniform3(location, v.X, v.Y, v.Z);
        }

        public void SetUniform2(string name, ref Vector2 v)
        {
            int location = IGL.Primary.GetUniformLocation(Id, name);
            IGL.Primary.Uniform2(location, v.X, v.Y);
        }

        public void SetUniform4F(string name, ref Vector4 v)
        {
            int location = IGL.Primary.GetUniformLocation(Id, name);
            IGL.Primary.Uniform4(location, v.X, v.Y, v.Z, v.W);
        }

        public void Release()
        {
            if (Id != 0)
            {
                IGL.Primary.DeleteProgram(Id);
                Id = 0;
            }
        }
    }
}
