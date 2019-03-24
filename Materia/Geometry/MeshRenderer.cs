using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Materia.Buffers;
using OpenTK.Graphics.OpenGL;
using OpenTK;
using Materia.Material;
using Materia.Shaders;
using Materia.Textures;
using RSMI.Containers;

namespace Materia.Geometry
{
    public class MeshRenderer : Geometry
    {
        public PBRMaterial Mat { get; set; }

        public GLTextuer2D IrradianceMap { get; set; }
        public GLTextuer2D PrefilterMap { get; set; }

        public Vector3 CameraPosition { get; set; }
        public Vector3 LightPosition { get; set; }
        public Vector3 LightColor { get; set; }

        public Matrix4 Projection { get; set; }
        public Matrix4 View { get; set; }
        public Matrix4 Model { get; set; }
        public Vector2 Tiling { get; set; }

        protected GLArrayBuffer vbo;
        protected GLVertexArray vao;
        protected GLElementBuffer ebo;

        protected int indicesCount;

        public int IndicesCount
        {
            get
            {
                return indicesCount;
            }
        }

        public MeshRenderer(Mesh model)
        {
            Tiling = new Vector2(1, 1);

            vbo = new GLArrayBuffer(BufferUsageHint.StaticDraw);
            ebo = new GLElementBuffer(BufferUsageHint.StaticDraw);
            vao = new GLVertexArray();

            vao.Bind();
            vbo.Bind();
            ebo.Bind();

            vbo.SetData(model.Compact());
            ebo.SetData(model.indices.ToArray());
            indicesCount = model.indices.Count;

            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 12 * sizeof(float), 0);
            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 12 * sizeof(float), (3 * 4) + (2 * 4));
            GL.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, 12 * sizeof(float), 3 * 4);
            GL.VertexAttribPointer(3, 4, VertexAttribPointerType.Float, false, 12 * sizeof(float), (3 * 4) + (2 * 4) + (3 * 4));
            GL.EnableVertexAttribArray(0);
            GL.EnableVertexAttribArray(1);
            GL.EnableVertexAttribArray(2);
            GL.EnableVertexAttribArray(3);

            GLArrayBuffer.Unbind();

            GLVertexArray.Unbind();

            GLElementBuffer.Unbind();
        }

        public void Bind()
        {
            if (vao != null)
            {
                vao.Bind();
            }
        }

        public virtual void DrawForDepth()
        {
            if (Mat != null && Mat.Shader != null)
            {
                GLShaderProgram shader = Mat.Shader;

                //use shader
                shader.Use();

                Matrix4 proj = Projection;
                Matrix4 view = View;
                Matrix4 m = Model;
                Matrix4 norm = Matrix4.Invert(m);
                norm.Transpose();

                shader.SetUniformMatrix4("projectionMatrix", ref proj);
                shader.SetUniformMatrix4("viewMatrix", ref view);
                shader.SetUniformMatrix4("modelMatrix", ref m);
                shader.SetUniformMatrix4("normalMatrix", ref norm);

                Vector2 tiling = Tiling;

                shader.SetUniform2("tiling", ref tiling);

                ///draw
                vao.Bind();
                GL.DrawElements(BeginMode.Triangles, indicesCount, DrawElementsType.UnsignedInt, 0);
                GLVertexArray.Unbind();
            }
        }

        public override void Draw()
        {
            if(Mat != null && Mat.Shader != null)
            {
                GLShaderProgram shader = Mat.Shader;

                GL.ActiveTexture(TextureUnit.Texture0);
                if (Mat.Albedo != null)
                {
                    Mat.Albedo.Bind();
                }

                GL.ActiveTexture(TextureUnit.Texture1);
                if (Mat.Metallic != null)
                {
                    Mat.Metallic.Bind();
                }

                GL.ActiveTexture(TextureUnit.Texture2);
                if (Mat.Roughness != null)
                {
                    Mat.Roughness.Bind();
                }

                GL.ActiveTexture(TextureUnit.Texture3);
                if (Mat.Occlusion != null)
                {
                    Mat.Occlusion.Bind();
                }

                GL.ActiveTexture(TextureUnit.Texture4);
                if (Mat.Normal != null)
                {
                    Mat.Normal.Bind();
                }

                GL.ActiveTexture(TextureUnit.Texture5);
                if (Mat.Height != null)
                {
                    Mat.Height.Bind();
                }

                GL.ActiveTexture(TextureUnit.Texture6);
                PBRMaterial.BRDFLut.Bind();

                GL.ActiveTexture(TextureUnit.Texture7);
                if (IrradianceMap != null)
                {
                    IrradianceMap.Bind();
                }

                GL.ActiveTexture(TextureUnit.Texture8);
                if (PrefilterMap != null)
                {
                    PrefilterMap.Bind();
                }

                //use shader
                shader.Use();

                //set texture bind points
                shader.SetUniform("albedo", 0);
                shader.SetUniform("metallicMap", 1);
                shader.SetUniform("roughnessMap", 2);
                shader.SetUniform("occlusionMap", 3);
                shader.SetUniform("normalMap", 4);
                shader.SetUniform("heightMap", 5);
                shader.SetUniform("brdfLUT", 6);
                shader.SetUniform("irradianceMap", 7);
                shader.SetUniform("prefilterMap", 8);

                Vector3 lpos = LightPosition;
                Vector3 lc = LightColor;

                Vector3 cam = CameraPosition;

                //set camera and light stuff
                shader.SetUniform3("cameraPosition", ref cam);
                shader.SetUniform3("lightPosition", ref lpos);
                shader.SetUniform3("lightColor", ref lc);

                //setup MVP and N matrices
                Matrix4 proj = Projection;
                Matrix4 view = View;
                Matrix4 m = Model;
                Matrix4 norm = Matrix4.Invert(m);
                norm.Transpose();

                shader.SetUniformMatrix4("projectionMatrix", ref proj);
                shader.SetUniformMatrix4("viewMatrix", ref view);
                shader.SetUniformMatrix4("modelMatrix", ref m);
                shader.SetUniformMatrix4("normalMatrix", ref norm);

                Vector2 tiling = Tiling;

                shader.SetUniform2("tiling", ref tiling);

                ///draw
                vao.Bind();
                GL.DrawElements(BeginMode.Triangles, indicesCount, DrawElementsType.UnsignedInt, 0);
                GLVertexArray.Unbind();
                GLTextuer2D.Unbind();
            }
        }

        public override void Release()
        {
            if(vbo != null)
            {
                vbo.Release();
            }

            if(ebo != null)
            {
                ebo.Release();
            }

            if(vao != null)
            {
                vao.Release();
            }

            if(Mat != null)
            {
                Mat.Release();
            }
        }
    }
}
