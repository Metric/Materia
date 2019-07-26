using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Materia.Textures;
using System.Drawing;
using Materia.Shaders;
using Materia.Imaging;
using NLog;
using Materia.GLInterfaces;

namespace Materia.Material
{
    public class PBRMaterial : Material
    {
        private static ILogger Log = LogManager.GetCurrentClassLogger();

        public GLTextuer2D Albedo { get; set; }
        public GLTextuer2D Normal { get; set; }
        public GLTextuer2D Metallic { get; set; }
        public GLTextuer2D Roughness { get; set; }
        public GLTextuer2D Occlusion { get; set; }
        public GLTextuer2D Height { get; set; }
        public GLTextuer2D Thickness { get; set; }
        public GLTextuer2D Emission { get; set; }
        public static GLTextuer2D BRDFLut { get; protected set; }
        public static bool BRDFLoaded { get; protected set; }

        /// <summary>
        /// SSS Related Properties
        /// </summary>
        public float SSSDistortion { get; set; }
        public float SSSAmbient { get; set; }
        public float SSSPower { get; set; }

        public bool UseDisplacement { get; set; }

        public float IOR { get; set; }
        public float HeightScale { get; set; }

        public bool ClipHeight { get; set; }
        public float ClipHeightBias { get; set; }

        public PBRMaterial()
        {

            SSSDistortion = 0.5f;
            SSSAmbient = 0f;
            SSSPower = 1f;

            if (BRDFLut == null)
            {
                BRDFLut = new GLTextuer2D(PixelInternalFormat.Rgba8);
            }

            LoadShader();
        }

        protected virtual void LoadShader()
        {
            Shader = GetShader("pbr.glsl", "pbr.glsl");

            if(Shader == null)
            {
                Log.Error("Failed to load pbr shader");
            }

            LoadBRDF();
        }

        protected void LoadBRDF()
        {
            if (BRDFLoaded) return;

            BRDFLoaded = true;

            Bitmap bmp = (Bitmap)Bitmap.FromFile(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "brdf.png"));
            FloatBitmap fbmp = FloatBitmap.FromBitmap(bmp);

            BRDFLut.Bind();
            BRDFLut.SetData(fbmp.Image, PixelFormat.Rgba, fbmp.Width, fbmp.Height);
            BRDFLut.SetFilter((int)TextureMinFilter.Linear, (int)TextureMagFilter.Linear);
            BRDFLut.SetWrap((int)TextureWrapMode.ClampToEdge);
            GLTextuer2D.Unbind();
        }

        public static void ReleaseBRDF()
        {
            if (BRDFLut != null)
            {
                BRDFLut.Release();
                BRDFLut = null;
            }
        }

        public virtual void Release()
        {
            if (Albedo != null)
            {
                Albedo.Release();
                Albedo = null;
            }
            if (Height != null)
            {
                Height.Release();
                Height = null;
            }
            if (Normal != null)
            {
                Normal.Release();
                Normal = null;
            }
            if (Metallic != null)
            {
                Metallic.Release();
                Metallic = null;
            }
            if (Roughness != null)
            {
                Roughness.Release();
                Roughness = null;
            }
            if (Occlusion != null)
            {
                Occlusion.Release();
                Occlusion = null;
            }
            if(Thickness != null)
            {
                Thickness.Release();
                Thickness = null;
            }
            if(Emission != null)
            {
                Emission.Release();
                Emission = null;
            }
        }
    }
}
