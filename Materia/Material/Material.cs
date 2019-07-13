using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Materia.Shaders;
using System.IO;
using NLog;

namespace Materia.Material
{
    public abstract class Material
    {
        private static ILogger Log = LogManager.GetCurrentClassLogger();

        protected static Dictionary<string, GLShaderProgram> Shaders = new Dictionary<string, GLShaderProgram>();
        public string Name { get; set; }
        public GLShaderProgram Shader { get; protected set; }

        public static GLShaderProgram CompileFragWithVert(string vertFile, string fragData)
        {
            string path = AppDomain.CurrentDomain.BaseDirectory;
            string vertexPath = Path.Combine(path, "Shaders", "Vertex", vertFile);

            GLShaderProgram shader = null;

            if (!File.Exists(vertexPath)) return null;

            string vertexData = File.ReadAllText(vertexPath);

            GLFragmentShader frag = new GLFragmentShader(fragData);
            string log = null;
            if (!frag.Compile(out log))
            {
                frag.Release();
                Log.Error(log);
                return null;
            }

            GLVertexShader vert = new GLVertexShader(vertexData);
            if (!vert.Compile(out log))
            {
                vert.Release();
                Log.Error(log);
                return null;
            }

            shader = new GLShaderProgram(true);
            shader.AttachShader(vert);
            shader.AttachShader(frag);

            if (!shader.Link(out log))
            {
                shader.Release();
                Log.Error(log);
                return null;
            }

            return shader;
        }

        public static GLShaderProgram GetShader(string vertFile, string tcsFile, string tesFile, string fragFile)
        {
            string path = AppDomain.CurrentDomain.BaseDirectory;
            string vertexPath = Path.Combine(path, "Shaders", "Vertex", vertFile);
            string fragPath = Path.Combine(path, "Shaders", "Frag", fragFile);
            string tcsPath = Path.Combine(path, "Shaders", "Tess", tcsFile);
            string tesPath = Path.Combine(path, "Shaders", "Tess", tesFile);

            GLShaderProgram shader = null;

            if(Shaders.TryGetValue(vertexPath + tcsPath + tesPath + fragPath, out shader))
            {
                return shader;
            }

            if (!File.Exists(vertexPath)) return null;
            if (!File.Exists(tcsPath)) return null;
            if (!File.Exists(tesPath)) return null;
            if (!File.Exists(fragPath)) return null;

            string vertexData = File.ReadAllText(vertexPath);
            string fragData = File.ReadAllText(fragPath);
            string tcsData = File.ReadAllText(tcsPath);
            string tesData = File.ReadAllText(tesPath);

            GLFragmentShader frag = new GLFragmentShader(fragData);
            string log = null;
            if(!frag.Compile(out log))
            {
                frag.Release();
                Log.Error(log);
                return null;
            }

            GLVertexShader vert = new GLVertexShader(vertexData);
            if (!vert.Compile(out log))
            {
                vert.Release();
                Log.Error(log);
                return null;
            }

            GLTcsShader tcs = new GLTcsShader(tcsData);
            if (!tcs.Compile(out log))
            {
                tcs.Release();
                Log.Error(log);
                return null;
            }

            GLTesShader tes = new GLTesShader(tesData);
            if (!tes.Compile(out log))
            {
                tes.Release();
                Log.Error(log);
                return null;
            }

            shader = new GLShaderProgram(true);
            shader.AttachShader(vert);
            shader.AttachShader(tcs);
            shader.AttachShader(tes);
            shader.AttachShader(frag);

            if(!shader.Link(out log))
            {
                shader.Release();
                Log.Error(log);
            }

            Shaders[vertexPath + tcsPath + tesPath + fragPath] = shader;
            return shader;
        }

        public static GLShaderProgram GetShader(string vertFile, string fragFile)
        {
            string path = AppDomain.CurrentDomain.BaseDirectory;
            string vertexPath = Path.Combine(path, "Shaders", "Vertex", vertFile);
            string fragPath = Path.Combine(path, "Shaders", "Frag", fragFile);

            GLShaderProgram shader = null;

            if(Shaders.TryGetValue(vertexPath + fragPath, out shader)) {
                return shader;
            }

            if (!File.Exists(vertexPath)) return null;
            if (!File.Exists(fragPath)) return null;

            string vertexData = File.ReadAllText(vertexPath);
            string fragdata = File.ReadAllText(fragPath);

            GLFragmentShader frag = new GLFragmentShader(fragdata);
            string log = null;
            if (!frag.Compile(out log))
            {
                frag.Release();
                Log.Error(log);
                return null;
            }

            GLVertexShader vert = new GLVertexShader(vertexData);
            if (!vert.Compile(out log))
            {
                vert.Release();
                Log.Error(log);
                return null;
            }

            shader = new GLShaderProgram(true);
            shader.AttachShader(vert);
            shader.AttachShader(frag);

            if (!shader.Link(out log))
            {
                shader.Release();
                Log.Error(log);
                return null;
            }

            Shaders[vertexPath + fragPath] = shader;

            return shader;
        }

        public static void ReleaseAll()
        {
            foreach(GLShaderProgram p in Shaders.Values)
            {
                p.Release();
            }

            Shaders.Clear();
        }
    }
}
