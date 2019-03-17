using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Materia.Shaders;
using System.IO;

namespace Materia.Material
{
    public abstract class Material
    {
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
                Console.WriteLine(log);
                return null;
            }

            GLVertexShader vert = new GLVertexShader(vertexData);
            if (!vert.Compile(out log))
            {
                vert.Release();
                Console.WriteLine(log);
                return null;
            }

            shader = new GLShaderProgram(true);
            shader.AttachShader(vert);
            shader.AttachShader(frag);

            if (!shader.Link(out log))
            {
                shader.Release();
                Console.WriteLine(log);
                return null;
            }

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
                Console.WriteLine(log);
                return null;
            }

            GLVertexShader vert = new GLVertexShader(vertexData);
            if (!vert.Compile(out log))
            {
                vert.Release();
                Console.WriteLine(log);
                return null;
            }

            shader = new GLShaderProgram(true);
            shader.AttachShader(vert);
            shader.AttachShader(frag);

            if (!shader.Link(out log))
            {
                shader.Release();
                Console.WriteLine(log);
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
