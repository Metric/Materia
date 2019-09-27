using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Materia.Language;
using NLog;
using System.IO;

namespace Materia.Shelf
{
    public class ShelfDescriptions
    {
        static ILogger Log = LogManager.GetCurrentClassLogger();
        static Localization local;

        static ShelfDescriptions()
        {
            string code = CultureInfo.InstalledUICulture.TwoLetterISOLanguageName;
            local = Localization.Load(code, "Shelf");

            if (local == null)
            {
                Log.Warn("Failed to load shelf descriptions for language: " + code);
            }
        }

        public static string Get(NodeResource nr)
        {
            string value = null;

            if (nr.Type.Contains("/") || nr.Type.Contains("\\"))
            {
                string filename = Path.GetFileName(nr.Type);

                if (local != null)
                {
                    local.Get(filename, out value);
                }
            }
            else
            {
                if (local != null)
                {
                    local.Get(nr.Type, out value);
                }
            }

            return value;
        }
    }
}
