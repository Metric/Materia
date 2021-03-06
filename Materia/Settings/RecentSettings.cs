﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;
using NLog;

namespace Materia.Settings
{
    public class RecentSettings : Settings
    {
        public class RecentPath
        {
            public string path;
            public long accessed;
        }

        private static ILogger Log = LogManager.GetCurrentClassLogger();
        public List<RecentPath> Paths { get; set; }

        public RecentSettings()
        {
            Paths = new List<RecentPath>();
            name = "recent";
        }

        public void Add(string path)
        {
            if(Paths.Count + 1 > 10)
            {
                Paths.RemoveAt(Paths.Count - 1);
            }

            RecentPath f = Paths.Find(m => m.path.Equals(path));
            if(f == null)
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

            Sort();
        }

        void Sort()
        {
            Paths.Sort((m1, m2) =>
            {
                if (m1.accessed < m2.accessed)
                {
                    return 1;
                }
                else if (m1.accessed > m2.accessed)
                {
                    return -1;
                }

                return 0;
            });
        }

        public override void Load()
        {
            try
            {
                if (File.Exists(FilePath))
                {
                    RecentSettings mt = JsonConvert.DeserializeObject<RecentSettings>(File.ReadAllText(FilePath));

                    if (mt != null)
                    {
                        Paths = mt.Paths;
                    }
                }

                Sort();
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }

        protected override string GetContent()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
