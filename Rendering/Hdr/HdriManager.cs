using Materia.Rendering.Textures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Materia.Rendering.Hdr
{
    public struct HdrMap
    {
        public GLTextureCube irradiance;
        public GLTextureCube prefilter;
    }

    public static class HdriManager
    { 
        private static readonly List<string> available = new List<string>();
        public static List<string> Available { get => available.ToList();  }
        public static void Scan(string directory)
        {
            available.Clear();
            string[] files = System.IO.Directory.GetFiles(directory, "*.hdr");
            for (int i = 0; i < files.Length; ++i)
            {
                var f = files[i];
                available.Add(System.IO.Path.Combine(directory, f));
            }
        }

        public static HdrFile Load(string f)
        {
            if (string.IsNullOrEmpty(f)) return null;
            if (!System.IO.File.Exists(f)) return null;
            HdrFile hdrFile = new HdrFile(f);
            hdrFile.Load();
            return hdrFile;
        }

        public static HdrMap Process(HdrFile f)
        {
            if (f == null || f.Width == 0 || f.Height == 0 || f.Pixels == null) return new HdrMap();

            Converter cv = new Converter();
            GLTexture2D hdMap = f.GetTexture();

            GLTextureCube prefilter = cv.ToCube(hdMap);
            GLTextureCube irradiance = cv.ToIrradiance(prefilter);

            f.Dispose();
            cv.Dispose();

            return new HdrMap
            {
                prefilter = prefilter,
                irradiance = irradiance
            };
        }
    }
}
