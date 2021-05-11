using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Materia.Rendering.Interfaces;
using Materia.Rendering.Mathematics;
using OpenTK.Graphics.OpenGL;

namespace MateriaCore
{
    public class TKGL : IGL
    {
        public TKGL()
        {
            Primary = this;
        }

        public override void Finish()
        {
            GL.Finish();
        }

        public override void Flush()
        {
            GL.Flush();
        }

        public override void MemoryBarrier(int mask)
        {
            GL.MemoryBarrier((OpenTK.Graphics.OpenGL.MemoryBarrierFlags)mask);
        }

        public override void ClearTexImage(int id, int format, int type)
        {
            GL.ClearTexImage(id, 0, (OpenTK.Graphics.OpenGL.PixelFormat)format, (OpenTK.Graphics.OpenGL.PixelType)type, IntPtr.Zero);
        }

        public override IntPtr MapBufferRange(int target, IntPtr start, int length, int flags)
        {
            return GL.MapBufferRange((OpenTK.Graphics.OpenGL.BufferTarget)target, start, length, (OpenTK.Graphics.OpenGL.BufferAccessMask)flags);
        }

        public override void BufferStorage(int target, int size, IntPtr data, int flags)
        {
            GL.BufferStorage((OpenTK.Graphics.OpenGL.BufferTarget)target, size, data, (OpenTK.Graphics.OpenGL.BufferStorageFlags)flags); 
        }

        public override IntPtr MapBuffer(int target, int access)
        {
            return GL.MapBuffer((OpenTK.Graphics.OpenGL.BufferTarget)target, (OpenTK.Graphics.OpenGL.BufferAccess)access);
        }

        public override bool UnmapBuffer(int target)
        {
            return GL.UnmapBuffer((OpenTK.Graphics.OpenGL.BufferTarget)target);
        }

        public override Materia.Rendering.Interfaces.ErrorCode GetError()
        {
            return (Materia.Rendering.Interfaces.ErrorCode)(int)GL.GetError();
        }

        public override void ReadTexture(float[] buffer)
        {
            GL.GetTexImage(OpenTK.Graphics.OpenGL.TextureTarget.Texture2D, 0, OpenTK.Graphics.OpenGL.PixelFormat.Rgba, OpenTK.Graphics.OpenGL.PixelType.Float, buffer);
        }

        public override void TexStorage2D(int target, int level, int internalSize, int width, int height)
        {
            GL.TexStorage2D((OpenTK.Graphics.OpenGL.TextureTarget2d)target, level, (OpenTK.Graphics.OpenGL.SizedInternalFormat)internalSize, width, height);
        }

        public override void BindImageTexture(int unit, int id, int level, bool layerd, int layer, int access, int internalSize)
        {
            
            GL.BindImageTexture(unit, id, level, layerd, layer, (OpenTK.Graphics.OpenGL.TextureAccess)access, (OpenTK.Graphics.OpenGL.SizedInternalFormat)internalSize);
        }

        public override void DispatchCompute(int w, int h, int z)
        {
            GL.DispatchCompute(w, h, z);
            GL.MemoryBarrier(OpenTK.Graphics.OpenGL.MemoryBarrierFlags.TextureFetchBarrierBit | OpenTK.Graphics.OpenGL.MemoryBarrierFlags.TextureUpdateBarrierBit | OpenTK.Graphics.OpenGL.MemoryBarrierFlags.ShaderStorageBarrierBit | OpenTK.Graphics.OpenGL.MemoryBarrierFlags.ShaderImageAccessBarrierBit);
        }
        public override void ActiveTexture(int index)
        {
            GL.ActiveTexture((OpenTK.Graphics.OpenGL.TextureUnit)index);
        }

        public override void AttachShader(int id, int sid)
        {
            GL.AttachShader(id, sid);
        }

        public override void BindBuffer(int target, int id)
        {
            GL.BindBuffer((OpenTK.Graphics.OpenGL.BufferTarget)target, id);
        }

        public override void BindBufferBase(int rangeTarget, int pos, int id)
        {
            GL.BindBufferBase((OpenTK.Graphics.OpenGL.BufferRangeTarget)rangeTarget, pos, id);
        }

        public override void BindFramebuffer(int target, int id)
        {
            GL.BindFramebuffer((OpenTK.Graphics.OpenGL.FramebufferTarget)target, id);
        }

        public override void BindRenderbuffer(int target, int id)
        {
            GL.BindRenderbuffer((OpenTK.Graphics.OpenGL.RenderbufferTarget)target, id);
        }

        public override void BindTexture(int target, int id)
        {
            GL.BindTexture((OpenTK.Graphics.OpenGL.TextureTarget)target, id);
        }

        public override void BindVertexArray(int id)
        {
            GL.BindVertexArray(id);
        }

        public override void BlendFunc(int src, int dst)
        {
            GL.BlendFunc((OpenTK.Graphics.OpenGL.BlendingFactor)src, (OpenTK.Graphics.OpenGL.BlendingFactor)dst);
        }

        public override void BlendFuncSeparate(int colorsrc, int colordst, int alphasrc, int alphadst)
        {
            GL.BlendFuncSeparate((OpenTK.Graphics.OpenGL.BlendingFactorSrc)colorsrc, (OpenTK.Graphics.OpenGL.BlendingFactorDest)colordst, (OpenTK.Graphics.OpenGL.BlendingFactorSrc)alphasrc, (OpenTK.Graphics.OpenGL.BlendingFactorDest)alphadst);
        }

        public override void BlendEquationSeparate(int rgbBlend, int alphaBlend)
        {
            GL.BlendEquationSeparate((OpenTK.Graphics.OpenGL.BlendEquationMode)rgbBlend, (OpenTK.Graphics.OpenGL.BlendEquationMode)alphaBlend);
        }

        public override void BlitFramebuffer(int sleft, int stop, int swidth, int sheight, int tleft, int ttop, int twidth, int theight, int bit, int filter)
        {
            GL.BlitFramebuffer(sleft, stop, swidth, sheight, tleft, ttop, twidth, theight, (OpenTK.Graphics.OpenGL.ClearBufferMask)bit, (OpenTK.Graphics.OpenGL.BlitFramebufferFilter)filter);
        }

        public override void BufferData(int target, int length, IntPtr data, int type)
        {
            GL.BufferData((OpenTK.Graphics.OpenGL.BufferTarget)target, length, data, (OpenTK.Graphics.OpenGL.BufferUsageHint)type);
        }

        public override void BufferData(int target, int length, float[] data, int type)
        {
            GL.BufferData((OpenTK.Graphics.OpenGL.BufferTarget)target, length, data, (OpenTK.Graphics.OpenGL.BufferUsageHint)type);
        }

        public override void BufferData(int target, int length, int[] data, int type)
        {
            GL.BufferData((OpenTK.Graphics.OpenGL.BufferTarget)target, length, data, (OpenTK.Graphics.OpenGL.BufferUsageHint)type);
        }

        public override void BufferData(int target, int length, uint[] data, int type)
        {
            GL.BufferData((OpenTK.Graphics.OpenGL.BufferTarget)target, length, data, (OpenTK.Graphics.OpenGL.BufferUsageHint)type);
        }

        public override void BufferSubData<T1>(int target, IntPtr offset, int length, T1[] data)
        {
            GL.BufferSubData((OpenTK.Graphics.OpenGL.BufferTarget)target, offset, length, data); 
        }

        public override void BufferSubData<T3>(int target, IntPtr offset, int length, ref T3 data)
        {
            GL.BufferSubData((OpenTK.Graphics.OpenGL.BufferTarget)target, offset, length, ref data);
        }

        public override int CheckFramebufferStatus(int target)
        {
            return (int)GL.CheckFramebufferStatus((OpenTK.Graphics.OpenGL.FramebufferTarget)target);
        }

        public override void Clear(int mask)
        {
            GL.Clear((OpenTK.Graphics.OpenGL.ClearBufferMask)mask);
        }

        public override void ClearColor(float r, float g, float b, float a)
        {
            GL.ClearColor(r, g, b, a);
        }

        public override void CompileShader(int id)
        {
            GL.CompileShader(id);
        }

        public override void CopyTexImage2D(int target, int level, int internalFormat, int x, int y, int w, int h, int border)
        {
            GL.CopyTexImage2D((OpenTK.Graphics.OpenGL.TextureTarget)target, level, (OpenTK.Graphics.OpenGL.InternalFormat)internalFormat, x, y, w, h, border);
        }

        public override int CreateProgram()
        {
            return GL.CreateProgram();
        }

        public override int CreateShader(int type)
        {
            return GL.CreateShader((OpenTK.Graphics.OpenGL.ShaderType)type);
        }

        public override void CullFace(int mode)
        {
            GL.CullFace((OpenTK.Graphics.OpenGL.CullFaceMode)mode);
        }

        public override void DeleteBuffer(int id)
        {
            GL.DeleteBuffer(id);
        }

        public override void DeleteFramebuffer(int id)
        {
            GL.DeleteFramebuffer(id);
        }

        public override void DeleteProgram(int id)
        {
            GL.DeleteProgram(id);
        }

        public override void DeleteRenderbuffer(int id)
        {
            GL.DeleteRenderbuffer(id);
        }

        public override void DeleteShader(int id)
        {
            GL.DeleteShader(id);
        }

        public override void DeleteTexture(int id)
        {
            GL.DeleteTexture(id);
        }

        public override void DeleteVertexArray(int id)
        {
            GL.DeleteVertexArray(id);
        }

        public override void DepthFunc(int func)
        {
            GL.DepthFunc((OpenTK.Graphics.OpenGL.DepthFunction)func);
        }

        public override void DetachShader(int id, int sid)
        {
            GL.DetachShader(id, sid);
        }

        public override void Disable(int cap)
        {
            GL.Disable((OpenTK.Graphics.OpenGL.EnableCap)cap);
        }

        public override void DrawBuffer(int mode)
        {
            GL.DrawBuffer((OpenTK.Graphics.OpenGL.DrawBufferMode)mode);
        }

        public override void DrawBuffers(int[] modes)
        {
            OpenTK.Graphics.OpenGL.DrawBuffersEnum[] fmodes = new OpenTK.Graphics.OpenGL.DrawBuffersEnum[modes.Length];

            for(int i = 0; i < modes.Length; ++i)
            {
                fmodes[i] = (OpenTK.Graphics.OpenGL.DrawBuffersEnum)modes[i];
            }

            GL.DrawBuffers(fmodes.Length, fmodes);
        }

        public override void DrawElements(int mode, int count, int type, int indices)
        {
            GL.DrawElements((OpenTK.Graphics.OpenGL.BeginMode)mode, count, (OpenTK.Graphics.OpenGL.DrawElementsType)type, indices);
        }

        public override void Enable(int cap)
        {
            GL.Enable((OpenTK.Graphics.OpenGL.EnableCap)cap);
        }

        public override void EnableVertexAttribArray(int index)
        {
            GL.EnableVertexAttribArray(index);
        }

        public override void FramebufferRenderbuffer(int target, int attachment, int renderTarget, int id)
        {
            GL.FramebufferRenderbuffer((OpenTK.Graphics.OpenGL.FramebufferTarget)target, (OpenTK.Graphics.OpenGL.FramebufferAttachment)attachment, (OpenTK.Graphics.OpenGL.RenderbufferTarget)renderTarget, id);
        }

        public override void FramebufferTexture2D(int target, int attachment, int textureTarget, int id, int mip)
        {
            GL.FramebufferTexture2D((OpenTK.Graphics.OpenGL.FramebufferTarget)target, (OpenTK.Graphics.OpenGL.FramebufferAttachment)attachment, (OpenTK.Graphics.OpenGL.TextureTarget)textureTarget, id, mip);
        }

        public override int GenBuffer()
        {
            return GL.GenBuffer();
        }

        public override void GenerateMipmap(int target)
        {
            GL.GenerateMipmap((OpenTK.Graphics.OpenGL.GenerateMipmapTarget)target);
        }

        public override int GenFramebuffer()
        {
            return GL.GenFramebuffer();
        }

        public override int GenRenderbuffer()
        {
            return GL.GenRenderbuffer();
        }

        public override int GenTexture()
        {
            return GL.GenTexture();
        }

        public override int GenVertexArray()
        {
            return GL.GenVertexArray();
        }

        public override void GetProgram(int id, int programParamName, out int status)
        {
            GL.GetProgram(id, (OpenTK.Graphics.OpenGL.GetProgramParameterName)programParamName, out status);
        }

        public override void GetProgramInfoLog(int id, int size, out int length, out string log)
        {
            GL.GetProgramInfoLog(id, size, out length, out log);
        }

        public override void GetShader(int id, int param, out int status)
        {
            GL.GetShader(id, (OpenTK.Graphics.OpenGL.ShaderParameter)param, out status);
        }

        public override void GetShaderInfoLog(int id, int size, out int length, out string log)
        {
            GL.GetShaderInfoLog(id, size, out length, out log);
        }

        public override int GetUniformBlockIndex(int id, string name)
        {
            return GL.GetUniformBlockIndex(id, name);
        }

        public override int GetUniformLocation(int id, string name)
        {
            return GL.GetUniformLocation(id, name);
        }

        public override void LinkProgram(int id)
        {
            GL.LinkProgram(id);
        }

        public override void PatchParameter(int type, int count)
        {
            GL.PatchParameter((OpenTK.Graphics.OpenGL.PatchParameterInt)type, count);
        }

        public override void PolygonMode(int mask, int mode)
        {
            GL.PolygonMode((OpenTK.Graphics.OpenGL.MaterialFace)mask, (OpenTK.Graphics.OpenGL.PolygonMode)mode);
        }

        public override void ReadBuffer(int mode)
        {
            GL.ReadBuffer((OpenTK.Graphics.OpenGL.ReadBufferMode)mode);
        }

        public override void ReadPixels(int x, int y, int w, int h, int format, int type, byte[] buffer)
        {
            GL.ReadPixels(x, y, w, h, (OpenTK.Graphics.OpenGL.PixelFormat)format, (OpenTK.Graphics.OpenGL.PixelType)type, buffer);
        }

        public override void ReadPixels(int x, int y, int w, int h, int format, int type, float[] buffer)
        {
            GL.ReadPixels(x, y, w, h, (OpenTK.Graphics.OpenGL.PixelFormat)format, (OpenTK.Graphics.OpenGL.PixelType)type, buffer);
        }

        public override void RenderbufferStorage(int target, int format, int width, int height)
        {
            GL.RenderbufferStorage((OpenTK.Graphics.OpenGL.RenderbufferTarget)target, (OpenTK.Graphics.OpenGL.RenderbufferStorage)format, width, height);
        }

        public override void ShaderSource(int id, string data)
        {
            GL.ShaderSource(id, data);
        }

        public override void TexImage2D(int target, int level, int internalFormat, int w, int h, int border, int format, int pixelType, IntPtr data)
        {
            GL.TexImage2D((OpenTK.Graphics.OpenGL.TextureTarget)target, level, (OpenTK.Graphics.OpenGL.PixelInternalFormat)internalFormat, w, h, border, (OpenTK.Graphics.OpenGL.PixelFormat)format, (OpenTK.Graphics.OpenGL.PixelType)pixelType, data);
        }

        public override void TexImage2D(int target, int level, int internalFormat, int w, int h, int border, int format, int pixelType, float[] data)
        {
            GL.TexImage2D((OpenTK.Graphics.OpenGL.TextureTarget)target, level, (OpenTK.Graphics.OpenGL.PixelInternalFormat)internalFormat, w, h, border, (OpenTK.Graphics.OpenGL.PixelFormat)format, (OpenTK.Graphics.OpenGL.PixelType)pixelType, data);
        }

        public override void TexImage2D(int target, int level, int internalFormat, int w, int h, int border, int format, int pixelType, byte[] data)
        {
            GL.TexImage2D((OpenTK.Graphics.OpenGL.TextureTarget)target, level, (OpenTK.Graphics.OpenGL.PixelInternalFormat)internalFormat, w, h, border, (OpenTK.Graphics.OpenGL.PixelFormat)format, (OpenTK.Graphics.OpenGL.PixelType)pixelType, data);
        }

        public override void TexParameter(int target, int param, int data)
        {
            GL.TexParameter((OpenTK.Graphics.OpenGL.TextureTarget)target, (OpenTK.Graphics.OpenGL.TextureParameterName)param, data);
        }

        public override void TexParameterI(int target, int param, int[] data)
        {
            GL.TexParameterI((OpenTK.Graphics.OpenGL.TextureTarget)target, (OpenTK.Graphics.OpenGL.TextureParameterName)param, data);
        }

        public override void Uniform1(int location, int i)
        {
            GL.Uniform1(location, i);
        }

        public override void Uniform1(int location, uint i)
        {
            GL.Uniform1(location, i);
        }

        public override void Uniform1(int location, float f)
        {
            GL.Uniform1(location, f);
        }

        public override void Uniform2(int location, float x, float y)
        {
            GL.Uniform2(location, x, y);
        }

        public override void Uniform2(int location, int x, int y)
        {
            GL.Uniform2(location, x, y);
        }

        public override void Uniform3(int location, float x, float y, float z)
        {
            GL.Uniform3(location, x, y, z);
        }

        public override void Uniform3(int location, int x, int y, int z)
        {
            GL.Uniform3(location, x, y, z);
        }

        public override void Uniform4(int location, float x, float y, float z, float w)
        {
            GL.Uniform4(location, x, y, z, w);
        }

        public override void Uniform4(int location, int x, int y, int z, int w)
        {
            GL.Uniform4(location, x, y, z, w);
        }

        public override void UniformBlockBinding(int id, int index, int pos)
        {
            GL.UniformBlockBinding(id, index, pos);
        }

        public override void UniformMatrix3(int location, ref Materia.Rendering.Mathematics.Matrix3 m)
        {
            unsafe
            {
                fixed(float* ptr = &m.Row0.X)
                {
                    GL.UniformMatrix3(location, 1, false, ptr);
                }
            }
        }

        public override void UniformMatrix3(int location, ref Materia.Rendering.Mathematics.Matrix3d m)
        {
            unsafe
            {
                fixed (double* ptr = &m.Row0.X)
                {
                    GL.UniformMatrix3(location, 1, false, ptr);
                }
            }
        }

        public override void UniformMatrix4(int location, ref Materia.Rendering.Mathematics.Matrix4 m)
        {
            unsafe
            {
                fixed(float* ptr = &m.Row0.X)
                {
                    GL.UniformMatrix4(location, 1, false, ptr);
                }
            }
        }

        public override void UniformMatrix4(int location, ref Materia.Rendering.Mathematics.Matrix4d m)
        {
            unsafe
            {
                fixed (double* ptr = &m.Row0.X)
                {
                    GL.UniformMatrix4(location, 1, false, ptr);
                }
            }
        }

        public override void UseProgram(int id)
        {
            GL.UseProgram(id);
        }

        public override void VertexAttribPointer(int index, int size, int type, bool normalized, int stride, int offset)
        {
            GL.VertexAttribPointer(index, size, (OpenTK.Graphics.OpenGL.VertexAttribPointerType)type, normalized, stride, offset);
        }

        public override void Viewport(int x, int y, int w, int h)
        {
            GL.Viewport(x, y, w, h);
        }

        public override void DisableVertexAttribArray(int index)
        {
            GL.DisableVertexAttribArray(index);
        }

        public override void DrawArrays(int mode, int first, int count)
        {
            GL.DrawArrays((OpenTK.Graphics.OpenGL.PrimitiveType)mode, first, count);
        }

        public override void BlendEquation(int mode)
        {
            GL.BlendEquation((OpenTK.Graphics.OpenGL.BlendEquationMode)mode);
        }

        public override void BlendBarrier()
        {
            GL.Khr.BlendBarrier();
        }

        //Stencil
        public override void StencilMask(int m) 
        {
            GL.StencilMask(m);
        }

        public override void StencilMask(int face, int m)
        {
            GL.StencilMaskSeparate((OpenTK.Graphics.OpenGL.StencilFace)face, m);
        }

        public override void StencilOp(int sfail, int dfail, int dsfail) 
        {
            GL.StencilOp((OpenTK.Graphics.OpenGL.StencilOp)sfail, (OpenTK.Graphics.OpenGL.StencilOp)dfail, (OpenTK.Graphics.OpenGL.StencilOp)dsfail);
        }
        public override void StencilOp(int face, int sfail, int dfail, int dsfail)
        {
            GL.StencilOpSeparate((OpenTK.Graphics.OpenGL.StencilFace)face, (OpenTK.Graphics.OpenGL.StencilOp)sfail, (OpenTK.Graphics.OpenGL.StencilOp)dfail, (OpenTK.Graphics.OpenGL.StencilOp)dsfail);
        }

        public override void StencilFunc(int func, int @ref, int m) 
        {
            GL.StencilFunc((OpenTK.Graphics.OpenGL.StencilFunction)func, @ref, m);
        }

        public override void StencilFunc(int face, int func, int @ref, int m)
        {
            GL.StencilFuncSeparate((OpenTK.Graphics.OpenGL.StencilFace)face, (OpenTK.Graphics.OpenGL.StencilFunction)func, @ref, m);
        }

        public override void AlphaFunc(int mode, float threshold)
        {
            GL.AlphaFunc((OpenTK.Graphics.OpenGL.AlphaFunction)mode, threshold);
        }
    }
}
