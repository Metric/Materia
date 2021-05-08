using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Newtonsoft.Json;

namespace MateriaCore.Settings
{
    public abstract class Settings 
    {
        protected string name;

        protected string FilePath
        {
            get
            {
                return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, name + ".json");
            }
        }

        public abstract void Load();

        protected abstract string GetContent();

        public void Save()
        {
            try
            {
                string data = GetContent();
                if (string.IsNullOrEmpty(data)) return;
                File.WriteAllText(FilePath, data);
            }
            catch (Exception e)
            {
                MLog.Log.Error(e);
            }
        }
    }
}
