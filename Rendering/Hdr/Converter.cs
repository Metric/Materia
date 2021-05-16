﻿using Materia.Rendering.Textures;
using Materia.Rendering.Buffers;
using Materia.Rendering.Mathematics;
using Materia.Rendering.Geometry;
using Materia.Rendering.Extensions;
using Materia.Rendering.Shaders;
using Materia.Rendering.Interfaces;
using System;

namespace Materia.Rendering.Hdr
{
    public class Converter : IDisposable
    {
        static float[] cubeVertices = new float[] 
        {
            -0.5f, -0.5f, -0.5f,
            0.5f, -0.5f, -0.5f,
            0.5f, 0.5f, -0.5f,
            -0.5f, 0.5f, -0.5f,
            -0.5f, 0.5f, 0.5f,
            0.5f, 0.5f, 0.5f,
            0.5f, -0.5f, 0.5f,
            -0.5f, -0.5f, 0.5f
        };

        static int[] cubeTriangles = new int[]
        {
            0, 2, 1, //face front
	        0, 3, 2,
            2, 3, 4, //face top
	        2, 4, 5,
            1, 2, 5, //face right
	        1, 5, 6,
            0, 7, 4, //face left
	        0, 4, 3,
            5, 4, 7, //face back
	        5, 7, 6,
            0, 6, 7, //face bottom
	        0, 1, 6
        };

        static Matrix4 projection = Matrix4.CreatePerspectiveFieldOfView(90f.ToRadians(), 1.0f, 0.1f, 10f);

        static readonly Matrix4[] views = new Matrix4[]
        {
            Matrix4.LookAt(Vector3.Zero, new Vector3(1,0,0), new Vector3(0,-1,0)),
            Matrix4.LookAt(Vector3.Zero, new Vector3(-1,0,0), new Vector3(0,-1,0)),
            Matrix4.LookAt(Vector3.Zero, new Vector3(0,1,0), new Vector3(0,0,1)),
            Matrix4.LookAt(Vector3.Zero, new Vector3(0,-1,0), new Vector3(0,0,-1)),
            Matrix4.LookAt(Vector3.Zero, new Vector3(0,0,1), new Vector3(0,-1,0)),
            Matrix4.LookAt(Vector3.Zero, new Vector3(0,0,-1), new Vector3(0,-1,0))
        };

        IGLProgram sphericalShader;
        IGLProgram irradianceShader;
        IGLProgram prefilterShader;

        GLFrameBuffer frameBuffer;
        GLRenderBuffer renderBuffer;

        MeshRenderer cube;

        public GLTextureCube ToPrefilter(GLTextureCube hdrMap)
        {
            if (prefilterShader == null)
            {
                prefilterShader = GLShaderCache.GetShader("basic.glsl", "prefilter.glsl");
            }

            if (cube == null)
            {
                cube = new MeshRenderer(cubeVertices, cubeTriangles);
            }

            if (frameBuffer == null)
            {
                frameBuffer = new GLFrameBuffer();
            }

            int maxSize = 512;

            renderBuffer?.Dispose();
            renderBuffer = new GLRenderBuffer();

            renderBuffer.Bind();
            renderBuffer.SetBufferStorageAsDepth(maxSize, maxSize);
            renderBuffer.Unbind();

            frameBuffer.Bind();
            frameBuffer.AttachDepth(renderBuffer);
            frameBuffer.Unbind();

            GLTextureCube cubeMap = new GLTextureCube(PixelInternalFormat.Rgb16f);
            cubeMap.Bind();
            for (int i = 0; i < 6; ++i)
            {
                cubeMap.SetData(i, IntPtr.Zero, PixelFormat.Rgb, maxSize, maxSize);
            }
            cubeMap.SetFilter((int)TextureMinFilter.LinearMipmapLinear, (int)TextureMagFilter.Linear);
            cubeMap.SetWrap((int)TextureWrapMode.ClampToEdge);
            cubeMap.GenerateMipMaps();
            GLTextureCube.Unbind();

            IGLProgram shader = prefilterShader;

            if (shader == null) return cubeMap;

            frameBuffer.Bind();

            IGL.Primary.Enable((int)EnableCap.TextureCubeMapSeamless);

            //use shader
            shader.Use();

            shader.SetUniform("hdrMap", 0);
            IGL.Primary.ActiveTexture((int)TextureUnit.Texture0);
            hdrMap.Bind();

            Matrix4 proj = projection;

            MeshRenderer.SharedVao?.Bind();

            shader.SetUniformMatrix4("projectionMatrix", ref proj);

            IGL.Primary.ClearColor(0, 0, 0, 1);
            IGL.Primary.Disable((int)EnableCap.CullFace);
            IGL.Primary.PolygonMode((int)MaterialFace.FrontAndBack, (int)PolygonMode.Fill);

            for (int mip = 0; mip < 5; ++mip)
            {
                int mipWidth = mip == 0 ? maxSize : (int)(maxSize * Math.Pow(0.5, mip));
                IGL.Primary.Viewport(0, 0, mipWidth, mipWidth);
                float roughness = (float)mip / 4.0f;
                shader.SetUniform("roughness", roughness);

                for (int i = 0; i < 6; ++i)
                {
                    Matrix4 view = views[i];
                    shader.SetUniformMatrix4("viewMatrix", ref view);
                    frameBuffer.AttachColor(cubeMap, i, 0, mip);
                    IGL.Primary.Clear((int)(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit));
                    cube.DrawBasic();
                }
            }

            GLTextureCube.Unbind();
            frameBuffer.Unbind();

            MeshRenderer.SharedVao?.Unbind();

            IGL.Primary.Enable((int)EnableCap.CullFace);

            return cubeMap;
        }

        public GLTextureCube ToIrradiance(GLTextureCube hdrMap)
        {
            if (irradianceShader == null)
            {
                irradianceShader = GLShaderCache.GetShader("basic.glsl", "irradiance.glsl");
            }

            if (cube == null)
            {
                cube = new MeshRenderer(cubeVertices, cubeTriangles);
            }

            if (frameBuffer == null)
            {
                frameBuffer = new GLFrameBuffer();
            }

            renderBuffer?.Dispose();
            renderBuffer = new GLRenderBuffer();

            renderBuffer.Bind();
            renderBuffer.SetBufferStorageAsDepth(32, 32);
            renderBuffer.Unbind();

            frameBuffer.Bind();
            frameBuffer.AttachDepth(renderBuffer);
            frameBuffer.Unbind();

            GLTextureCube cubeMap = new GLTextureCube(PixelInternalFormat.Rgb16f);
            cubeMap.Bind();
            for (int i = 0; i < 6; ++i)
            {
                cubeMap.SetData(i, IntPtr.Zero, PixelFormat.Rgb, 32, 32);
            }
            cubeMap.SetFilter((int)TextureMinFilter.LinearMipmapLinear, (int)TextureMagFilter.Linear);
            cubeMap.SetWrap((int)TextureWrapMode.ClampToEdge);
            GLTextureCube.Unbind();

            IGLProgram shader = irradianceShader;

            if (shader == null) return cubeMap;

            frameBuffer.Bind();

            IGL.Primary.Enable((int)EnableCap.TextureCubeMapSeamless);

            //use shader
            shader.Use();

            shader.SetUniform("hdrMap", 0);
            IGL.Primary.ActiveTexture((int)TextureUnit.Texture0);
            hdrMap.Bind();

            Matrix4 proj = projection;

            MeshRenderer.SharedVao?.Bind();

            shader.SetUniformMatrix4("projectionMatrix", ref proj);
            IGL.Primary.Viewport(0, 0, 32, 32);
            IGL.Primary.ClearColor(0, 0, 0, 1);
            IGL.Primary.Disable((int)EnableCap.CullFace);
            IGL.Primary.PolygonMode((int)MaterialFace.FrontAndBack, (int)PolygonMode.Fill);
            for (int i = 0; i < 6; ++i)
            {
                Matrix4 view = views[i];
                shader.SetUniformMatrix4("viewMatrix", ref view);
                frameBuffer.AttachColor(cubeMap, i);
                IGL.Primary.Clear((int)(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit));
                cube.DrawBasic();
            }
            GLTextureCube.Unbind();
            frameBuffer.Unbind();

            cubeMap.Bind();
            cubeMap.GenerateMipMaps();
            GLTextureCube.Unbind();

            MeshRenderer.SharedVao?.Unbind();

            IGL.Primary.Enable((int)EnableCap.CullFace);

            return cubeMap;
        }

        public GLTextureCube ToCube(GLTexture2D hdrMap)
        {
            if (sphericalShader == null)
            {
                sphericalShader = GLShaderCache.GetShader("basic.glsl", "spherical.glsl");
            }

            if (cube == null)
            {
                cube = new MeshRenderer(cubeVertices, cubeTriangles);
            }

            renderBuffer?.Dispose();
            renderBuffer = new GLRenderBuffer();

            renderBuffer.Bind();
            renderBuffer.SetBufferStorageAsDepth(512, 512);
            renderBuffer.Unbind();

            if (frameBuffer == null)
            {
                frameBuffer = new GLFrameBuffer();
            }

            frameBuffer.Bind();
            frameBuffer.AttachDepth(renderBuffer);
            frameBuffer.Unbind();

            GLTextureCube cubeMap = new GLTextureCube(PixelInternalFormat.Rgb16f);
            cubeMap.Bind();
            for (int i = 0; i < 6; ++i)
            {
                cubeMap.SetData(i, IntPtr.Zero, PixelFormat.Rgb, 512, 512);
            }
            cubeMap.SetFilter((int)TextureMinFilter.LinearMipmapLinear, (int)TextureMagFilter.Linear);
            cubeMap.SetWrap((int)TextureWrapMode.ClampToEdge);
            GLTextureCube.Unbind();

            IGLProgram shader = sphericalShader;

            if (shader == null) return cubeMap;
            
            frameBuffer.Bind();

            //use shader
            shader.Use();

            shader.SetUniform("hdrMap", 0);
            IGL.Primary.ActiveTexture((int)TextureUnit.Texture0);
            hdrMap.Bind();

            Matrix4 proj = projection;

            MeshRenderer.SharedVao?.Bind();

            shader.SetUniformMatrix4("projectionMatrix", ref proj);
            IGL.Primary.Viewport(0, 0, 512, 512);
            IGL.Primary.Disable((int)EnableCap.CullFace);
            IGL.Primary.PolygonMode((int)MaterialFace.FrontAndBack, (int)PolygonMode.Fill);
            for (int i = 0; i < 6; ++i)
            {
                Matrix4 view = views[i];
                shader.SetUniformMatrix4("viewMatrix", ref view);
                frameBuffer.AttachColor(cubeMap, i);
                IGL.Primary.Clear((int)(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit));
                cube.DrawBasic();
            }
            GLTexture2D.Unbind();
            frameBuffer.Unbind();

            cubeMap.Bind();
            cubeMap.GenerateMipMaps();
            GLTextureCube.Unbind();

            IGL.Primary.Enable((int)EnableCap.CullFace);

            MeshRenderer.SharedVao?.Unbind();

            return cubeMap;
        }

        public void Dispose()
        {
            renderBuffer?.Dispose();
            renderBuffer = null;

            frameBuffer?.Dispose();
            frameBuffer = null;

            cube?.Dispose();
            cube = null;
        }
    }
}