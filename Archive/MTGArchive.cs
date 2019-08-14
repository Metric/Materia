using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.IO.Compression;

namespace Materia.Archive
{
    public class MTGArchive
    {
        public string FilePath { get; protected set; }
        public bool Exists
        {
            get
            {
                if(string.IsNullOrEmpty(FilePath))
                {
                    return false;
                }
                return File.Exists(FilePath);
            }
        }

        public MTGArchive(string path)
        {
            FilePath = path;   
        }

        public bool Create(string mtgFile, bool removeSource = true)
        {
            if (string.IsNullOrEmpty(mtgFile)) return false;
            if (!File.Exists(mtgFile)) return false;

            string dir = Path.GetDirectoryName(mtgFile);
            string resources = Path.Combine(dir, "resources");

            if(Exists)
            {
                File.Delete(FilePath);
            }

            try
            {
                using (var archive = ZipFile.Open(FilePath, ZipArchiveMode.Create))
                {
                    archive.CreateEntryFromFile(mtgFile, Path.GetFileName(mtgFile), CompressionLevel.Fastest);
                    if(Directory.Exists(resources))
                    {
                        string[] res = Directory.GetFiles(resources);
                        foreach(string f in res)
                        {
                            var entry = archive.CreateEntry(Path.Combine("resources", Path.GetFileName(f)), CompressionLevel.Fastest);
                            using(FileStream fs = new FileStream(Path.Combine(resources, f), FileMode.Open)) 
                            using (Stream es = entry.Open())
                            {
                                fs.CopyTo(es);
                            }
                        }
                    }
                }

                if(removeSource)
                {
                    File.Delete(mtgFile);

                    if (Directory.Exists(resources))
                    {
                        Directory.Delete(resources, true);
                    }
                }

                return true;
            }
            catch (Exception e)
            {

            }

            return false;
        }

        public bool UnzipTo(string path)
        {
            if (!Exists) return false;

            try
            {
                if(Directory.Exists(path))
                {
                    Directory.Delete(path, true);
                }

                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                ZipFile.ExtractToDirectory(FilePath, path);
                return true;
            }
            catch (Exception e)
            {
            }

            return false;
        }

    }
}
