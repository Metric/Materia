using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MateriaCore.Settings
{
    public class Recent : Settings
    {
        public class RecentPath
        {
            public string path;
            public long accessed;
        }

        public List<RecentPath> Paths { get; set; } = new List<RecentPath>();

        public Recent()
        {
            name = "recent";
            Load();
        }

        public void Add(string path)
        {
            if (Paths.Count + 1 > 10)
            {
                Paths.RemoveAt(Paths.Count - 1);
            }

            RecentPath f = Paths.Find(m => m.path.Equals(path));
            if (f == null)
            {
                Paths.Add(new RecentPath()
                {
                    path = path,
                    accessed = DateTime.Now.Ticks
                });
            }
            else
            {
                f.accessed = DateTime.Now.Ticks;
            }

            Paths.Sort(Sort);
        }

        protected int Sort(RecentPath m1, RecentPath m2)
        {
            if (m1.accessed < m2.accessed)
            {
                return 1;
            }
            else if(m1.accessed > m2.accessed)
            {
                return -1;
            }

            return 0;
        }

        public override void Load()
        {
            try
            {
                if (File.Exists(FilePath))
                {
                    var paths = JsonConvert.DeserializeObject<List<RecentPath>>(File.ReadAllText(FilePath));

                    if (paths != null)
                    {
                        Paths = paths;
                    }
                }

                Paths.Sort(Sort);
            }
            catch (Exception e)
            {
                MLog.Log.Error(e);
            }
        }

        protected override string GetContent()
        {
            return JsonConvert.SerializeObject(Paths);
        }
    }
}
