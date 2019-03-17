using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Materia.Textures;
using System.Drawing;
using Materia.Shaders;
using OpenTK.Graphics.OpenGL;
using Materia.Imaging;
using Materia.UI;

namespace Materia.Material
{
    public class PBRMaterial : Material
    {
        public GLTextuer2D Albedo { get; set; }
        public GLTextuer2D Normal { get; set; }
        public GLTextuer2D Metallic { get; set; }
        public GLTextuer2D Roughness { get; set; }
        public GLTextuer2D Occlusion { get; set; }
        public GLTextuer2D Height { get; set; }
        public static GLTextuer2D BRDFLut { get; protected set; }
        public static bool BRDFLoaded { get; protected set; }

        public PBRMaterial()
        {
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
                Console.WriteLine("Failed to load pbr shader");
            }

            LoadBRDF();
        }

        //need to move this out into a main cache area
        void LoadBRDF()
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
            if (UI3DPreview.Instance != null)
            {
                var prev = UI3DPreview.Instance;
                if (Albedo != null && Albedo != prev.defaultBlack && Albedo != prev.defaultDarkGray && Albedo != prev.defaultGray && Albedo != prev.defaultWhite)
                {
                    Albedo.Release();
                    Albedo = null;
                }
                if (Height != null && Height != prev.defaultBlack && Height != prev.defaultDarkGray && Height != prev.defaultGray && Height != prev.defaultWhite)
                {
                    Height.Release();
                    Height = null;
                }
                if (Normal != null && Normal != prev.defaultBlack && Normal != prev.defaultDarkGray && Normal != prev.defaultGray && Normal != prev.defaultWhite)
                {
                    Normal.Release();
                    Normal = null;
                }
                if (Metallic != null && Metallic != prev.defaultBlack && Metallic != prev.defaultDarkGray && Metallic != prev.defaultGray && Metallic != prev.defaultWhite)
                {
                    Metallic.Release();
                    Metallic = null;
                }
                if (Roughness != null && Roughness != prev.defaultBlack && Roughness != prev.defaultDarkGray && Roughness != prev.defaultGray && Roughness != prev.defaultWhite)
                {
                    Roughness.Release();
                    Roughness = null;
                }
                if (Occlusion != null && Occlusion != prev.defaultBlack && Occlusion != prev.defaultDarkGray && Occlusion != prev.defaultGray && Occlusion != prev.defaultWhite)
                {
                    Occlusion.Release();
                    Occlusion = null;
                }
            }
            else
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
            }
        }
    }
}
