using System;
using System.Collections.Generic;
using Materia.Rendering.Interfaces;
using System.IO;
using Microsoft.Extensions.FileProviders;
using MLog;
using System.Diagnostics;

namespace Materia.Rendering.Shaders
{
    public class GLShaderCache
    {
        protected static Dictionary<string, IGLProgram> Shaders = new Dictionary<string, IGLProgram>();

        static string GetEmbeddedShader(string path)
        {
            try
            {
                EmbeddedFileProvider provider = new EmbeddedFileProvider(typeof(GLShaderCache).Assembly);
                using (Stream stream = provider.GetFileInfo(path).CreateReadStream())
                using (StreamReader reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("failed to load embedded shader text?");
            }

            return null;
        }

        public static IGLProgram CompileCompute(string shader)
        {
            GLComputeShader cmp = new GLComputeShader(shader);
            string log = null;
            if (!cmp.Compile(out log))
            {
                cmp.Dispose();
                Log.Error(log);
                Log.Debug(Environment.StackTrace);
                return null;
            }

            IGLProgram program = new GLShaderProgram(true);
            program.AttachShader(cmp);

            if (!program.Link(out log))
            {
                program.Dispose();
                Log.Error(log);
                Log.Debug(Environment.StackTrace);
                return null;
            }

            return program;
        }

        public static IGLProgram CompileFragWithVert(string vertFile, string fragData)
        {
            string vertexPath = Path.Combine("Embedded", "Vertex", vertFile);

            IGLProgram shader = null;

            string vertexData = GetEmbeddedShader(vertexPath);

            if(string.IsNullOrEmpty(vertexData))
            {
                Log.Debug(Environment.StackTrace);
                Log.Debug("Shader failed to get embedded " + vertFile);
                return null;
            }

            GLFragmentShader frag = new GLFragmentShader(fragData);
            string log = null;
            if (!frag.Compile(out log))
            {
                frag.Dispose();
                Log.Error(log);
                Log.Debug(Environment.StackTrace);
                Log.Debug("Shader failure for: " + vertFile);
                return null;
            }

            GLVertexShader vert = new GLVertexShader(vertexData);
            if (!vert.Compile(out log))
            {
                vert.Dispose();
                Log.Error(log);
                Log.Debug(Environment.StackTrace);
                Log.Debug("Shader failure for: " + vertFile);
                return null;
            }

            shader = new GLShaderProgram(true);
            shader.AttachShader(vert);
            shader.AttachShader(frag);

            if (!shader.Link(out log))
            {
                shader.Dispose();
                Log.Error(log);
                Log.Debug(Environment.StackTrace);
                Log.Debug("Shader failure for: " + vertFile);
                return null;
            }

            return shader;
        }

        public static IGLProgram GetShader(string vertFile, string tcsFile, string tesFile, string fragFile)
        {
            string vertexPath = Path.Combine("Embedded", "Vertex", vertFile);
            string fragPath = Path.Combine("Embedded", "Frag", fragFile);
            string tcsPath = Path.Combine("Embedded", "Tess", tcsFile);
            string tesPath = Path.Combine("Embedded", "Tess", tesFile);

            IGLProgram shader = null;

            if (Shaders.TryGetValue(vertexPath + tcsPath + tesPath + fragPath, out shader))
            {
                return shader;
            }

            string vertexData = GetEmbeddedShader(vertexPath);
            string fragData = GetEmbeddedShader(fragPath);
            string tcsData = GetEmbeddedShader(tcsPath);
            string tesData = GetEmbeddedShader(tesPath);

            if (string.IsNullOrEmpty(vertexData) || string.IsNullOrEmpty(fragData) 
                || string.IsNullOrEmpty(tcsData) || string.IsNullOrEmpty(tesData))
            {
                Log.Debug(Environment.StackTrace);
                Log.Debug("Shader failure for: " + vertFile + " | " + tcsFile + " | " + tesFile + " | " + fragFile);
                return null;
            }

            GLFragmentShader frag = new GLFragmentShader(fragData);
            string log = null;
            if (!frag.Compile(out log))
            {
                frag.Dispose();
                Log.Error(log);
                Log.Debug(Environment.StackTrace);
                Log.Debug("Shader failure for: " + vertFile + " | " + tcsFile + " | " + tesFile + " | " + fragFile);
                return null;
            }

            GLVertexShader vert = new GLVertexShader(vertexData);
            if (!vert.Compile(out log))
            {
                vert.Dispose();
                Log.Error(log);
                Log.Debug(Environment.StackTrace);
                Log.Debug("Shader failure for: " + vertFile + " | " + tcsFile + " | " + tesFile + " | " + fragFile);
                return null;
            }

            GLTcsShader tcs = new GLTcsShader(tcsData);
            if (!tcs.Compile(out log))
            {
                tcs.Dispose();
                Log.Error(log);
                Log.Debug(Environment.StackTrace);
                Log.Debug("Shader failure for: " + vertFile + " | " + tcsFile + " | " + tesFile + " | " + fragFile);
                return null;
            }

            GLTesShader tes = new GLTesShader(tesData);
            if (!tes.Compile(out log))
            {
                tes.Dispose();
                Log.Error(log);
                Log.Debug(Environment.StackTrace);
                Log.Debug("Shader failure for: " + vertFile + " | " + tcsFile + " | " + tesFile + " | " + fragFile);
                return null;
            }

            shader = new GLShaderProgram(true);
            shader.AttachShader(vert);
            shader.AttachShader(tcs);
            shader.AttachShader(tes);
            shader.AttachShader(frag);

            if (!shader.Link(out log))
            {
                shader.Dispose();
                Log.Error(log);
            }

            Shaders[vertexPath + tcsPath + tesPath + fragPath] = shader;
            return shader;
        }

        public static string GetRawFrag(string fragFile)
        {
            string fragPath = Path.Combine("Embedded", "Frag", fragFile);
            return GetEmbeddedShader(fragPath);
        }

        public static IGLProgram GetShader(string vertFile, string geoFile, string fragFile)
        {
            string vertexPath = Path.Combine("Embedded", "Vertex", vertFile);
            string fragPath = Path.Combine("Embedded", "Frag", fragFile);
            string geoPath = Path.Combine("Embedded", "Geom", geoFile);

            IGLProgram shader = null;

            if (Shaders.TryGetValue(vertexPath + geoPath + fragPath, out shader))
            {
                return shader;
            }

            string vertexData = GetEmbeddedShader(vertexPath);
            string fragdata = GetEmbeddedShader(fragPath);
            string geoData = GetEmbeddedShader(geoPath);

            if (string.IsNullOrEmpty(vertexData) || string.IsNullOrEmpty(fragdata))    
            {
                Log.Debug(Environment.StackTrace);
                Log.Debug("Shader failure for: " + vertFile + " | " + fragFile);
                return null;
            }

            GLFragmentShader frag = new GLFragmentShader(fragdata);
            string log = null;
            if (!frag.Compile(out log))
            {
                frag.Dispose();
                Log.Error(log);
                Log.Debug(Environment.StackTrace);
                Log.Debug("Shader failure for: " + vertFile + " | " + fragFile + " | " + geoFile);
                return null;
            }

            GLVertexShader vert = new GLVertexShader(vertexData);
            if (!vert.Compile(out log))
            {
                vert.Dispose();
                Log.Error(log);
                Log.Debug(Environment.StackTrace);
                Log.Debug("Shader failure for: " + vertFile + " | " + fragFile + " | " + geoFile);
                return null;
            }

            GLGeometryShader geo = new GLGeometryShader(geoData);
            if (!geo.Compile(out log))
            {
                geo.Dispose();
                Log.Error(log);
                Log.Debug(Environment.StackTrace);
                Log.Debug("Shader failure for: " + vertFile + " | " + fragFile + " | " + geoFile);
                return null;
            }

            shader = new GLShaderProgram(true);
            shader.AttachShader(vert);
            shader.AttachShader(geo);
            shader.AttachShader(frag);

            if (!shader.Link(out log))
            {
                shader.Dispose();
                Log.Error(log);
                Log.Debug(Environment.StackTrace);
                Log.Debug("Shader failure for: " + vertFile + " | " + fragFile + " | " + geoFile);
                return null;
            }

            Shaders[vertexPath + geoPath + fragPath] = shader;

            return shader;
        }

        public static IGLProgram GetShader(string vertFile, string fragFile)
        {
            string vertexPath = Path.Combine("Embedded", "Vertex", vertFile);
            string fragPath = Path.Combine("Embedded", "Frag", fragFile);

            IGLProgram shader = null;

            if (Shaders.TryGetValue(vertexPath + fragPath, out shader))
            {
                return shader;
            }

            string vertexData = GetEmbeddedShader(vertexPath);
            string fragdata = GetEmbeddedShader(fragPath);

            if (string.IsNullOrEmpty(vertexData) || string.IsNullOrEmpty(fragdata))
            {
                Log.Debug(Environment.StackTrace);
                Log.Debug("Shader failure for: " + vertFile + " | " + fragFile);
                return null;
            }

            GLFragmentShader frag = new GLFragmentShader(fragdata);
            string log = null;
            if (!frag.Compile(out log))
            {
                frag.Dispose();
                Log.Error(log);
                Log.Debug(Environment.StackTrace);
                Log.Debug("Shader failure for: " + vertFile + " | " + fragFile);
                return null;
            }

            GLVertexShader vert = new GLVertexShader(vertexData);
            if (!vert.Compile(out log))
            {
                vert.Dispose();
                Log.Error(log);
                Log.Debug(Environment.StackTrace);
                Log.Debug("Shader failure for: " + vertFile + " | " + fragFile);
                return null;
            }

            shader = new GLShaderProgram(true);
            shader.AttachShader(vert);
            shader.AttachShader(frag);

            if (!shader.Link(out log))
            {
                shader.Dispose();
                Log.Error(log);
                Log.Debug(Environment.StackTrace);
                Log.Debug("Shader failure for: " + vertFile + " | " + fragFile);
                return null;
            }

            Shaders[vertexPath + fragPath] = shader;

            return shader;
        }

        public static void Dispose()
        {
            foreach (GLShaderProgram p in Shaders.Values)
            {
                p.Dispose();
            }

            Shaders.Clear();
        }
    }
}
