using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.IO.Compression;

namespace Materia.Archive
{
    public class MTGArchive : IDisposable
    {
        public class ArchiveFile
        {
            public string path;
            ZipArchiveEntry entry;

            public ArchiveFile(string p, ZipArchiveEntry e)
            {
                path = p;
                entry = e;
            }

            public byte[] ExtractBinary()
            {
                using (MemoryStream ms = new MemoryStream())
                using (var stream = entry.Open())
                {
                    stream.CopyTo(ms);
                    return ms.ToArray();
                }
            }

            public string ExtractText()
            {
                using (MemoryStream ms = new MemoryStream())
                using (var stream = entry.Open())
                {
                    stream.CopyTo(ms);
                    return Encoding.UTF8.GetString(ms.ToArray());
                }
            }

            public Stream GetStream()
            {
                return entry.Open();
            }
        }

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

        public bool IsOpen { get; protected set; }

        private ZipArchive archive;
        private MemoryStream stream;

        public MTGArchive(string path)
        {
            FilePath = path;   
        }

        public MTGArchive(string path, byte[] data)
        {
            FilePath = path;
            stream = new MemoryStream(data);
        }

        private void OpenWithStream()
        {
            if (IsOpen) return;

            if(archive != null)
            {
                archive.Dispose();
                archive = null;
            }

            if (stream == null || stream.Length == 0) return;

            //reset stream position to 0
            stream.Position = 0;
            //tell the ziparchive class not to close the memory
            //stream so we can reuse it
            archive = new ZipArchive(stream, ZipArchiveMode.Read, true);
            IsOpen = true;
        }

        public void Open()
        {
            if (IsOpen) return; 

            if(archive != null)
            {
                archive.Dispose();
                archive = null;
            }

            if (stream != null && stream.Length > 0)
            {
                OpenWithStream();
                return;
            }

            if (!Exists) return;

            archive = ZipFile.OpenRead(FilePath);
            IsOpen = true;
        }

        public List<ArchiveFile> GetAvailableFiles()
        {
            if (!IsOpen || archive == null) return null;

            List<ArchiveFile> files = new List<ArchiveFile>();

            foreach(var entry in archive.Entries)
            {
                files.Add(new ArchiveFile(entry.FullName, entry));
            }

            return files;
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

        public void Close()
        {
            if(IsOpen)
            {
                if(archive != null)
                {
                    archive.Dispose();
                    archive = null;
                }

                IsOpen = false;
            }
        }

        public void Dispose()
        {
            if(IsOpen)
            {
                if(archive != null)
                {
                    archive.Dispose();
                    archive = null;
                }

                IsOpen = false;
            }

            if(stream != null)
            {
                stream.Dispose();
                stream = null;
            }
        }
    }
}
