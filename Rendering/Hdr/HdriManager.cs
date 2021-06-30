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
        private static IHdrFile fileToProcess = null;
        public static HdrMap Map { get; private set; }

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
            if (fileToProcess != null) return null;

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
            fileToProcess = file;
            return file;
        }

        public static bool Process()
        {
            if (fileToProcess == null) return false;

            IHdrFile f = fileToProcess;

            if (f.Width == 0 || f.Height == 0 || f.Pixels == null)
            {
                fileToProcess = null;
                return false;
            }

            Map.Dispose();

            Converter cv = new Converter();
            GLTexture2D hdMap = f.GetTexture();

            GLTextureCube cubeSky = cv.ToCube(hdMap); //this is working as expected now
            GLTextureCube irradiance = cv.ToIrradiance(cubeSky);
            GLTextureCube prefilter = cv.ToPrefilter(cubeSky);

            f.Dispose();
            cv.Dispose();

            Map = new HdrMap
            {
                //testing cube map generator from equi
                prefilter = prefilter,
                irradiance = irradiance,
                environment = cubeSky
            };

            fileToProcess = null;
            return true;
        }

        public static void Dispose()
        {
            fileToProcess?.Dispose();
            fileToProcess = null;

            Map.Dispose();
        }
    }
}
