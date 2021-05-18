using Materia.Rendering.Textures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Materia.Rendering.Hdr
{
    public struct HdrMap
    {
        public GLTextureCube environment;
        public GLTextureCube irradiance;
        public GLTextureCube prefilter;

        public void Dispose()
        {
            environment?.Dispose();
            environment = null;
            irradiance?.Dispose();
            irradiance = null;
            prefilter?.Dispose();
            prefilter = null;
        }
    }

    public static class HdriManager
    { 
        private static readonly List<string> available = new List<string>();
        public static List<string> Available { get => available.ToList();  }
        public static void Scan(string directory)
        {
            available.Clear();
            string[] files = Directory.GetFiles(directory, "*.hdr");
            for (int i = 0; i < files.Length; ++i)
            {
                var f = files[i];
                available.Add(Path.Combine(directory, f));
            }
            files = Directory.GetFiles(directory, "*.exr");
            for (int i = 0; i < files.Length; ++i)
            {
                var f = files[i];
                available.Add(Path.Combine(directory, f));
            }
        }

        public static IHdrFile Load(string f)
        {
            IHdrFile file;
            if (string.IsNullOrEmpty(f)) return null;
            if (!File.Exists(f)) return null;
            if (f.ToLower().EndsWith(".exr"))
            {
                file = new ExrFile(f);
            }
            else
            {
                file = new HdrFile(f);
            }

            file.Load();
            return file;
        }

        public static HdrMap Process(IHdrFile f)
        {
            if (f == null || f.Width == 0 || f.Height == 0 || f.Pixels == null) return new HdrMap();

            Converter cv = new Converter();
            GLTexture2D hdMap = f.GetTexture();

            GLTextureCube cubeSky = cv.ToCube(hdMap); //this is working as expected now
            GLTextureCube irradiance = cv.ToIrradiance(cubeSky);
            GLTextureCube prefilter = cv.ToPrefilter(cubeSky);

            f.Dispose();
            cv.Dispose();

            return new HdrMap
            {
                //testing cube map generator from equi
                prefilter = prefilter,
                irradiance = irradiance,
                environment = cubeSky
            };
        }
    }
}
