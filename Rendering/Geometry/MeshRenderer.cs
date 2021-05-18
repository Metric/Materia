using System.Linq;
using Materia.Rendering.Buffers;
using Materia.Rendering.Mathematics;
using Materia.Rendering.Interfaces;
using Materia.Rendering.Material;
using Materia.Rendering.Textures;
using Materia.Rendering.Utils;

namespace Materia.Rendering.Geometry
{
    public enum MeshRenderType
    {
        PBR,
        Light,
        Depth,
        Skybox
    }

    public class MeshRenderer : IGeometry, IDisposeShared
    {
        protected enum InternalRenderType
        {
            Basic,
            Advanced
        }

        public PBRMaterial Mat { get; set; }

        public GLTextureCube IrradianceMap { get; set; }
        public GLTextureCube PrefilterMap { get; set; }

        public GLTextureCube EnvironmentMap { get; set; }

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

        protected InternalRenderType renderType;

        public MeshRenderType RenderMode { get; set; } = MeshRenderType.PBR;

        protected static bool isSharedDisposed = false;
        protected static GLVertexArray sharedVao;
        /// <summary>
        /// Gets the shared vao. Make sure the Stroke is set before calling this.
        /// </summary>
        /// <value>
        /// The shared vao.
        /// </value>
        public static GLVertexArray SharedVao
        {
            get
            {
                if (sharedVao == null && !isSharedDisposed)
                {
                    sharedVao = new GLVertexArray();
                }

                return sharedVao;
            }
        }

        protected GLArrayBuffer vbo;
        protected GLElementBuffer ebo;

        protected int indicesCount;

        public int IndicesCount
        {
            get
            {
                return indicesCount;
            }
        }

        public MeshRenderer(float[] verts, int[] indices)
        {
            GeometryCache.RegisterForDispose(this);

            Tiling = new Vector2(1, 1);

            renderType = InternalRenderType.Basic;

            vbo = new GLArrayBuffer(BufferUsageHint.StaticDraw);
            ebo = new GLElementBuffer(BufferUsageHint.StaticDraw);

            vbo.Bind();
            ebo.Bind();

            vbo.SetData(verts);
            ebo.SetData(indices);
            indicesCount = indices.Length;

            vbo.Unbind();
            ebo.Unbind();
        }

        public MeshRenderer(Mesh model)
        {
            GeometryCache.RegisterForDispose(this);

            renderType = InternalRenderType.Advanced;

            Tiling = new Vector2(1, 1);

            vbo = new GLArrayBuffer(BufferUsageHint.StaticDraw);
            ebo = new GLElementBuffer(BufferUsageHint.StaticDraw);
            vbo.Bind();
            ebo.Bind();

            vbo.SetData(model.Compact());
            ebo.SetData(model.indices.ToArray());
            indicesCount = model.indices.Count;

            vbo.Unbind();
            ebo.Unbind();
        }

        public void Update()
        {
            //do nothing here
        }

        public virtual void DrawBasic()
        {
            vbo?.Bind();
            ebo?.Bind();

            int dataSize = renderType == InternalRenderType.Advanced ? 12 : 3;       

            if (renderType == InternalRenderType.Basic)
            {
                IGL.Primary.VertexAttribPointer(0, 3, (int)VertexAttribPointerType.Float, false, dataSize * sizeof(float), 0);
                IGL.Primary.EnableVertexAttribArray(0);
                IGL.Primary.DisableVertexAttribArray(1);
                IGL.Primary.DisableVertexAttribArray(2);
                IGL.Primary.DisableVertexAttribArray(3);
            }
            else
            {
                IGL.Primary.VertexAttribPointer(0, 3, (int)VertexAttribPointerType.Float, false, dataSize * sizeof(float), 0);
                IGL.Primary.VertexAttribPointer(1, 3, (int)VertexAttribPointerType.Float, false, dataSize * sizeof(float), (3 * 4) + (2 * 4));
                IGL.Primary.VertexAttribPointer(2, 2, (int)VertexAttribPointerType.Float, false, dataSize * sizeof(float), 3 * 4);
                IGL.Primary.VertexAttribPointer(3, 4, (int)VertexAttribPointerType.Float, false, dataSize * sizeof(float), (3 * 4) + (2 * 4) + (3 * 4));
                IGL.Primary.EnableVertexAttribArray(0);
                IGL.Primary.EnableVertexAttribArray(1);
                IGL.Primary.EnableVertexAttribArray(2);
                IGL.Primary.EnableVertexAttribArray(3);
            }

            IGL.Primary.DrawElements((int)BeginMode.Triangles, indicesCount, (int)DrawElementsType.UnsignedInt, 0);

            ebo?.Unbind();
            vbo?.Unbind();
        }

        public virtual void DrawAsSkybox()
        {
            if (Mat != null && Mat.Shader != null)
            {
                IGLProgram shader = Mat.Shader;
                shader.Use();

                IGL.Primary.ActiveTexture((int)TextureUnit.Texture0);
                Mat.Albedo?.Bind();

                shader.SetUniform("hdrMap", 0);

                Matrix4 proj = Projection;
                Matrix4 view = View;

                shader.SetUniformMatrix4("projectionMatrix", ref proj);
                shader.SetUniformMatrix4("viewMatrix", ref view);

                DrawBasic();

                GLTexture2D.Unbind();
            }
        }

        public virtual void DrawAsDepth()
        {
            if (Mat != null && Mat.Shader != null)
            {
                IGLProgram shader = Mat.Shader;

                //use shader
                shader.Use();

                Matrix4 proj = Projection;
                Matrix4 view = View;
                Matrix4 model = Model;

                shader.SetUniformMatrix4("projectionMatrix", ref proj);
                shader.SetUniformMatrix4("viewMatrix", ref view);
                shader.SetUniformMatrix4("modelMatrix", ref model);

                DrawBasic();
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
                Matrix4 model = Model;

                shader.SetUniformMatrix4("projectionMatrix", ref proj);
                shader.SetUniformMatrix4("viewMatrix", ref view);
                shader.SetUniformMatrix4("modelMatrix", ref model);

                Vector4 lightColor = new Vector4(LightColor * LightPower, 1);

                shader.SetUniform4("color", ref lightColor);

                DrawBasic();
            }
        }

        public void Draw()
        {
            switch (RenderMode)
            {
                case MeshRenderType.Depth:
                    DrawAsDepth();
                    break;
                case MeshRenderType.Light:
                    DrawAsLight();
                    break;
                case MeshRenderType.Skybox:
                    DrawAsSkybox();
                    break;
                case MeshRenderType.PBR:
                    DrawAsPBR();
                    break;
            }
        }

        public virtual void DrawAsPBR()
        {
            if(Mat != null && Mat.Shader != null)
            {
                IGLProgram shader = Mat.Shader;

                //use shader
                shader.Use();

                IGL.Primary.ActiveTexture((int)TextureUnit.Texture0);
                Mat.Albedo?.Bind();

                IGL.Primary.ActiveTexture((int)TextureUnit.Texture1);
                Mat.Metallic?.Bind();

                IGL.Primary.ActiveTexture((int)TextureUnit.Texture2);
                Mat.Roughness?.Bind();

                IGL.Primary.ActiveTexture((int)TextureUnit.Texture3);
                Mat.Occlusion?.Bind();

                IGL.Primary.ActiveTexture((int)TextureUnit.Texture4);
                Mat.Normal?.Bind();

                IGL.Primary.ActiveTexture((int)TextureUnit.Texture5);
                Mat.Height?.Bind();

                IGL.Primary.ActiveTexture((int)TextureUnit.Texture6);
                BRDF.Lut?.Bind();

                IGL.Primary.ActiveTexture((int)TextureUnit.Texture7);
                Mat.Thickness?.Bind();

                IGL.Primary.ActiveTexture((int)TextureUnit.Texture8);
                Mat.Emission?.Bind();

                IGL.Primary.ActiveTexture((int)TextureUnit.Texture9);
                IrradianceMap?.Bind();

                IGL.Primary.ActiveTexture((int)TextureUnit.Texture10);
                PrefilterMap?.Bind();

                IGL.Primary.ActiveTexture((int)TextureUnit.Texture11);
                EnvironmentMap?.Bind();

                //set texture bind points
                shader.SetUniform("albedoMap", 0);
                shader.SetUniform("metallicMap", 1);
                shader.SetUniform("roughnessMap", 2);
                shader.SetUniform("occlusionMap", 3);
                shader.SetUniform("normalMap", 4);
                shader.SetUniform("heightMap", 5);
                shader.SetUniform("brdfLUT", 6);
                shader.SetUniform("thicknessMap", 7);
                shader.SetUniform("emissionMap", 8);
                shader.SetUniform("irradianceMap", 9);
                shader.SetUniform("prefilterMap", 10);
                shader.SetUniform("environmentMap", 11);

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
                Matrix4 model = Model;

                shader.SetUniformMatrix4("projectionMatrix", ref proj);
                shader.SetUniformMatrix4("viewMatrix", ref view);
                shader.SetUniformMatrix4("modelMatrix", ref model);

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

                Vector3 tint = Mat.Tint;
                //set tint
                shader.SetUniform3("tint", ref tint);

                ///draw
                vbo?.Bind();
                ebo?.Bind();

                IGL.Primary.VertexAttribPointer(0, 3, (int)VertexAttribPointerType.Float, false, 12 * sizeof(float), 0);
                IGL.Primary.VertexAttribPointer(1, 3, (int)VertexAttribPointerType.Float, false, 12 * sizeof(float), (3 * 4) + (2 * 4));
                IGL.Primary.VertexAttribPointer(2, 2, (int)VertexAttribPointerType.Float, false, 12 * sizeof(float), 3 * 4);
                IGL.Primary.VertexAttribPointer(3, 4, (int)VertexAttribPointerType.Float, false, 12 * sizeof(float), (3 * 4) + (2 * 4) + (3 * 4));
                IGL.Primary.EnableVertexAttribArray(0);
                IGL.Primary.EnableVertexAttribArray(1);
                IGL.Primary.EnableVertexAttribArray(2);
                IGL.Primary.EnableVertexAttribArray(3);

                if (Mat.UseDisplacement)
                {
                    IGL.Primary.DrawElements((int)BeginMode.Patches, indicesCount, (int)DrawElementsType.UnsignedInt, 0);
                }
                else
                {
                    IGL.Primary.DrawElements((int)BeginMode.Triangles, indicesCount, (int)DrawElementsType.UnsignedInt, 0);
                }

                vbo?.Unbind();
                ebo?.Unbind();

                GLTexture2D.Unbind();
            }
        }

        public void DisposeShared()
        {
            isSharedDisposed = true;
            sharedVao?.Dispose();
            sharedVao = null;
        }

        public void Dispose()
        {
            vbo?.Dispose();
            vbo = null;

            ebo?.Dispose();
            ebo = null;
        }
    }
}
