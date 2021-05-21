using System;
using Materia.Rendering.Mathematics;

namespace Materia.Rendering.Interfaces
{
    public abstract class IGL
    {
        public static IGL Primary { get; set; }

        public abstract void Finish();

        public abstract void Flush();

        public abstract void MemoryBarrier(int mask);
        public abstract IntPtr MapBufferRange(int target, IntPtr start, int length, int flags);
        public abstract void BufferStorage(int target, int size, IntPtr data, int flags);
        public abstract bool UnmapBuffer(int target);
        public abstract IntPtr MapBuffer(int target, int access);
        public abstract void ClearTexImage(int id, int format, int type);
        public abstract ErrorCode GetError();

        /// Shaders
        public abstract int CreateProgram();
        public abstract int CreateShader(int type);

        public abstract void ShaderSource(int id, string data);
        public abstract void CompileShader(int id);
        public abstract void GetShader(int id, int param, out int status);
        public abstract void GetShaderInfoLog(int id, int size, out int length, out string log);

        public abstract void AttachShader(int id, int sid);
        public abstract void DetachShader(int id, int sid);

        public abstract void LinkProgram(int id);
        public abstract void UseProgram(int id);

        public abstract void DeleteProgram(int id);
        public abstract void DeleteShader(int id);

        public abstract void DispatchCompute(int w, int h, int z);

        public abstract void GetProgram(int id, int programParamName, out int status);
        public abstract void GetProgramInfoLog(int id, int size, out int length, out string log);

        public abstract void VertexAttribPointer(int index, int size, int type, bool normalized, int stride, int offset);
        public abstract void EnableVertexAttribArray(int index);
        public abstract void DisableVertexAttribArray(int index);

        ///Uniforms
        public abstract int GetUniformLocation(int id, string name);
        public abstract int GetUniformBlockIndex(int id, string name);
        public abstract void UniformMatrix4(int location, ref Matrix4 m);
        public abstract void UniformMatrix4(int location, ref Matrix4d m);

        public abstract void UniformMatrix3(int location, ref Matrix3 m);

        public abstract void UniformMatrix3(int location, ref Matrix3d m);

        public abstract void UniformBlockBinding(int id, int index, int pos);
        public abstract void Uniform1(int location, int i);
        public abstract void Uniform1(int location, uint i);
        public abstract void Uniform1(int location, float f);
        public abstract void Uniform3(int location, float x, float y, float z);
        public abstract void Uniform3(int location, int x, int y, int z);
        public abstract void Uniform2(int location, float x, float y);
        public abstract void Uniform2(int location, int x, int y);
        public abstract void Uniform4(int location, float x, float y, float z, float w);

        public abstract void Uniform4(int location, int x, int y, int z, int w);

        ///Buffers
        public abstract int GenBuffer();
        public abstract int GenRenderbuffer();
        public abstract int GenFramebuffer();
        public abstract int GenVertexArray();

        public abstract void BindBuffer(int target, int id);
        public abstract void BindVertexArray(int id);
        public abstract void BindBufferBase(int rangeTarget, int pos, int id);
        public abstract void BufferData(int target, int length, IntPtr data, int type);
        public abstract void BufferData(int target, int length, float[] data, int type);
        public abstract void BufferData(int target, int length, int[] data, int type);
        public abstract void BufferData(int target, int length, uint[] data, int type);
        public abstract void BufferSubData<T1>(int target, IntPtr offset, int length, T1[] data) where T1 : struct;
        public abstract void BufferSubData<T3>(int target, IntPtr offset, int length, ref T3 data) where T3 : struct;

        public abstract void DeleteBuffer(int id);
        public abstract void DeleteRenderbuffer(int id);
        public abstract void DeleteFramebuffer(int id);
        public abstract void DeleteVertexArray(int id);

        public abstract void BindRenderbuffer(int target, int id);
        public abstract void RenderbufferStorage(int target, int format, int width, int height);

        public abstract void BindFramebuffer(int target, int id);
        public abstract void FramebufferTexture2D(int target, int attachment, int textureTarget, int id, int mip);
        public abstract void FramebufferRenderbuffer(int target, int attachment, int renderTarget, int id);
        public abstract int CheckFramebufferStatus(int target);

        public abstract void ReadPixels(int x, int y, int w, int h, int format, int type, byte[] buffer);
        public abstract void ReadPixels(int x, int y, int w, int h, int format, int type, float[] buffer);

        public abstract void ReadTexture(float[] buffer);

        public abstract void DrawBuffer(int mode);
        public abstract void DrawBuffers(int[] modes);
        public abstract void ReadBuffer(int mode);

        public abstract void BlitFramebuffer(int sleft, int stop, int swidth, int sheight, int tleft, int ttop, int twidth, int theight, int blit, int filter);

        ///Textures
        public abstract int GenTexture();
        public abstract void BindTexture(int target, int id);

        public abstract void BindImageTexture(int unit, int id, int level, bool layerd, int layer, int access, int internalSize);

        public abstract void GenerateMipmap(int target);

        public abstract void TexStorage2D(int target, int level, int internalSize, int width, int height);

        public abstract void TexImage2D(int target, int level, int internalFormat, int w, int h, int border, int format, int pixelType, IntPtr data);
        public abstract void TexImage2D(int target, int level, int internalFormat, int w, int h, int border, int format, int pixelType, float[] data);
        public abstract void TexImage2D(int target, int level, int internalFormat, int w, int h, int border, int format, int pixelType, byte[] data);
        public abstract void CopyTexImage2D(int target, int level, int internalFormat, int x, int y, int w, int h, int border);
        public abstract void TexParameterI(int target, int param, int[] data);
        public abstract void TexParameter(int target, int param, int data);
        public abstract void ActiveTexture(int index);
        public abstract void DeleteTexture(int id);

        //Standard
        public abstract void Clear(int mask);
        public abstract void ClearColor(float r, float g, float b, float a);
        public abstract void Viewport(int x, int y, int w, int h);
        public abstract void PolygonMode(int mask, int mode);
        public abstract void PatchParameter(int type, int count);
        public abstract void Enable(int cap);
        public abstract void Disable(int cap);
        public abstract void DepthFunc(int func);
        public abstract void BlendFunc(int src, int dst);
        public abstract void BlendFuncSeparate(int colorsrc, int colordst, int alphasrc, int alphadst);
        public abstract void BlendEquationSeparate(int rgbMode, int alphaMode);
        public abstract void CullFace(int mode);

        ///Draw
        public abstract void DrawElements(int mode, int count, int type, int indices);
        public abstract void DrawArrays(int mode, int first, int count);

        //Blending
        public abstract void BlendEquation(int mode);
        public abstract void BlendBarrier();

        //Stencil
        public abstract void StencilMask(int m);
        public abstract void StencilMask(int face, int m);
        public abstract void StencilOp(int sfail, int dfail, int dsfail);
        public abstract void StencilOp(int face, int sfail, int dfail, int dsfail);
        public abstract void StencilFunc(int func, int @ref, int m);
        public abstract void StencilFunc(int face, int func, int @ref, int m);

        public abstract void AlphaFunc(int mode, float threshold);

        //line stuff
        public abstract void LineWidth(float width);

    }
}
