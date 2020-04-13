﻿using System.Linq;
using Materia.Rendering.Buffers;
using Materia.Rendering.Mathematics;
using Materia.Rendering.Interfaces;
using Materia.Rendering.Material;
using Materia.Rendering.Textures;

namespace Materia.Rendering.Geometry
{
    public enum MeshRendererType
    {
        Light,
        PBR,
        Spherical,
        SkyBox
    }

    public class MeshRenderer : IGeometry
    {
        public PBRMaterial Mat { get; set; }

        public GLTextureCube IrradianceMap { get; set; }
        public GLTextureCube PrefilterMap { get; set; }

        public Vector3 CameraPosition { get; set; }
        public Vector3 LightPosition { get; set; }
        public Vector3 LightColor { get; set; }
        public float LightPower { get; set; }

        public Matrix4 Projection { get; set; }
        public Matrix4 View { get; set; }
        public Matrix4 Model { get; set; }
        public Vector2 Tiling { get; set; }

        public float Near { get; set; }
        public float Far { get; set; }

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

        public bool IsLight { get; set; }

        public MeshRenderer(float[] verts, int[] indices)
        {
            IsLight = false;
            Tiling = new Vector2(1, 1);

            vbo = new GLArrayBuffer(BufferUsageHint.StaticDraw);
            ebo = new GLElementBuffer(BufferUsageHint.StaticDraw);
            vao = new GLVertexArray();

            vao.Bind();
            vbo.Bind();
            ebo.Bind();

            vbo.SetData(verts);
            ebo.SetData(indices);
            indicesCount = indices.Length;

            IGL.Primary.VertexAttribPointer(0, 3, (int)VertexAttribPointerType.Float, false, 12 * sizeof(float), 0);
            IGL.Primary.EnableVertexAttribArray(0);

            GLArrayBuffer.Unbind();

            GLVertexArray.Unbind();

            GLElementBuffer.Unbind();
        }

        public MeshRenderer(Mesh model)
        {
            IsLight = false;

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

            IGL.Primary.VertexAttribPointer(0, 3, (int)VertexAttribPointerType.Float, false, 12 * sizeof(float), 0);
            IGL.Primary.VertexAttribPointer(1, 3, (int)VertexAttribPointerType.Float, false, 12 * sizeof(float), (3 * 4) + (2 * 4));
            IGL.Primary.VertexAttribPointer(2, 2, (int)VertexAttribPointerType.Float, false, 12 * sizeof(float), 3 * 4);
            IGL.Primary.VertexAttribPointer(3, 4, (int)VertexAttribPointerType.Float, false, 12 * sizeof(float), (3 * 4) + (2 * 4) + (3 * 4));
            IGL.Primary.EnableVertexAttribArray(0);
            IGL.Primary.EnableVertexAttribArray(1);
            IGL.Primary.EnableVertexAttribArray(2);
            IGL.Primary.EnableVertexAttribArray(3);

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

        public virtual void DrawBasic()
        {
            ///draw
            vao.Bind();
            IGL.Primary.DrawElements((int)BeginMode.Triangles, indicesCount, (int)DrawElementsType.UnsignedInt, 0);
            GLVertexArray.Unbind();
        }

        public virtual void DrawForDepth()
        {
            if (Mat != null && Mat.Shader != null)
            {
                IGLProgram shader = Mat.Shader;

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
                IGL.Primary.DrawElements((int)BeginMode.Triangles, indicesCount, (int)DrawElementsType.UnsignedInt, 0);
                GLVertexArray.Unbind();
            }
        }

        public virtual void DrawAsLight()
        {
            if(Mat != null && Mat.Shader != null)
            {
                IGLProgram shader = Mat.Shader;

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

                Vector4 lightColor = new Vector4(LightColor * LightPower, 1);

                shader.SetUniform2("tiling", ref tiling);

                shader.SetUniform4F("color", ref lightColor);

                ///draw
                vao.Bind();
                IGL.Primary.DrawElements((int)BeginMode.Triangles, indicesCount, (int)DrawElementsType.UnsignedInt, 0);
                GLVertexArray.Unbind();
            }
        }

        public void Draw()
        {
            if(IsLight)
            {
                DrawAsLight();
            }
            else
            {
                DrawFull();
            }
        }

        public virtual void DrawFull()
        {
            if(Mat != null && Mat.Shader != null)
            {
                IGLProgram shader = Mat.Shader;

                IGL.Primary.ActiveTexture((int)TextureUnit.Texture0);
                if (Mat.Albedo != null)
                {
                    Mat.Albedo.Bind();
                }

                IGL.Primary.ActiveTexture((int)TextureUnit.Texture1);
                if (Mat.Metallic != null)
                {
                    Mat.Metallic.Bind();
                }

                IGL.Primary.ActiveTexture((int)TextureUnit.Texture2);
                if (Mat.Roughness != null)
                {
                    Mat.Roughness.Bind();
                }

                IGL.Primary.ActiveTexture((int)TextureUnit.Texture3);
                if (Mat.Occlusion != null)
                {
                    Mat.Occlusion.Bind();
                }

                IGL.Primary.ActiveTexture((int)TextureUnit.Texture4);
                if (Mat.Normal != null)
                {
                    Mat.Normal.Bind();
                }

                IGL.Primary.ActiveTexture((int)TextureUnit.Texture5);
                if (Mat.Height != null)
                {
                    Mat.Height.Bind();
                }

                IGL.Primary.ActiveTexture((int)TextureUnit.Texture6);
                BRDF.Lut.Bind();

                IGL.Primary.ActiveTexture((int)TextureUnit.Texture7);
                if (IrradianceMap != null)
                {
                    IrradianceMap.Bind();
                }

                IGL.Primary.ActiveTexture((int)TextureUnit.Texture8);
                if (PrefilterMap != null)
                {
                    PrefilterMap.Bind();
                }

                IGL.Primary.ActiveTexture((int)TextureUnit.Texture9);
                if(Mat.Thickness != null)
                {
                    Mat.Thickness.Bind();
                }

                IGL.Primary.ActiveTexture((int)TextureUnit.Texture10);
                if(Mat.Emission != null)
                {
                    Mat.Emission.Bind();
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
                shader.SetUniform("thicknessMap", 9);
                shader.SetUniform("emissionMap", 10);

                Vector3 lpos = LightPosition;
                Vector3 lc = LightColor;

                Vector3 cam = CameraPosition;

                //set camera and light stuff
                shader.SetUniform3("cameraPosition", ref cam);
                shader.SetUniform3("lightPosition", ref lpos);
                shader.SetUniform3("lightColor", ref lc);
                shader.SetUniform("lightPower", LightPower);

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

                shader.SetUniform("far", Far);
                shader.SetUniform("near", Near);

                shader.SetUniform("heightScale", Mat.HeightScale);
                shader.SetUniform("refraction", Mat.IOR);

                ///SSS Related
                shader.SetUniform("SSS_Distortion", Mat.SSSDistortion);
                shader.SetUniform("SSS_Ambient", Mat.SSSAmbient);
                shader.SetUniform("SSS_Power", Mat.SSSPower);

                if(Mat.ClipHeight)
                {
                    shader.SetUniform("occlusionClipBias", Mat.ClipHeightBias);
                }
                else
                {
                    shader.SetUniform("occlusionClipBias", -1.0f);
                }

                shader.SetUniform("displace", Mat.UseDisplacement);

                Vector2 tiling = Tiling;

                shader.SetUniform2("tiling", ref tiling);

                ///draw
                vao.Bind();

                if (Mat.UseDisplacement)
                {
                    IGL.Primary.DrawElements((int)BeginMode.Patches, indicesCount, (int)DrawElementsType.UnsignedInt, 0);
                }
                else
                {
                    IGL.Primary.DrawElements((int)BeginMode.Triangles, indicesCount, (int)DrawElementsType.UnsignedInt, 0);
                }

                GLVertexArray.Unbind();
                GLTexture2D.Unbind();
            }
        }

        public void Dispose()
        {
            if(vbo != null)
            {
                vbo.Dispose();
            }

            if(ebo != null)
            {
                ebo.Dispose();
            }

            if(vao != null)
            {
                vao.Dispose();
            }

            if(Mat != null)
            {
                Mat.Dispose();
            }
        }
    }
}
