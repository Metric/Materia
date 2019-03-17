using System.Linq;

namespace DDSReader.Utilities
{
    public static class ExtensionMethods
    {
        public static bool ArrayEquals(this char[] charArray, string compareString)
        {
            if (charArray == null)
                return false;

            if (compareString == null)
                return false;

            if (charArray.Length != compareString.Length)
                return false;

            return !charArray.Where((t, i) => t != compareString[i]).Any();
        }
    }
}