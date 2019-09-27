using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NLog;
using System.IO;

namespace Materia.Language
{
    public class Localization
    {
        private static ILogger Log = LogManager.GetCurrentClassLogger();
        private const string LANG_DIRECTORY = "Lang";
        private static Dictionary<string, Localization> cache = new Dictionary<string, Localization>();

        public string Code
        {
            get; protected set;
        }

        public string Type
        {
            get; protected set;
        }

        protected Dictionary<string, string> lookup;

        public Localization()
        {
            lookup = new Dictionary<string, string>();
        }

        protected Localization(string code, string type, Dictionary<string, string> data)
        {
            Code = code;
            lookup = data;
        }

        public void Set(string key, string data)
        {
            lookup[key] = data;
        }

        public bool Get(string key, out string result)
        {
            return lookup.TryGetValue(key, out result);
        }

        public static Localization Load(string code, string type)
        {
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, LANG_DIRECTORY, code, type + ".json");

            Localization result = null;
            if (cache.TryGetValue(path, out result))
            {
                return result;
            }

            if(File.Exists(path))
            {
                string data = File.ReadAllText(path);

                if(!string.IsNullOrEmpty(data))
                {
                    try
                    {
                        Dictionary<string,string> lookup = JsonConvert.DeserializeObject<Dictionary<string, string>>(data);
                        Localization loc = new Localization(code, type, lookup);
                        cache[path] = loc;
                        return loc;
                    }
                    catch (Exception e)
                    {
                        Log.Error(e);
                    }
                }
            }

            return null;
        }
    }
}
