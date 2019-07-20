using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Materia.Math3D;

namespace Materia.GLInterfaces
{
    public class DummyGL : IGL
    {
        public DummyGL()
        {
            Primary = this;
        }

        public override void ActiveTexture(int index)
        {
           
        }

        public override void AttachShader(int id, int sid)
        {
            
        }

        public override void BindBuffer(int target, int id)
        {
           
        }

        public override void BindBufferBase(int rangeTarget, int pos, int id)
        {
            
        }

        public override void BindFramebuffer(int target, int id)
        {
            
        }

        public override void BindRenderbuffer(int target, int id)
        {
            
        }

        public override void BindTexture(int target, int id)
        {
            
        }

        public override void BindVertexArray(int id)
        {
            
        }

        public override void BlendFunc(int src, int dst)
        {
           
        }

        public override void BufferData(int target, int length, IntPtr data, int type)
        {
            
        }

        public override void BufferData(int target, int length, float[] data, int type)
        {
           
        }

        public override void BufferData(int target, int length, int[] data, int type)
        {
           
        }

        public override void BufferData(int target, int length, uint[] data, int type)
        {
           
        }

        public override void BufferSubData<T1>(int target, IntPtr offset, int length, T1[] data)
        {
            
        }

        public override void BufferSubData<T3>(int target, IntPtr offset, int length, ref T3 data)
        {
            
        }

        public override int CheckFramebufferStatus(int target)
        {
            return 0;
        }

        public override void Clear(int mask)
        {
  
        }

        public override void ClearColor(float r, float g, float b, float a)
        {
      
        }

        public override void CompileShader(int id)
        {
           
        }

        public override void CopyTexImage2D(int target, int level, int internalFormat, int x, int y, int w, int h, int border)
        {
            
        }

        public override int CreateProgram()
        {
            return 0;
        }

        public override int CreateShader(int type)
        {
            return 0;
        }

        public override void CullFace(int mode)
        {

        }

        public override void DeleteBuffer(int id)
        {

        }

        public override void DeleteFramebuffer(int id)
        {

        }

        public override void DeleteProgram(int id)
        {
   
        }

        public override void DeleteRenderbuffer(int id)
        {

        }

        public override void DeleteShader(int id)
        {

        }

        public override void DeleteTexture(int id)
        {

        }

        public override void DeleteVertexArray(int id)
        {

        }

        public override void DepthFunc(int func)
        {

        }

        public override void DetachShader(int id, int sid)
        {
 
        }

        public override void Disable(int cap)
        {

        }

        public override void DrawBuffer(int mode)
        {

        }

        public override void DrawElements(int mode, int count, int type, int indices)
        {
   
        }

        public override void Enable(int cap)
        {
 
        }

        public override void EnableVertexAttribArray(int index)
        {
       
        }

        public override void FramebufferRenderbuffer(int target, int attachment, int renderTarget, int id)
        {
         
        }

        public override void FramebufferTexture2D(int target, int attachment, int textureTarget, int id, int mip)
        {
            
        }

        public override int GenBuffer()
        {
            return 0;
        }

        public override void GenerateMipmap(int target)
        {

        }

        public override int GenFramebuffer()
        {
            return 0;
        }

        public override int GenRenderbuffer()
        {
            return 0;
        }

        public override int GenTexture()
        {
            return 0;
        }

        public override int GenVertexArray()
        {
            return 0;
        }

        public override void GetProgram(int id, int programParamName, out int status)
        {
            status = 0;
        }

        public override void GetProgramInfoLog(int id, int size, out int length, out string log)
        {
            length = 0;
            log = "";
        }

        public override void GetShader(int id, int param, out int status)
        {
            status = 0;
        }

        public override void GetShaderInfoLog(int id, int size, out int length, out string log)
        {
            length = 0;
            log = "";
        }

        public override int GetUniformBlockIndex(int id, string name)
        {
            return 0;
        }

        public override int GetUniformLocation(int id, string name)
        {
            return 0;
        }

        public override void LinkProgram(int id)
        {
            
        }

        public override void PatchParameter(int type, int count)
        {
            
        }

        public override void PolygonMode(int mask, int mode)
        {
           
        }

        public override void ReadBuffer(int mode)
        {
           
        }

        public override void ReadPixels(int x, int y, int w, int h, int format, int type, byte[] buffer)
        {
            
        }

        public override void ReadPixels(int x, int y, int w, int h, int format, int type, float[] buffer)
        {
            
        }

        public override void RenderbufferStorage(int target, int format, int width, int height)
        {
            
        }

        public override void ShaderSource(int id, string data)
        {
            
        }

        public override void TexImage2D(int target, int level, int internalFormat, int w, int h, int border, int format, int pixelType, IntPtr data)
        {
           
        }

        public override void TexImage2D(int target, int level, int internalFormat, int w, int h, int border, int format, int pixelType, float[] data)
        {
            
        }

        public override void TexImage2D(int target, int level, int internalFormat, int w, int h, int border, int format, int pixelType, byte[] data)
        {
            
        }

        public override void TexParameter(int target, int param, int data)
        {
            
        }

        public override void TexParameterI(int target, int param, int[] data)
        {
            
        }

        public override void Uniform1(int location, int i)
        {
            
        }

        public override void Uniform1(int location, uint i)
        {
            
        }

        public override void Uniform1(int location, float f)
        {
           
        }

        public override void Uniform2(int location, float x, float y)
        {
            
        }

        public override void Uniform3(int location, float x, float y, float z)
        {
            
        }

        public override void Uniform4(int location, float x, float y, float z, float w)
        {
            
        }

        public override void UniformBlockBinding(int id, int index, int pos)
        {
            
        }

        public override void UniformMatrix3(int location, ref Matrix3 m)
        {
            
        }

        public override void UniformMatrix4(int location, ref Matrix4 m)
        {
            
        }

        public override void UseProgram(int id)
        {
            
        }

        public override void VertexAttribPointer(int index, int size, int type, bool normalized, int stride, int offset)
        {
            
        }

        public override void Viewport(int x, int y, int w, int h)
        {
            
        }
    }
}
