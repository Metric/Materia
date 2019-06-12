using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using NLog;

namespace Materia.Settings
{
    public class Settings
    {
        private static ILogger Log = LogManager.GetCurrentClassLogger();

        protected string name;

        protected string FilePath
        {
            get
            {
                return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, name + ".json");
            }
        }

        public virtual void Load()
        {

        }

        public void Save()
        {
            try
            {
                
                File.WriteAllText(FilePath, GetContent());
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }

        protected virtual string GetContent()
        {
            return "";
        }
    }
}
