using MateriaCore.Components.GL;
using System;
using System.Collections.Generic;
using System.Text;

namespace MateriaCore.Utils
{
    public static class ShelfDescriptions
    {
        private static Localization.Local language;

        public static string Get(UINodeSource nr)
        {
            if (language == null)
            {
                language = new Localization.Local();
            }

            string v = null;
            if (nr.Type.Contains("/") || nr.Type.Contains("\\"))
            {
                string filename = System.IO.Path.GetFileName(nr.Type);
                return language.Get(filename);
            }


            return language.Get(nr.Type);
        }
    }
}
